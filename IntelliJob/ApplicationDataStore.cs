using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace IntelliJob
{
    public static class ApplicationDataStore
    {
        private static string GetApplicationDraftLockFilePath(int userId, int jobId)
        {
            return Path.Combine(GetApplicationDraftFolder(userId, jobId), "profile-only.lock");
        }

        private static string GetAppDataRoot()
        {
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Server.MapPath("~/App_Data");
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        }

        private static string GetApplicationFolder(int userId, int appliedJobId)
        {
            return Path.Combine(GetAppDataRoot(), "ResumeArtifacts", userId.ToString(), appliedJobId.ToString());
        }

        private static string GetApplicationDraftFolder(int userId, int jobId)
        {
            return Path.Combine(GetAppDataRoot(), "ResumeDrafts", userId.ToString(), jobId.ToString());
        }

        private static string GetApplicationDraftFilePath(int userId, int jobId)
        {
            return Path.Combine(GetApplicationDraftFolder(userId, jobId), "resume-draft.json");
        }

        private static string GetResumeSelectionFilePath(int userId, int appliedJobId)
        {
            return Path.Combine(GetApplicationFolder(userId, appliedJobId), "resume-selection.json");
        }

        private static string GetReportFilePath(int userId, int appliedJobId)
        {
            return Path.Combine(GetApplicationFolder(userId, appliedJobId), "resume-report.json");
        }

        private static string ResolvePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            string trimmed = path.Trim();
            if (Path.IsPathRooted(trimmed))
                return trimmed;

            string virtualPath = trimmed.Replace('\\', '/').TrimStart('/');
            if (virtualPath.StartsWith("~/", StringComparison.Ordinal))
                virtualPath = virtualPath.Substring(2);

            if (HttpContext.Current != null)
                return HttpContext.Current.Server.MapPath("~/" + virtualPath);

            return Path.GetFullPath(virtualPath);
        }

        public static ApplicationResumeSelection SaveApplicationResumeSelection(int userId, int appliedJobId, string sourceResumePath, string resumeSource, string originalFileName)
        {
            string resolvedSource = ResolvePath(sourceResumePath);
            if (string.IsNullOrWhiteSpace(resolvedSource) || !File.Exists(resolvedSource))
                return null;

            string structuredJson = string.Empty;
            ResumeProfileDocument imported = null;
            if (string.Equals(Path.GetExtension(resolvedSource), ".json", StringComparison.OrdinalIgnoreCase))
            {
                imported = ResumeProfileService.DeserializeDocument(File.ReadAllText(resolvedSource));
            }

            if (imported != null)
                structuredJson = ResumeProfileService.SerializeDocument(imported);

            var selection = new ApplicationResumeSelection
            {
                UserId = userId,
                AppliedJobId = appliedJobId,
                StoredResumePath = resolvedSource,
                ResumeSource = resumeSource ?? string.Empty,
                OriginalFileName = string.IsNullOrWhiteSpace(originalFileName) ? Path.GetFileName(resolvedSource) : originalFileName,
                StructuredJson = structuredJson,
                SavedAt = DateTime.UtcNow
            };

            File.WriteAllText(GetResumeSelectionFilePath(userId, appliedJobId), JsonConvert.SerializeObject(selection, Formatting.Indented));
            return selection;
        }

        public static ApplicationResumeSelection SaveApplicationResumeSelection(int userId, int appliedJobId, HttpPostedFile postedFile, string resumeSource, string originalFileName)
        {
            if (postedFile == null || postedFile.ContentLength <= 0)
                return null;

            string folder = GetApplicationFolder(userId, appliedJobId);
            Directory.CreateDirectory(folder);

            string extension = Path.GetExtension(postedFile.FileName);
            if (string.IsNullOrWhiteSpace(extension))
                extension = ".pdf";

            string snapshotPath = Path.Combine(folder, "resume-snapshot" + extension);
            postedFile.SaveAs(snapshotPath);

            var selection = new ApplicationResumeSelection
            {
                UserId = userId,
                AppliedJobId = appliedJobId,
                StoredResumePath = snapshotPath,
                ResumeSource = resumeSource ?? string.Empty,
                OriginalFileName = string.IsNullOrWhiteSpace(originalFileName) ? Path.GetFileName(postedFile.FileName) : originalFileName,
                StructuredJson = string.Empty,
                SavedAt = DateTime.UtcNow
            };

            File.WriteAllText(GetResumeSelectionFilePath(userId, appliedJobId), JsonConvert.SerializeObject(selection, Formatting.Indented));
            return selection;
        }

        public static ApplicationResumeSelection SaveApplicationResumeSelection(int userId, int appliedJobId, ResumeProfileDocument document, string resumeSource, string originalFileName)
        {
            if (document == null)
                return null;

            string structuredJson = ResumeProfileService.SerializeDocument(document);
            string folder = GetApplicationFolder(userId, appliedJobId);
            Directory.CreateDirectory(folder);
            string snapshotPath = Path.Combine(folder, "resume-snapshot.json");
            File.WriteAllText(snapshotPath, structuredJson);

            var selection = new ApplicationResumeSelection
            {
                UserId = userId,
                AppliedJobId = appliedJobId,
                StoredResumePath = snapshotPath,
                ResumeSource = resumeSource ?? string.Empty,
                OriginalFileName = string.IsNullOrWhiteSpace(originalFileName) ? "enhanced-resume.json" : originalFileName,
                StructuredJson = structuredJson,
                SavedAt = DateTime.UtcNow
            };

            File.WriteAllText(GetResumeSelectionFilePath(userId, appliedJobId), JsonConvert.SerializeObject(selection, Formatting.Indented));
            return selection;
        }

        public static bool TryGetApplicationResumeSelection(int userId, int appliedJobId, out ApplicationResumeSelection selection)
        {
            selection = null;
            string path = GetResumeSelectionFilePath(userId, appliedJobId);
            if (!File.Exists(path))
                return false;

            try
            {
                selection = JsonConvert.DeserializeObject<ApplicationResumeSelection>(File.ReadAllText(path));
                return selection != null;
            }
            catch
            {
                selection = null;
                return false;
            }
        }

        public static ApplicationResumeDraftRecord SaveApplicationResumeDraft(int userId, int jobId, HttpPostedFile postedFile, string resumeSource, string originalFileName)
        {
            if (postedFile == null || postedFile.ContentLength <= 0)
                return null;

            string folder = GetApplicationDraftFolder(userId, jobId);
            Directory.CreateDirectory(folder);

            ResumeImportResult importResult = ResumeProfileService.ImportAndParse(postedFile, folder, "ResumeDrafts");
            if (importResult == null || !importResult.IsSuccess || importResult.Document == null)
                return null;

            return SaveDraftRecord(userId, jobId, importResult.StoredPhysicalPath, resumeSource, originalFileName, importResult.Document, false);
        }

        public static ApplicationResumeDraftRecord SaveApplicationResumeDraft(int userId, int jobId, ResumeProfileDocument document, string resumeSource, string originalFileName)
        {
            if (document == null)
                return null;

            return SaveDraftRecord(userId, jobId, GetApplicationDraftFilePath(userId, jobId), resumeSource, originalFileName, document, true);
        }

        public static bool TryGetApplicationResumeDraft(int userId, int jobId, out ApplicationResumeDraftRecord draft)
        {
            draft = null;
            string path = GetApplicationDraftFilePath(userId, jobId);
            if (!File.Exists(path))
                return false;

            try
            {
                draft = JsonConvert.DeserializeObject<ApplicationResumeDraftRecord>(File.ReadAllText(path));
                return draft != null;
            }
            catch
            {
                draft = null;
                return false;
            }
        }

        public static bool DeleteApplicationResumeDraft(int userId, int jobId, bool deleteStoredFile)
        {
            string folder = GetApplicationDraftFolder(userId, jobId);
            ApplicationResumeDraftRecord draft;

            if (!TryGetApplicationResumeDraft(userId, jobId, out draft))
            {
                if (!Directory.Exists(folder))
                    return false;

                try
                {
                    Directory.Delete(folder, true);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            if (deleteStoredFile && draft != null && !string.IsNullOrWhiteSpace(draft.StoredResumePath))
            {
                try
                {
                    if (File.Exists(draft.StoredResumePath))
                        File.Delete(draft.StoredResumePath);
                }
                catch
                {
                }
            }

            try
            {
                if (Directory.Exists(folder))
                    Directory.Delete(folder, true);
                return true;
            }
            catch
            {
                try
                {
                    string draftFile = GetApplicationDraftFilePath(userId, jobId);
                    if (File.Exists(draftFile))
                        File.Delete(draftFile);
                }
                catch
                {
                }

                return false;
            }
        }

        public static void SetApplicationResumeProfileOnly(int userId, int jobId, bool profileOnly)
        {
            string lockFile = GetApplicationDraftLockFilePath(userId, jobId);
            string folder = GetApplicationDraftFolder(userId, jobId);

            if (profileOnly)
            {
                Directory.CreateDirectory(folder);
                File.WriteAllText(lockFile, DateTime.UtcNow.ToString("O"));
                return;
            }

            try
            {
                if (File.Exists(lockFile))
                    File.Delete(lockFile);
            }
            catch
            {
            }

            try
            {
                if (Directory.Exists(folder) && !Directory.EnumerateFileSystemEntries(folder).Any())
                    Directory.Delete(folder, true);
            }
            catch
            {
            }
        }

        public static bool IsApplicationResumeProfileOnly(int userId, int jobId)
        {
            return File.Exists(GetApplicationDraftLockFilePath(userId, jobId));
        }

        public static void ClearApplicationResumeState(int userId, int jobId)
        {
            DeleteApplicationResumeDraft(userId, jobId, deleteStoredFile: true);
            SetApplicationResumeProfileOnly(userId, jobId, false);
        }

        public static ApplicationResumeSelection FinalizeApplicationResumeDraft(int userId, int jobId, int appliedJobId)
        {
            ApplicationResumeDraftRecord draft;
            if (!TryGetApplicationResumeDraft(userId, jobId, out draft) || draft == null)
                return null;

            return SaveApplicationResumeSelection(userId, appliedJobId, draft.StructuredJson, draft.ResumeSource, draft.OriginalFileName, draft.StoredResumePath);
        }

        public static ApplicationResumeSelection SaveApplicationResumeSelection(int userId, int appliedJobId, string structuredJson, string resumeSource, string originalFileName, string storedResumePath)
        {
            var selection = new ApplicationResumeSelection
            {
                UserId = userId,
                AppliedJobId = appliedJobId,
                StoredResumePath = storedResumePath ?? string.Empty,
                ResumeSource = resumeSource ?? string.Empty,
                OriginalFileName = string.IsNullOrWhiteSpace(originalFileName) ? string.Empty : originalFileName,
                StructuredJson = structuredJson ?? string.Empty,
                SavedAt = DateTime.UtcNow
            };

            string folder = GetApplicationFolder(userId, appliedJobId);
            Directory.CreateDirectory(folder);
            File.WriteAllText(GetResumeSelectionFilePath(userId, appliedJobId), JsonConvert.SerializeObject(selection, Formatting.Indented));
            return selection;
        }

        private static ApplicationResumeDraftRecord SaveDraftRecord(int userId, int jobId, string storedResumePath, string resumeSource, string originalFileName, ResumeProfileDocument document, bool isConfirmed)
        {
            ApplicationResumeDraftRecord draft = new ApplicationResumeDraftRecord
            {
                UserId = userId,
                JobId = jobId,
                StoredResumePath = storedResumePath,
                ResumeSource = resumeSource ?? string.Empty,
                OriginalFileName = string.IsNullOrWhiteSpace(originalFileName) ? string.Empty : Path.GetFileName(originalFileName),
                StructuredJson = ResumeProfileService.SerializeDocument(document),
                RawText = document != null ? document.RawText : string.Empty,
                IsConfirmed = isConfirmed,
                SavedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Directory.CreateDirectory(GetApplicationDraftFolder(userId, jobId));
            File.WriteAllText(GetApplicationDraftFilePath(userId, jobId), JsonConvert.SerializeObject(draft, Formatting.Indented));
            return draft;
        }

        private static void AppendEducationEntries(StringBuilder builder, System.Collections.Generic.IEnumerable<ResumeEducationEntry> entries)
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

        private static void AppendExperienceEntries(StringBuilder builder, System.Collections.Generic.IEnumerable<ResumeExperienceEntry> entries)
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

        private static void AppendProjectEntries(StringBuilder builder, System.Collections.Generic.IEnumerable<ResumeProjectEntry> entries)
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

        private static void AppendSection(StringBuilder builder, string heading, string content)
        {
            if (builder == null || string.IsNullOrWhiteSpace(content))
                return;

            builder.AppendLine(heading + ":");
            builder.AppendLine(content.Trim());
            builder.AppendLine();
        }

        private static string JoinLines(System.Collections.Generic.IEnumerable<string> lines)
        {
            if (lines == null)
                return string.Empty;

            StringBuilder builder = new StringBuilder();
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (builder.Length > 0)
                    builder.AppendLine();

                builder.AppendLine(line.Trim());
            }

            return builder.ToString().Trim();
        }

        private static string JoinCommaSeparated(System.Collections.Generic.IEnumerable<string> values)
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

        public static void SaveResumeEnhancementReport(ResumeEnhancementReportRecord report)
        {
            if (report == null)
                return;

            string folder = GetApplicationFolder(report.UserId, report.AppliedJobId);
            Directory.CreateDirectory(folder);
            File.WriteAllText(GetReportFilePath(report.UserId, report.AppliedJobId), JsonConvert.SerializeObject(report, Formatting.Indented));

            string enhancedPath = Path.Combine(folder, "enhanced-resume.json");
            if (!string.IsNullOrWhiteSpace(report.UpdatedResumeStructuredJson))
                File.WriteAllText(enhancedPath, report.UpdatedResumeStructuredJson);
            else
                TryDeleteFile(enhancedPath);
        }

        public static bool TryGetResumeEnhancementReport(int userId, int appliedJobId, out ResumeEnhancementReportRecord report)
        {
            report = null;
            string path = GetReportFilePath(userId, appliedJobId);
            if (!File.Exists(path))
                return false;

            try
            {
                report = JsonConvert.DeserializeObject<ResumeEnhancementReportRecord>(File.ReadAllText(path));
                return report != null;
            }
            catch
            {
                report = null;
                return false;
            }
        }

        public static void DeleteResumeEnhancementReport(int userId, int appliedJobId)
        {
            string path = GetReportFilePath(userId, appliedJobId);
            try { if (File.Exists(path)) File.Delete(path); } catch { }
        }
    }
}
