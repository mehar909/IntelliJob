using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI.WebControls;

namespace IntelliJob.User
{
    public partial class Interview : System.Web.UI.Page
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
                LoadRecentInterviews();
            }
        }

        protected void btnStartInterview_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            string role = txtRole.Text.Trim();
            string level = ddlLevel.SelectedValue;
            string interviewType = ddlType.SelectedValue;
            string techStack = hdnTechStack.Value;
            int questionCount = Convert.ToInt32(ddlQuestionCount.SelectedValue);

            try
            {
                int interviewId = 0;
                using (SqlConnection con = new SqlConnection(str))
                {
                    // Insert the interview record
                    string query = @"INSERT INTO Interviews (UserId, Role, Level, InterviewType, TechStack, QuestionCount, Status)
                                     VALUES (@UserId, @Role, @Level, @InterviewType, @TechStack, @QuestionCount, 'pending');
                                     SELECT SCOPE_IDENTITY();";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@Role", role);
                        cmd.Parameters.AddWithValue("@Level", level);
                        cmd.Parameters.AddWithValue("@InterviewType", interviewType);
                        cmd.Parameters.AddWithValue("@TechStack", string.IsNullOrEmpty(techStack) ? (object)DBNull.Value : techStack);
                        cmd.Parameters.AddWithValue("@QuestionCount", questionCount);
                        con.Open();
                        interviewId = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }

                // Phase 2: Generate questions using Gemini AI
                // Load previously asked questions for this user to avoid repetition
                var previousQuestions = LoadPreviousQuestions(Convert.ToInt32(Session["userId"]), role);
                string resumeText = GetProfileResumeText(userId);
                bool generated = GenerateQuestionsWithGemini(interviewId, role, level, interviewType, techStack, questionCount, previousQuestions, resumeText);
                if (!generated)
                {
                    // Fallback to dummy questions if Gemini fails
                    InsertDummyQuestions(interviewId, role, interviewType, questionCount);
                }

                // Stay on this page, reload list and show success message
                LoadRecentInterviews();
                lblMsg.Visible = true;
                lblMsg.Text = "Interview created successfully! Click <strong>Start</strong> below to begin.";
                lblMsg.CssClass = "alert alert-success";

                // Clear form fields
                txtRole.Text = "";
                ddlLevel.SelectedIndex = 0;
                ddlType.SelectedIndex = 0;
                ddlQuestionCount.SelectedIndex = 1;
                hdnTechStack.Value = "";
            }
            catch (Exception ex)
            {
                lblMsg.Visible = true;
                lblMsg.Text = "Error creating interview. Please try again.";
                lblMsg.CssClass = "alert alert-danger";
            }
        }

        private bool GenerateQuestionsWithGemini(int interviewId, string role, string level, string type, string techStack, int count, List<string> previousQuestions = null, string resumeText = null)
        {
            try
            {
                var gemini = new GeminiService();
                // Use Task.Run to avoid ASP.NET synchronization context deadlock
                List<string> questions = System.Threading.Tasks.Task.Run(
                    () => gemini.GenerateQuestionsAsync(role, level, type, techStack, count, previousQuestions, resumeText)
                ).GetAwaiter().GetResult();

                if (questions == null || questions.Count == 0)
                    return false;

                using (SqlConnection con = new SqlConnection(str))
                {
                    con.Open();
                    for (int i = 0; i < questions.Count && i < count; i++)
                    {
                        string query = @"INSERT INTO InterviewQuestions (InterviewId, QuestionText, SortOrder)
                                         VALUES (@InterviewId, @QuestionText, @SortOrder)";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@InterviewId", interviewId);
                            cmd.Parameters.AddWithValue("@QuestionText", questions[i]);
                            cmd.Parameters.AddWithValue("@SortOrder", i + 1);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                // Gemini failed — fall back to dummy
                return false;
            }
        }

        private void InsertDummyQuestions(int interviewId, string role, string type, int count)
        {
            // Fallback: Hardcoded dummy questions if Gemini is unavailable
            string[] technicalQuestions = {
                "Can you explain the difference between an abstract class and an interface?",
                "What is dependency injection and why is it useful?",
                "How would you optimize a slow database query?",
                "Explain the concept of RESTful APIs and their key principles.",
                "What design patterns have you used in your projects and why?",
                "How do you handle error handling and logging in production applications?",
                "Explain the difference between synchronous and asynchronous programming.",
                "What is your approach to writing unit tests?",
                "How would you design a scalable microservices architecture?",
                "What security best practices do you follow in web development?"
            };

            string[] behavioralQuestions = {
                "Tell me about a time you had to deal with a difficult team member.",
                "Describe a situation where you had to meet a tight deadline.",
                "How do you handle constructive criticism?",
                "Tell me about a project you are most proud of and why.",
                "Describe a time when you had to learn a new technology quickly.",
                "How do you prioritize tasks when you have multiple deadlines?",
                "Tell me about a time you disagreed with your manager's decision.",
                "Describe a situation where you had to mentor a junior developer.",
                "How do you stay updated with the latest technology trends?",
                "Tell me about a time you failed and what you learned from it."
            };

            string[] mixedQuestions = {
                "Tell me about yourself and your technical background.",
                "What is your approach to solving complex technical problems?",
                "How do you handle disagreements in code reviews?",
                "Explain a complex technical concept to a non-technical person.",
                "What motivates you as a developer?",
                "How would you design a simple e-commerce system?",
                "Describe your experience working in agile teams.",
                "What is your approach to debugging production issues?",
                "How do you balance code quality with delivery speed?",
                "Where do you see yourself in 5 years?"
            };

            string[] questions;
            switch (type)
            {
                case "Technical": questions = technicalQuestions; break;
                case "Behavioral": questions = behavioralQuestions; break;
                default: questions = mixedQuestions; break;
            }

            using (SqlConnection con = new SqlConnection(str))
            {
                con.Open();
                for (int i = 0; i < count && i < questions.Length; i++)
                {
                    string query = @"INSERT INTO InterviewQuestions (InterviewId, QuestionText, SortOrder)
                                     VALUES (@InterviewId, @QuestionText, @SortOrder)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@InterviewId", interviewId);
                        cmd.Parameters.AddWithValue("@QuestionText", questions[i]);
                        cmd.Parameters.AddWithValue("@SortOrder", i + 1);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private void LoadRecentInterviews()
        {
            int userId = Convert.ToInt32(Session["userId"]);
            using (SqlConnection con = new SqlConnection(str))
            {
                // For user-created interviews, we only show the LATEST interview per
                // (Role, Level, InterviewType) group so that retakes replace the old
                // cancelled row instead of adding a new one.
                // Company interviews (those with an InterviewInvitations row) are always
                // shown individually regardless of duplicates.
                string query = @"SELECT TOP 5
                                 mi.InterviewId, mi.Role, mi.Level, mi.InterviewType, mi.Status, mi.CreatedAt,
                                 ISNULL(mf.TotalScore, -1) AS TotalScore,
                                 CASE WHEN ii.InterviewId IS NOT NULL THEN 1 ELSE 0 END AS IsCompanyInterview,
                                 ISNULL(ii.IsPasswordUsed, 0) AS IsPasswordUsed,
                                 ii.AccessToken,
                                 CASE
                                     WHEN ii.InterviewId IS NOT NULL
                                          AND ISNULL(ii.IsPasswordUsed,0) = 1
                                          AND mi.Status NOT IN ('completed','cancelled')
                                     THEN 'access-revoked'
                                     ELSE mi.Status
                                 END AS DisplayStatus
                                 FROM Interviews mi
                                 LEFT JOIN InterviewFeedback mf ON mi.InterviewId = mf.InterviewId
                                 LEFT JOIN InterviewInvitations ii ON mi.InterviewId = ii.InterviewId
                                 WHERE mi.UserId = @UserId
                                   AND (
                                                                                 -- Interviews created from job applications should always be shown.
                                                                                 mi.AppliedJobId IS NOT NULL
                                                                                 OR
                                         -- Always include company interviews
                                         ii.InterviewId IS NOT NULL
                                         OR
                                         -- For user interviews: only the latest per Role+Level+Type group
                                         mi.InterviewId = (
                                             SELECT TOP 1 sub.InterviewId
                                             FROM Interviews sub
                                             LEFT JOIN InterviewInvitations subii ON sub.InterviewId = subii.InterviewId
                                             WHERE sub.UserId = mi.UserId
                                               AND sub.Role = mi.Role
                                               AND sub.Level = mi.Level
                                               AND sub.InterviewType = mi.InterviewType
                                               AND subii.InterviewId IS NULL
                                             ORDER BY sub.CreatedAt DESC
                                         )
                                       )
                                 ORDER BY mi.CreatedAt DESC";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        rptRecentInterviews.DataSource = dt;
                        rptRecentInterviews.DataBind();

                        if (dt.Rows.Count == 0)
                        {
                            litNoInterviews.Text = @"<div style='text-align: center; padding: 40px; background: #f8f9fa; border-radius: 12px; border: 1px dashed #dee2e6;'>
                                <i class='fas fa-microphone-alt' style='font-size: 48px; color: #dee2e6; margin-bottom: 15px; display: block;'></i>
                                <p style='font-size: 16px; color: #636e72; margin: 0;'>No interviews yet. Create your first one above!</p>
                            </div>";
                            litNoInterviews.Visible = true;
                        }
                    }
                }
            }
        }

        private List<string> LoadPreviousQuestions(int userId, string role)
        {
            var questions = new List<string>();
            using (SqlConnection con = new SqlConnection(str))
            {
                // Get questions from the user's last 3 interviews for this role
                string query = @"SELECT TOP 30 q.QuestionText 
                                 FROM InterviewQuestions q
                                 INNER JOIN Interviews mi ON q.InterviewId = mi.InterviewId
                                 WHERE mi.UserId = @UserId AND mi.Role = @Role
                                 ORDER BY mi.CreatedAt DESC";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Role", role);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            questions.Add(reader["QuestionText"].ToString());
                    }
                }
            }
            return questions;
        }

        private string GetProfileResumeText(int userId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(str))
                {
                    string query = "SELECT ResumeStructuredJson, Resume FROM JobSeekers WHERE ProfileId = @UserId";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string structuredJson = reader["ResumeStructuredJson"] != DBNull.Value ? reader["ResumeStructuredJson"].ToString() : string.Empty;
                                if (!string.IsNullOrWhiteSpace(structuredJson))
                                {
                                    ResumeProfileDocument document = ResumeProfileService.DeserializeDocument(structuredJson);
                                    if (document != null)
                                        return ResumeProfileService.BuildResumeText(document);
                                }

                                string resumePath = reader["Resume"] != DBNull.Value ? reader["Resume"].ToString() : string.Empty;
                                if (!string.IsNullOrWhiteSpace(resumePath) && File.Exists(resumePath))
                                    return ResumeTextExtractor.ExtractText(resumePath);
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        public string GetTimeAgo(object date)
        {
            if (date == DBNull.Value || date == null) return string.Empty;
            DateTime postDate = Convert.ToDateTime(date);
            TimeSpan timeSpan = DateTime.Now.Subtract(postDate);
            if (timeSpan.TotalMinutes < 1) return "Just now";
            if (timeSpan.TotalHours < 1) return (int)timeSpan.TotalMinutes + " min ago";
            if (timeSpan.TotalDays < 1) return (int)timeSpan.TotalHours + " hrs ago";
            if (timeSpan.TotalDays < 30) return (int)timeSpan.TotalDays + " days ago";
            return postDate.ToString("MMM dd, yyyy");
        }

        public string GetScoreBadge(object score)
        {
            if (score == DBNull.Value || score == null) return "";
            int s = Convert.ToInt32(score);
            if (s < 0) return "";
            string css = s >= 70 ? "score-high" : s >= 40 ? "score-mid" : "score-low";
            return "<span class='score-badge " + css + "'>" + s + "/100</span>";
        }

        public string GetInterviewLink(object interviewId, object status, object isCompanyInterview, object isPasswordUsed, object accessToken)
        {
            string s = status.ToString().ToLower();
            if (s == "completed")
                return "InterviewFeedback.aspx?id=" + interviewId;
            if (s == "cancelled" || s == "access-revoked")
                return "javascript:void(0)";

            bool isCompany = Convert.ToInt32(isCompanyInterview) == 1;
            bool pwdUsed = Convert.ToBoolean(isPasswordUsed);

            if (isCompany && pwdUsed)
                return "javascript:void(0)";

            if (isCompany && !pwdUsed)
            {
                string token = accessToken?.ToString() ?? "";
                return "InterviewAccess.aspx?token=" + token;
            }

            return "TakeInterview.aspx?id=" + interviewId;
        }

        public string GetActionText(object status, object isCompanyInterview, object isPasswordUsed)
        {
            string s = status.ToString().ToLower();
            if (s == "completed") return "<i class='fas fa-chart-bar'></i> View Feedback";
            if (s == "cancelled") return "<i class='fas fa-times'></i>  Cancelled";
            if (s == "access-revoked") return "<i class='fas fa-times'></i> Access Revoked";

            bool isCompany = Convert.ToInt32(isCompanyInterview) == 1;
            bool pwdUsed = Convert.ToBoolean(isPasswordUsed);

            if (isCompany && pwdUsed) return "Access Revoked";
            if (s == "in-progress") return "<i class='fas fa-play'></i> Continue";
            return "<i class='fas fa-play'></i> Start";
        }

        public string GetRetakeButton(object interviewId, object status, object isCompanyInterview)
        {
            string s = status.ToString().ToLower();
            bool isCompany = Convert.ToInt32(isCompanyInterview) == 1;

            // Company interviews cannot be retaken — the one-time password is consumed
            // and the company controls re-invitation. Also hide for access-revoked.
            if (isCompany) return "";

            if (s == "cancelled")
            {
                return "<a href='javascript:void(0)' onclick='retakeInterview(" + interviewId + ")' class='btn-card-outline' title='Retake with same settings'><i class='fas fa-redo'></i> Retake</a>";
            }
            return "";
        }
    }
}
