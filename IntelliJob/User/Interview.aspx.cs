using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
                bool generated = GenerateQuestionsWithGemini(interviewId, role, level, interviewType, techStack, questionCount, previousQuestions);
                if (!generated)
                {
                    // Fallback to dummy questions if Gemini fails
                    InsertDummyQuestions(interviewId, role, interviewType, questionCount);
                }

                // Redirect to the interview page
                Response.Redirect("TakeInterview.aspx?id=" + interviewId);
            }
            catch (Exception ex)
            {
                lblMsg.Visible = true;
                lblMsg.Text = "Error creating interview. Please try again.";
                lblMsg.CssClass = "alert alert-danger";
            }
        }

        private bool GenerateQuestionsWithGemini(int interviewId, string role, string level, string type, string techStack, int count, List<string> previousQuestions = null)
        {
            try
            {
                var gemini = new GeminiService();
                // Use Task.Run to avoid ASP.NET synchronization context deadlock
                List<string> questions = System.Threading.Tasks.Task.Run(
                    () => gemini.GenerateQuestionsAsync(role, level, type, techStack, count, previousQuestions)
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
                string query = @"SELECT TOP 5 mi.InterviewId, mi.Role, mi.Level, mi.InterviewType, mi.Status, mi.CreatedAt,
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

        public string GetInterviewLink(object interviewId, object status)
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
            if (s == "completed") return "View Feedback";
            if (s == "in-progress") return "Continue";
            if (s == "cancelled") return "Cancelled";
            return "Start";
        }

        public string GetRetakeButton(object interviewId, object status)
        {
            string s = status.ToString().ToLower();
            if (s == "cancelled")
            {
                return "<a href='javascript:void(0)' onclick='retakeInterview(" + interviewId + ")' class='btn btn-sm' style='border:1px solid #00b894; color:#00b894; border-radius:6px; margin-left:5px;' title='Retake with same questions'><i class='fas fa-redo'></i> Retake</a>";
            }
            return "";
        }
    }
}
