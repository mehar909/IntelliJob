using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IntelliJob
{
    public static class ResumeProfileService
    {
        private const int MaxResumeBytes = 1 * 1024 * 1024; // 1 MB
        private const int MaxResumeEstimatedPages = 2;
        private const int MaxResumeWordCount = 1800; // approx 2 pages safety cap
        private static readonly Regex EmailRegex = new Regex(@"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex PhoneRegex = new Regex(@"(\+?\d[\d\-\s().]{7,}\d)", RegexOptions.Compiled);
        private static readonly Regex BulletPrefixRegex = new Regex(@"^\s*(?:[-*•]|\d+[\.)])\s*", RegexOptions.Compiled);

        public static ResumeImportResult ImportAndParse(HttpPostedFile postedFile, string physicalFolderPath, string virtualFolderPath)
        {
            ResumeImportResult result = new ResumeImportResult();

            if (postedFile == null || postedFile.ContentLength <= 0)
            {
                result.ValidationMessage = "Please upload a valid resume file.";
                return result;
            }

            Directory.CreateDirectory(physicalFolderPath);

            string safeFileName = Guid.NewGuid().ToString("N") + "_" + Path.GetFileName(postedFile.FileName);
            string physicalPath = Path.Combine(physicalFolderPath, safeFileName);
            postedFile.SaveAs(physicalPath);

            string relativeFolder = NormalizeRelativeFolder(virtualFolderPath);
            string relativePath = string.IsNullOrWhiteSpace(relativeFolder)
                ? safeFileName
                : relativeFolder + "/" + safeFileName;

            result.StoredPhysicalPath = physicalPath;
            result.StoredRelativePath = relativePath;

            return ParseStoredResume(physicalPath, relativePath, postedFile.FileName, deleteOnFailure: true);
        }

        public static ResumeImportResult ParseExistingResume(string physicalPath, string relativePath, string originalFileName)
        {
            return ParseStoredResume(physicalPath, relativePath, originalFileName, deleteOnFailure: false);
        }

        public static ResumeProfileDocument ParseRawText(string rawText, string originalFileName)
        {
            return ParseText(rawText, originalFileName);
        }

        public static ResumeProfileDocument DeserializeDocument(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                JObject exact = JObject.Parse(json);
                if (exact["personalInfo"] != null || exact["professionalSummary"] != null || exact["education"] != null || exact["experience"] != null || exact["projects"] != null || exact["skills"] != null || exact["metadata"] != null)
                    return NormalizeDocument(MapFromExactJson(exact));

                CanonicalResumeDocument canonical = JsonConvert.DeserializeObject<CanonicalResumeDocument>(json);
                if (canonical != null && (canonical.PersonalInfo != null || canonical.ProfessionalSummary != null || canonical.Education != null || canonical.Experience != null || canonical.Projects != null || canonical.Skills != null || canonical.Metadata != null))
                    return NormalizeDocument(MapFromCanonical(canonical));

                ResumeProfileDocument legacyDocument = JsonConvert.DeserializeObject<ResumeProfileDocument>(json);
                return NormalizeDocument(legacyDocument);
            }
            catch
            {
                return null;
            }
        }

        public static string SerializeDocument(ResumeProfileDocument document)
        {
            if (document == null)
                return string.Empty;

            ResumeProfileDocument normalized = NormalizeDocument(document);
            return BuildExactJson(normalized).ToString(Formatting.Indented);
        }

        public static void AddResumeProfileParameters(SqlCommand command, ResumeProfileDocument document, string resumePath)
        {
            document = NormalizeDocument(document);

            AddParameter(command, "@Resume", resumePath);
            AddParameter(command, "@ResumeOriginalFileName", document != null ? document.OriginalFileName : string.Empty);
            AddParameter(command, "@ResumeParseStatus", document != null && document.IsValid ? "ready" : "none");
            AddParameter(command, "@ResumeValidationMessage", document != null ? document.ValidationMessage : string.Empty);
            AddParameter(command, "@ResumeUploadedAt", document != null ? (object)document.ParsedAt : DBNull.Value);
            AddParameter(command, "@ResumeParsedAt", document != null ? (object)document.ParsedAt : DBNull.Value);
            AddParameter(command, "@ResumeStructuredJson", SerializeDocument(document));
            AddParameter(command, "@ResumeRawText", document != null ? document.RawText : string.Empty);
        }

        public static string BuildResumeText(ResumeProfileDocument document)
        {
            document = NormalizeDocument(document);
            if (document == null)
                return string.Empty;

            StringBuilder builder = new StringBuilder();

            AppendSection(builder, "Full Name", document.FullName);
            AppendSection(builder, "Email", document.Email);
            AppendSection(builder, "Mobile", document.Mobile);
            AppendSection(builder, "Address", document.Address);
            AppendSection(builder, "LinkedIn", document.LinkedInUrl);
            AppendSection(builder, "Portfolio", document.PortfolioUrl);
            AppendSection(builder, "Professional Summary", document.ProfessionalSummary);
            AppendEducationEntries(builder, document.EducationDetails);
            AppendExperienceEntries(builder, document.ExperienceDetails);
            AppendProjectEntries(builder, document.ProjectDetails);
            AppendSkillGroups(builder, document.SkillGroups);
            AppendSection(builder, "Certifications", JoinLines(document.Certifications));
            AppendSection(builder, "Languages", JoinLines(document.Languages));

            return builder.ToString().Trim();
        }

        private static ResumeImportResult ParseStoredResume(string physicalPath, string relativePath, string originalFileName, bool deleteOnFailure)
        {
            ResumeImportResult result = new ResumeImportResult
            {
                StoredPhysicalPath = physicalPath,
                StoredRelativePath = relativePath
            };

            if (string.IsNullOrWhiteSpace(physicalPath) || !File.Exists(physicalPath))
            {
                result.ValidationMessage = "The uploaded resume file could not be found.";
                return result;
            }

            FileInfo fileInfo = new FileInfo(physicalPath);
            if (fileInfo.Length > MaxResumeBytes)
            {
                result.ValidationMessage = "Resume upload is limited to 1 MB.";
                if (deleteOnFailure)
                    TryDeleteFile(physicalPath);
                result.StoredPhysicalPath = string.Empty;
                result.StoredRelativePath = string.Empty;
                return result;
            }

            string extractedText = ResumeTextExtractor.ExtractText(physicalPath);
            int estimatedPages = ResumeTextExtractor.EstimatePageCount(physicalPath, extractedText);
            if (estimatedPages > MaxResumeEstimatedPages)
            {
                result.ValidationMessage = "Resume upload allows up to 2 pages only.";
                if (deleteOnFailure)
                    TryDeleteFile(physicalPath);
                result.StoredPhysicalPath = string.Empty;
                result.StoredRelativePath = string.Empty;
                return result;
            }

            int wordCount = CountWords(extractedText);
            if (wordCount > MaxResumeWordCount)
            {
                result.ValidationMessage = "Resume is too long. Please keep it within about 2 pages.";
                if (deleteOnFailure)
                    TryDeleteFile(physicalPath);
                result.StoredPhysicalPath = string.Empty;
                result.StoredRelativePath = string.Empty;
                return result;
            }

            ResumeProfileDocument heuristicDocument = ParseText(extractedText, originalFileName);
            ResumeProfileDocument document = BuildStructuredDocumentWithGemini(extractedText, originalFileName, relativePath, heuristicDocument);
            ApplyFormConstraints(document);
            document.StoredFilePath = relativePath;

            result.Document = document;
            result.IsSuccess = document.IsValid;
            result.ValidationMessage = document.ValidationMessage;

            if (!document.IsValid && deleteOnFailure)
            {
                TryDeleteFile(physicalPath);
                result.StoredPhysicalPath = string.Empty;
                result.StoredRelativePath = string.Empty;
            }

            return result;
        }

        private static int CountWords(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            return Regex.Matches(value, @"\b[\w@.+-]+\b").Count;
        }

        /// <summary>
        /// Applies the same practical limits used by ResumeBuild form validation
        /// while preserving missing fields as empty/null values.
        /// </summary>
        private static void ApplyFormConstraints(ResumeProfileDocument document)
        {
            if (document == null)
                return;

            document.EducationDetails = (document.EducationDetails ?? new List<ResumeEducationEntry>())
                .Where(e => e != null)
                .Take(2)
                .Select(SanitizeEducationEntry)
                .ToList();

            document.ExperienceDetails = (document.ExperienceDetails ?? new List<ResumeExperienceEntry>())
                .Where(e => e != null)
                .Take(5)
                .Select(SanitizeExperienceEntry)
                .ToList();

            document.ProjectDetails = (document.ProjectDetails ?? new List<ResumeProjectEntry>())
                .Where(p => p != null)
                .Take(5)
                .Select(SanitizeProjectEntry)
                .ToList();

            document.SkillGroups = document.SkillGroups ?? new ResumeSkillGroups();
            document.SkillGroups.ProgrammingLanguages = LimitList(document.SkillGroups.ProgrammingLanguages, 5);
            document.SkillGroups.FrameworksLibraries = LimitList(document.SkillGroups.FrameworksLibraries, 5);
            document.SkillGroups.ToolsCloudDatabaseSkills = LimitList(document.SkillGroups.ToolsCloudDatabaseSkills, 5);
            document.SkillGroups.SoftSkillsLanguages = LimitList(document.SkillGroups.SoftSkillsLanguages, 5);
            document.SkillGroups.CustomHeading = SafeTrim(document.SkillGroups.CustomHeading, 100);
            document.SkillGroups.CustomItems = LimitList(document.SkillGroups.CustomItems, 5);
        }

        private static ResumeEducationEntry SanitizeEducationEntry(ResumeEducationEntry entry)
        {
            var cleaned = new ResumeEducationEntry
            {
                SchoolName = SafeTrim(Regex.Replace(entry.SchoolName ?? string.Empty, @"[^A-Za-z\s]", string.Empty), 100),
                Location = SafeTrim(entry.Location, 50),
                Degree = SafeTrim(entry.Degree, 50),
                StartMonth = NormalizeMonth(entry.StartMonth),
                StartYear = NormalizeYear(entry.StartYear),
                EndMonth = NormalizeMonth(entry.EndMonth),
                EndYear = NormalizeYear(entry.EndYear),
                Grade = SafeTrim(entry.Grade, 10),
                Coursework = SafeTrim(entry.Coursework, 500)
            };

            return cleaned;
        }

        private static ResumeExperienceEntry SanitizeExperienceEntry(ResumeExperienceEntry entry)
        {
            List<string> bullets = (entry.Bullets ?? new List<string>())
                .Where(b => !string.IsNullOrWhiteSpace(b))
                .Select(b => b.Trim())
                .ToList();

            string description = string.Join(Environment.NewLine, bullets);
            description = SafeTrim(description, 1000);
            List<string> clippedBullets = string.IsNullOrWhiteSpace(description)
                ? new List<string>()
                : description.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            return new ResumeExperienceEntry
            {
                JobTitle = SafeTrim(entry.JobTitle, 50),
                Company = SafeTrim(entry.Company, 50),
                Location = SafeTrim(entry.Location, 50),
                StartMonth = NormalizeMonth(entry.StartMonth),
                StartYear = NormalizeYear(entry.StartYear),
                EndMonth = NormalizeMonth(entry.EndMonth),
                EndYear = NormalizeYear(entry.EndYear),
                IsCurrent = entry.IsCurrent,
                Bullets = clippedBullets
            };
        }

        private static ResumeProjectEntry SanitizeProjectEntry(ResumeProjectEntry entry)
        {
            return new ResumeProjectEntry
            {
                ProjectTitle = SafeTrim(entry.ProjectTitle, 50),
                TechStack = LimitList(entry.TechStack, 4),
                Description = SafeTrim(entry.Description, 250)
            };
        }

        private static List<string> LimitList(IEnumerable<string> values, int limit)
        {
            return (values ?? Enumerable.Empty<string>())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim())
                .Take(limit)
                .ToList();
        }

        private static string SafeTrim(string value, int maxLength)
        {
            string text = value == null ? string.Empty : value.Trim();
            if (text.Length <= maxLength)
                return text;
            return text.Substring(0, maxLength);
        }

        private static int? NormalizeYear(int? year)
        {
            if (!year.HasValue)
                return null;
            return (year.Value >= 1900 && year.Value <= 2099) ? year : null;
        }

        private static int? NormalizeMonth(int? month)
        {
            if (!month.HasValue)
                return null;
            return (month.Value >= 1 && month.Value <= 12) ? month : null;
        }

        /// <summary>
        /// Tries Gemini as a strict field classifier for canonical resume JSON.
        /// Falls back to local parser if Gemini fails or yields invalid content.
        /// </summary>
        private static ResumeProfileDocument BuildStructuredDocumentWithGemini(string extractedText, string originalFileName, string relativePath, ResumeProfileDocument fallback)
        {
            if (string.IsNullOrWhiteSpace(extractedText))
                return fallback ?? new ResumeProfileDocument();

            try
            {
                GeminiService gemini = new GeminiService();
                ResumeProfileDocument aiDocument = gemini
                    .ClassifyResumeTextToStructuredJsonAsync(extractedText, originalFileName, relativePath)
                    .GetAwaiter()
                    .GetResult();

                if (aiDocument == null)
                    return fallback ?? new ResumeProfileDocument();

                aiDocument.RawText = string.IsNullOrWhiteSpace(aiDocument.RawText) ? extractedText : aiDocument.RawText;
                aiDocument.OriginalFileName = string.IsNullOrWhiteSpace(aiDocument.OriginalFileName) ? originalFileName : aiDocument.OriginalFileName;
                aiDocument.StoredFilePath = string.IsNullOrWhiteSpace(aiDocument.StoredFilePath) ? relativePath : aiDocument.StoredFilePath;
                aiDocument.ParsedAt = DateTime.UtcNow;

                if (fallback != null)
                {
                    if (string.IsNullOrWhiteSpace(aiDocument.FullName)) aiDocument.FullName = fallback.FullName;
                    if (string.IsNullOrWhiteSpace(aiDocument.Email)) aiDocument.Email = fallback.Email;
                    if (string.IsNullOrWhiteSpace(aiDocument.Mobile)) aiDocument.Mobile = fallback.Mobile;
                    if (string.IsNullOrWhiteSpace(aiDocument.Address)) aiDocument.Address = fallback.Address;
                    if (string.IsNullOrWhiteSpace(aiDocument.ProfessionalSummary)) aiDocument.ProfessionalSummary = fallback.ProfessionalSummary;
                    if (aiDocument.EducationDetails == null || aiDocument.EducationDetails.Count == 0) aiDocument.EducationDetails = fallback.EducationDetails;
                    if (aiDocument.ExperienceDetails == null || aiDocument.ExperienceDetails.Count == 0) aiDocument.ExperienceDetails = fallback.ExperienceDetails;
                    if (aiDocument.ProjectDetails == null || aiDocument.ProjectDetails.Count == 0) aiDocument.ProjectDetails = fallback.ProjectDetails;
                    if (aiDocument.SkillGroups == null) aiDocument.SkillGroups = fallback.SkillGroups;
                    if (aiDocument.Metadata == null) aiDocument.Metadata = fallback.Metadata;
                }

                return NormalizeDocument(aiDocument);
            }
            catch
            {
                return fallback ?? new ResumeProfileDocument();
            }
        }

        private static ResumeProfileDocument ParseText(string rawText, string originalFileName)
        {
            ResumeProfileDocument document = new ResumeProfileDocument
            {
                RawText = NormalizeText(rawText),
                OriginalFileName = string.IsNullOrWhiteSpace(originalFileName) ? string.Empty : Path.GetFileName(originalFileName),
                ParsedAt = DateTime.UtcNow
            };

            if (string.IsNullOrWhiteSpace(document.RawText))
            {
                document.ValidationMessage = "The selected file did not contain readable resume text.";
                return document;
            }

            List<string> allLines = document.RawText
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(CleanLine)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            List<string> headerLines = new List<string>();
            Dictionary<string, List<string>> sections = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            string activeSection = "header";

            foreach (string line in allLines)
            {
                string sectionKey;
                if (TryGetSectionKey(line, out sectionKey))
                {
                    activeSection = sectionKey;
                    continue;
                }

                if (string.Equals(activeSection, "header", StringComparison.OrdinalIgnoreCase))
                {
                    headerLines.Add(line);
                    continue;
                }

                if (!sections.ContainsKey(activeSection))
                    sections[activeSection] = new List<string>();

                sections[activeSection].Add(line);
            }

            document.Email = ExtractFirstMatch(EmailRegex, document.RawText);
            document.Mobile = NormalizePhone(ExtractFirstMatch(PhoneRegex, document.RawText));
            document.FullName = ExtractFullName(headerLines);
            document.Headline = ExtractHeadline(headerLines, document.FullName);
            document.Address = ExtractAddress(headerLines, document.Email, document.Mobile);
            document.ProfessionalSummary = BuildSectionText(GetSectionLines(sections, "summary"));
            document.Summary = document.ProfessionalSummary;
            document.Skills = ParseSectionList(GetSectionLines(sections, "skills"), splitTokens: true);
            document.Education = ParseSectionList(GetSectionLines(sections, "education"), splitTokens: false);
            document.Experience = ParseSectionList(GetSectionLines(sections, "experience"), splitTokens: false);
            document.Projects = ParseSectionList(GetSectionLines(sections, "projects"), splitTokens: false);
            document.Certifications = ParseSectionList(GetSectionLines(sections, "certifications"), splitTokens: false);
            document.Languages = ParseSectionList(GetSectionLines(sections, "languages"), splitTokens: false);
            document.EducationDetails = ParseEducationEntries(GetSectionLines(sections, "education"));
            document.ExperienceDetails = ParseExperienceEntries(GetSectionLines(sections, "experience"));
            document.ProjectDetails = ParseProjectEntries(GetSectionLines(sections, "projects"));
            document.SkillGroups = ParseSkillGroups(GetSectionLines(sections, "skills"));

            List<string> validationErrors = new List<string>();
            if (document.RawText.Length < 120)
                validationErrors.Add("The uploaded resume is too short to parse reliably.");

            if (string.IsNullOrWhiteSpace(document.FullName))
                validationErrors.Add("No candidate name could be detected.");

            if (string.IsNullOrWhiteSpace(document.Email) && string.IsNullOrWhiteSpace(document.Mobile))
                validationErrors.Add("No contact email or mobile number could be detected.");

            bool hasStructuredContent = !string.IsNullOrWhiteSpace(document.Summary)
                                        || document.Skills.Count > 0
                                        || document.Education.Count > 0
                                        || document.Experience.Count > 0
                                        || document.Projects.Count > 0
                                        || document.Certifications.Count > 0
                                        || document.Languages.Count > 0;

            if (!hasStructuredContent)
                validationErrors.Add("No structured resume sections could be detected.");

            document.IsValid = validationErrors.Count == 0;
            document.ValidationMessage = document.IsValid
                ? string.Empty
                : string.Join(" ", validationErrors);

            return NormalizeDocument(document);
        }

        private static string ExtractFirstMatch(Regex regex, string text)
        {
            if (regex == null || string.IsNullOrWhiteSpace(text))
                return string.Empty;

            Match match = regex.Match(text);
            return match.Success ? match.Value.Trim() : string.Empty;
        }

        private static string ExtractFullName(IList<string> headerLines)
        {
            if (headerLines == null)
                return string.Empty;

            foreach (string line in headerLines)
            {
                if (LooksLikeName(line))
                    return line.Trim();
            }

            return string.Empty;
        }

        private static string ExtractHeadline(IList<string> headerLines, string fullName)
        {
            if (headerLines == null)
                return string.Empty;

            foreach (string line in headerLines)
            {
                if (string.Equals(line.Trim(), fullName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (ContainsContactDetails(line))
                    continue;

                if (LooksLikeSectionHeading(line))
                    continue;

                if (line.Length > 3)
                    return line.Trim();
            }

            return string.Empty;
        }

        private static string ExtractAddress(IList<string> headerLines, string email, string mobile)
        {
            if (headerLines == null)
                return string.Empty;

            foreach (string line in headerLines)
            {
                if (!string.IsNullOrWhiteSpace(email) && line.IndexOf(email, StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;

                if (!string.IsNullOrWhiteSpace(mobile) && NormalizePhone(line).Replace(" ", string.Empty).Contains(mobile.Replace(" ", string.Empty)))
                    continue;

                if (ContainsContactDetails(line))
                    continue;

                if (line.IndexOf(',') >= 0 || line.IndexOf('|') >= 0)
                    return line.Trim();
            }

            return string.Empty;
        }

        private static bool ContainsContactDetails(string line)
        {
            return EmailRegex.IsMatch(line ?? string.Empty) || PhoneRegex.IsMatch(line ?? string.Empty);
        }

        private static bool LooksLikeName(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            string trimmed = line.Trim();
            if (trimmed.Length < 3 || trimmed.Length > 80)
                return false;

            if (trimmed.IndexOf('@') >= 0 || trimmed.IndexOf(':') >= 0 || trimmed.IndexOf('/') >= 0)
                return false;

            if (ContainsContactDetails(trimmed))
                return false;

            if (trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length > 5)
                return false;

            return Regex.IsMatch(trimmed, @"[A-Za-z]");
        }

        private static string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            string value = Regex.Replace(phone, @"[\s().-]", string.Empty);
            return value.Trim();
        }

        private static bool TryGetSectionKey(string line, out string sectionKey)
        {
            sectionKey = string.Empty;

            if (string.IsNullOrWhiteSpace(line))
                return false;

            string normalized = NormalizeHeading(line);
            if (string.IsNullOrWhiteSpace(normalized))
                return false;

            if (normalized == "summary" || normalized == "professional summary" || normalized == "profile summary" || normalized == "objective" || normalized == "about me")
            {
                sectionKey = "summary";
                return true;
            }

            if (normalized == "skills" || normalized == "technical skills" || normalized == "key skills" || normalized == "core skills" || normalized == "competencies" || normalized == "expertise")
            {
                sectionKey = "skills";
                return true;
            }

            if (normalized == "education" || normalized == "academic background" || normalized == "educational background" || normalized == "qualifications")
            {
                sectionKey = "education";
                return true;
            }

            if (normalized == "experience" || normalized == "work experience" || normalized == "professional experience" || normalized == "employment history" || normalized == "work history")
            {
                sectionKey = "experience";
                return true;
            }

            if (normalized == "projects" || normalized == "personal projects" || normalized == "project experience")
            {
                sectionKey = "projects";
                return true;
            }

            if (normalized == "certifications" || normalized == "certificates" || normalized == "training")
            {
                sectionKey = "certifications";
                return true;
            }

            if (normalized == "languages" || normalized == "language skills")
            {
                sectionKey = "languages";
                return true;
            }

            return false;
        }

        private static bool LooksLikeSectionHeading(string line)
        {
            string sectionKey;
            return TryGetSectionKey(line, out sectionKey);
        }

        private static string NormalizeHeading(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return string.Empty;

            string trimmed = StripBulletPrefix(line).Trim().TrimEnd(':');
            trimmed = Regex.Replace(trimmed, @"\s+", " ");
            return trimmed.ToLowerInvariant();
        }

        private static IEnumerable<string> GetSectionLines(Dictionary<string, List<string>> sections, string key)
        {
            if (sections == null || string.IsNullOrWhiteSpace(key) || !sections.ContainsKey(key))
                return Enumerable.Empty<string>();

            return sections[key];
        }

        private static List<string> ParseSectionList(IEnumerable<string> lines, bool splitTokens)
        {
            List<string> items = new List<string>();
            if (lines == null)
                return items;

            foreach (string line in lines)
            {
                string cleaned = StripBulletPrefix(line).Trim();
                if (string.IsNullOrWhiteSpace(cleaned))
                    continue;

                if (splitTokens)
                {
                    foreach (string token in cleaned.Split(new[] { ',', ';', '|', '•' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string tokenValue = token.Trim();
                        if (!string.IsNullOrWhiteSpace(tokenValue))
                            items.Add(tokenValue);
                    }
                }
                else
                {
                    items.Add(cleaned);
                }
            }

            return items
                .Select(item => Regex.Replace(item, @"\s+", " ").Trim())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<ResumeEducationEntry> ParseEducationEntries(IEnumerable<string> lines)
        {
            var entries = new List<ResumeEducationEntry>();
            if (lines == null)
                return entries;

            foreach (string line in lines)
            {
                string cleaned = StripBulletPrefix(line).Trim();
                if (string.IsNullOrWhiteSpace(cleaned))
                    continue;

                string[] parts = SplitStructuredParts(cleaned);
                ResumeEducationEntry entry = new ResumeEducationEntry();
                if (parts.Length >= 1) entry.SchoolName = parts[0].Trim();
                if (parts.Length >= 2) entry.Location = parts[1].Trim();
                if (parts.Length >= 3) entry.Degree = parts[2].Trim();

                string timeline = parts.Length >= 4 ? parts[3].Trim() : string.Empty;
                string gradeOrNotes = parts.Length >= 5 ? parts[4].Trim() : string.Empty;
                string coursework = parts.Length >= 6 ? string.Join(" | ", parts.Skip(5).Select(part => part.Trim()).Where(part => !string.IsNullOrWhiteSpace(part))) : string.Empty;

                TryParseDateRange(timeline, out int? startMonth, out int? startYear, out int? endMonth, out int? endYear, out bool isCurrent);
                entry.StartMonth = startMonth;
                entry.StartYear = startYear;
                entry.EndMonth = endMonth;
                entry.EndYear = endYear;

                if (LooksLikeGrade(gradeOrNotes))
                    entry.Grade = gradeOrNotes;
                else if (string.IsNullOrWhiteSpace(coursework))
                    coursework = gradeOrNotes;
                else if (!string.IsNullOrWhiteSpace(gradeOrNotes))
                    coursework = gradeOrNotes + " | " + coursework;

                entry.Coursework = coursework;
                entries.Add(entry);
            }

            return entries.Take(2).ToList();
        }

        private static List<ResumeExperienceEntry> ParseExperienceEntries(IEnumerable<string> lines)
        {
            var entries = new List<ResumeExperienceEntry>();
            if (lines == null)
                return entries;

            foreach (string line in lines)
            {
                string cleaned = StripBulletPrefix(line).Trim();
                if (string.IsNullOrWhiteSpace(cleaned))
                    continue;

                string[] parts = SplitStructuredParts(cleaned);
                ResumeExperienceEntry entry = new ResumeExperienceEntry();
                if (parts.Length >= 1) entry.JobTitle = parts[0].Trim();
                if (parts.Length >= 2) entry.Company = parts[1].Trim();
                if (parts.Length >= 3) entry.Location = parts[2].Trim();

                string timeline = parts.Length >= 4 ? parts[3].Trim() : string.Empty;
                TryParseDateRange(timeline, out int? startMonth, out int? startYear, out int? endMonth, out int? endYear, out bool isCurrent);
                entry.StartMonth = startMonth;
                entry.StartYear = startYear;
                entry.EndMonth = endMonth;
                entry.EndYear = endYear;
                entry.IsCurrent = isCurrent || ContainsPresentToken(timeline);

                var bullets = new List<string>();
                if (parts.Length >= 5)
                    bullets.AddRange(parts.Skip(4).Select(part => part.Trim()).Where(part => !string.IsNullOrWhiteSpace(part)));

                bullets.AddRange(SplitBulletLines(cleaned)
                    .Where(item => !string.IsNullOrWhiteSpace(item)));

                entry.Bullets = bullets
                    .Select(item => Regex.Replace(item, "\\s+", " ").Trim())
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(10)
                    .ToList();

                if (entry.Bullets.Count == 0)
                    entry.Bullets.Add(cleaned);

                entries.Add(entry);
            }

            return entries.Take(5).ToList();
        }

        private static List<ResumeProjectEntry> ParseProjectEntries(IEnumerable<string> lines)
        {
            var entries = new List<ResumeProjectEntry>();
            if (lines == null)
                return entries;

            foreach (string line in lines)
            {
                string cleaned = StripBulletPrefix(line).Trim();
                if (string.IsNullOrWhiteSpace(cleaned))
                    continue;

                string[] parts = SplitStructuredParts(cleaned);
                ResumeProjectEntry entry = new ResumeProjectEntry();
                if (parts.Length >= 1) entry.ProjectTitle = parts[0].Trim();

                if (parts.Length >= 2)
                    entry.TechStack = parts[1].Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(token => token.Trim()).Where(token => !string.IsNullOrWhiteSpace(token)).Distinct(StringComparer.OrdinalIgnoreCase).Take(4).ToList();

                if (parts.Length >= 3)
                    entry.Description = string.Join(" | ", parts.Skip(2).Select(part => part.Trim()).Where(part => !string.IsNullOrWhiteSpace(part)));

                if (string.IsNullOrWhiteSpace(entry.Description))
                    entry.Description = cleaned;

                entries.Add(entry);
            }

            return entries.Take(5).ToList();
        }

        private static ResumeSkillGroups ParseSkillGroups(IEnumerable<string> lines)
        {
            ResumeSkillGroups skillGroups = new ResumeSkillGroups();
            if (lines == null)
                return skillGroups;

            foreach (string line in lines)
            {
                string cleaned = StripBulletPrefix(line).Trim();
                if (string.IsNullOrWhiteSpace(cleaned))
                    continue;

                string[] parts = cleaned.Split(new[] { ':', '|' }, 2);
                string heading = parts.Length > 1 ? parts[0].Trim() : string.Empty;
                string values = parts.Length > 1 ? parts[1].Trim() : cleaned;

                List<string> tokens = values.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(token => token.Trim())
                    .Where(token => !string.IsNullOrWhiteSpace(token))
                    .Take(10)
                    .ToList();

                if (heading.IndexOf("framework", StringComparison.OrdinalIgnoreCase) >= 0)
                    skillGroups.FrameworksLibraries = tokens.Take(5).ToList();
                else if (heading.IndexOf("tool", StringComparison.OrdinalIgnoreCase) >= 0 || heading.IndexOf("cloud", StringComparison.OrdinalIgnoreCase) >= 0 || heading.IndexOf("database", StringComparison.OrdinalIgnoreCase) >= 0)
                    skillGroups.ToolsCloudDatabaseSkills = tokens.Take(5).ToList();
                else if (heading.IndexOf("soft", StringComparison.OrdinalIgnoreCase) >= 0 || heading.IndexOf("language", StringComparison.OrdinalIgnoreCase) >= 0)
                    skillGroups.SoftSkillsLanguages = tokens.Take(5).ToList();
                else if (heading.IndexOf("program", StringComparison.OrdinalIgnoreCase) >= 0 || heading.IndexOf("language", StringComparison.OrdinalIgnoreCase) >= 0)
                    skillGroups.ProgrammingLanguages = tokens.Take(10).ToList();
                else if (!string.IsNullOrWhiteSpace(heading))
                {
                    skillGroups.CustomHeading = heading;
                    skillGroups.CustomItems = tokens.Take(5).ToList();
                }
                else if (skillGroups.ProgrammingLanguages.Count == 0)
                {
                    skillGroups.ProgrammingLanguages = tokens.Take(10).ToList();
                }
            }

            return skillGroups;
        }

        private static string[] SplitStructuredParts(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new string[0];

            return value.Split(new[] { '|', '>' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Trim())
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .ToArray();
        }

        private static IEnumerable<string> SplitBulletLines(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Enumerable.Empty<string>();

            return value.Split(new[] { '\r', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => StripBulletPrefix(item).Trim())
                .Where(item => !string.IsNullOrWhiteSpace(item));
        }

        private static bool ContainsPresentToken(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.IndexOf("present", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool LooksLikeGrade(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            string trimmed = value.Trim();
            return Regex.IsMatch(trimmed, @"\b(CGPA|GPA|grade|percentage|percent|score)\b", RegexOptions.IgnoreCase)
                   || Regex.IsMatch(trimmed, @"\d+(?:\.\d+)?\s*(?:/|out of)\s*\d+(?:\.\d+)?", RegexOptions.IgnoreCase)
                   || Regex.IsMatch(trimmed, @"\d+(?:\.\d+)?%$");
        }

        private static void TryParseDateRange(string value, out int? startMonth, out int? startYear, out int? endMonth, out int? endYear, out bool isCurrent)
        {
            startMonth = null;
            startYear = null;
            endMonth = null;
            endYear = null;
            isCurrent = false;

            if (string.IsNullOrWhiteSpace(value))
                return;

            string cleaned = value.Replace("to", "-").Replace("–", "-").Replace("—", "-");
            if (ContainsPresentToken(cleaned))
                isCurrent = true;

            string[] rangeParts = cleaned.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Trim())
                .ToArray();

            if (rangeParts.Length == 1)
            {
                ParseSingleDateToken(rangeParts[0], out startMonth, out startYear);
                return;
            }

            ParseSingleDateToken(rangeParts[0], out startMonth, out startYear);
            if (!isCurrent)
                ParseSingleDateToken(rangeParts[rangeParts.Length - 1], out endMonth, out endYear);
        }

        private static void ParseSingleDateToken(string value, out int? month, out int? year)
        {
            month = null;
            year = null;

            if (string.IsNullOrWhiteSpace(value))
                return;

            string trimmed = value.Trim();
            Match monthYear = Regex.Match(trimmed, @"^(?<month>\d{1,2})[\/\-](?<year>\d{4})$");
            if (monthYear.Success)
            {
                int parsedMonth;
                int parsedYear;
                if (int.TryParse(monthYear.Groups["month"].Value, out parsedMonth) && int.TryParse(monthYear.Groups["year"].Value, out parsedYear))
                {
                    month = parsedMonth;
                    year = parsedYear;
                }
                return;
            }

            Match yearOnly = Regex.Match(trimmed, @"\b(19|20)\d{2}\b");
            if (yearOnly.Success)
            {
                int parsedYear;
                if (int.TryParse(yearOnly.Value, out parsedYear))
                    year = parsedYear;
            }
        }

        private static string BuildSectionText(IEnumerable<string> lines)
        {
            if (lines == null)
                return string.Empty;

            return string.Join(Environment.NewLine, lines
                .Select(line => StripBulletPrefix(line).Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line)));
        }

        private static string StripBulletPrefix(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return BulletPrefixRegex.Replace(value, string.Empty);
        }

        private static string CleanLine(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string cleaned = value.Replace('\u2022', '-').Replace('\r', ' ').Trim();
            cleaned = Regex.Replace(cleaned, "[ \t]{2,}", " ");
            return cleaned;
        }

        private static string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            string normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
            normalized = Regex.Replace(normalized, "[ \t]{2,}", " ");
            normalized = Regex.Replace(normalized, "\n{3,}", "\n\n");
            return normalized.Trim();
        }

        private static string NormalizeRelativeFolder(string virtualFolderPath)
        {
            if (string.IsNullOrWhiteSpace(virtualFolderPath))
                return string.Empty;

            string folder = virtualFolderPath.Trim().Replace('\\', '/');
            if (folder.StartsWith("~/", StringComparison.Ordinal))
                folder = folder.Substring(2);

            return folder.Trim('/');
        }

        private static string JoinLines(IEnumerable<string> lines)
        {
            if (lines == null)
                return string.Empty;

            return string.Join(Environment.NewLine, lines.Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Trim()));
        }

        private static void AddParameter(SqlCommand command, string parameterName, object value)
        {
            if (command.Parameters.Contains(parameterName))
                command.Parameters[parameterName].Value = value ?? DBNull.Value;
            else
                command.Parameters.AddWithValue(parameterName, value ?? DBNull.Value);
        }

        private static ResumeProfileDocument NormalizeDocument(ResumeProfileDocument document)
        {
            if (document == null)
                return null;

            document.PersonalInfo = document.PersonalInfo ?? new ResumePersonalInfo();
            document.SkillGroups = document.SkillGroups ?? new ResumeSkillGroups();
            document.Sections = document.Sections ?? new List<ResumeSectionNode>();
            document.Metadata = document.Metadata ?? new ResumeMetadata();
            document.EducationDetails = document.EducationDetails ?? new List<ResumeEducationEntry>();
            document.ExperienceDetails = document.ExperienceDetails ?? new List<ResumeExperienceEntry>();
            document.ProjectDetails = document.ProjectDetails ?? new List<ResumeProjectEntry>();

            document.PersonalInfo.FullName = Coalesce(document.PersonalInfo.FullName, document.FullName);
            document.PersonalInfo.Email = Coalesce(document.PersonalInfo.Email, document.Email);
            document.PersonalInfo.Mobile = Coalesce(document.PersonalInfo.Mobile, document.Mobile);
            document.PersonalInfo.Address = Coalesce(document.PersonalInfo.Address, document.Address);
            document.PersonalInfo.LinkedInUrl = Coalesce(document.PersonalInfo.LinkedInUrl, document.LinkedInUrl);
            document.PersonalInfo.PortfolioUrl = Coalesce(document.PersonalInfo.PortfolioUrl, document.PortfolioUrl);

            document.FullName = Coalesce(document.FullName, document.PersonalInfo.FullName);
            document.Email = Coalesce(document.Email, document.PersonalInfo.Email);
            document.Mobile = Coalesce(document.Mobile, document.PersonalInfo.Mobile);
            document.Address = Coalesce(document.Address, document.PersonalInfo.Address);
            document.LinkedInUrl = Coalesce(document.LinkedInUrl, document.PersonalInfo.LinkedInUrl);
            document.PortfolioUrl = Coalesce(document.PortfolioUrl, document.PersonalInfo.PortfolioUrl);
            document.ProfessionalSummary = Coalesce(document.ProfessionalSummary, document.Summary);
            document.Summary = Coalesce(document.Summary, document.ProfessionalSummary);
            document.Headline = string.Empty;

            document.Metadata.ResumeType = string.IsNullOrWhiteSpace(document.Metadata.ResumeType) ? "profile" : document.Metadata.ResumeType;
            document.Metadata.OriginalFileName = Coalesce(document.Metadata.OriginalFileName, document.OriginalFileName);
            document.Metadata.StoredFilePath = Coalesce(document.Metadata.StoredFilePath, document.StoredFilePath);
            document.Metadata.ParsedAt = document.Metadata.ParsedAt == DateTime.MinValue ? document.ParsedAt : document.Metadata.ParsedAt;
            document.Metadata.IsValid = document.Metadata.IsValid || document.IsValid;
            if (string.IsNullOrWhiteSpace(document.Metadata.UpdatedAt.ToString("o")) && document.ParsedAt != DateTime.MinValue)
                document.Metadata.UpdatedAt = document.ParsedAt;

            if (document.Metadata.ValidationMessages == null)
                document.Metadata.ValidationMessages = new List<string>();
            if (!string.IsNullOrWhiteSpace(document.ValidationMessage) && document.Metadata.ValidationMessages.Count == 0)
                document.Metadata.ValidationMessages.Add(document.ValidationMessage);

            if (document.Sections.Count == 0)
            {
                document.Sections = new List<ResumeSectionNode>
                {
                    BuildSectionNode("Professional Summary", document.ProfessionalSummary, asBody: true),
                    BuildEducationSection(document.EducationDetails),
                    BuildExperienceSection(document.ExperienceDetails),
                    BuildProjectSection(document.ProjectDetails),
                    BuildSkillGroupsSection(document.SkillGroups),
                    BuildSectionNode("Certifications", JoinLines(document.Certifications)),
                    BuildSectionNode("Languages", JoinLines(document.Languages))
                }.Where(node => node != null && (!string.IsNullOrWhiteSpace(node.Body) || (node.Items != null && node.Items.Count > 0) || (node.Subsections != null && node.Subsections.Count > 0))).ToList();
            }
            else
            {
                document.Sections = document.Sections.Where(node => node != null).ToList();
            }

            return document;
        }

        private static ResumeSectionNode BuildSectionNode(string heading, string content, bool asBody = false)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            ResumeSectionNode node = new ResumeSectionNode
            {
                Heading = heading ?? string.Empty
            };

            if (asBody)
                node.Body = content.Trim();
            else
                node.Items = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(item => item.Trim())
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .ToList();

            return node;
        }

        private static ResumeSectionNode BuildEducationSection(IEnumerable<ResumeEducationEntry> educationDetails)
        {
            if (educationDetails == null)
                return null;

            var entries = educationDetails.Where(entry => entry != null).ToList();
            if (entries.Count == 0)
                return null;

            return new ResumeSectionNode
            {
                Heading = "Education",
                Subsections = entries.Select(entry => new ResumeSectionNode
                {
                    Heading = entry.SchoolName,
                    Body = string.Join(Environment.NewLine, new[]
                    {
                        entry.Location,
                        entry.Degree,
                        FormatMonthYear(entry.StartMonth, entry.StartYear) + " - " + FormatMonthYear(entry.EndMonth, entry.EndYear),
                        entry.Grade,
                        entry.Coursework
                    }.Where(item => !string.IsNullOrWhiteSpace(item)))
                }).ToList()
            };
        }

        private static ResumeSectionNode BuildExperienceSection(IEnumerable<ResumeExperienceEntry> experienceDetails)
        {
            if (experienceDetails == null)
                return null;

            var entries = experienceDetails.Where(entry => entry != null).ToList();
            if (entries.Count == 0)
                return null;

            return new ResumeSectionNode
            {
                Heading = "Experience",
                Subsections = entries.Select(entry => new ResumeSectionNode
                {
                    Heading = entry.JobTitle,
                    Body = string.Join(Environment.NewLine, new[]
                    {
                        entry.Company,
                        entry.Location,
                        FormatMonthYear(entry.StartMonth, entry.StartYear) + " - " + (entry.IsCurrent ? "Present" : FormatMonthYear(entry.EndMonth, entry.EndYear))
                    }.Where(item => !string.IsNullOrWhiteSpace(item))),
                    Items = entry.Bullets ?? new List<string>()
                }).ToList()
            };
        }

        private static ResumeSectionNode BuildProjectSection(IEnumerable<ResumeProjectEntry> projectDetails)
        {
            if (projectDetails == null)
                return null;

            var entries = projectDetails.Where(entry => entry != null).ToList();
            if (entries.Count == 0)
                return null;

            return new ResumeSectionNode
            {
                Heading = "Projects",
                Subsections = entries.Select(entry => new ResumeSectionNode
                {
                    Heading = entry.ProjectTitle,
                    Body = JoinLines(entry.TechStack),
                    Items = (entry.Bullets != null && entry.Bullets.Count > 0)
                        ? entry.Bullets
                        : (!string.IsNullOrWhiteSpace(entry.Description)
                            ? new List<string> { entry.Description }
                            : new List<string>())
                }).ToList()
            };
        }

        private static ResumeSectionNode BuildSkillGroupsSection(ResumeSkillGroups skillGroups)
        {
            if (skillGroups == null)
                return null;

            var items = new List<string>();
            AddSkillSection(items, "Programming Languages", skillGroups.ProgrammingLanguages);
            AddSkillSection(items, "Frameworks/Libraries", skillGroups.FrameworksLibraries);
            AddSkillSection(items, "Tools/Cloud/Database Skills", skillGroups.ToolsCloudDatabaseSkills);
            AddSkillSection(items, "Soft Skills/Languages", skillGroups.SoftSkillsLanguages);
            AddSkillSection(items, skillGroups.CustomHeading, skillGroups.CustomItems);

            if (items.Count == 0)
                return null;

            return new ResumeSectionNode
            {
                Heading = "Skills",
                Items = items
            };
        }

        private static void AddSkillSection(ICollection<string> items, string heading, IEnumerable<string> values)
        {
            if (items == null || string.IsNullOrWhiteSpace(heading) || values == null)
                return;

            string joined = JoinCommaSeparated(values);
            if (string.IsNullOrWhiteSpace(joined))
                return;

            items.Add(heading.Trim() + ": " + joined);
        }

        private static string JoinCommaSeparated(IEnumerable<string> values)
        {
            if (values == null)
                return string.Empty;

            return string.Join(", ", values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()));
        }

        private static string FormatMonthYear(int? month, int? year)
        {
            if (!month.HasValue && !year.HasValue)
                return string.Empty;

            if (!month.HasValue)
                return year.HasValue ? year.Value.ToString() : string.Empty;

            if (!year.HasValue)
                return month.Value.ToString();

            return month.Value.ToString("00") + "/" + year.Value;
        }

        private static JObject BuildExactJson(ResumeProfileDocument document)
        {
            document = NormalizeDocument(document) ?? new ResumeProfileDocument();

            return new JObject
            {
                ["personalInfo"] = new JObject
                {
                    ["fullName"] = document.FullName ?? string.Empty,
                    ["email"] = document.Email ?? string.Empty,
                    ["mobile"] = document.Mobile ?? string.Empty,
                    ["address"] = document.Address ?? string.Empty,
                    ["country"] = document.PersonalInfo != null ? document.PersonalInfo.Country ?? string.Empty : string.Empty,
                    ["linkedinUrl"] = document.LinkedInUrl ?? string.Empty,
                    ["portfolioUrl"] = document.PortfolioUrl ?? string.Empty
                },
                ["professionalSummary"] = document.ProfessionalSummary ?? string.Empty,
                ["education"] = new JArray((document.EducationDetails ?? new List<ResumeEducationEntry>()).Select(entry => new JObject
                {
                    ["schoolName"] = entry?.SchoolName ?? string.Empty,
                    ["location"] = entry?.Location ?? string.Empty,
                    ["degree"] = entry?.Degree ?? string.Empty,
                    ["startMonth"] = entry != null && entry.StartMonth.HasValue ? (JToken)new JValue(entry.StartMonth.Value) : JValue.CreateNull(),
                    ["startYear"] = entry != null && entry.StartYear.HasValue ? (JToken)new JValue(entry.StartYear.Value) : JValue.CreateNull(),
                    ["endMonth"] = entry != null && entry.EndMonth.HasValue ? (JToken)new JValue(entry.EndMonth.Value) : JValue.CreateNull(),
                    ["endYear"] = entry != null && entry.EndYear.HasValue ? (JToken)new JValue(entry.EndYear.Value) : JValue.CreateNull(),
                    ["grade"] = entry?.Grade ?? string.Empty,
                    ["coursework"] = entry?.Coursework ?? string.Empty
                })),
                ["experience"] = new JArray((document.ExperienceDetails ?? new List<ResumeExperienceEntry>()).Select(entry => new JObject
                {
                    ["jobTitle"] = entry?.JobTitle ?? string.Empty,
                    ["company"] = entry?.Company ?? string.Empty,
                    ["location"] = entry?.Location ?? string.Empty,
                    ["startMonth"] = entry != null && entry.StartMonth.HasValue ? (JToken)new JValue(entry.StartMonth.Value) : JValue.CreateNull(),
                    ["startYear"] = entry != null && entry.StartYear.HasValue ? (JToken)new JValue(entry.StartYear.Value) : JValue.CreateNull(),
                    ["endMonth"] = entry != null && entry.EndMonth.HasValue ? (JToken)new JValue(entry.EndMonth.Value) : JValue.CreateNull(),
                    ["endYear"] = entry != null && entry.EndYear.HasValue ? (JToken)new JValue(entry.EndYear.Value) : JValue.CreateNull(),
                    ["isCurrent"] = entry != null && entry.IsCurrent,
                    ["bullets"] = new JArray((entry?.Bullets ?? new List<string>()).Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim()))
                })),
                ["projects"] = new JArray((document.ProjectDetails ?? new List<ResumeProjectEntry>()).Select(entry => new JObject
                {
                    ["projectTitle"] = entry?.ProjectTitle ?? string.Empty,
                    ["techStack"] = new JArray((entry?.TechStack ?? new List<string>()).Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim())),
                    ["bullets"] = new JArray((entry?.Bullets != null && entry.Bullets.Count > 0
                        ? entry.Bullets
                        : (!string.IsNullOrWhiteSpace(entry?.Description) ? new List<string> { entry.Description } : new List<string>()))
                        .Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim())),
                    ["description"] = entry?.Description ?? string.Empty
                })),
                ["skills"] = new JObject
                {
                    ["programmingLanguages"] = new JArray((document.SkillGroups != null ? document.SkillGroups.ProgrammingLanguages : document.Skills ?? new List<string>()).Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim())),
                    ["frameworksLibraries"] = new JArray((document.SkillGroups != null ? document.SkillGroups.FrameworksLibraries : new List<string>()).Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim())),
                    ["toolsCloudDatabase"] = new JArray((document.SkillGroups != null ? document.SkillGroups.ToolsCloudDatabaseSkills : new List<string>()).Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim())),
                    ["softSkillsLanguages"] = new JArray((document.SkillGroups != null ? document.SkillGroups.SoftSkillsLanguages : new List<string>()).Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim())),
                    ["customSection"] = new JObject
                    {
                        ["heading"] = document.SkillGroups != null ? document.SkillGroups.CustomHeading ?? string.Empty : string.Empty,
                        ["items"] = new JArray((document.SkillGroups != null ? document.SkillGroups.CustomItems : new List<string>()).Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim()))
                    }
                },
                ["metadata"] = new JObject
                {
                    ["resumeType"] = document.Metadata != null && !string.IsNullOrWhiteSpace(document.Metadata.ResumeType) ? document.Metadata.ResumeType : "profile",
                    ["originalFileName"] = document.OriginalFileName ?? string.Empty,
                    ["storedFilePath"] = document.StoredFilePath ?? string.Empty,
                    ["parsedAt"] = document.ParsedAt == DateTime.MinValue ? (JToken)JValue.CreateNull() : JToken.FromObject(document.ParsedAt),
                    ["updatedAt"] = document.Metadata != null && document.Metadata.UpdatedAt != DateTime.MinValue ? JToken.FromObject(document.Metadata.UpdatedAt) : JToken.FromObject(DateTime.UtcNow),
                    ["isValid"] = document.IsValid,
                    ["validationMessages"] = new JArray(CollectValidationMessages(document))
                }
            };
        }

        private static List<string> CollectValidationMessages(ResumeProfileDocument document)
        {
            if (document == null)
                return new List<string>();

            if (document.Metadata != null && document.Metadata.ValidationMessages != null && document.Metadata.ValidationMessages.Count > 0)
                return document.Metadata.ValidationMessages.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim()).ToList();

            if (!string.IsNullOrWhiteSpace(document.ValidationMessage))
                return new List<string> { document.ValidationMessage };

            return new List<string>();
        }

        private static ResumeProfileDocument MapFromExactJson(JObject root)
        {
            if (root == null)
                return null;

            var document = new ResumeProfileDocument();
            JObject personal = root["personalInfo"] as JObject ?? new JObject();
            document.PersonalInfo = new ResumePersonalInfo
            {
                FullName = personal["fullName"]?.Value<string>() ?? string.Empty,
                Email = personal["email"]?.Value<string>() ?? string.Empty,
                Mobile = personal["mobile"]?.Value<string>() ?? string.Empty,
                Address = personal["address"]?.Value<string>() ?? string.Empty,
                Country = personal["country"]?.Value<string>() ?? string.Empty,
                LinkedInUrl = personal["linkedinUrl"]?.Value<string>() ?? string.Empty,
                PortfolioUrl = personal["portfolioUrl"]?.Value<string>() ?? string.Empty
            };

            document.FullName = document.PersonalInfo.FullName;
            document.Email = document.PersonalInfo.Email;
            document.Mobile = document.PersonalInfo.Mobile;
            document.Address = document.PersonalInfo.Address;
            document.LinkedInUrl = document.PersonalInfo.LinkedInUrl;
            document.PortfolioUrl = document.PersonalInfo.PortfolioUrl;
            document.ProfessionalSummary = root["professionalSummary"]?.Value<string>() ?? string.Empty;
            document.Summary = document.ProfessionalSummary;

            document.EducationDetails = (root["education"] as JArray ?? new JArray()).Select(token => new ResumeEducationEntry
            {
                SchoolName = token["schoolName"]?.Value<string>() ?? string.Empty,
                Location = token["location"]?.Value<string>() ?? string.Empty,
                Degree = token["degree"]?.Value<string>() ?? string.Empty,
                StartMonth = token["startMonth"]?.Type == JTokenType.Integer ? token["startMonth"].Value<int?>() : null,
                StartYear = token["startYear"]?.Type == JTokenType.Integer ? token["startYear"].Value<int?>() : null,
                EndMonth = token["endMonth"]?.Type == JTokenType.Integer ? token["endMonth"].Value<int?>() : null,
                EndYear = token["endYear"]?.Type == JTokenType.Integer ? token["endYear"].Value<int?>() : null,
                Grade = token["grade"]?.Value<string>() ?? string.Empty,
                Coursework = token["coursework"]?.Value<string>() ?? string.Empty
            }).ToList();

            document.ExperienceDetails = (root["experience"] as JArray ?? new JArray()).Select(token => new ResumeExperienceEntry
            {
                JobTitle = token["jobTitle"]?.Value<string>() ?? string.Empty,
                Company = token["company"]?.Value<string>() ?? string.Empty,
                Location = token["location"]?.Value<string>() ?? string.Empty,
                StartMonth = token["startMonth"]?.Type == JTokenType.Integer ? token["startMonth"].Value<int?>() : null,
                StartYear = token["startYear"]?.Type == JTokenType.Integer ? token["startYear"].Value<int?>() : null,
                EndMonth = token["endMonth"]?.Type == JTokenType.Integer ? token["endMonth"].Value<int?>() : null,
                EndYear = token["endYear"]?.Type == JTokenType.Integer ? token["endYear"].Value<int?>() : null,
                IsCurrent = token["isCurrent"]?.Value<bool>() ?? false,
                Bullets = (token["bullets"] as JArray ?? new JArray()).Select(item => item.Value<string>() ?? string.Empty).Where(item => !string.IsNullOrWhiteSpace(item)).ToList()
            }).ToList();

            document.ProjectDetails = (root["projects"] as JArray ?? new JArray()).Select(token => new ResumeProjectEntry
            {
                ProjectTitle = token["projectTitle"]?.Value<string>() ?? string.Empty,
                TechStack = (token["techStack"] as JArray ?? new JArray()).Select(item => item.Value<string>() ?? string.Empty).Where(item => !string.IsNullOrWhiteSpace(item)).ToList(),
                Bullets = (token["bullets"] as JArray ?? new JArray()).Select(item => item.Value<string>() ?? string.Empty).Where(item => !string.IsNullOrWhiteSpace(item)).ToList(),
                Description = token["description"]?.Value<string>() ?? string.Empty
            }).ToList();

            JObject skills = root["skills"] as JObject ?? new JObject();
            JObject customSection = skills["customSection"] as JObject ?? new JObject();
            document.SkillGroups = new ResumeSkillGroups
            {
                ProgrammingLanguages = (skills["programmingLanguages"] as JArray ?? new JArray()).Select(item => item.Value<string>() ?? string.Empty).Where(item => !string.IsNullOrWhiteSpace(item)).ToList(),
                FrameworksLibraries = (skills["frameworksLibraries"] as JArray ?? new JArray()).Select(item => item.Value<string>() ?? string.Empty).Where(item => !string.IsNullOrWhiteSpace(item)).ToList(),
                ToolsCloudDatabaseSkills = (skills["toolsCloudDatabase"] as JArray ?? new JArray()).Select(item => item.Value<string>() ?? string.Empty).Where(item => !string.IsNullOrWhiteSpace(item)).ToList(),
                SoftSkillsLanguages = (skills["softSkillsLanguages"] as JArray ?? new JArray()).Select(item => item.Value<string>() ?? string.Empty).Where(item => !string.IsNullOrWhiteSpace(item)).ToList(),
                CustomHeading = customSection["heading"]?.Value<string>() ?? string.Empty,
                CustomItems = (customSection["items"] as JArray ?? new JArray()).Select(item => item.Value<string>() ?? string.Empty).Where(item => !string.IsNullOrWhiteSpace(item)).ToList()
            };
            document.Skills = BuildSkillList(document.SkillGroups);

            JObject metadata = root["metadata"] as JObject ?? new JObject();
            document.Metadata = new ResumeMetadata
            {
                ResumeType = metadata["resumeType"]?.Value<string>() ?? "profile",
                OriginalFileName = metadata["originalFileName"]?.Value<string>() ?? string.Empty,
                StoredFilePath = metadata["storedFilePath"]?.Value<string>() ?? string.Empty,
                ParsedAt = metadata["parsedAt"]?.Type == JTokenType.Date ? metadata["parsedAt"].Value<DateTime>() : DateTime.MinValue,
                UpdatedAt = metadata["updatedAt"]?.Type == JTokenType.Date ? metadata["updatedAt"].Value<DateTime>() : DateTime.UtcNow,
                IsValid = metadata["isValid"]?.Value<bool>() ?? true,
                ValidationMessages = (metadata["validationMessages"] as JArray ?? new JArray()).Select(item => item.Value<string>() ?? string.Empty).Where(item => !string.IsNullOrWhiteSpace(item)).ToList()
            };

            document.RawText = string.Empty;
            document.ValidationMessage = document.Metadata.ValidationMessages.Count > 0 ? string.Join(" ", document.Metadata.ValidationMessages) : string.Empty;
            document.IsValid = document.Metadata.IsValid;
            document.OriginalFileName = document.Metadata.OriginalFileName;
            document.StoredFilePath = document.Metadata.StoredFilePath;
            document.ParsedAt = document.Metadata.ParsedAt;
            document.Headline = string.Empty;
            document.Certifications = new List<string>();
            document.Languages = new List<string>();
            document.Sections = BuildCanonicalSections(document);
            return document;
        }

        private static string Coalesce(string firstValue, string secondValue)
        {
            return string.IsNullOrWhiteSpace(firstValue) ? (secondValue ?? string.Empty) : firstValue;
        }

        private static CanonicalResumeDocument MapToCanonical(ResumeProfileDocument document)
        {
            document = NormalizeDocument(document) ?? new ResumeProfileDocument();

            return new CanonicalResumeDocument
            {
                SchemaVersion = document.SchemaVersion,
                PersonalInfo = new CanonicalPersonalInfo
                {
                    FullName = document.FullName,
                    Email = document.Email,
                    Mobile = document.Mobile,
                    Address = document.Address,
                    Country = document.PersonalInfo != null ? document.PersonalInfo.Country : string.Empty,
                    LinkedInUrl = document.LinkedInUrl,
                    PortfolioUrl = document.PortfolioUrl
                },
                ProfessionalSummary = document.ProfessionalSummary,
                Education = (document.EducationDetails ?? new List<ResumeEducationEntry>()).Select(MapEducationEntryToCanonical).ToList(),
                Experience = (document.ExperienceDetails ?? new List<ResumeExperienceEntry>()).Select(MapExperienceEntryToCanonical).ToList(),
                Projects = (document.ProjectDetails ?? new List<ResumeProjectEntry>()).Select(MapProjectEntryToCanonical).ToList(),
                Skills = MapSkillGroupsToCanonical(document.SkillGroups, document.Skills),
                Metadata = MapMetadataToCanonical(document)
            };
        }

        private static ResumeProfileDocument MapFromCanonical(CanonicalResumeDocument canonical)
        {
            if (canonical == null)
                return null;

            ResumeProfileDocument document = new ResumeProfileDocument();
            document.SchemaVersion = canonical.SchemaVersion <= 0 ? 1 : canonical.SchemaVersion;
            document.PersonalInfo = new ResumePersonalInfo
            {
                FullName = canonical.PersonalInfo != null ? canonical.PersonalInfo.FullName : string.Empty,
                Email = canonical.PersonalInfo != null ? canonical.PersonalInfo.Email : string.Empty,
                Mobile = canonical.PersonalInfo != null ? canonical.PersonalInfo.Mobile : string.Empty,
                Address = canonical.PersonalInfo != null ? canonical.PersonalInfo.Address : string.Empty,
                Country = canonical.PersonalInfo != null ? canonical.PersonalInfo.Country : string.Empty,
                LinkedInUrl = canonical.PersonalInfo != null ? canonical.PersonalInfo.LinkedInUrl : string.Empty,
                PortfolioUrl = canonical.PersonalInfo != null ? canonical.PersonalInfo.PortfolioUrl : string.Empty
            };

            document.FullName = document.PersonalInfo.FullName;
            document.Email = document.PersonalInfo.Email;
            document.Mobile = document.PersonalInfo.Mobile;
            document.Address = document.PersonalInfo.Address;
            document.LinkedInUrl = document.PersonalInfo.LinkedInUrl;
            document.PortfolioUrl = document.PersonalInfo.PortfolioUrl;
            document.ProfessionalSummary = canonical.ProfessionalSummary ?? string.Empty;
            document.Summary = document.ProfessionalSummary;
            document.EducationDetails = (canonical.Education ?? new List<CanonicalEducationEntry>()).Select(MapEducationEntryFromCanonical).Where(entry => entry != null).ToList();
            document.ExperienceDetails = (canonical.Experience ?? new List<CanonicalExperienceEntry>()).Select(MapExperienceEntryFromCanonical).Where(entry => entry != null).ToList();
            document.ProjectDetails = (canonical.Projects ?? new List<CanonicalProjectEntry>()).Select(MapProjectEntryFromCanonical).Where(entry => entry != null).ToList();
            document.SkillGroups = MapSkillGroupsFromCanonical(canonical.Skills);
            document.Skills = BuildSkillList(document.SkillGroups);
            document.Education = document.EducationDetails.Select(MapEducationEntryToLegacy).Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
            document.Experience = document.ExperienceDetails.Select(MapExperienceEntryToLegacy).Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
            document.Projects = document.ProjectDetails.Select(MapProjectEntryToLegacy).Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
            document.Certifications = new List<string>();
            document.Languages = new List<string>();
            document.Metadata = MapMetadataFromCanonical(canonical.Metadata, document);
            document.RawText = string.Empty;
            document.ValidationMessage = document.Metadata.ValidationMessages.Count > 0 ? string.Join(" ", document.Metadata.ValidationMessages) : string.Empty;
            document.IsValid = document.Metadata.IsValid;
            document.OriginalFileName = document.Metadata.OriginalFileName;
            document.StoredFilePath = document.Metadata.StoredFilePath;
            document.ParsedAt = document.Metadata.ParsedAt;
            document.Headline = string.Empty;
            document.Sections = BuildCanonicalSections(document);
            return document;
        }

        private static CanonicalPersonalInfo MapPersonalInfo(ResumeProfileDocument document)
        {
            return new CanonicalPersonalInfo
            {
                FullName = document.FullName,
                Email = document.Email,
                Mobile = document.Mobile,
                Address = document.Address,
                Country = document.PersonalInfo != null ? document.PersonalInfo.Country : string.Empty,
                LinkedInUrl = document.LinkedInUrl,
                PortfolioUrl = document.PortfolioUrl
            };
        }

        private static CanonicalEducationEntry MapEducationEntryToCanonical(ResumeEducationEntry entry)
        {
            if (entry == null)
                return null;

            return new CanonicalEducationEntry
            {
                SchoolName = entry.SchoolName,
                Location = entry.Location,
                Degree = entry.Degree,
                StartMonth = entry.StartMonth,
                StartYear = entry.StartYear,
                EndMonth = entry.EndMonth,
                EndYear = entry.EndYear,
                Grade = entry.Grade,
                Coursework = entry.Coursework
            };
        }

        private static ResumeEducationEntry MapEducationEntryFromCanonical(CanonicalEducationEntry entry)
        {
            if (entry == null)
                return null;

            return new ResumeEducationEntry
            {
                SchoolName = entry.SchoolName ?? string.Empty,
                Location = entry.Location ?? string.Empty,
                Degree = entry.Degree ?? string.Empty,
                StartMonth = entry.StartMonth,
                StartYear = entry.StartYear,
                EndMonth = entry.EndMonth,
                EndYear = entry.EndYear,
                Grade = entry.Grade ?? string.Empty,
                Coursework = entry.Coursework ?? string.Empty
            };
        }

        private static string MapEducationEntryToLegacy(ResumeEducationEntry entry)
        {
            if (entry == null)
                return string.Empty;

            return string.Join(" | ", new[]
            {
                entry.SchoolName,
                entry.Location,
                entry.Degree,
                FormatMonthYear(entry.StartMonth, entry.StartYear),
                FormatMonthYear(entry.EndMonth, entry.EndYear),
                entry.Grade,
                entry.Coursework
            }.Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private static CanonicalExperienceEntry MapExperienceEntryToCanonical(ResumeExperienceEntry entry)
        {
            if (entry == null)
                return null;

            return new CanonicalExperienceEntry
            {
                JobTitle = entry.JobTitle,
                Company = entry.Company,
                Location = entry.Location,
                StartMonth = entry.StartMonth,
                StartYear = entry.StartYear,
                EndMonth = entry.EndMonth,
                EndYear = entry.EndYear,
                IsCurrent = entry.IsCurrent,
                Bullets = entry.Bullets ?? new List<string>()
            };
        }

        private static ResumeExperienceEntry MapExperienceEntryFromCanonical(CanonicalExperienceEntry entry)
        {
            if (entry == null)
                return null;

            return new ResumeExperienceEntry
            {
                JobTitle = entry.JobTitle ?? string.Empty,
                Company = entry.Company ?? string.Empty,
                Location = entry.Location ?? string.Empty,
                StartMonth = entry.StartMonth,
                StartYear = entry.StartYear,
                EndMonth = entry.EndMonth,
                EndYear = entry.EndYear,
                IsCurrent = entry.IsCurrent,
                Bullets = entry.Bullets != null ? entry.Bullets.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim()).ToList() : new List<string>()
            };
        }

        private static string MapExperienceEntryToLegacy(ResumeExperienceEntry entry)
        {
            if (entry == null)
                return string.Empty;

            var parts = new List<string> { entry.JobTitle, entry.Company, entry.Location, FormatMonthYear(entry.StartMonth, entry.StartYear), entry.IsCurrent ? "Present" : FormatMonthYear(entry.EndMonth, entry.EndYear) };
            if (entry.Bullets != null && entry.Bullets.Count > 0)
                parts.Add(string.Join("; ", entry.Bullets.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim())));

            return string.Join(" | ", parts.Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private static CanonicalProjectEntry MapProjectEntryToCanonical(ResumeProjectEntry entry)
        {
            if (entry == null)
                return null;

            return new CanonicalProjectEntry
            {
                ProjectTitle = entry.ProjectTitle,
                TechStack = entry.TechStack ?? new List<string>(),
                Bullets = entry.Bullets ?? new List<string>(),
                Description = entry.Description
            };
        }

        private static ResumeProjectEntry MapProjectEntryFromCanonical(CanonicalProjectEntry entry)
        {
            if (entry == null)
                return null;

            return new ResumeProjectEntry
            {
                ProjectTitle = entry.ProjectTitle ?? string.Empty,
                TechStack = entry.TechStack != null ? entry.TechStack.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim()).ToList() : new List<string>(),
                Bullets = entry.Bullets != null ? entry.Bullets.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim()).ToList() : new List<string>(),
                Description = entry.Description ?? string.Empty
            };
        }

        private static string MapProjectEntryToLegacy(ResumeProjectEntry entry)
        {
            if (entry == null)
                return string.Empty;

            string descPart = (entry.Bullets != null && entry.Bullets.Count > 0)
                ? string.Join("; ", entry.Bullets.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim()))
                : (entry.Description ?? string.Empty);

            return string.Join(" | ", new[]
            {
                entry.ProjectTitle,
                entry.TechStack != null ? string.Join(", ", entry.TechStack.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim())) : string.Empty,
                descPart
            }.Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private static CanonicalSkills MapSkillGroupsToCanonical(ResumeSkillGroups skillGroups, IEnumerable<string> fallbackSkills)
        {
            skillGroups = skillGroups ?? new ResumeSkillGroups();
            return new CanonicalSkills
            {
                ProgrammingLanguages = skillGroups.ProgrammingLanguages ?? BuildSkillListFromFallback(fallbackSkills),
                FrameworksLibraries = skillGroups.FrameworksLibraries ?? new List<string>(),
                ToolsCloudDatabase = skillGroups.ToolsCloudDatabaseSkills ?? new List<string>(),
                SoftSkillsLanguages = skillGroups.SoftSkillsLanguages ?? new List<string>(),
                CustomSection = new CanonicalSkillSection
                {
                    Heading = skillGroups.CustomHeading ?? string.Empty,
                    Items = skillGroups.CustomItems ?? new List<string>()
                }
            };
        }

        private static ResumeSkillGroups MapSkillGroupsFromCanonical(CanonicalSkills skills)
        {
            skills = skills ?? new CanonicalSkills();
            return new ResumeSkillGroups
            {
                ProgrammingLanguages = skills.ProgrammingLanguages ?? new List<string>(),
                FrameworksLibraries = skills.FrameworksLibraries ?? new List<string>(),
                ToolsCloudDatabaseSkills = skills.ToolsCloudDatabase ?? new List<string>(),
                SoftSkillsLanguages = skills.SoftSkillsLanguages ?? new List<string>(),
                CustomHeading = skills.CustomSection != null ? skills.CustomSection.Heading ?? string.Empty : string.Empty,
                CustomItems = skills.CustomSection != null ? skills.CustomSection.Items ?? new List<string>() : new List<string>()
            };
        }

        private static List<string> BuildSkillList(ResumeSkillGroups skillGroups)
        {
            var items = new List<string>();
            AddSkillListItem(items, "Programming Languages", skillGroups != null ? skillGroups.ProgrammingLanguages : null);
            AddSkillListItem(items, "Frameworks/Libraries", skillGroups != null ? skillGroups.FrameworksLibraries : null);
            AddSkillListItem(items, "Tools/Cloud/Database Skills", skillGroups != null ? skillGroups.ToolsCloudDatabaseSkills : null);
            AddSkillListItem(items, "Soft Skills/Languages", skillGroups != null ? skillGroups.SoftSkillsLanguages : null);
            if (skillGroups != null && !string.IsNullOrWhiteSpace(skillGroups.CustomHeading))
                AddSkillListItem(items, skillGroups.CustomHeading, skillGroups.CustomItems);

            return items;
        }

        private static List<string> BuildSkillListFromFallback(IEnumerable<string> fallbackSkills)
        {
            return fallbackSkills == null ? new List<string>() : fallbackSkills.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim()).ToList();
        }

        private static void AddSkillListItem(List<string> items, string heading, IEnumerable<string> values)
        {
            if (items == null || values == null)
                return;

            List<string> list = values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToList();
            if (list.Count == 0)
                return;

            items.Add(heading + ": " + string.Join(", ", list));
        }

        private static CanonicalMetadata MapMetadataToCanonical(ResumeProfileDocument document)
        {
            return new CanonicalMetadata
            {
                ResumeType = document.Metadata != null && !string.IsNullOrWhiteSpace(document.Metadata.ResumeType) ? document.Metadata.ResumeType : "profile",
                OriginalFileName = document.OriginalFileName,
                StoredFilePath = document.StoredFilePath,
                ParsedAt = document.ParsedAt == DateTime.MinValue ? (document.Metadata != null ? document.Metadata.ParsedAt : DateTime.UtcNow) : document.ParsedAt,
                UpdatedAt = document.Metadata != null && document.Metadata.UpdatedAt != DateTime.MinValue ? document.Metadata.UpdatedAt : DateTime.UtcNow,
                IsValid = document.IsValid,
                ValidationMessages = string.IsNullOrWhiteSpace(document.ValidationMessage)
                    ? (document.Metadata != null && document.Metadata.ValidationMessages != null ? document.Metadata.ValidationMessages : new List<string>())
                    : new List<string> { document.ValidationMessage }
            };
        }

        private static ResumeMetadata MapMetadataFromCanonical(CanonicalMetadata metadata, ResumeProfileDocument document)
        {
            metadata = metadata ?? new CanonicalMetadata();
            return new ResumeMetadata
            {
                ResumeType = string.IsNullOrWhiteSpace(metadata.ResumeType) ? "profile" : metadata.ResumeType,
                OriginalFileName = metadata.OriginalFileName ?? string.Empty,
                StoredFilePath = metadata.StoredFilePath ?? string.Empty,
                ParsedAt = metadata.ParsedAt == DateTime.MinValue ? document.ParsedAt : metadata.ParsedAt,
                UpdatedAt = metadata.UpdatedAt == DateTime.MinValue ? DateTime.UtcNow : metadata.UpdatedAt,
                IsValid = metadata.IsValid || document.IsValid,
                ValidationMessages = metadata.ValidationMessages != null ? metadata.ValidationMessages.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim()).ToList() : new List<string>()
            };
        }

        private static List<ResumeSectionNode> BuildCanonicalSections(ResumeProfileDocument document)
        {
            var sections = new List<ResumeSectionNode>();

            if (!string.IsNullOrWhiteSpace(document.ProfessionalSummary))
                sections.Add(BuildSectionNode("Professional Summary", document.ProfessionalSummary, asBody: true));

            ResumeSectionNode education = BuildEducationSection(document.EducationDetails);
            if (education != null) sections.Add(education);
            ResumeSectionNode experience = BuildExperienceSection(document.ExperienceDetails);
            if (experience != null) sections.Add(experience);
            ResumeSectionNode projects = BuildProjectSection(document.ProjectDetails);
            if (projects != null) sections.Add(projects);
            ResumeSectionNode skills = BuildSkillGroupsSection(document.SkillGroups);
            if (skills != null) sections.Add(skills);

            if (document.Certifications != null && document.Certifications.Count > 0)
                sections.Add(BuildSectionNode("Certifications", JoinLines(document.Certifications)));
            if (document.Languages != null && document.Languages.Count > 0)
                sections.Add(BuildSectionNode("Languages", JoinLines(document.Languages)));

            return sections.Where(node => node != null).ToList();
        }

        private class CanonicalResumeDocument
        {
            [JsonProperty("schemaVersion")]
            public int SchemaVersion { get; set; } = 1;

            [JsonProperty("personalInfo")]
            public CanonicalPersonalInfo PersonalInfo { get; set; } = new CanonicalPersonalInfo();

            [JsonProperty("professionalSummary")]
            public string ProfessionalSummary { get; set; } = string.Empty;

            [JsonProperty("education")]
            public List<CanonicalEducationEntry> Education { get; set; } = new List<CanonicalEducationEntry>();

            [JsonProperty("experience")]
            public List<CanonicalExperienceEntry> Experience { get; set; } = new List<CanonicalExperienceEntry>();

            [JsonProperty("projects")]
            public List<CanonicalProjectEntry> Projects { get; set; } = new List<CanonicalProjectEntry>();

            [JsonProperty("skills")]
            public CanonicalSkills Skills { get; set; } = new CanonicalSkills();

            [JsonProperty("metadata")]
            public CanonicalMetadata Metadata { get; set; } = new CanonicalMetadata();
        }

        private class CanonicalPersonalInfo
        {
            [JsonProperty("fullName")]
            public string FullName { get; set; } = string.Empty;

            [JsonProperty("email")]
            public string Email { get; set; } = string.Empty;

            [JsonProperty("mobile")]
            public string Mobile { get; set; } = string.Empty;

            [JsonProperty("address")]
            public string Address { get; set; } = string.Empty;

            [JsonProperty("country")]
            public string Country { get; set; } = string.Empty;

            [JsonProperty("linkedinUrl")]
            public string LinkedInUrl { get; set; } = string.Empty;

            [JsonProperty("portfolioUrl")]
            public string PortfolioUrl { get; set; } = string.Empty;
        }

        private class CanonicalEducationEntry
        {
            [JsonProperty("schoolName")]
            public string SchoolName { get; set; } = string.Empty;

            [JsonProperty("location")]
            public string Location { get; set; } = string.Empty;

            [JsonProperty("degree")]
            public string Degree { get; set; } = string.Empty;

            [JsonProperty("startMonth")]
            public int? StartMonth { get; set; }

            [JsonProperty("startYear")]
            public int? StartYear { get; set; }

            [JsonProperty("endMonth")]
            public int? EndMonth { get; set; }

            [JsonProperty("endYear")]
            public int? EndYear { get; set; }

            [JsonProperty("grade")]
            public string Grade { get; set; } = string.Empty;

            [JsonProperty("coursework")]
            public string Coursework { get; set; } = string.Empty;
        }

        private class CanonicalExperienceEntry
        {
            [JsonProperty("jobTitle")]
            public string JobTitle { get; set; } = string.Empty;

            [JsonProperty("company")]
            public string Company { get; set; } = string.Empty;

            [JsonProperty("location")]
            public string Location { get; set; } = string.Empty;

            [JsonProperty("startMonth")]
            public int? StartMonth { get; set; }

            [JsonProperty("startYear")]
            public int? StartYear { get; set; }

            [JsonProperty("endMonth")]
            public int? EndMonth { get; set; }

            [JsonProperty("endYear")]
            public int? EndYear { get; set; }

            [JsonProperty("isCurrent")]
            public bool IsCurrent { get; set; }

            [JsonProperty("bullets")]
            public List<string> Bullets { get; set; } = new List<string>();
        }

        private class CanonicalProjectEntry
        {
            [JsonProperty("projectTitle")]
            public string ProjectTitle { get; set; } = string.Empty;

            [JsonProperty("techStack")]
            public List<string> TechStack { get; set; } = new List<string>();

            [JsonProperty("bullets")]
            public List<string> Bullets { get; set; } = new List<string>();

            [JsonProperty("description")]
            public string Description { get; set; } = string.Empty;
        }

        private class CanonicalSkills
        {
            [JsonProperty("programmingLanguages")]
            public List<string> ProgrammingLanguages { get; set; } = new List<string>();

            [JsonProperty("frameworksLibraries")]
            public List<string> FrameworksLibraries { get; set; } = new List<string>();

            [JsonProperty("toolsCloudDatabase")]
            public List<string> ToolsCloudDatabase { get; set; } = new List<string>();

            [JsonProperty("softSkillsLanguages")]
            public List<string> SoftSkillsLanguages { get; set; } = new List<string>();

            [JsonProperty("customSection")]
            public CanonicalSkillSection CustomSection { get; set; } = new CanonicalSkillSection();
        }

        private class CanonicalSkillSection
        {
            [JsonProperty("heading")]
            public string Heading { get; set; } = string.Empty;

            [JsonProperty("items")]
            public List<string> Items { get; set; } = new List<string>();
        }

        private class CanonicalMetadata
        {
            [JsonProperty("resumeType")]
            public string ResumeType { get; set; } = string.Empty;

            [JsonProperty("originalFileName")]
            public string OriginalFileName { get; set; } = string.Empty;

            [JsonProperty("storedFilePath")]
            public string StoredFilePath { get; set; } = string.Empty;

            [JsonProperty("parsedAt")]
            public DateTime ParsedAt { get; set; }

            [JsonProperty("updatedAt")]
            public DateTime UpdatedAt { get; set; }

            [JsonProperty("isValid")]
            public bool IsValid { get; set; }

            [JsonProperty("validationMessages")]
            public List<string> ValidationMessages { get; set; } = new List<string>();
        }

        private static void TryDeleteFile(string physicalPath)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(physicalPath) && File.Exists(physicalPath))
                    File.Delete(physicalPath);
            }
            catch
            {
            }
        }

        private static void AppendSection(StringBuilder builder, string heading, string content)
        {
            if (builder == null || string.IsNullOrWhiteSpace(content))
                return;

            builder.AppendLine(heading + ":");
            builder.AppendLine(content.Trim());
            builder.AppendLine();
        }

        private static void AppendEducationEntries(StringBuilder builder, IEnumerable<ResumeEducationEntry> entries)
        {
            if (builder == null || entries == null)
                return;

            foreach (ResumeEducationEntry entry in entries.Where(item => item != null))
            {
                AppendSection(builder, "Education - " + (string.IsNullOrWhiteSpace(entry.SchoolName) ? "Entry" : entry.SchoolName),
                    string.Join(Environment.NewLine, new[]
                    {
                        entry.Location,
                        entry.Degree,
                        FormatMonthYear(entry.StartMonth, entry.StartYear) + " - " + FormatMonthYear(entry.EndMonth, entry.EndYear),
                        entry.Grade,
                        entry.Coursework
                    }.Where(value => !string.IsNullOrWhiteSpace(value))));
            }
        }

        private static void AppendExperienceEntries(StringBuilder builder, IEnumerable<ResumeExperienceEntry> entries)
        {
            if (builder == null || entries == null)
                return;

            foreach (ResumeExperienceEntry entry in entries.Where(item => item != null))
            {
                var details = new StringBuilder();
                AppendSection(details, "Company", entry.Company);
                AppendSection(details, "Location", entry.Location);
                AppendSection(details, "Period", FormatMonthYear(entry.StartMonth, entry.StartYear) + " - " + (entry.IsCurrent ? "Present" : FormatMonthYear(entry.EndMonth, entry.EndYear)));
                if (entry.Bullets != null && entry.Bullets.Count > 0)
                    AppendSection(details, "Description", JoinLines(entry.Bullets));

                AppendSection(builder, "Experience - " + (string.IsNullOrWhiteSpace(entry.JobTitle) ? "Entry" : entry.JobTitle), details.ToString().Trim());
            }
        }

        private static void AppendProjectEntries(StringBuilder builder, IEnumerable<ResumeProjectEntry> entries)
        {
            if (builder == null || entries == null)
                return;

            foreach (ResumeProjectEntry entry in entries.Where(item => item != null))
            {
                AppendSection(builder, "Project - " + (string.IsNullOrWhiteSpace(entry.ProjectTitle) ? "Entry" : entry.ProjectTitle),
                    string.Join(Environment.NewLine, new[]
                    {
                        JoinCommaSeparated(entry.TechStack),
                        entry.Description
                    }.Where(value => !string.IsNullOrWhiteSpace(value))));
            }
        }

        private static void AppendSkillGroups(StringBuilder builder, ResumeSkillGroups skillGroups)
        {
            if (builder == null || skillGroups == null)
                return;

            AppendSection(builder, "Programming Languages", JoinCommaSeparated(skillGroups.ProgrammingLanguages));
            AppendSection(builder, "Frameworks/Libraries", JoinCommaSeparated(skillGroups.FrameworksLibraries));
            AppendSection(builder, "Tools/Cloud/Database Skills", JoinCommaSeparated(skillGroups.ToolsCloudDatabaseSkills));
            AppendSection(builder, "Soft Skills/Languages", JoinCommaSeparated(skillGroups.SoftSkillsLanguages));
            AppendSection(builder, string.IsNullOrWhiteSpace(skillGroups.CustomHeading) ? "Custom Skills" : skillGroups.CustomHeading, JoinCommaSeparated(skillGroups.CustomItems));
        }
    }
}