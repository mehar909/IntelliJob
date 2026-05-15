using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;

namespace IntelliJob
{
    public static class ResumeProfileService
    {
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
                return JsonConvert.DeserializeObject<ResumeProfileDocument>(json);
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

            return JsonConvert.SerializeObject(document, Formatting.Indented);
        }

        public static void AddResumeProfileParameters(SqlCommand command, ResumeProfileDocument document, string resumePath)
        {
            AddParameter(command, "@Resume", resumePath);
            AddParameter(command, "@ResumeOriginalFileName", document != null ? document.OriginalFileName : string.Empty);
            AddParameter(command, "@ResumeParseStatus", document != null && document.IsValid ? "ready" : "none");
            AddParameter(command, "@ResumeValidationMessage", document != null ? document.ValidationMessage : string.Empty);
            AddParameter(command, "@ResumeUploadedAt", document != null ? (object)document.ParsedAt : DBNull.Value);
            AddParameter(command, "@ResumeParsedAt", document != null ? (object)document.ParsedAt : DBNull.Value);
            AddParameter(command, "@ResumeStructuredJson", SerializeDocument(document));
            AddParameter(command, "@ResumeRawText", document != null ? document.RawText : string.Empty);
            AddParameter(command, "@ResumeHeadline", document != null ? document.Headline : string.Empty);
            AddParameter(command, "@ResumeSummary", document != null ? document.Summary : string.Empty);
            AddParameter(command, "@ResumeSkills", JoinLines(document != null ? document.Skills : null));
            AddParameter(command, "@ResumeEducation", JoinLines(document != null ? document.Education : null));
            AddParameter(command, "@ResumeExperienceDetails", JoinLines(document != null ? document.Experience : null));
            AddParameter(command, "@ResumeProjects", JoinLines(document != null ? document.Projects : null));
            AddParameter(command, "@ResumeCertifications", JoinLines(document != null ? document.Certifications : null));
            AddParameter(command, "@ResumeLanguages", JoinLines(document != null ? document.Languages : null));
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

            string extractedText = ResumeTextExtractor.ExtractText(physicalPath);
            ResumeProfileDocument document = ParseText(extractedText, originalFileName);
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
            document.Summary = BuildSectionText(GetSectionLines(sections, "summary"));
            document.Skills = ParseSectionList(GetSectionLines(sections, "skills"), splitTokens: true);
            document.Education = ParseSectionList(GetSectionLines(sections, "education"), splitTokens: false);
            document.Experience = ParseSectionList(GetSectionLines(sections, "experience"), splitTokens: false);
            document.Projects = ParseSectionList(GetSectionLines(sections, "projects"), splitTokens: false);
            document.Certifications = ParseSectionList(GetSectionLines(sections, "certifications"), splitTokens: false);
            document.Languages = ParseSectionList(GetSectionLines(sections, "languages"), splitTokens: false);

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

            return document;
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
    }
}