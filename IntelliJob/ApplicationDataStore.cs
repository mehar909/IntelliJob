using System;
using System.IO;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace IntelliJob
{
    public static class ApplicationDataStore
    {
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

            string folder = GetApplicationFolder(userId, appliedJobId);
            Directory.CreateDirectory(folder);

            string extension = Path.GetExtension(resolvedSource);
            if (string.IsNullOrWhiteSpace(extension))
                extension = ".pdf";

            string snapshotPath = Path.Combine(folder, "resume-snapshot" + extension);
            File.Copy(resolvedSource, snapshotPath, true);

            var selection = new ApplicationResumeSelection
            {
                UserId = userId,
                AppliedJobId = appliedJobId,
                StoredResumePath = snapshotPath,
                ResumeSource = resumeSource ?? string.Empty,
                OriginalFileName = string.IsNullOrWhiteSpace(originalFileName) ? Path.GetFileName(resolvedSource) : originalFileName,
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
                SavedAt = DateTime.UtcNow
            };

            File.WriteAllText(GetResumeSelectionFilePath(userId, appliedJobId), JsonConvert.SerializeObject(selection, Formatting.Indented));
            return selection;
        }

        public static ApplicationResumeSelection SaveApplicationResumeSelection(int userId, int appliedJobId, ResumeProfileDocument document, string resumeSource, string originalFileName)
        {
            if (document == null)
                return null;

            string folder = GetApplicationFolder(userId, appliedJobId);
            Directory.CreateDirectory(folder);

            string snapshotPath = Path.Combine(folder, "resume-snapshot.txt");
            File.WriteAllText(snapshotPath, BuildStructuredResumeText(document));

            var selection = new ApplicationResumeSelection
            {
                UserId = userId,
                AppliedJobId = appliedJobId,
                StoredResumePath = snapshotPath,
                ResumeSource = resumeSource ?? string.Empty,
                OriginalFileName = string.IsNullOrWhiteSpace(originalFileName) ? "enhanced-resume.txt" : originalFileName,
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

            string snapshotPath = Path.Combine(folder, "resume-draft.txt");
            File.WriteAllText(snapshotPath, BuildStructuredResumeText(importResult.Document));
            TryDeleteFile(importResult.StoredPhysicalPath);

            return SaveDraftRecord(userId, jobId, snapshotPath, resumeSource, originalFileName, importResult.Document, false);
        }

        public static ApplicationResumeDraftRecord SaveApplicationResumeDraft(int userId, int jobId, ResumeProfileDocument document, string resumeSource, string originalFileName)
        {
            if (document == null)
                return null;

            string folder = GetApplicationDraftFolder(userId, jobId);
            Directory.CreateDirectory(folder);

            string snapshotPath = Path.Combine(folder, "resume-draft.txt");
            File.WriteAllText(snapshotPath, BuildStructuredResumeText(document));

            return SaveDraftRecord(userId, jobId, snapshotPath, resumeSource, originalFileName, document, true);
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

        public static ApplicationResumeSelection FinalizeApplicationResumeDraft(int userId, int jobId, int appliedJobId)
        {
            ApplicationResumeDraftRecord draft;
            if (!TryGetApplicationResumeDraft(userId, jobId, out draft) || draft == null)
                return null;

            if (string.IsNullOrWhiteSpace(draft.StoredResumePath) || !File.Exists(draft.StoredResumePath))
                return null;

            return SaveApplicationResumeSelection(userId, appliedJobId, draft.StoredResumePath, draft.ResumeSource, draft.OriginalFileName);
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

        private static string BuildStructuredResumeText(ResumeProfileDocument document)
        {
            if (document == null)
                return string.Empty;

            StringBuilder builder = new StringBuilder();

            AppendSection(builder, "Full Name", document.FullName);
            AppendSection(builder, "Email", document.Email);
            AppendSection(builder, "Mobile", document.Mobile);
            AppendSection(builder, "Address", document.Address);
            AppendSection(builder, "Headline", document.Headline);
            AppendSection(builder, "Summary", document.Summary);
            AppendSection(builder, "Skills", JoinLines(document.Skills));
            AppendSection(builder, "Education", JoinLines(document.Education));
            AppendSection(builder, "Experience", JoinLines(document.Experience));
            AppendSection(builder, "Projects", JoinLines(document.Projects));
            AppendSection(builder, "Certifications", JoinLines(document.Certifications));
            AppendSection(builder, "Languages", JoinLines(document.Languages));

            return builder.ToString().Trim();
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
    }
}
