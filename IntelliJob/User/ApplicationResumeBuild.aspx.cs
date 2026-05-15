using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.UI;

namespace IntelliJob.User
{
    public partial class ApplicationResumeBuild : Page
    {
        private readonly string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["user"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!int.TryParse(Request.QueryString["jobId"], out int jobId))
            {
                Response.Redirect("JobListing.aspx");
                return;
            }

            lnkBackToJob.NavigateUrl = "JobDetails.aspx?id=" + jobId;

            if (!IsPostBack)
                LoadDraft(jobId);
        }

        private void LoadDraft(int jobId)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            ApplicationResumeDraftRecord draft = GetDraft(userId, jobId);
            if (draft == null)
            {
                Response.Redirect("JobDetails.aspx?id=" + jobId);
                return;
            }

            ResumeProfileDocument document = GetDraftDocument(draft);
            if (document == null)
            {
                lblMsg.Visible = true;
                lblMsg.Text = "The application resume could not be loaded.";
                lblMsg.CssClass = "alert alert-danger";
                return;
            }

            lblJobInfo.Text = GetJobInfo(jobId);
            lblDraftNote.Text = draft.IsConfirmed
                ? "This resume draft is already confirmed. You can keep editing the sections below and confirm again when ready."
                : "This is a one-time application resume draft. Edit the sections below and confirm when you are ready to apply.";

            PopulateForm(document);
        }

        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(Request.QueryString["jobId"], out int jobId))
            {
                Response.Redirect("JobListing.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["userId"]);
            ApplicationResumeDraftRecord draft = GetDraft(userId, jobId);
            if (draft == null)
            {
                lblMsg.Visible = true;
                lblMsg.Text = "Your application resume draft could not be found.";
                lblMsg.CssClass = "alert alert-danger";
                return;
            }

            ResumeProfileDocument current = GetDraftDocument(draft) ?? new ResumeProfileDocument();
            ResumeProfileDocument updated = BuildDocumentFromForm();
            MergeDocumentDefaults(updated, current);

            updated.OriginalFileName = draft.OriginalFileName;
            updated.StoredFilePath = draft.StoredResumePath;
            updated.RawText = string.IsNullOrWhiteSpace(current.RawText) ? draft.RawText : current.RawText;
            updated.ValidationMessage = string.Empty;
            updated.IsValid = true;

            ApplicationDataStore.SaveApplicationResumeDraft(userId, jobId, updated, draft.ResumeSource, draft.OriginalFileName);

            Response.Redirect("JobDetails.aspx?id=" + jobId, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private ApplicationResumeDraftRecord GetDraft(int userId, int jobId)
        {
            ApplicationResumeDraftRecord draft;
            if (!ApplicationDataStore.TryGetApplicationResumeDraft(userId, jobId, out draft))
                return null;

            return draft != null && !string.IsNullOrWhiteSpace(draft.StoredResumePath) && File.Exists(draft.StoredResumePath)
                ? draft
                : null;
        }

        private ResumeProfileDocument GetDraftDocument(ApplicationResumeDraftRecord draft)
        {
            if (draft == null)
                return null;

            ResumeProfileDocument document = ResumeProfileService.DeserializeDocument(draft.StructuredJson);
            if (document != null)
                return document;

            if (!string.IsNullOrWhiteSpace(draft.StoredResumePath) && File.Exists(draft.StoredResumePath))
            {
                ResumeImportResult parsed = ResumeProfileService.ParseExistingResume(draft.StoredResumePath, draft.StoredResumePath, draft.OriginalFileName);
                if (parsed != null && parsed.Document != null)
                    return parsed.Document;
            }

            return new ResumeProfileDocument();
        }

        private void PopulateForm(ResumeProfileDocument document)
        {
            txtAppResumeHeadline.Text = document.Headline ?? string.Empty;
            txtAppResumeSummary.Text = document.Summary ?? string.Empty;
            txtAppResumeSkills.Text = JoinLines(document.Skills);
            txtAppResumeEducation.Text = JoinLines(document.Education);
            txtAppResumeExperienceDetails.Text = JoinLines(document.Experience);
            txtAppResumeProjects.Text = JoinLines(document.Projects);
            txtAppResumeCertifications.Text = JoinLines(document.Certifications);
            txtAppResumeLanguages.Text = JoinLines(document.Languages);
        }

        private ResumeProfileDocument BuildDocumentFromForm()
        {
            return new ResumeProfileDocument
            {
                Headline = txtAppResumeHeadline.Text.Trim(),
                Summary = txtAppResumeSummary.Text.Trim(),
                Skills = SplitLines(txtAppResumeSkills.Text),
                Education = SplitLines(txtAppResumeEducation.Text),
                Experience = SplitLines(txtAppResumeExperienceDetails.Text),
                Projects = SplitLines(txtAppResumeProjects.Text),
                Certifications = SplitLines(txtAppResumeCertifications.Text),
                Languages = SplitLines(txtAppResumeLanguages.Text),
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

        private List<string> SplitLines(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new List<string>();

            return value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToList();
        }

        private string JoinLines(IEnumerable<string> lines)
        {
            if (lines == null)
                return string.Empty;

            return string.Join(Environment.NewLine, lines.Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Trim()));
        }

        private string GetJobInfo(int jobId)
        {
            using (SqlConnection con = new SqlConnection(str))
            using (SqlCommand cmd = new SqlCommand("SELECT Title, CompanyName FROM Jobs WHERE JobId = @JobId", con))
            {
                cmd.Parameters.AddWithValue("@JobId", jobId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return "Application resume draft";

                    return "For " + reader["Title"].ToString() + " at " + reader["CompanyName"].ToString();
                }
            }
        }
    }
}
