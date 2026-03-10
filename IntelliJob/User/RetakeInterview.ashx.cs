using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.SessionState;

namespace IntelliJob.User
{
    public class RetakeInterview : IHttpHandler, IRequiresSessionState
    {
        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            if (context.Session["userId"] == null)
            {
                context.Response.StatusCode = 401;
                context.Response.Write("{\"error\":\"Not authenticated\"}");
                return;
            }

            if (context.Request.HttpMethod != "POST")
            {
                context.Response.StatusCode = 405;
                context.Response.Write("{\"error\":\"POST only\"}");
                return;
            }

            try
            {
                string idStr = context.Request.Form["InterviewId"];
                if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out int interviewId))
                {
                    context.Response.StatusCode = 400;
                    context.Response.Write("{\"error\":\"Invalid InterviewId\"}");
                    return;
                }

                int userId = Convert.ToInt32(context.Session["userId"]);

                // Load original interview and verify ownership
                string role = "", level = "", interviewType = "", techStack = "";
                int questionCount = 0;
                string status = "";

                using (SqlConnection con = new SqlConnection(str))
                {
                    con.Open();
                    string query = "SELECT UserId, Role, Level, InterviewType, TechStack, QuestionCount, Status FROM Interviews WHERE InterviewId = @Id";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", interviewId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read() || Convert.ToInt32(reader["UserId"]) != userId)
                            {
                                context.Response.StatusCode = 403;
                                context.Response.Write("{\"error\":\"Access denied\"}");
                                return;
                            }
                            status = reader["Status"].ToString().ToLower();
                            role = reader["Role"].ToString();
                            level = reader["Level"].ToString();
                            interviewType = reader["InterviewType"].ToString();
                            techStack = reader["TechStack"] == DBNull.Value ? "" : reader["TechStack"].ToString();
                            questionCount = Convert.ToInt32(reader["QuestionCount"]);
                        }
                    }
                }

                // Only allow retake for cancelled or failed-feedback interviews
                // Completed interviews with valid transcript cannot be retaken
                if (status == "completed")
                {
                    // Check if feedback failed (TotalScore = 0 with ERROR)
                    bool feedbackFailed = false;
                    using (SqlConnection con = new SqlConnection(str))
                    {
                        con.Open();
                        string fbQuery = "SELECT TotalScore, FinalAssessment FROM InterviewFeedback WHERE InterviewId = @Id";
                        using (SqlCommand cmd = new SqlCommand(fbQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@Id", interviewId);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    int score = Convert.ToInt32(reader["TotalScore"]);
                                    string assessment = reader["FinalAssessment"].ToString();
                                    feedbackFailed = (score == 0 || assessment.StartsWith("ERROR:"));
                                }
                            }
                        }
                    }

                    if (!feedbackFailed)
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Write("{\"error\":\"Completed interviews with valid feedback cannot be retaken\"}");
                        return;
                    }
                }

                // Create new interview with same settings
                int newInterviewId = 0;
                using (SqlConnection con = new SqlConnection(str))
                {
                    con.Open();
                    string insertQuery = @"INSERT INTO Interviews (UserId, Role, Level, InterviewType, TechStack, QuestionCount, Status)
                                           VALUES (@UserId, @Role, @Level, @InterviewType, @TechStack, @QuestionCount, 'pending');
                                           SELECT SCOPE_IDENTITY();";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@Role", role);
                        cmd.Parameters.AddWithValue("@Level", level);
                        cmd.Parameters.AddWithValue("@InterviewType", interviewType);
                        cmd.Parameters.AddWithValue("@TechStack", string.IsNullOrEmpty(techStack) ? (object)DBNull.Value : techStack);
                        cmd.Parameters.AddWithValue("@QuestionCount", questionCount);
                        newInterviewId = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }

                // Copy questions from original interview
                using (SqlConnection con = new SqlConnection(str))
                {
                    con.Open();
                    string copyQuery = @"INSERT INTO InterviewQuestions (InterviewId, QuestionText, SortOrder)
                                         SELECT @NewId, QuestionText, SortOrder 
                                         FROM InterviewQuestions 
                                         WHERE InterviewId = @OldId 
                                         ORDER BY SortOrder";
                    using (SqlCommand cmd = new SqlCommand(copyQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@NewId", newInterviewId);
                        cmd.Parameters.AddWithValue("@OldId", interviewId);
                        cmd.ExecuteNonQuery();
                    }
                }

                context.Response.Write("{\"success\":true,\"newInterviewId\":" + newInterviewId + "}");
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write("{\"error\":\"" + ex.Message.Replace("\"", "'") + "\"}");
            }
        }

        public bool IsReusable { get { return false; } }
    }
}
