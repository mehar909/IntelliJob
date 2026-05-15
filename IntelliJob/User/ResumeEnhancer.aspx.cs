using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

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

            if (!IsPostBack)
                LoadEnhancement();
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
                                        js.Resume, js.Name
                                 FROM AppliedJobs aj
                                 INNER JOIN Jobs j ON aj.JobId = j.JobId
                                 LEFT JOIN Interviews i ON i.AppliedJobId = aj.AppliedJobId AND i.UserId = aj.UserId
                                 LEFT JOIN InterviewFeedback fb ON fb.InterviewId = i.InterviewId
                                 LEFT JOIN JobSeekers js ON js.ProfileId = aj.UserId
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
            ReportLoaded = true;
            ShowReportLoadedStatus(savedReport.AppliedJobId, "Saved resume history loaded from your application record.");

            if (string.Equals(Request.QueryString["download"], "pdf", StringComparison.OrdinalIgnoreCase))
            {
                DownloadResumeReportPdf(savedReport);
                return;
            }

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
                                        js.Resume, js.Name
                                 FROM Interviews i
                                 INNER JOIN Jobs j ON i.JobId = j.JobId
                                 LEFT JOIN InterviewFeedback fb ON fb.InterviewId = i.InterviewId
                                 LEFT JOIN JobSeekers js ON js.ProfileId = i.UserId
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
                ShowReportLoadedStatus(savedReport.AppliedJobId, "Saved resume report loaded from your application history.");
                return;
            }

            string resumeText = ResumeTextExtractor.ExtractText(resumePath);
            if (string.IsNullOrWhiteSpace(resumeText))
            {
                resumeText = "No readable resume text could be extracted from the stored resume file.";
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
            RenderReport(row, report);
            BindEditablePreview(row, report);
            ReportLoaded = true;
            ShowReportLoadedStatus(report.AppliedJobId, "This resume report has been saved to your application history.");

            if (string.Equals(Request.QueryString["download"], "pdf", StringComparison.OrdinalIgnoreCase))
            {
                DownloadResumeReportPdf(report);
                return;
            }

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

        private void RenderReport(DataRow row, ResumeEnhancementReportRecord report)
        {
            ResumeEnhancementResult result = report.Result ?? new ResumeEnhancementResult();

            litRole.Text = Server.HtmlEncode(string.IsNullOrWhiteSpace(report.JobTitle) ? row["Role"].ToString() : report.JobTitle);
            litCompany.Text = Server.HtmlEncode(string.IsNullOrWhiteSpace(report.CompanyName) ? row["CompanyName"].ToString() : report.CompanyName);
            litLevel.Text = Server.HtmlEncode(row["Level"].ToString());
            litInterviewType.Text = Server.HtmlEncode(row["InterviewType"].ToString());

            litOverallScore.Text = result.OverallScore.ToString();
            litAtsScore.Text = result.AtsScore.ToString();
            litSemanticScore.Text = result.SemanticScore.ToString();
            litKeywordScore.Text = result.KeywordScore.ToString();

            litResumeSummary.Text = Server.HtmlEncode(result.ResumeSummary ?? string.Empty);
            litStrengths.Text = BuildListHtml(result.Strengths, "No strengths were returned yet.", true);
            litGaps.Text = BuildListHtml(result.Gaps, "No gaps were returned yet.", true);
            litPriorityKeywords.Text = BuildListHtml(result.PriorityKeywords, "No priority keywords were returned yet.", true);
            litRewriteSuggestions.Text = BuildRewriteHtml(result.RewriteSuggestions);
            litFinalAssessment.Text = Server.HtmlEncode(result.FinalAssessment ?? string.Empty);

            string previewText = !string.IsNullOrWhiteSpace(report.UpdatedResumeText)
                ? report.UpdatedResumeText
                : (!string.IsNullOrWhiteSpace(result.UpdatedResumeText) ? result.UpdatedResumeText : report.OriginalResumeText);
            litResumePreview.Text = Server.HtmlEncode(TruncateText(previewText, 5000));
        }

        protected void btnToggleEnhPreviewEdit_Click(object sender, EventArgs e)
        {
            PreviewEditMode = !PreviewEditMode;
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
            ResumeProfileDocument document = BuildDocumentFromForm();
            MergeDocumentDefaults(document, fallback);

            string structuredText = BuildStructuredResumeText(document);
            if (rblEnhSaveTarget.SelectedValue == "profile")
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
                string originalFileName = string.IsNullOrWhiteSpace(report.JobTitle) ? "enhanced-resume.txt" : SanitizeFileName(report.JobTitle) + "-enhanced.txt";
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
            if (report.Result == null)
                report.Result = new ResumeEnhancementResult();
            report.Result.UpdatedResumeText = structuredText;

            ApplicationDataStore.SaveResumeEnhancementReport(report);
            PersistPreviewDocument(document);
            BindEditablePreview(null, report);
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

            ResumeProfileDocument document = BuildEditableDocument(report);
            if (document == null)
                document = new ResumeProfileDocument();

            hfLoadedAppliedJobId.Value = report.AppliedJobId.ToString();
            hfLoadedInterviewId.Value = report.InterviewId.ToString();
            hfLoadedResumePath.Value = report.ResumePath ?? string.Empty;
            hfLoadedResumeSource.Value = report.ResumeSource ?? string.Empty;
            hfLoadedOriginalFileName.Value = string.IsNullOrWhiteSpace(report.ResumePath) ? string.Empty : Path.GetFileName(report.ResumePath);

            if (rblEnhSaveTarget.Items.FindByValue("job") != null && rblEnhSaveTarget.Items.FindByValue("profile") != null)
            {
                string defaultTarget = string.Equals(report.ResumeSource, "profile", StringComparison.OrdinalIgnoreCase) ? "profile" : "job";
                rblEnhSaveTarget.SelectedValue = defaultTarget;
            }

            PopulateEditablePreview(document);
            ViewState[CurrentDocumentViewStateKey] = ResumeProfileService.SerializeDocument(document);
        }

        private ResumeProfileDocument BuildEditableDocument(ResumeEnhancementReportRecord report)
        {
            if (report == null)
                return new ResumeProfileDocument();

            string originalFileName = string.IsNullOrWhiteSpace(report.ResumePath)
                ? string.Empty
                : Path.GetFileName(report.ResumePath);

            string previewText = !string.IsNullOrWhiteSpace(report.UpdatedResumeText)
                ? report.UpdatedResumeText
                : report.OriginalResumeText;

            ResumeProfileDocument document = ResumeProfileService.ParseRawText(previewText, originalFileName);
            if (!HasPreviewContent(document) && !string.IsNullOrWhiteSpace(report.ResumePath) && File.Exists(report.ResumePath))
            {
                string extractedText = ResumeTextExtractor.ExtractText(report.ResumePath);
                document = ResumeProfileService.ParseRawText(extractedText, originalFileName);
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
                   || (document.Skills != null && document.Skills.Count > 0)
                   || (document.Education != null && document.Education.Count > 0)
                   || (document.Experience != null && document.Experience.Count > 0)
                   || (document.Projects != null && document.Projects.Count > 0)
                   || (document.Certifications != null && document.Certifications.Count > 0)
                   || (document.Languages != null && document.Languages.Count > 0);
        }

        private void PopulateEditablePreview(ResumeProfileDocument document)
        {
            txtEnhFullName.Text = document != null ? document.FullName : string.Empty;
            txtEnhEmail.Text = document != null ? document.Email : string.Empty;
            txtEnhMobile.Text = document != null ? document.Mobile : string.Empty;
            txtEnhAddress.Text = document != null ? document.Address : string.Empty;
            txtEnhHeadline.Text = document != null ? document.Headline : string.Empty;
            txtEnhSummary.Text = document != null ? document.Summary : string.Empty;
            txtEnhSkills.Text = JoinLines(document != null ? document.Skills : null);
            txtEnhEducation.Text = JoinLines(document != null ? document.Education : null);
            txtEnhExperience.Text = JoinLines(document != null ? document.Experience : null);
            txtEnhProjects.Text = JoinLines(document != null ? document.Projects : null);
            txtEnhCertifications.Text = JoinLines(document != null ? document.Certifications : null);
            txtEnhLanguages.Text = JoinLines(document != null ? document.Languages : null);
        }

        private ResumeProfileDocument GetCurrentEditableDocument()
        {
            string json = ViewState[CurrentDocumentViewStateKey] as string;
            ResumeProfileDocument document = ResumeProfileService.DeserializeDocument(json);
            return document ?? new ResumeProfileDocument();
        }

        private ResumeProfileDocument BuildDocumentFromForm()
        {
            return new ResumeProfileDocument
            {
                FullName = txtEnhFullName.Text.Trim(),
                Email = txtEnhEmail.Text.Trim(),
                Mobile = txtEnhMobile.Text.Trim(),
                Address = txtEnhAddress.Text.Trim(),
                Headline = txtEnhHeadline.Text.Trim(),
                Summary = txtEnhSummary.Text.Trim(),
                Skills = SplitLines(txtEnhSkills.Text),
                Education = SplitLines(txtEnhEducation.Text),
                Experience = SplitLines(txtEnhExperience.Text),
                Projects = SplitLines(txtEnhProjects.Text),
                Certifications = SplitLines(txtEnhCertifications.Text),
                Languages = SplitLines(txtEnhLanguages.Text),
                RawText = GetCurrentEditableDocument().RawText,
                OriginalFileName = hfLoadedOriginalFileName.Value,
                StoredFilePath = hfLoadedResumePath.Value,
                ParsedAt = DateTime.UtcNow,
                IsValid = true
            };
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
            if (target.Skills == null || target.Skills.Count == 0) target.Skills = fallback.Skills;
            if (target.Education == null || target.Education.Count == 0) target.Education = fallback.Education;
            if (target.Experience == null || target.Experience.Count == 0) target.Experience = fallback.Experience;
            if (target.Projects == null || target.Projects.Count == 0) target.Projects = fallback.Projects;
            if (target.Certifications == null || target.Certifications.Count == 0) target.Certifications = fallback.Certifications;
            if (target.Languages == null || target.Languages.Count == 0) target.Languages = fallback.Languages;
            if (string.IsNullOrWhiteSpace(target.RawText)) target.RawText = fallback.RawText;
            if (string.IsNullOrWhiteSpace(target.OriginalFileName)) target.OriginalFileName = fallback.OriginalFileName;
            if (string.IsNullOrWhiteSpace(target.StoredFilePath)) target.StoredFilePath = fallback.StoredFilePath;
        }

        private string BuildStructuredResumeText(ResumeProfileDocument document)
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

        private void AppendSection(StringBuilder builder, string heading, string content)
        {
            if (builder == null || string.IsNullOrWhiteSpace(content))
                return;

            builder.AppendLine(heading + ":");
            builder.AppendLine(content.Trim());
            builder.AppendLine();
        }

        private string SaveEnhancedProfileResume(int userId, ResumeProfileDocument document)
        {
            if (document == null)
                return string.Empty;

            string folder = Server.MapPath("~/Resumes");
            Directory.CreateDirectory(folder);

            string fileName = "enhanced-profile-" + userId + "-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".txt";
            string physicalPath = Path.Combine(folder, fileName);
            File.WriteAllText(physicalPath, BuildStructuredResumeText(document));

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
                        jobSeekerQuery = @"UPDATE JobSeekers SET Name=@Name, Mobile=@Mobile, WorksOn=@WorksOn, Experience=@Experience,
                                            Resume=@Resume, ResumeOriginalFileName=@ResumeOriginalFileName, ResumeParseStatus=@ResumeParseStatus,
                                            ResumeValidationMessage=@ResumeValidationMessage, ResumeUploadedAt=@ResumeUploadedAt, ResumeParsedAt=@ResumeParsedAt,
                                            ResumeStructuredJson=@ResumeStructuredJson, ResumeRawText=@ResumeRawText, ResumeHeadline=@ResumeHeadline,
                                            ResumeSummary=@ResumeSummary, ResumeSkills=@ResumeSkills, ResumeEducation=@ResumeEducation,
                                            ResumeExperienceDetails=@ResumeExperienceDetails, ResumeProjects=@ResumeProjects,
                                            ResumeCertifications=@ResumeCertifications, ResumeLanguages=@ResumeLanguages
                                            WHERE ProfileId=@ProfileId";
                    }
                    else
                    {
                        jobSeekerQuery = @"INSERT INTO JobSeekers (ProfileId, Name, Mobile, TenthGrade, TwelfthGrade, GraduationGrade,
                                            PostGraduationGrade, Phd, WorksOn, Experience, Photo,
                                            Resume, ResumeOriginalFileName, ResumeParseStatus, ResumeValidationMessage,
                                            ResumeUploadedAt, ResumeParsedAt, ResumeStructuredJson, ResumeRawText,
                                            ResumeHeadline, ResumeSummary, ResumeSkills, ResumeEducation,
                                            ResumeExperienceDetails, ResumeProjects, ResumeCertifications, ResumeLanguages)
                                            VALUES (@ProfileId, @Name, @Mobile, NULL, NULL, NULL,
                                            NULL, NULL, @WorksOn, @Experience, 'avatar.png',
                                            @Resume, @ResumeOriginalFileName, @ResumeParseStatus, @ResumeValidationMessage,
                                            @ResumeUploadedAt, @ResumeParsedAt, @ResumeStructuredJson, @ResumeRawText,
                                            @ResumeHeadline, @ResumeSummary, @ResumeSkills, @ResumeEducation,
                                            @ResumeExperienceDetails, @ResumeProjects, @ResumeCertifications, @ResumeLanguages)";
                    }

                    using (SqlCommand jobSeekerCmd = new SqlCommand(jobSeekerQuery, con, tran))
                    {
                        jobSeekerCmd.Parameters.AddWithValue("@ProfileId", userId);
                        jobSeekerCmd.Parameters.AddWithValue("@Name", string.IsNullOrWhiteSpace(document.FullName) ? (object)DBNull.Value : document.FullName);
                        jobSeekerCmd.Parameters.AddWithValue("@Mobile", string.IsNullOrWhiteSpace(document.Mobile) ? (object)DBNull.Value : document.Mobile);
                        jobSeekerCmd.Parameters.AddWithValue("@WorksOn", string.IsNullOrWhiteSpace(document.Headline) ? (object)DBNull.Value : document.Headline);
                        string experienceSummary = GetPrimaryExperienceSummary(document.Experience);
                        jobSeekerCmd.Parameters.AddWithValue("@Experience", string.IsNullOrWhiteSpace(experienceSummary) ? (object)DBNull.Value : experienceSummary);
                        ResumeProfileService.AddResumeProfileParameters(jobSeekerCmd, document, relativePath);
                        jobSeekerCmd.ExecuteNonQuery();
                    }

                    tran.Commit();
                }
            }

            return physicalPath;
        }

        private string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            foreach (char invalid in Path.GetInvalidFileNameChars())
                value = value.Replace(invalid, '-');

            return string.Join("-", value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
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

            txtEnhFullName.ReadOnly = !editable;
            txtEnhEmail.ReadOnly = !editable;
            txtEnhMobile.ReadOnly = !editable;
            txtEnhAddress.ReadOnly = !editable;
            txtEnhHeadline.ReadOnly = !editable;
            txtEnhSummary.ReadOnly = !editable;
            txtEnhSkills.ReadOnly = !editable;
            txtEnhEducation.ReadOnly = !editable;
            txtEnhExperience.ReadOnly = !editable;
            txtEnhProjects.ReadOnly = !editable;
            txtEnhCertifications.ReadOnly = !editable;
            txtEnhLanguages.ReadOnly = !editable;

            btnToggleEnhPreviewEdit.Text = editable ? "Lock Preview" : "Edit Preview";
            pnlEnhSaveOptions.Visible = editable;
            rblEnhSaveTarget.Enabled = editable;
            btnSaveEnhancedResume.Enabled = editable;
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
            litStatus.Text = "<div class='status-note'>" + Server.HtmlEncode(message) + "</div>" +
                             "<div class='hero-actions' style='margin-top:12px;'>" +
                             "<a href='ResumeEnhancer.aspx?applicationId=" + appliedJobId + "&history=1&download=pdf' class='hero-action-btn primary'>" +
                             "<i class='fas fa-file-pdf'></i> Export PDF</a>" +
                             "</div>";
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
            foreach (ResumeRewriteSuggestion suggestion in list)
            {
                builder.AppendLine("<div class='rewrite-card'>");
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

        private void ShowStatus(string message, bool isWarning)
        {
            string css = isWarning ? "status-note" : "status-note";
            litStatus.Text = "<div class='" + css + "'>" + Server.HtmlEncode(message) + "</div>";
        }

        private void DownloadResumeReportPdf(ResumeEnhancementReportRecord report)
        {
            byte[] pdfBytes = ResumePdfExporter.Build(report);
            string fileName = string.Format(System.Globalization.CultureInfo.InvariantCulture, "Resume-Report-{0}-{1}.pdf", report.AppliedJobId, DateTime.Now.ToString("yyyyMMddHHmmss"));

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
