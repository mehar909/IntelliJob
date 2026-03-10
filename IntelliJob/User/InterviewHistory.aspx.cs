using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace IntelliJob.User
{
    public partial class InterviewHistory : System.Web.UI.Page
    {
        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["user"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadStats();
                LoadInterviews();
            }
        }

        private void LoadStats()
        {
            int userId = Convert.ToInt32(Session["userId"]);
            using (SqlConnection con = new SqlConnection(str))
            {
                con.Open();

                // Total interviews
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Interviews WHERE UserId = @UserId", con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    litTotalInterviews.Text = cmd.ExecuteScalar().ToString();
                }

                // Completed
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Interviews WHERE UserId = @UserId AND Status = 'completed'", con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    litCompletedCount.Text = cmd.ExecuteScalar().ToString();
                }

                // Avg score
                using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(AVG(TotalScore), 0) FROM InterviewFeedback WHERE UserId = @UserId", con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    litAvgScore.Text = cmd.ExecuteScalar().ToString();
                }

                // Best score
                using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(MAX(TotalScore), 0) FROM InterviewFeedback WHERE UserId = @UserId", con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    litBestScore.Text = cmd.ExecuteScalar().ToString();
                }
            }
        }

        private void LoadInterviews()
        {
            int userId = Convert.ToInt32(Session["userId"]);
            using (SqlConnection con = new SqlConnection(str))
            {
                string query = @"SELECT mi.InterviewId, mi.Role, mi.Level, mi.InterviewType, mi.TechStack, 
                                 mi.Status, mi.CreatedAt,
                                 ISNULL(mf.TotalScore, -1) as TotalScore
                                 FROM Interviews mi
                                 LEFT JOIN InterviewFeedback mf ON mi.InterviewId = mf.InterviewId
                                 WHERE mi.UserId = @UserId
                                 ORDER BY mi.CreatedAt DESC";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        rptInterviews.DataSource = dt;
                        rptInterviews.DataBind();

                        if (dt.Rows.Count == 0)
                        {
                            litEmpty.Text = @"<div class='empty-state'>
                                <i class='fas fa-microphone-alt'></i>
                                <p>You haven't taken any interviews yet.</p>
                                <a href='Interview.aspx'>Start Your First Interview</a>
                            </div>";
                            litEmpty.Visible = true;
                        }
                    }
                }
            }
        }

        public string GetScoreBadge(object score)
        {
            if (score == DBNull.Value || score == null) return "";
            int s = Convert.ToInt32(score);
            if (s < 0) return "<span style='color:#b2bec3;'>—</span>";
            string css = s >= 70 ? "score-high" : s >= 40 ? "score-mid" : "score-low";
            return "<span class='score-badge " + css + "'>" + s + "/100</span>";
        }

        public string GetTechStackDisplay(object techStack)
        {
            if (techStack == DBNull.Value || techStack == null || string.IsNullOrEmpty(techStack.ToString()))
                return "<span style='color:#b2bec3; font-size:13px;'>General</span>";

            string ts = techStack.ToString();
            string[] items = ts.Split(',');
            if (items.Length <= 2) return "<span style='color:#636e72; font-size:13px;'>" + ts + "</span>";
            return "<span style='color:#636e72; font-size:13px;'>" + items[0].Trim() + ", " + items[1].Trim() + " +" + (items.Length - 2) + "</span>";
        }

        public string GetActionLink(object interviewId, object status)
        {
            string s = status.ToString().ToLower();
            if (s == "completed")
                return "InterviewFeedback.aspx?id=" + interviewId;
            else if (s == "cancelled")
                return "javascript:void(0)";
            else
                return "TakeInterview.aspx?id=" + interviewId;
        }

        public string GetActionText(object status)
        {
            string s = status.ToString().ToLower();
            if (s == "completed") return "<i class='fas fa-chart-bar'></i> View Feedback";
            if (s == "in-progress") return "<i class='fas fa-play'></i> Continue";
            if (s == "cancelled") return "<i class='fas fa-times'></i> Cancelled";
            return "<i class='fas fa-play'></i> Start";
        }
    }
}
