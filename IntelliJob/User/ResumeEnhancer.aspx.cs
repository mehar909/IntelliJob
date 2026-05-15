using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IntelliJob.User
{
    public partial class ResumeEnhancer : Page
    {
        private const string PreviewEditModeViewStateKey = "ResumeEnhancerPreviewEditMode";
        private const string CurrentDocumentViewStateKey = "ResumeEnhancerCurrentDocumentJson";
        private const string ReportLoadedViewStateKey = "ResumeEnhancerReportLoaded";
        private readonly string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        private bool PreviewEditMode
        {
            get { return ViewState[PreviewEditModeViewStateKey] != null && Convert.ToBoolean(ViewState[PreviewEditModeViewStateKey]); }
            set { ViewState[PreviewEditModeViewStateKey] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["user"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            BindEnhancerMonthDropdowns();
            if (!IsPostBack)
            {
                LoadEnhancement();
            }
            else
            {
                // After ViewState has been applied, overwrite shifted card values
                // produced by a delete action detected in Page_Init.
                ApplyPendingCardData();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (ReportLoaded)
                RevealReportBody();

            ApplyPreviewEditorState();
        }

        private bool ReportLoaded
        {
            get { return ViewState[ReportLoadedViewStateKey] != null && Convert.ToBoolean(ViewState[ReportLoadedViewStateKey]); }
            set { ViewState[ReportLoadedViewStateKey] = value; }
        }

        private void LoadEnhancement()
        {
            int userId = Convert.ToInt32(Session["userId"]);
            bool historyMode = !string.IsNullOrWhiteSpace(Request.QueryString["applicationId"]);

            if (historyMode)
            {
                if (!int.TryParse(Request.QueryString["applicationId"], out int appliedJobId))
                {
                    ShowStatus("Please open this page from a saved resume history entry.", true);
                    return;
                }

                LoadSavedReport(userId, appliedJobId);
                return;
            }

            if (!int.TryParse(Request.QueryString["id"], out int interviewId))
            {
                ShowStatus("Please open this page from a completed interview.", true);
                return;
            }

            LoadFromInterview(userId, interviewId);
        }

        private void LoadSavedReport(int userId, int appliedJobId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(str))
            {
                string query = @"SELECT aj.AppliedJobId, aj.JobId, aj.Shortlisted,
                                        j.Title, j.CompanyName, j.Description, j.Specialization, j.Qualification, j.JobType, j.Experience,
                                        i.InterviewId, i.Role, i.Level, i.InterviewType, i.TechStack,
                                        fb.TotalScore, fb.FinalAssessment, fb.Strengths, fb.AreasForImprovement,
                                        js.Resume, js.Name, js.Mobile,
                                        u.Email, u.Address
                                 FROM AppliedJobs aj
                                 INNER JOIN Jobs j ON aj.JobId = j.JobId
                                 LEFT JOIN Interviews i ON i.AppliedJobId = aj.AppliedJobId AND i.UserId = aj.UserId
                                 LEFT JOIN InterviewFeedback fb ON fb.InterviewId = i.InterviewId
                                 LEFT JOIN JobSeekers js ON js.ProfileId = aj.UserId
                                 LEFT JOIN Users u ON u.UserId = aj.UserId
                                 WHERE aj.AppliedJobId = @AppliedJobId AND aj.UserId = @UserId";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@AppliedJobId", appliedJobId);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(dt);
                    }
                }
            }

            if (dt.Rows.Count == 0)
            {
                ShowStatus("No saved resume history was found for this application.", true);
                return;
            }

            ResumeEnhancementReportRecord savedReport;
            if (!ApplicationDataStore.TryGetResumeEnhancementReport(userId, appliedJobId, out savedReport) || savedReport == null)
            {
                ShowStatus("There is no resume history or summary that is suitable yet.", true);
                return;
            }

            if (savedReport.Result == null)
                savedReport.Result = new ResumeEnhancementResult();

            if (string.IsNullOrWhiteSpace(savedReport.UpdatedResumeText))
                savedReport.UpdatedResumeText = !string.IsNullOrWhiteSpace(savedReport.Result.UpdatedResumeText)
                    ? savedReport.Result.UpdatedResumeText
                    : savedReport.OriginalResumeText;

            if (string.IsNullOrWhiteSpace(savedReport.ResumePath))
                savedReport.ResumePath = ResolveApplicationResumePath(userId, appliedJobId, dt.Rows[0]["Resume"] == DBNull.Value ? string.Empty : dt.Rows[0]["Resume"].ToString(), out _, out _);

            DataRow row = dt.Rows[0];
            RenderReport(row, savedReport);
            BindEditablePreview(row, savedReport);
            StorePreviewJson(savedReport);
            ReportLoaded = true;
            ShowReportLoadedStatus(savedReport.AppliedJobId, "Saved resume history loaded from your application record.");


            RevealReportBody();
        }

        private void LoadFromInterview(int userId, int interviewId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(str))
            {
                string query = @"SELECT i.InterviewId, i.AppliedJobId, i.Role, i.Level, i.InterviewType, i.TechStack, i.JobId,
                                        j.Title, j.CompanyName, j.Description, j.Specialization, j.Qualification, j.JobType, j.Experience,
                                        fb.TotalScore, fb.FinalAssessment, fb.Strengths, fb.AreasForImprovement,
                                        js.Resume, js.Name, js.Mobile,
                                        u.Email, u.Address
                                 FROM Interviews i
                                 INNER JOIN Jobs j ON i.JobId = j.JobId
                                 LEFT JOIN InterviewFeedback fb ON fb.InterviewId = i.InterviewId
                                 LEFT JOIN JobSeekers js ON js.ProfileId = i.UserId
                                 LEFT JOIN Users u ON u.UserId = i.UserId
                                 WHERE i.InterviewId = @InterviewId AND i.UserId = @UserId";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@InterviewId", interviewId);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(dt);
                    }
                }
            }

            if (dt.Rows.Count == 0)
            {
                ShowStatus("No job-linked interview was found for this account.", true);
                return;
            }

            if (dt.Rows.Count == 0)
            {
                ShowStatus("No job-linked interview was found for this account.", true);
                return;
            }

            DataRow row = dt.Rows[0];
            if (row["AppliedJobId"] == DBNull.Value || Convert.ToInt32(row["AppliedJobId"]) <= 0)
            {
                ShowStatus("This resume enhancer is available only for job-linked applications.", true);
                return;
            }

            int appliedJobId = Convert.ToInt32(row["AppliedJobId"]);
            int jobId = Convert.ToInt32(row["JobId"]);
            string jobTitle = row["Title"].ToString();
            string companyName = row["CompanyName"].ToString();
            string jobDescription = row["Description"].ToString();
            bool hasInterviewFeedback = row["TotalScore"] != DBNull.Value || row["FinalAssessment"] != DBNull.Value || row["Strengths"] != DBNull.Value || row["AreasForImprovement"] != DBNull.Value;

            if (!hasInterviewFeedback)
            {
                ShowStatus("Resume enhancer is available only after an interview with feedback has been completed.", true);
                return;
            }

            string interviewFeedback = BuildInterviewFeedbackText(row);
            string keywordHints = BuildKeywordHints(row);
            string profileResumePath = row["Resume"] == DBNull.Value ? string.Empty : row["Resume"].ToString();

            string resumeSourceLabel;
            string originalFileName;
            string resumePath = ResolveApplicationResumePath(userId, appliedJobId, profileResumePath, out resumeSourceLabel, out originalFileName);

            if (string.IsNullOrWhiteSpace(resumePath))
            {
                ShowStatus("No resume was available for this application. Upload one from your profile or attach one while applying for a job.", true);
                return;
            }

            ResumeEnhancementReportRecord savedReport;
            if (ApplicationDataStore.TryGetResumeEnhancementReport(userId, appliedJobId, out savedReport))
            {
                if (savedReport.Result == null)
                    savedReport.Result = new ResumeEnhancementResult();

                if (string.IsNullOrWhiteSpace(savedReport.UpdatedResumeText))
                    savedReport.UpdatedResumeText = !string.IsNullOrWhiteSpace(savedReport.Result.UpdatedResumeText)
                        ? savedReport.Result.UpdatedResumeText
                        : savedReport.OriginalResumeText;

                if (string.IsNullOrWhiteSpace(savedReport.ResumePath))
                    savedReport.ResumePath = resumePath;

                RenderReport(row, savedReport);
                BindEditablePreview(row, savedReport);
                StorePreviewJson(savedReport);
                ReportLoaded = true;
                ShowReportLoadedStatus(savedReport.AppliedJobId, "Saved resume report loaded from your application history.");
                RevealReportBody();
                return;
            }

            ResumeProfileDocument resumeDocument = LoadResumeProfileDocumentForEnhancement(userId, appliedJobId, resumePath, row);
            string resumeText = ResumeProfileService.BuildResumeText(resumeDocument);
            if (string.IsNullOrWhiteSpace(resumeText) && resumeDocument != null && !string.IsNullOrWhiteSpace(resumeDocument.RawText))
                resumeText = resumeDocument.RawText.Trim();
            if (string.IsNullOrWhiteSpace(resumeText))
            {
                resumeText = "No readable resume text could be extracted from the stored resume.";
                ShowStatus("The resume was found, but its text could not be fully extracted. The enhancer still reviewed the available job and interview context.", false);
            }

            ResumeEnhancementResult result;
            try
            {
                GeminiService gemini = new GeminiService();
                result = Task.Run(() => gemini.GenerateResumeEnhancementAsync(
                    resumeText,
                    jobTitle,
                    jobDescription,
                    interviewFeedback,
                    keywordHints)).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                result = new ResumeEnhancementResult
                {
                    OverallScore = 0,
                    AtsScore = 0,
                    SemanticScore = 0,
                    KeywordScore = 0,
                    ResumeSummary = "Resume enhancement could not be generated right now.",
                    UpdatedResumeText = resumeText,
                    Strengths = new List<string> { "Retry after the AI service becomes available." },
                    Gaps = new List<string> { "The enhancer could not complete this analysis." },
                    PriorityKeywords = new List<string>(),
                    RewriteSuggestions = new List<ResumeRewriteSuggestion>(),
                    FinalAssessment = "ERROR: " + ex.Message
                };
                ShowStatus("The AI review could not be completed right now, but the page is still available.", true);
            }

            if (string.IsNullOrWhiteSpace(result.UpdatedResumeText))
                result.UpdatedResumeText = resumeText;

            var report = new ResumeEnhancementReportRecord
            {
                UserId = userId,
                AppliedJobId = appliedJobId,
                InterviewId = interviewId,
                JobId = jobId,
                JobTitle = jobTitle,
                CompanyName = companyName,
                JobDescription = jobDescription,
                ResumePath = resumePath,
                ResumeSource = string.IsNullOrWhiteSpace(resumeSourceLabel) ? "profile" : resumeSourceLabel,
                OriginalResumeText = resumeText,
                UpdatedResumeText = result.UpdatedResumeText,
                InterviewFeedback = interviewFeedback,
                KeywordHints = keywordHints,
                GeneratedAt = DateTime.UtcNow,
                Result = result
            };

            ApplicationDataStore.SaveResumeEnhancementReport(report);

            // Immediately persist the Gemini-structured JSON so the preview can display it
            if (result.EnhancedResumeDocument != null)
            {
                report.UpdatedResumeStructuredJson = ResumeProfileService.SerializeDocument(result.EnhancedResumeDocument);
                ApplicationDataStore.SaveResumeEnhancementReport(report);
            }

            RenderReport(row, report);
            BindEditablePreview(row, report);
            StorePreviewJson(report);
            ReportLoaded = true;
            ShowReportLoadedStatus(report.AppliedJobId, "This resume report has been saved to your application history.");


            RevealReportBody();
        }

        private string ResolveApplicationResumePath(int userId, int appliedJobId, string profileResumePath, out string resumeSourceLabel, out string originalFileName)
        {
            resumeSourceLabel = "profile";
            originalFileName = string.Empty;

            ApplicationResumeSelection selection;
            if (ApplicationDataStore.TryGetApplicationResumeSelection(userId, appliedJobId, out selection) &&
                selection != null &&
                !string.IsNullOrWhiteSpace(selection.StoredResumePath) &&
                File.Exists(selection.StoredResumePath))
            {
                resumeSourceLabel = string.IsNullOrWhiteSpace(selection.ResumeSource) ? "application" : selection.ResumeSource;
                originalFileName = selection.OriginalFileName;
                return selection.StoredResumePath;
            }

            if (string.IsNullOrWhiteSpace(profileResumePath))
                return string.Empty;

            ApplicationResumeSelection saved = ApplicationDataStore.SaveApplicationResumeSelection(userId, appliedJobId, profileResumePath, "profile", Path.GetFileName(profileResumePath));
            if (saved != null && File.Exists(saved.StoredResumePath))
            {
                resumeSourceLabel = "profile";
                originalFileName = saved.OriginalFileName;
                return saved.StoredResumePath;
            }

            originalFileName = Path.GetFileName(profileResumePath);
            return profileResumePath;
        }

        /// <summary>
        /// Loads canonical resume JSON for Gemini and previews: prefers application resume-selection StructuredJson,
        /// unwraps draft wrapper files (StoredResumePath pointing at resume-draft.json), and fills identity from DB when missing.
        /// </summary>
        private ResumeProfileDocument LoadResumeProfileDocumentForEnhancement(int userId, int appliedJobId, string resolvedResumeFilePath, DataRow profileRow)
        {
            ApplicationResumeSelection selection;
            if (ApplicationDataStore.TryGetApplicationResumeSelection(userId, appliedJobId, out selection) &&
                selection != null &&
                !string.IsNullOrWhiteSpace(selection.StructuredJson))
            {
                ResumeProfileDocument doc = ResumeProfileService.DeserializeDocument(selection.StructuredJson) ?? new ResumeProfileDocument();
                MergeProfileRowIntoDocument(doc, profileRow);
                TryAppendRawTextFromDraftFile(doc, resolvedResumeFilePath);
                return doc;
            }

            if (!string.IsNullOrWhiteSpace(resolvedResumeFilePath) && File.Exists(resolvedResumeFilePath))
            {
                string content = File.ReadAllText(resolvedResumeFilePath);
                ResumeProfileDocument doc = DeserializeResumeFromStoredFileContent(content) ?? new ResumeProfileDocument();
                MergeProfileRowIntoDocument(doc, profileRow);
                TryMergeDraftRawTextFromWrapperJson(doc, content);
                return doc;
            }

            ResumeProfileDocument empty = new ResumeProfileDocument();
            MergeProfileRowIntoDocument(empty, profileRow);
            return empty;
        }

        private static ResumeProfileDocument DeserializeResumeFromStoredFileContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            try
            {
                JObject jo = JObject.Parse(content);
                foreach (string key in new[] { "StructuredJson", "structuredJson" })
                {
                    JToken sj = jo[key];
                    if (sj != null && sj.Type == JTokenType.String)
                    {
                        string inner = sj.Value<string>();
                        if (!string.IsNullOrWhiteSpace(inner))
                        {
                            ResumeProfileDocument innerDoc = ResumeProfileService.DeserializeDocument(inner);
                            if (innerDoc != null)
                                return innerDoc;
                        }
                    }
                }
            }
            catch
            {
                // Fall through to full-file deserialize
            }

            return ResumeProfileService.DeserializeDocument(content);
        }

        private static void TryAppendRawTextFromDraftFile(ResumeProfileDocument doc, string filePath)
        {
            if (doc == null || string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return;

            try
            {
                string content = File.ReadAllText(filePath);
                TryMergeDraftRawTextFromWrapperJson(doc, content);
            }
            catch
            {
                // ignore
            }
        }

        private static void TryMergeDraftRawTextFromWrapperJson(ResumeProfileDocument doc, string wrapperJson)
        {
            if (doc == null || string.IsNullOrWhiteSpace(wrapperJson))
                return;

            try
            {
                JObject jo = JObject.Parse(wrapperJson);
                JToken raw = jo["RawText"] ?? jo["rawText"];
                if (raw != null && raw.Type == JTokenType.String)
                {
                    string rt = raw.Value<string>();
                    if (!string.IsNullOrWhiteSpace(rt) && string.IsNullOrWhiteSpace(doc.RawText))
                        doc.RawText = rt.Trim();
                }
            }
            catch
            {
                // ignore
            }
        }

        private static void MergeProfileRowIntoDocument(ResumeProfileDocument doc, DataRow profileRow)
        {
            if (doc == null || profileRow == null || profileRow.Table == null)
                return;

            string name = ResumeEnhancerColumn(profileRow, "Name");
            string email = ResumeEnhancerColumn(profileRow, "Email");
            string mobile = ResumeEnhancerColumn(profileRow, "Mobile");
            string address = ResumeEnhancerColumn(profileRow, "Address");

            if (string.IsNullOrWhiteSpace(doc.FullName) && !string.IsNullOrWhiteSpace(name))
                doc.FullName = name;

            if (doc.PersonalInfo == null)
                doc.PersonalInfo = new ResumePersonalInfo();

            if (string.IsNullOrWhiteSpace(doc.PersonalInfo.FullName) && !string.IsNullOrWhiteSpace(name))
                doc.PersonalInfo.FullName = name;

            if (string.IsNullOrWhiteSpace(doc.Email) && !string.IsNullOrWhiteSpace(email))
                doc.Email = email;

            if (string.IsNullOrWhiteSpace(doc.PersonalInfo.Email) && !string.IsNullOrWhiteSpace(email))
                doc.PersonalInfo.Email = email;

            if (string.IsNullOrWhiteSpace(doc.Mobile) && !string.IsNullOrWhiteSpace(mobile))
                doc.Mobile = mobile;

            if (string.IsNullOrWhiteSpace(doc.PersonalInfo.Mobile) && !string.IsNullOrWhiteSpace(mobile))
                doc.PersonalInfo.Mobile = mobile;

            if (string.IsNullOrWhiteSpace(doc.Address) && !string.IsNullOrWhiteSpace(address))
                doc.Address = address;

            if (string.IsNullOrWhiteSpace(doc.PersonalInfo.Address) && !string.IsNullOrWhiteSpace(address))
                doc.PersonalInfo.Address = address;
        }

        private static string ResumeEnhancerColumn(DataRow row, string columnName)
        {
            if (row.Table == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return string.Empty;

            return row[columnName].ToString().Trim();
        }

        private void RenderReport(DataRow row, ResumeEnhancementReportRecord report)
        {
            ResumeEnhancementResult result = report.Result ?? new ResumeEnhancementResult();

            litRole.Text = Server.HtmlEncode(string.IsNullOrWhiteSpace(report.JobTitle) ? row["Role"].ToString() : report.JobTitle);
            litCompany.Text = Server.HtmlEncode(string.IsNullOrWhiteSpace(report.CompanyName) ? row["CompanyName"].ToString() : report.CompanyName);
            litLevel.Text = Server.HtmlEncode(row["Level"].ToString());
            litInterviewType.Text = Server.HtmlEncode(row["InterviewType"].ToString());
            litResumeSource.Text = Server.HtmlEncode(string.IsNullOrWhiteSpace(report.ResumeSource) ? "Profile Resume" : (report.ResumeSource.IndexOf("profile", StringComparison.OrdinalIgnoreCase) >= 0 ? "Profile Resume" : "Job Resume"));

            //litOverallScore.Text = result.OverallScore.ToString();
            //litAtsScore.Text = result.AtsScore.ToString();
            //litSemanticScore.Text = result.SemanticScore.ToString();
            //litKeywordScore.Text = result.KeywordScore.ToString();

            litOverallVisual.Text = GenerateScoreCircleHtml(result.OverallScore);
            litAtsVisual.Text = GenerateScoreCircleHtml(result.AtsScore);
            litSemanticVisual.Text = GenerateScoreCircleHtml(result.SemanticScore);
            litKeywordVisual.Text = GenerateScoreCircleHtml(result.KeywordScore);

            litResumeSummary.Text = Server.HtmlEncode(result.ResumeSummary ?? string.Empty);
            litStrengths.Text = BuildListHtml(result.Strengths, "No strengths were returned yet.", true);
            litGaps.Text = BuildListHtml(result.Gaps, "No gaps were returned yet.", true);
            litPriorityKeywords.Text = BuildListHtml(result.PriorityKeywords, "No priority keywords were returned yet.", true);
            int interviewScore = 0;
            if (row.Table.Columns.Contains("TotalScore") && row["TotalScore"] != DBNull.Value)
            {
                int.TryParse(row["TotalScore"].ToString(), out interviewScore);
            }

            int resumeScore = result.OverallScore;

            if (interviewScore > 0)
            {
                if (interviewScore >= 70 && resumeScore >= 60)
                {
                    litFinalAssessment.Text = "We will contact you shortly for the next phase.";
                }
                else if (interviewScore >= 70 && resumeScore < 60)
                {
                    litFinalAssessment.Text = "We will contact you shortly for the next phase. Please review the suggestions on your resume below.";
                }
                else if (interviewScore < 70 && resumeScore >= 60)
                {
                    litFinalAssessment.Text = "Please improve yourself and prepare more for future interviews. Your resume aligns well with the job.";
                }
                else
                {
                    litFinalAssessment.Text = "Please improve yourself and prepare more for future interviews. Also, review the suggestions on your resume below.";
                }
                litRewriteSuggestions.Text = BuildRewriteHtml(result.RewriteSuggestions);
            }
            else
            {
                litFinalAssessment.Text = Server.HtmlEncode(result.FinalAssessment ?? string.Empty);
                litRewriteSuggestions.Text = BuildRewriteHtml(result.RewriteSuggestions);
            }

        }

        private string GenerateScoreCircleHtml(int score)
        {
            double radius = 46;
            double stroke = 6;
            double normalizedRadius = radius - stroke / 2.0;
            double circumference = 2 * Math.PI * normalizedRadius;
            double progress = score / 100.0;
            double strokeDashoffset = circumference * (1 - progress);
            string gradId = "grad-" + Guid.NewGuid().ToString("N");

            return $@"
                <div class='visual-score-circle'>
                    <svg viewBox='0 0 100 100'>
                        <defs>
                            <linearGradient id='{gradId}' x1='1' y1='0' x2='0' y2='1'>
                                <stop offset='0%' stop-color='#FF97AD' />
                                <stop offset='100%' stop-color='#5171FF' />
                            </linearGradient>
                        </defs>
                        <circle class='circle-bg' cx='50' cy='50' r='{normalizedRadius}' style='stroke-width: {stroke}px;' />
                        <circle class='circle-progress' cx='50' cy='50' r='{normalizedRadius}'
                                style='stroke: url(#{gradId}); stroke-width: {stroke}px; stroke-dasharray: {circumference}; stroke-dashoffset: {strokeDashoffset};' />
                    </svg>
                    <div class='score-text'>{score}%</div>
                </div>";
        }

        protected void btnToggleEnhPreviewEdit_Click(object sender, EventArgs e)
        {
            PreviewEditMode = true;
            ApplyPreviewEditorState();
        }

        protected void btnCancelEnhancedResume_Click(object sender, EventArgs e)
        {
            PreviewEditMode = false;
            ApplyPreviewEditorState();
        }

        protected void btnSaveEnhancedResume_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            int appliedJobId = GetLoadedAppliedJobId();
            int interviewId = GetLoadedInterviewId();

            if (appliedJobId <= 0)
            {
                ShowStatus("This enhanced resume cannot be saved yet because no application history is linked to it.", true);
                return;
            }

            ResumeEnhancementReportRecord report;
            if (!ApplicationDataStore.TryGetResumeEnhancementReport(userId, appliedJobId, out report) || report == null)
            {
                ShowStatus("The enhanced resume report could not be found.", true);
                return;
            }

            ResumeProfileDocument fallback = GetCurrentEditableDocument();
            ResumeProfileDocument document = BuildStructuredDocumentFromForm();
            MergeDocumentDefaults(document, fallback);

            string structuredText = ResumeProfileService.BuildResumeText(document);
            bool isProfileResume = string.Equals(hfLoadedResumeSource.Value, "profile", StringComparison.OrdinalIgnoreCase);
            if (isProfileResume)
            {
                string savedProfilePath = SaveEnhancedProfileResume(userId, document);
                if (string.IsNullOrWhiteSpace(savedProfilePath))
                {
                    ShowStatus("The enhanced resume could not be saved to your profile.", true);
                    return;
                }

                report.ResumePath = savedProfilePath;
                report.ResumeSource = "profile";
                ShowStatus("The enhanced resume was saved to your profile.", false);
            }
            else
            {
                string originalFileName = string.IsNullOrWhiteSpace(report.JobTitle) ? "enhanced-resume.json" : SanitizeFileName(report.JobTitle) + "-enhanced.json";
                ApplicationResumeSelection selection = ApplicationDataStore.SaveApplicationResumeSelection(userId, appliedJobId, document, "enhanced-job", originalFileName);
                if (selection == null || string.IsNullOrWhiteSpace(selection.StoredResumePath))
                {
                    ShowStatus("The enhanced resume could not be saved to this application.", true);
                    return;
                }

                report.ResumePath = selection.StoredResumePath;
                report.ResumeSource = string.IsNullOrWhiteSpace(selection.ResumeSource) ? "enhanced-job" : selection.ResumeSource;
                ShowStatus("The enhanced resume was saved to this job application.", false);
            }

            report.UpdatedResumeText = structuredText;
            report.UpdatedResumeStructuredJson = ResumeProfileService.SerializeDocument(document);
            if (report.Result == null)
                report.Result = new ResumeEnhancementResult();
            report.Result.UpdatedResumeText = structuredText;
            report.Result.EnhancedResumeDocument = document;

            ApplicationDataStore.SaveResumeEnhancementReport(report);
            StorePreviewJson(report);
            PersistPreviewDocument(document);
            PreviewEditMode = false;
            ApplyPreviewEditorState();
        }

        private void RevealReportBody()
        {
            ClientScript.RegisterStartupScript(GetType(), "showResumeReportBody", "var reportBody=document.getElementById('resumeReportBody'); if(reportBody){ reportBody.style.display='block'; }", true);
        }

        private void BindEditablePreview(DataRow row, ResumeEnhancementReportRecord report)
        {
            if (report == null)
                return;

            ResumeProfileDocument document = BuildEditableDocument(report, row);
            if (document == null)
                document = new ResumeProfileDocument();

            hfLoadedAppliedJobId.Value = report.AppliedJobId.ToString();
            hfLoadedInterviewId.Value = report.InterviewId.ToString();
            hfLoadedResumePath.Value = report.ResumePath ?? string.Empty;
            hfLoadedResumeSource.Value = report.ResumeSource ?? string.Empty;
            hfLoadedOriginalFileName.Value = string.IsNullOrWhiteSpace(report.ResumePath) ? string.Empty : Path.GetFileName(report.ResumePath);

            PopulateEditablePreview(document);
            ViewState[CurrentDocumentViewStateKey] = ResumeProfileService.SerializeDocument(document);
        }

        private ResumeProfileDocument BuildEditableDocument(ResumeEnhancementReportRecord report, DataRow profileRow = null)
        {
            if (report == null)
                return new ResumeProfileDocument();

            // 1. Prefer the saved structured JSON (exact user edits)
            if (!string.IsNullOrWhiteSpace(report.UpdatedResumeStructuredJson))
            {
                ResumeProfileDocument fromJson = ResumeProfileService.DeserializeDocument(report.UpdatedResumeStructuredJson);
                if (fromJson != null && HasPreviewContent(fromJson))
                    return fromJson;
            }

            // 2. Use the Gemini-produced structured document if available
            if (report.Result?.EnhancedResumeDocument != null && HasPreviewContent(report.Result.EnhancedResumeDocument))
                return report.Result.EnhancedResumeDocument;

            string originalFileName = string.IsNullOrWhiteSpace(report.ResumePath)
                ? string.Empty
                : Path.GetFileName(report.ResumePath);

            string previewText = !string.IsNullOrWhiteSpace(report.UpdatedResumeText)
                ? report.UpdatedResumeText
                : report.OriginalResumeText;

            ResumeProfileDocument document = ResumeProfileService.ParseRawText(previewText, originalFileName);
            if (!HasPreviewContent(document) && !string.IsNullOrWhiteSpace(report.ResumePath) && File.Exists(report.ResumePath))
            {
                ResumeProfileDocument jsonDocument = LoadResumeProfileDocumentForEnhancement(report.UserId, report.AppliedJobId, report.ResumePath, profileRow);
                if (jsonDocument != null && HasPreviewContent(jsonDocument))
                    document = jsonDocument;
            }

            if (document == null)
                document = new ResumeProfileDocument();

            if (string.IsNullOrWhiteSpace(document.RawText))
                document.RawText = previewText ?? string.Empty;

            return document;
        }

        private bool HasPreviewContent(ResumeProfileDocument document)
        {
            if (document == null)
                return false;

            return !string.IsNullOrWhiteSpace(document.FullName)
                   || !string.IsNullOrWhiteSpace(document.Email)
                   || !string.IsNullOrWhiteSpace(document.Mobile)
                   || !string.IsNullOrWhiteSpace(document.Address)
                   || !string.IsNullOrWhiteSpace(document.Headline)
                   || !string.IsNullOrWhiteSpace(document.Summary)
                   || !string.IsNullOrWhiteSpace(document.ProfessionalSummary)
                   || (document.PersonalInfo != null && !string.IsNullOrWhiteSpace(document.PersonalInfo.Country))
                   || (document.EducationDetails != null && document.EducationDetails.Count > 0)
                   || (document.ExperienceDetails != null && document.ExperienceDetails.Count > 0)
                   || (document.ProjectDetails != null && document.ProjectDetails.Count > 0)
                   || (document.SkillGroups != null && (
                           (document.SkillGroups.ProgrammingLanguages != null && document.SkillGroups.ProgrammingLanguages.Count > 0)
                           || (document.SkillGroups.FrameworksLibraries != null && document.SkillGroups.FrameworksLibraries.Count > 0)))
                   || (document.Skills != null && document.Skills.Count > 0)
                   || (document.Education != null && document.Education.Count > 0)
                   || (document.Experience != null && document.Experience.Count > 0)
                   || (document.Projects != null && document.Projects.Count > 0)
                   || (document.Certifications != null && document.Certifications.Count > 0)
                   || (document.Languages != null && document.Languages.Count > 0);
        }

        private void PopulateEditablePreview(ResumeProfileDocument document)
        {
            PopulateStructuredPreview(document);
        }

        private ResumeProfileDocument GetCurrentEditableDocument()
        {
            string json = ViewState[CurrentDocumentViewStateKey] as string;
            ResumeProfileDocument document = ResumeProfileService.DeserializeDocument(json);
            return document ?? new ResumeProfileDocument();
        }

        private void MergeDocumentDefaults(ResumeProfileDocument target, ResumeProfileDocument fallback)
        {
            if (target == null || fallback == null)
                return;

            if (string.IsNullOrWhiteSpace(target.FullName)) target.FullName = fallback.FullName;
            if (string.IsNullOrWhiteSpace(target.Email)) target.Email = fallback.Email;
            if (string.IsNullOrWhiteSpace(target.Mobile)) target.Mobile = fallback.Mobile;
            if (string.IsNullOrWhiteSpace(target.Address)) target.Address = fallback.Address;
            if (string.IsNullOrWhiteSpace(target.Headline)) target.Headline = fallback.Headline;
            if (string.IsNullOrWhiteSpace(target.Summary)) target.Summary = fallback.Summary;
            if (string.IsNullOrWhiteSpace(target.ProfessionalSummary)) target.ProfessionalSummary = fallback.ProfessionalSummary;
            if (target.PersonalInfo == null) target.PersonalInfo = new ResumePersonalInfo();
            if (fallback.PersonalInfo != null)
            {
                if (string.IsNullOrWhiteSpace(target.PersonalInfo.Country)) target.PersonalInfo.Country = fallback.PersonalInfo.Country;
                if (string.IsNullOrWhiteSpace(target.PersonalInfo.FullName)) target.PersonalInfo.FullName = fallback.PersonalInfo.FullName;
                if (string.IsNullOrWhiteSpace(target.PersonalInfo.Email)) target.PersonalInfo.Email = fallback.PersonalInfo.Email;
                if (string.IsNullOrWhiteSpace(target.PersonalInfo.Mobile)) target.PersonalInfo.Mobile = fallback.PersonalInfo.Mobile;
                if (string.IsNullOrWhiteSpace(target.PersonalInfo.Address)) target.PersonalInfo.Address = fallback.PersonalInfo.Address;
                if (string.IsNullOrWhiteSpace(target.PersonalInfo.LinkedInUrl)) target.PersonalInfo.LinkedInUrl = fallback.PersonalInfo.LinkedInUrl;
                if (string.IsNullOrWhiteSpace(target.PersonalInfo.PortfolioUrl)) target.PersonalInfo.PortfolioUrl = fallback.PersonalInfo.PortfolioUrl;
            }

            // Note: empty list (Count == 0) means the user intentionally deleted all
            // entries — only null means "not provided", so only null falls back.
            if (target.EducationDetails == null)
                target.EducationDetails = fallback.EducationDetails != null ? new List<ResumeEducationEntry>(fallback.EducationDetails) : new List<ResumeEducationEntry>();
            if (target.ExperienceDetails == null)
                target.ExperienceDetails = fallback.ExperienceDetails != null ? new List<ResumeExperienceEntry>(fallback.ExperienceDetails) : new List<ResumeExperienceEntry>();
            if (target.ProjectDetails == null)
                target.ProjectDetails = fallback.ProjectDetails != null ? new List<ResumeProjectEntry>(fallback.ProjectDetails) : new List<ResumeProjectEntry>();
            if (target.SkillGroups == null)
                target.SkillGroups = CloneSkillGroups(fallback.SkillGroups);

            if (target.Skills == null) target.Skills = fallback.Skills;
            if (target.Education == null) target.Education = fallback.Education;
            if (target.Experience == null) target.Experience = fallback.Experience;
            if (target.Projects == null) target.Projects = fallback.Projects;
            if (target.Certifications == null) target.Certifications = fallback.Certifications;
            if (target.Languages == null) target.Languages = fallback.Languages;
            if (string.IsNullOrWhiteSpace(target.LinkedInUrl)) target.LinkedInUrl = fallback.LinkedInUrl;
            if (string.IsNullOrWhiteSpace(target.PortfolioUrl)) target.PortfolioUrl = fallback.PortfolioUrl;
            if (string.IsNullOrWhiteSpace(target.RawText)) target.RawText = fallback.RawText;
            if (string.IsNullOrWhiteSpace(target.OriginalFileName)) target.OriginalFileName = fallback.OriginalFileName;
            if (string.IsNullOrWhiteSpace(target.StoredFilePath)) target.StoredFilePath = fallback.StoredFilePath;
        }

        private static bool IsSkillGroupsEmpty(ResumeSkillGroups g)
        {
            if (g == null)
                return true;
            return (g.ProgrammingLanguages == null || g.ProgrammingLanguages.Count == 0)
                   && (g.FrameworksLibraries == null || g.FrameworksLibraries.Count == 0)
                   && (g.ToolsCloudDatabaseSkills == null || g.ToolsCloudDatabaseSkills.Count == 0)
                   && (g.SoftSkillsLanguages == null || g.SoftSkillsLanguages.Count == 0)
                   && string.IsNullOrWhiteSpace(g.CustomHeading)
                   && (g.CustomItems == null || g.CustomItems.Count == 0);
        }

        private static ResumeSkillGroups CloneSkillGroups(ResumeSkillGroups g)
        {
            if (g == null)
                return null;
            return new ResumeSkillGroups
            {
                ProgrammingLanguages = g.ProgrammingLanguages != null ? new List<string>(g.ProgrammingLanguages) : new List<string>(),
                FrameworksLibraries = g.FrameworksLibraries != null ? new List<string>(g.FrameworksLibraries) : new List<string>(),
                ToolsCloudDatabaseSkills = g.ToolsCloudDatabaseSkills != null ? new List<string>(g.ToolsCloudDatabaseSkills) : new List<string>(),
                SoftSkillsLanguages = g.SoftSkillsLanguages != null ? new List<string>(g.SoftSkillsLanguages) : new List<string>(),
                CustomHeading = g.CustomHeading,
                CustomItems = g.CustomItems != null ? new List<string>(g.CustomItems) : new List<string>()
            };
        }

        private string SaveEnhancedProfileResume(int userId, ResumeProfileDocument document)
        {
            if (document == null)
                return string.Empty;

            string folder = Server.MapPath("~/Resumes");
            Directory.CreateDirectory(folder);

            string fileName = "enhanced-profile-" + userId + "-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".json";
            string physicalPath = Path.Combine(folder, fileName);
            File.WriteAllText(physicalPath, ResumeProfileService.SerializeDocument(document));

            string relativePath = "Resumes/" + fileName;

            using (SqlConnection con = new SqlConnection(str))
            {
                con.Open();
                using (SqlTransaction tran = con.BeginTransaction())
                {
                    bool profileExists;
                    using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM JobSeekers WHERE ProfileId = @UserId", con, tran))
                    {
                        checkCmd.Parameters.AddWithValue("@UserId", userId);
                        profileExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                    }

                    string updateUserQuery = "UPDATE Users SET Email=@Email, Address=@Address, UpdatedAt=GETDATE() WHERE UserId=@UserId";
                    using (SqlCommand userCmd = new SqlCommand(updateUserQuery, con, tran))
                    {
                        userCmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(document.Email) ? (object)DBNull.Value : document.Email);
                        userCmd.Parameters.AddWithValue("@Address", string.IsNullOrWhiteSpace(document.Address) ? (object)DBNull.Value : document.Address);
                        userCmd.Parameters.AddWithValue("@UserId", userId);
                        userCmd.ExecuteNonQuery();
                    }

                    string jobSeekerQuery;
                    if (profileExists)
                    {
                            jobSeekerQuery = @"UPDATE JobSeekers SET Name=@Name, Mobile=@Mobile,
                                                Resume=@Resume, ResumeOriginalFileName=@ResumeOriginalFileName, ResumeParseStatus=@ResumeParseStatus,
                                                ResumeValidationMessage=@ResumeValidationMessage, ResumeUploadedAt=@ResumeUploadedAt, ResumeParsedAt=@ResumeParsedAt,
                                                ResumeStructuredJson=@ResumeStructuredJson, ResumeRawText=@ResumeRawText
                                                WHERE ProfileId=@ProfileId";
                    }
                    else
                    {
                        jobSeekerQuery = @"INSERT INTO JobSeekers (ProfileId, Name, Mobile, Photo,
                                            Resume, ResumeOriginalFileName, ResumeParseStatus, ResumeValidationMessage,
                                            ResumeUploadedAt, ResumeParsedAt, ResumeStructuredJson, ResumeRawText)
                                            VALUES (@ProfileId, @Name, @Mobile, 'avatar.png',
                                            @Resume, @ResumeOriginalFileName, @ResumeParseStatus, @ResumeValidationMessage,
                                            @ResumeUploadedAt, @ResumeParsedAt, @ResumeStructuredJson, @ResumeRawText)";
                    }

                    using (SqlCommand jobSeekerCmd = new SqlCommand(jobSeekerQuery, con, tran))
                    {
                        jobSeekerCmd.Parameters.AddWithValue("@ProfileId", userId);
                        jobSeekerCmd.Parameters.AddWithValue("@Name", string.IsNullOrWhiteSpace(document.FullName) ? (object)DBNull.Value : document.FullName);
                        jobSeekerCmd.Parameters.AddWithValue("@Mobile", string.IsNullOrWhiteSpace(document.Mobile) ? (object)DBNull.Value : document.Mobile);

                        ResumeProfileService.AddResumeProfileParameters(jobSeekerCmd, document, relativePath);
                        jobSeekerCmd.ExecuteNonQuery();
                    }

                    tran.Commit();
                }
            }

            return physicalPath;
        }

        private void PersistPreviewDocument(ResumeProfileDocument document)
        {
            if (document == null)
                return;

            ViewState[CurrentDocumentViewStateKey] = ResumeProfileService.SerializeDocument(document);
            PopulateEditablePreview(document);
        }

        private void ApplyPreviewEditorState()
        {
            bool editable = PreviewEditMode;

            SetEnhancerEditorsReadOnly(!editable);

            btnToggleEnhPreviewEdit.Visible = !editable;
            pnlEnhSaveOptions.Visible = editable;

            // Toggle CSS class on report body to hide/show non-editor sections
            string editModeScript = editable
                ? "var rb=document.getElementById('resumeReportBody'); if(rb) rb.classList.add('enhancer-editing');"
                : "var rb=document.getElementById('resumeReportBody'); if(rb) rb.classList.remove('enhancer-editing');";
            ClientScript.RegisterStartupScript(GetType(), "enhancerEditMode", editModeScript, true);
        }

        private int GetLoadedAppliedJobId()
        {
            int appliedJobId;
            if (int.TryParse(hfLoadedAppliedJobId.Value, out appliedJobId))
                return appliedJobId;

            return 0;
        }

        private int GetLoadedInterviewId()
        {
            int interviewId;
            if (int.TryParse(hfLoadedInterviewId.Value, out interviewId))
                return interviewId;

            return 0;
        }

        private string JoinLines(IEnumerable<string> lines)
        {
            if (lines == null)
                return string.Empty;

            return string.Join(Environment.NewLine, lines.Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Trim()));
        }

        private string JoinCommaSeparated(IEnumerable<string> values)
        {
            if (values == null)
                return string.Empty;

            return string.Join(", ", values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()));
        }

        private string FormatMonthYear(int? month, int? year)
        {
            if (!month.HasValue && !year.HasValue)
                return string.Empty;

            if (!month.HasValue)
                return year.HasValue ? year.Value.ToString() : string.Empty;

            if (!year.HasValue)
                return month.Value.ToString();

            return month.Value.ToString("00") + "/" + year.Value;
        }

        private string GetPrimaryExperienceSummary(IEnumerable<string> experience)
        {
            if (experience == null)
                return string.Empty;

            string summary = experience
                .Select(line => string.IsNullOrWhiteSpace(line) ? string.Empty : line.Trim())
                .FirstOrDefault(line => !string.IsNullOrWhiteSpace(line));

            if (string.IsNullOrWhiteSpace(summary))
                return string.Empty;

            return summary.Length <= 50 ? summary : summary.Substring(0, 50);
        }

        private List<string> SplitLines(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new List<string>();

            return value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToList();
        }

        private void ShowReportLoadedStatus(int appliedJobId, string message)
        {
            litStatus.Text = "<div class='status-note'>" + Server.HtmlEncode(message) + "</div>";
            //+
            //                 "<div class='hero-actions' style='margin-top:12px;'>" +
            //                 "<a href='ResumeEnhancer.aspx?applicationId=" + appliedJobId + "&history=1&download=pdf' class='hero-action-btn primary'>" +
            //                 "<i class='fas fa-file-pdf'></i> Export PDF</a>" +
            //                 "</div>";
        }

        private void StorePreviewJson(ResumeEnhancementReportRecord report)
        {
            if (hfResumePreviewJson == null)
                return;

            hfResumePreviewJson.Value = string.Empty;
            if (litHtmlPreviewFrame != null)
                litHtmlPreviewFrame.Text = BuildHtmlPreviewFrameMarkup();
            if (report == null)
                return;

            try
            {
                ResumeProfileDocument doc = BuildEditableDocument(report, null);
                if (doc != null && HasPreviewContent(doc))
                {
                    string json = ResumeProfileService.SerializeDocument(doc);
                    hfResumePreviewJson.Value = JObject.Parse(json).ToString(Formatting.None);
                    if (litHtmlPreviewFrame != null)
                        litHtmlPreviewFrame.Text = BuildHtmlPreviewFrameMarkup();
                    return;
                }

                if (report.Result != null && !string.IsNullOrWhiteSpace(report.Result.RawGeminiJson))
                {
                    JObject jo = JObject.Parse(report.Result.RawGeminiJson);
                    JToken inner = jo["enhancedResumeDocument"];
                    hfResumePreviewJson.Value = inner != null ? inner.ToString(Formatting.None) : jo.ToString(Formatting.None);
                    if (litHtmlPreviewFrame != null)
                        litHtmlPreviewFrame.Text = BuildHtmlPreviewFrameMarkup();
                }
            }
            catch
            {
                if (report.Result != null && !string.IsNullOrWhiteSpace(report.Result.RawGeminiJson))
                {
                    hfResumePreviewJson.Value = report.Result.RawGeminiJson;
                    if (litHtmlPreviewFrame != null)
                        litHtmlPreviewFrame.Text = BuildHtmlPreviewFrameMarkup();
                }
            }
        }

        private string BuildHtmlPreviewFrameMarkup()
        {
            string json = hfResumePreviewJson != null ? hfResumePreviewJson.Value : string.Empty;
            if (string.IsNullOrWhiteSpace(json))
                return "<div class='muted-box'>No HTML preview data is available yet.</div>";

            string encoded = HttpUtility.UrlEncode(json).Replace("+", "%20");
            string src = ResolveUrl("~/ResumePreview.html") + "#data=" + encoded;
            return "<iframe class='html-preview-frame' src='" + System.Web.HttpUtility.HtmlAttributeEncode(src) + "' title='HTML Resume Preview' onload='resizeHtmlPreviewFrame(this)'></iframe>";
        }

        private string BuildInterviewFeedbackText(DataRow row)
        {
            var builder = new StringBuilder();
            string finalAssessment = row["FinalAssessment"] == DBNull.Value ? string.Empty : row["FinalAssessment"].ToString();
            string strengths = row["Strengths"] == DBNull.Value ? string.Empty : row["Strengths"].ToString();
            string areas = row["AreasForImprovement"] == DBNull.Value ? string.Empty : row["AreasForImprovement"].ToString();

            if (!string.IsNullOrWhiteSpace(finalAssessment))
                builder.AppendLine("Interview assessment: " + finalAssessment);
            if (!string.IsNullOrWhiteSpace(strengths))
                builder.AppendLine("Interview strengths: " + strengths.Replace("|", ", "));
            if (!string.IsNullOrWhiteSpace(areas))
                builder.AppendLine("Interview gaps: " + areas.Replace("|", ", "));

            return builder.ToString().Trim();
        }

        private string BuildKeywordHints(DataRow row)
        {
            var keywords = new List<string>();
            AddIfPresent(keywords, row["Specialization"]);
            AddIfPresent(keywords, row["Qualification"]);
            AddIfPresent(keywords, row["JobType"]);
            AddIfPresent(keywords, row["Experience"]);
            AddIfPresent(keywords, row["TechStack"]);
            return string.Join(", ", keywords.Distinct(StringComparer.OrdinalIgnoreCase));
        }

        private void AddIfPresent(ICollection<string> list, object value)
        {
            if (value != null && value != DBNull.Value)
            {
                string text = value.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(text))
                    list.Add(text);
            }
        }

        private string BuildListHtml(IEnumerable<string> items, string fallback, bool encodeItems)
        {
            if (items == null)
                return "<li>" + Server.HtmlEncode(fallback) + "</li>";

            var cleaned = items.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            if (cleaned.Count == 0)
                return "<li>" + Server.HtmlEncode(fallback) + "</li>";

            var builder = new StringBuilder();
            foreach (string item in cleaned)
            {
                builder.Append("<li>");
                builder.Append(encodeItems ? Server.HtmlEncode(item) : item);
                builder.AppendLine("</li>");
            }
            return builder.ToString();
        }

        private string BuildRewriteHtml(IEnumerable<ResumeRewriteSuggestion> suggestions)
        {
            if (suggestions == null)
                return "<div class='muted-box'>No rewrite suggestions were returned yet.</div>";

            var list = suggestions.Where(x => x != null && (!string.IsNullOrWhiteSpace(x.SectionName) || !string.IsNullOrWhiteSpace(x.SuggestedRewrite))).ToList();
            if (list.Count == 0)
                return "<div class='muted-box'>No rewrite suggestions were returned yet.</div>";

            var builder = new StringBuilder();
            builder.AppendLine("<div style='display: grid; grid-template-columns: 1fr 1fr; gap: 20px;'>");
            foreach (ResumeRewriteSuggestion suggestion in list)
            {
                builder.AppendLine("<div class='rewrite-card' style='margin-bottom: 0;'>");
                builder.AppendLine("<h4>" + Server.HtmlEncode(string.IsNullOrWhiteSpace(suggestion.SectionName) ? "Suggested Improvement" : suggestion.SectionName) + "</h4>");
                if (!string.IsNullOrWhiteSpace(suggestion.CurrentObservation))
                {
                    builder.AppendLine("<div class='current'><span>What we found</span>" + Server.HtmlEncode(suggestion.CurrentObservation) + "</div>");
                }
                if (!string.IsNullOrWhiteSpace(suggestion.SuggestedRewrite))
                {
                    builder.AppendLine("<div class='suggested'><span>Suggested rewrite</span>" + Server.HtmlEncode(suggestion.SuggestedRewrite) + "</div>");
                }
                builder.AppendLine("</div>");
            }
            builder.AppendLine("</div>");
            return builder.ToString();
        }

        private string BuildAssessmentContext(ResumeEnhancementReportRecord report, DataRow row)
        {
            var builder = new StringBuilder();
            builder.Append("Role: ").Append(string.IsNullOrWhiteSpace(report.JobTitle) ? row["Role"].ToString() : report.JobTitle).AppendLine();
            builder.Append("Company: ").Append(string.IsNullOrWhiteSpace(report.CompanyName) ? row["CompanyName"].ToString() : report.CompanyName).AppendLine();
            builder.Append("Level: ").Append(row["Level"]).AppendLine();
            builder.Append("Interview type: ").Append(row["InterviewType"]).AppendLine();
            builder.Append("Resume source: ").Append(string.IsNullOrWhiteSpace(report.ResumeSource) ? "profile" : report.ResumeSource).AppendLine();
            if (report.GeneratedAt != DateTime.MinValue)
                builder.Append("Saved at: ").Append(report.GeneratedAt.ToString("MMM d, yyyy h:mm tt")).AppendLine();
            builder.Append("Soft keyword hints: ").Append(report.KeywordHints ?? string.Empty).AppendLine();
            if (!string.IsNullOrWhiteSpace(report.InterviewFeedback))
            {
                builder.AppendLine();
                builder.AppendLine(report.InterviewFeedback);
            }
            return builder.ToString().Trim();
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            text = text.Trim();
            if (text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + Environment.NewLine + "...";
        }

        private string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string sanitized = value.Trim();
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                sanitized = sanitized.Replace(invalidChar.ToString(), string.Empty);
            }

            sanitized = Regex.Replace(sanitized, "\\s+", "-");
            sanitized = Regex.Replace(sanitized, "[^A-Za-z0-9._-]", "");

            return string.IsNullOrWhiteSpace(sanitized) ? string.Empty : sanitized.Trim('-');
        }

        private void ShowStatus(string message, bool isWarning)
        {
            string css = isWarning ? "status-note" : "status-note";
            litStatus.Text = "<div class='" + css + "'>" + Server.HtmlEncode(message) + "</div>";
        }


        protected void btnDeleteEnhancementHistory_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            int appliedJobId = GetLoadedAppliedJobId();

            if (appliedJobId > 0)
                ApplicationDataStore.DeleteResumeEnhancementReport(userId, appliedJobId);

            // Redirect to the same URL without the applicationId query param to force a fresh Gemini call
            string interviewId = Request.QueryString["id"];
            if (!string.IsNullOrWhiteSpace(interviewId))
                Response.Redirect("InterviewFeedback.aspx?id=" + interviewId);
            else
                Response.Redirect("InterviewFeedback.aspx");
        }

        protected void btnExportResumePdf_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            int appliedJobId = GetLoadedAppliedJobId();

            ResumeEnhancementReportRecord report;
            ResumeProfileDocument document;

            if (appliedJobId > 0 && ApplicationDataStore.TryGetResumeEnhancementReport(userId, appliedJobId, out report) && report != null)
            {
                document = BuildEditableDocument(report, null);
            }
            else
            {
                document = GetCurrentEditableDocument();
            }

            if (document == null || !HasPreviewContent(document))
            {
                ShowStatus("There is no resume data available to export.", true);
                return;
            }

            string iconsFolder = Server.MapPath("~/assets/img/resume-icons");
            if (!Directory.Exists(iconsFolder))
                iconsFolder = Server.MapPath("~/");

            byte[] pdfBytes = ResumePdfExporter.BuildResume(document, iconsFolder);
            string safeName = string.IsNullOrWhiteSpace(document.FullName) ? "resume" : SanitizeFileName(document.FullName);
            string fileName = safeName + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";

            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "application/pdf";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            Response.BinaryWrite(pdfBytes);
            Response.Flush();
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
