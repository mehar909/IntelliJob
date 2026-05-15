using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace IntelliJob.User
{
    public partial class JobApplications : Page
    {
        private readonly string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["user"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
                LoadApplications();
        }

        private void LoadApplications()
        {
            int userId = Convert.ToInt32(Session["userId"]);
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(str))
            {
                string query = @"SELECT aj.AppliedJobId, aj.JobId, aj.Shortlisted,
                                        COALESCE(
                                            (SELECT TOP 1 ii.CreatedAt FROM InterviewInvitations ii WHERE ii.AppliedJobId = aj.AppliedJobId ORDER BY ii.CreatedAt DESC),
                                            (SELECT TOP 1 i.CreatedAt FROM Interviews i WHERE i.AppliedJobId = aj.AppliedJobId ORDER BY i.CreatedAt DESC)
                                        ) AS AppliedAt,
                                        j.Title, j.CompanyName, j.JobType, j.Country, j.State, j.CompanyImage,
                                        ISNULL((SELECT COUNT(*) FROM Interviews i WHERE i.AppliedJobId = aj.AppliedJobId AND i.UserId = aj.UserId), 0) AS InterviewCount,
                                        (SELECT TOP 1 i.InterviewId FROM Interviews i INNER JOIN InterviewFeedback fb ON fb.InterviewId = i.InterviewId WHERE i.AppliedJobId = aj.AppliedJobId AND i.UserId = aj.UserId) AS EvaluatedInterviewId
                                 FROM AppliedJobs aj
                                 INNER JOIN Jobs j ON aj.JobId = j.JobId
                                 WHERE aj.UserId = @UserId
                                 ORDER BY aj.AppliedJobId DESC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(dt);
                    }
                }
            }

            if (dt.Rows.Count == 0)
            {
                litEmpty.Text = @"<div class='empty-state'>
                    <i class='fas fa-folder-open'></i>
                    <p>You have not applied for any jobs yet.</p>
                    <a href='JobListing.aspx' class='btn-action'>Browse Jobs</a>
                </div>";
                return;
            }

            dt.Columns.Add("ResumeSource", typeof(string));
            dt.Columns.Add("ApplicationDate", typeof(string));
            dt.Columns.Add("ShowInterviewFeedbackButton", typeof(string));
            dt.Columns.Add("ShowResumeFeedbackButton", typeof(string));

            foreach (DataRow row in dt.Rows)
            {
                int appliedJobId = Convert.ToInt32(row["AppliedJobId"]);
                ResumeEnhancementReportRecord report;
                bool hasResumeReport = ApplicationDataStore.TryGetResumeEnhancementReport(userId, appliedJobId, out report) && report != null;
                row["ResumeSource"] = hasResumeReport && !string.IsNullOrWhiteSpace(report.ResumeSource) ? "Attached With Application" : "Profile Resume";
                row["ApplicationDate"] = row["AppliedAt"] != DBNull.Value ? Convert.ToDateTime(row["AppliedAt"]).ToString("MMM d, yyyy") : "N/A";

                // Interview Feedback button
                if (row["EvaluatedInterviewId"] != DBNull.Value && Convert.ToInt32(row["EvaluatedInterviewId"]) > 0)
                {
                    int interviewId = Convert.ToInt32(row["EvaluatedInterviewId"]);
                    row["ShowInterviewFeedbackButton"] = "<a href='InterviewFeedback.aspx?id=" + interviewId + "' class='btn-action' style='margin-bottom:8px;margin-right:8px;'><i class='fas fa-comments'></i> Interview Feedback</a>";
                }
                else
                {
                    row["ShowInterviewFeedbackButton"] = string.Empty;
                }

                // Resume Feedback button
                if (hasResumeReport)
                {
                    row["ShowResumeFeedbackButton"] = "<a href='ResumeEnhancer.aspx?applicationId=" + appliedJobId + "&history=1' class='btn-action' style='margin-bottom:8px;'><i class='fas fa-file-alt'></i> Resume Feedback</a>";
                }
                else
                {
                    row["ShowResumeFeedbackButton"] = string.Empty;
                }
            }

            rptApplications.DataSource = dt;
            rptApplications.DataBind();
        }

        protected string GetImageUrl(object url)
        {
            if (url == null || url == DBNull.Value || string.IsNullOrEmpty(url.ToString()))
            {
                return ResolveUrl("~/Images/No_image.png");
            }

            string logoPath = url.ToString().Trim();
            if (string.IsNullOrWhiteSpace(logoPath))
            {
                return ResolveUrl("~/Images/No_image.png");
            }

            // If it's a full URL, return as-is
            if (logoPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || logoPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return logoPath;
            }

            // Normalize the path
            string normalizedPath = logoPath.StartsWith("~/", StringComparison.OrdinalIgnoreCase)
                ? logoPath
                : "~/" + logoPath.TrimStart('~', '/');

            // Try multiple path candidates to find the file
            string[] candidates = new[]
            {
                normalizedPath,
                "~/Images/" + logoPath.TrimStart('~', '/'),
                "~/photos/" + logoPath.TrimStart('~', '/')
            };

            foreach (string candidate in candidates)
            {
                try
                {
                    if (System.IO.File.Exists(Server.MapPath(candidate)))
                    {
                        return ResolveUrl(candidate);
                    }
                }
                catch
                {
                }
            }

            return ResolveUrl("~/Images/No_image.png");
        }

        public string GetScoreBadge(object score)
        {
            if (score == null || score == DBNull.Value || string.IsNullOrWhiteSpace(score.ToString()))
                return "<span class='score-badge score-none'>No report</span>";

            int s;
            if (!int.TryParse(score.ToString(), out s))
                return "<span class='score-badge score-none'>No report</span>";

            string css = s >= 70 ? "score-high" : s >= 40 ? "score-mid" : "score-low";
            return "<span class='score-badge " + css + "'>" + s + "%</span>";
        }
    }
}
