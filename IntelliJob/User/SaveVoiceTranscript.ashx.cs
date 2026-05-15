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
    /// Receives the completed Vapi voice transcript, persists it, and triggers
    /// AI feedback generation.  Now also stamps JobId into InterviewFeedback
    /// so the company reporting query can find it.
    /// </summary>
    public class SaveVoiceTranscript : IHttpHandler, IRequiresSessionState
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
                string body;
                using (var reader = new StreamReader(context.Request.InputStream))
                    body = reader.ReadToEnd();

                var payload = JsonConvert.DeserializeObject<VoiceTranscriptPayload>(body);
                if (payload == null || payload.InterviewId <= 0 ||
                    payload.Messages == null || payload.Messages.Count == 0)
                {
                    context.Response.StatusCode = 400;
                    context.Response.Write("{\"error\":\"Invalid payload\"}");
                    return;
                }

                int userId = Convert.ToInt32(context.Session["userId"]);

                // ── Verify ownership ─────────────────────────────────────────
                int? jobId = null;
                using (SqlConnection con = new SqlConnection(str))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT UserId, JobId FROM Interviews WHERE InterviewId = @Id", con))
                    {
                        cmd.Parameters.AddWithValue("@Id", payload.InterviewId);
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (!rdr.Read() || Convert.ToInt32(rdr["UserId"]) != userId)
                            {
                                context.Response.StatusCode = 403;
                                context.Response.Write("{\"error\":\"Access denied\"}");
                                return;
                            }
                            if (rdr["JobId"] != DBNull.Value)
                                jobId = Convert.ToInt32(rdr["JobId"]);
                        }
                    }
                }

                // ── Clear old transcript ─────────────────────────────────────
                using (SqlConnection con = new SqlConnection(str))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(
                        "DELETE FROM InterviewTranscripts WHERE InterviewId = @Id", con))
                    {
                        cmd.Parameters.AddWithValue("@Id", payload.InterviewId);
                        cmd.ExecuteNonQuery();
                    }
                }

                // ── Save transcript ──────────────────────────────────────────
                using (SqlConnection con = new SqlConnection(str))
                {
                    con.Open();
                    foreach (var msg in payload.Messages)
                    {
                        using (SqlCommand cmd = new SqlCommand(
                            @"INSERT INTO InterviewTranscripts (InterviewId, SpeakerRole, Content)
                              VALUES (@Id, @Role, @Content)", con))
                        {
                            cmd.Parameters.AddWithValue("@Id",      payload.InterviewId);
                            cmd.Parameters.AddWithValue("@Role",    msg.Role ?? "user");
                            cmd.Parameters.AddWithValue("@Content", msg.Content ?? "");
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // ── Mark completed ───────────────────────────────────────────
                using (SqlConnection con = new SqlConnection(str))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(
                        @"UPDATE Interviews
                          SET Status = 'completed', CompletedAt = GETDATE()
                          WHERE InterviewId = @Id", con))
                    {
                        cmd.Parameters.AddWithValue("@Id", payload.InterviewId);
                        cmd.ExecuteNonQuery();
                    }
                }

                // ── Delete previous (failed) feedback ────────────────────────
                using (SqlConnection con = new SqlConnection(str))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(
                        "DELETE FROM InterviewFeedback WHERE InterviewId = @Id", con))
                    {
                        cmd.Parameters.AddWithValue("@Id", payload.InterviewId);
                        cmd.ExecuteNonQuery();
                    }
                }

                // ── Generate AI feedback ─────────────────────────────────────
                System.Threading.Thread.Sleep(2000);
                GenerateAIFeedback(payload.InterviewId, userId, jobId);

                context.Response.Write("{\"success\":true}");
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write("{\"error\":\"" + ex.Message.Replace("\"","'") + "\"}");
            }
        }

        private void GenerateAIFeedback(int interviewId, int userId, int? jobId)
        {
            try
            {
                string level = "Mid-Level";
                var    transcript = new List<TranscriptMessage>();

                using (SqlConnection con = new SqlConnection(str))
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT Level FROM Interviews WHERE InterviewId = @Id", con))
                    {
                        cmd.Parameters.AddWithValue("@Id", interviewId);
                        con.Open();
                        object r = cmd.ExecuteScalar();
                        if (r != null) level = r.ToString();
                    }
                }

                using (SqlConnection con = new SqlConnection(str))
                {
                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT SpeakerRole, Content FROM InterviewTranscripts
                          WHERE  InterviewId = @Id ORDER BY TranscriptId", con))
                    {
                        cmd.Parameters.AddWithValue("@Id", interviewId);
                        con.Open();
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                transcript.Add(new TranscriptMessage
                                {
                                    Role    = rdr["SpeakerRole"].ToString(),
                                    Content = rdr["Content"].ToString()
                                });
                            }
                        }
                    }
                }

                if (transcript.Count == 0) return;

                var gemini   = new GeminiService();
                var feedback = System.Threading.Tasks.Task.Run(
                    () => gemini.GenerateFeedbackAsync(transcript, level)
                ).GetAwaiter().GetResult();

                SaveFeedback(interviewId, userId, jobId, feedback);
            }
            catch (Exception feedbackEx)
            {
                string errorDetail = feedbackEx.InnerException != null
                    ? feedbackEx.InnerException.Message
                    : feedbackEx.Message;

                SaveFeedback(interviewId, userId, jobId, new InterviewFeedbackResult
                {
                    TotalScore = 0,
                    CommunicationScore = 0, CommunicationComment = "Feedback generation failed.",
                    TechnicalScore     = 0, TechnicalComment     = "Feedback generation failed.",
                    ProblemSolvingScore= 0, ProblemSolvingComment= "Feedback generation failed.",
                    CulturalFitScore   = 0, CulturalFitComment   = "Feedback generation failed.",
                    ConfidenceScore    = 0, ConfidenceComment    = "Feedback generation failed.",
                    Strengths           = new List<string> { "Voice transcript saved" },
                    AreasForImprovement = new List<string> { "Use Regenerate button on feedback page" },
                    FinalAssessment     = "ERROR: " + errorDetail
                });
            }
        }

        private void SaveFeedback(int interviewId, int userId, int? jobId, InterviewFeedbackResult fb)
        {
            using (SqlConnection con = new SqlConnection(str))
            {
                string sql = @"
                    INSERT INTO InterviewFeedback
                        (InterviewId, UserId, JobId, TotalScore,
                         CommunicationScore, CommunicationComment,
                         TechnicalScore, TechnicalComment,
                         ProblemSolvingScore, ProblemSolvingComment,
                         CulturalFitScore, CulturalFitComment,
                         ConfidenceScore, ConfidenceComment,
                         Strengths, AreasForImprovement, FinalAssessment)
                    VALUES
                        (@InterviewId, @UserId, @JobId, @TotalScore,
                         @CommScore, @CommComment,
                         @TechScore, @TechComment,
                         @ProblemScore, @ProblemComment,
                         @CulturalScore, @CulturalComment,
                         @ConfidenceScore, @ConfidenceComment,
                         @Strengths, @Areas, @FinalAssessment)";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@InterviewId",    interviewId);
                    cmd.Parameters.AddWithValue("@UserId",          userId);
                    cmd.Parameters.AddWithValue("@JobId",           jobId.HasValue ? (object)jobId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@TotalScore",      fb.TotalScore);
                    cmd.Parameters.AddWithValue("@CommScore",       fb.CommunicationScore);
                    cmd.Parameters.AddWithValue("@CommComment",     fb.CommunicationComment ?? "");
                    cmd.Parameters.AddWithValue("@TechScore",       fb.TechnicalScore);
                    cmd.Parameters.AddWithValue("@TechComment",     fb.TechnicalComment ?? "");
                    cmd.Parameters.AddWithValue("@ProblemScore",    fb.ProblemSolvingScore);
                    cmd.Parameters.AddWithValue("@ProblemComment",  fb.ProblemSolvingComment ?? "");
                    cmd.Parameters.AddWithValue("@CulturalScore",   fb.CulturalFitScore);
                    cmd.Parameters.AddWithValue("@CulturalComment", fb.CulturalFitComment ?? "");
                    cmd.Parameters.AddWithValue("@ConfidenceScore", fb.ConfidenceScore);
                    cmd.Parameters.AddWithValue("@ConfidenceComment",fb.ConfidenceComment ?? "");
                    cmd.Parameters.AddWithValue("@Strengths",       fb.Strengths != null ? string.Join("|", fb.Strengths) : "");
                    cmd.Parameters.AddWithValue("@Areas",           fb.AreasForImprovement != null ? string.Join("|", fb.AreasForImprovement) : "");
                    cmd.Parameters.AddWithValue("@FinalAssessment", fb.FinalAssessment ?? "");
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool IsReusable => false;

        // ── Payload models ───────────────────────────────────────────────────
        private class VoiceTranscriptPayload
        {
            public int              InterviewId { get; set; }
            public List<VoiceMessage> Messages  { get; set; }
        }

        private class VoiceMessage
        {
            public string Role    { get; set; }
            public string Content { get; set; }
        }
    }
}
