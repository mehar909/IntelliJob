using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace IntelliJob.Company
{
    /// <summary>
    /// Read-only report page for companies.
    /// Shows the full AI-generated feedback for a specific candidate's
    /// interview on one of the company's job postings.
    /// URL: ViewCandidateReport.aspx?interviewId=NNN
    /// </summary>
    public partial class ViewCandidateReport : System.Web.UI.Page
    {
        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Must be a Company user
            if (Session["userId"] == null || Session["role"]?.ToString() != "Company")
            {
                Response.Redirect("../User/Login.aspx");
                return;
            }

            if (Request.QueryString["interviewId"] == null)
            {
                Response.Redirect("JobList.aspx");
                return;
            }

            if (!IsPostBack)
                LoadReport();
        }

        private void LoadReport()
        {
            if (!int.TryParse(Request.QueryString["interviewId"], out int interviewId))
            {
                Response.Redirect("JobList.aspx");
                return;
            }

            int companyId = Convert.ToInt32(Session["userId"]);

            using (SqlConnection con = new SqlConnection(str))
            {
                // ── Verify this interview belongs to a job posted by this company ──
                string verifySql = @"
                    SELECT COUNT(1)
                    FROM   Interviews  i
                    INNER JOIN Jobs    j ON i.JobId = j.JobId
                    INNER JOIN Companies c ON c.CompanyId = @CompanyId
                    WHERE  i.InterviewId = @InterviewId
                      AND  j.CompanyName = c.CompanyName";

                using (SqlCommand cmd = new SqlCommand(verifySql, con))
                {
                    cmd.Parameters.AddWithValue("@InterviewId", interviewId);
                    cmd.Parameters.AddWithValue("@CompanyId",   companyId);
                    con.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (count == 0)
                    {
                        Response.Redirect("JobList.aspx");
                        return;
                    }
                    con.Close();
                }

                // ── Load interview + feedback + candidate info ──
                string sql = @"
                    SELECT
                        u.Username,
                        u.Email,
                        i.Role,
                        i.Level,
                        i.InterviewType,
                        i.TechStack,
                        i.CreatedAt,
                        i.CompletedAt,
                        f.TotalScore,
                        f.CommunicationScore,   f.CommunicationComment,
                        f.TechnicalScore,       f.TechnicalComment,
                        f.ProblemSolvingScore,  f.ProblemSolvingComment,
                        f.CulturalFitScore,     f.CulturalFitComment,
                        f.ConfidenceScore,      f.ConfidenceComment,
                        f.Strengths,
                        f.AreasForImprovement,
                        f.FinalAssessment
                    FROM  Interviews       i
                    INNER JOIN Users       u ON i.UserId      = u.UserId
                    INNER JOIN InterviewFeedback f ON i.InterviewId = f.InterviewId
                    WHERE  i.InterviewId = @InterviewId";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@InterviewId", interviewId);
                    con.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);

                        if (dt.Rows.Count == 0)
                        {
                            Response.Redirect("JobList.aspx");
                            return;
                        }

                        DataRow row = dt.Rows[0];

                        // Header
                        litCandidateName.Text  = row["Username"].ToString();
                        litCandidateEmail.Text = row["Email"].ToString();
                        litRole.Text           = row["Role"].ToString();
                        litLevel.Text          = row["Level"].ToString();
                        litType.Text           = row["InterviewType"].ToString();
                        litTechStack.Text      = row["TechStack"] == DBNull.Value ? "General" : row["TechStack"].ToString();
                        litDate.Text           = Convert.ToDateTime(row["CreatedAt"]).ToString("MMM d, yyyy h:mm tt");
                        litTotalScore.Text     = row["TotalScore"].ToString();
                        litFinalAssessment.Text = row["FinalAssessment"].ToString();

                        // Score cards
                        litScoreCards.Text = BuildScoreCards(row);

                        // Strengths
                        string strengths = row["Strengths"].ToString();
                        if (!string.IsNullOrEmpty(strengths))
                        {
                            var sb = new StringBuilder();
                            foreach (string s in strengths.Split('|'))
                                if (!string.IsNullOrWhiteSpace(s))
                                    sb.AppendFormat("<li>{0}</li>", s.Trim());
                            litStrengths.Text = sb.ToString();
                        }

                        // Areas for improvement
                        string areas = row["AreasForImprovement"].ToString();
                        if (!string.IsNullOrEmpty(areas))
                        {
                            var sb = new StringBuilder();
                            foreach (string s in areas.Split('|'))
                                if (!string.IsNullOrWhiteSpace(s))
                                    sb.AppendFormat("<li>{0}</li>", s.Trim());
                            litAreas.Text = sb.ToString();
                        }
                    }
                }
            }
        }

        private string BuildScoreCards(DataRow row)
        {
            var sb = new StringBuilder();
            var cats = new[]
            {
                new { Name="Communication Skills", Score="CommunicationScore",  Comment="CommunicationComment",  Idx=1 },
                new { Name="Technical Knowledge",  Score="TechnicalScore",       Comment="TechnicalComment",      Idx=2 },
                new { Name="Problem Solving",      Score="ProblemSolvingScore",  Comment="ProblemSolvingComment", Idx=3 },
                new { Name="Cultural & Role Fit",  Score="CulturalFitScore",     Comment="CulturalFitComment",    Idx=4 },
                new { Name="Confidence & Clarity", Score="ConfidenceScore",      Comment="ConfidenceComment",     Idx=5 },
            };

            foreach (var cat in cats)
            {
                int    score   = Convert.ToInt32(row[cat.Score]);
                string comment = row[cat.Comment].ToString();
                string level   = score >= 70 ? "high" : score >= 40 ? "mid" : "low";

                sb.AppendFormat(@"
                <div class='score-card'>
                    <div class='sc-top'>
                        <span class='sc-index'>{0}</span>
                        <span class='sc-name'>{1}</span>
                        <span class='sc-badge {2}'>{3}/100</span>
                    </div>
                    <div class='sc-bar-wrap'>
                        <div class='sc-bar-fill {2}' data-score='{3}' style='width:0%;'></div>
                    </div>
                    <p class='sc-comment'>{4}</p>
                </div>", cat.Idx, cat.Name, level, score, comment);
            }

            return sb.ToString();
        }
    }
}
