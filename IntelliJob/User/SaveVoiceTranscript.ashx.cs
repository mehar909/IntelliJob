using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.SessionState;
using Newtonsoft.Json;

namespace IntelliJob.User
{
    /// <summary>
    /// Receives voice transcript from Vapi JS client, saves to DB, triggers AI feedback.
    /// </summary>
    public class SaveVoiceTranscript : IHttpHandler, IRequiresSessionState
    {
        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            // Auth check
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
                string body;
                using (var reader = new StreamReader(context.Request.InputStream))
                {
                    body = reader.ReadToEnd();
                }

                var payload = JsonConvert.DeserializeObject<VoiceTranscriptPayload>(body);
                if (payload == null || payload.InterviewId <= 0 || payload.Messages == null || payload.Messages.Count == 0)
                {
                    context.Response.StatusCode = 400;
                    context.Response.Write("{\"error\":\"Invalid payload\"}");
                    return;
                }

                int userId = Convert.ToInt32(context.Session["userId"]);

                // Verify user owns this interview
                using (SqlConnection con = new SqlConnection(str))
                {
                    con.Open();
                    string checkQuery = "SELECT UserId FROM Interviews WHERE InterviewId = @Id";
                    using (SqlCommand cmd = new SqlCommand(checkQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", payload.InterviewId);
                        object result = cmd.ExecuteScalar();
                        if (result == null || Convert.ToInt32(result) != userId)
                        {
                            context.Response.StatusCode = 403;
                            context.Response.Write("{\"error\":\"Access denied\"}");
                            return;
                        }
                    }
                }

                // Clear any existing transcript for this interview (in case of retry)
                using (SqlConnection con = new SqlConnection(str))
                {
                    con.Open();
                    string delQuery = "DELETE FROM InterviewTranscripts WHERE InterviewId = @Id";
                    using (SqlCommand cmd = new SqlCommand(delQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", payload.InterviewId);
                        cmd.ExecuteNonQuery();
                    }
                }

                // Save transcript messages
                using (SqlConnection con = new SqlConnection(str))
                {
                    con.Open();
                    foreach (var msg in payload.Messages)
                    {
                        string insertQuery = @"INSERT INTO InterviewTranscripts (InterviewId, SpeakerRole, Content)
                                               VALUES (@InterviewId, @Role, @Content)";
                        using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@InterviewId", payload.InterviewId);
                            cmd.Parameters.AddWithValue("@Role", msg.Role ?? "user");
                            cmd.Parameters.AddWithValue("@Content", msg.Content ?? "");
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // Update interview status to completed
                using (SqlConnection con = new SqlConnection(str))
                {
                    con.Open();
                    string updateQuery = @"UPDATE Interviews SET Status = 'completed', CompletedAt = GETDATE() 
                                           WHERE InterviewId = @Id";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", payload.InterviewId);
                        cmd.ExecuteNonQuery();
                    }
                }

                // Delete any previous failed feedback
                using (SqlConnection con = new SqlConnection(str))
                {
                    con.Open();
                    string delFb = "DELETE FROM InterviewFeedback WHERE InterviewId = @Id";
                    using (SqlCommand cmd = new SqlCommand(delFb, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", payload.InterviewId);
                        cmd.ExecuteNonQuery();
                    }
                }

                // Generate AI feedback  
                System.Threading.Thread.Sleep(3000); // Rate limit buffer
                GenerateAIFeedback(payload.InterviewId, userId);

                context.Response.Write("{\"success\":true}");
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write("{\"error\":\"" + ex.Message.Replace("\"", "'") + "\"}");
            }
        }

        private void GenerateAIFeedback(int interviewId, int userId)
        {
            try
            {
                var transcript = new List<TranscriptMessage>();
                string level = "Mid-Level";
                using (SqlConnection con = new SqlConnection(str))
                {
                    // Load level
                    string lvlQuery = "SELECT Level FROM Interviews WHERE InterviewId = @Id";
                    using (SqlCommand cmd = new SqlCommand(lvlQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", interviewId);
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null) level = result.ToString();
                    }
                }

                using (SqlConnection con = new SqlConnection(str))
                {
                    string query = @"SELECT SpeakerRole, Content FROM InterviewTranscripts 
                                     WHERE InterviewId = @Id ORDER BY TranscriptId";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", interviewId);
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                transcript.Add(new TranscriptMessage
                                {
                                    Role = reader["SpeakerRole"].ToString(),
                                    Content = reader["Content"].ToString()
                                });
                            }
                        }
                    }
                }

                if (transcript.Count == 0) return;

                var gemini = new GeminiService();
                InterviewFeedbackResult feedback = System.Threading.Tasks.Task.Run(
                    () => gemini.GenerateFeedbackAsync(transcript, level)
                ).GetAwaiter().GetResult();

                SaveFeedback(interviewId, userId, feedback);
            }
            catch (Exception)
            {
                // Save error feedback so user can use Regenerate button
                var fb = new InterviewFeedbackResult
                {
                    TotalScore = 0,
                    CommunicationScore = 0, CommunicationComment = "Feedback generation failed.",
                    TechnicalScore = 0, TechnicalComment = "Feedback generation failed.",
                    ProblemSolvingScore = 0, ProblemSolvingComment = "Feedback generation failed.",
                    CulturalFitScore = 0, CulturalFitComment = "Feedback generation failed.",
                    ConfidenceScore = 0, ConfidenceComment = "Feedback generation failed.",
                    Strengths = new List<string> { "Voice transcript saved successfully" },
                    AreasForImprovement = new List<string> { "Use Regenerate button on feedback page" },
                    FinalAssessment = "ERROR: AI feedback failed after voice interview. Use the Regenerate button."
                };
                SaveFeedback(interviewId, userId, fb);
            }
        }

        private void SaveFeedback(int interviewId, int userId, InterviewFeedbackResult fb)
        {
            using (SqlConnection con = new SqlConnection(str))
            {
                string query = @"INSERT INTO InterviewFeedback 
                    (InterviewId, UserId, TotalScore, 
                     CommunicationScore, CommunicationComment, TechnicalScore, TechnicalComment,
                     ProblemSolvingScore, ProblemSolvingComment, CulturalFitScore, CulturalFitComment,
                     ConfidenceScore, ConfidenceComment, Strengths, AreasForImprovement, FinalAssessment)
                    VALUES 
                    (@InterviewId, @UserId, @TotalScore,
                     @CommScore, @CommComment, @TechScore, @TechComment,
                     @ProblemScore, @ProblemComment, @CulturalScore, @CulturalComment,
                     @ConfidenceScore, @ConfidenceComment, @Strengths, @Areas, @FinalAssessment)";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@InterviewId", interviewId);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@TotalScore", fb.TotalScore);
                    cmd.Parameters.AddWithValue("@CommScore", fb.CommunicationScore);
                    cmd.Parameters.AddWithValue("@CommComment", fb.CommunicationComment ?? "");
                    cmd.Parameters.AddWithValue("@TechScore", fb.TechnicalScore);
                    cmd.Parameters.AddWithValue("@TechComment", fb.TechnicalComment ?? "");
                    cmd.Parameters.AddWithValue("@ProblemScore", fb.ProblemSolvingScore);
                    cmd.Parameters.AddWithValue("@ProblemComment", fb.ProblemSolvingComment ?? "");
                    cmd.Parameters.AddWithValue("@CulturalScore", fb.CulturalFitScore);
                    cmd.Parameters.AddWithValue("@CulturalComment", fb.CulturalFitComment ?? "");
                    cmd.Parameters.AddWithValue("@ConfidenceScore", fb.ConfidenceScore);
                    cmd.Parameters.AddWithValue("@ConfidenceComment", fb.ConfidenceComment ?? "");
                    cmd.Parameters.AddWithValue("@Strengths", fb.Strengths != null ? string.Join("|", fb.Strengths) : "");
                    cmd.Parameters.AddWithValue("@Areas", fb.AreasForImprovement != null ? string.Join("|", fb.AreasForImprovement) : "");
                    cmd.Parameters.AddWithValue("@FinalAssessment", fb.FinalAssessment ?? "");
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool IsReusable { get { return false; } }

        private class VoiceTranscriptPayload
        {
            public int InterviewId { get; set; }
            public List<VoiceMessage> Messages { get; set; }
        }

        private class VoiceMessage
        {
            public string Role { get; set; }
            public string Content { get; set; }
        }
    }
}
