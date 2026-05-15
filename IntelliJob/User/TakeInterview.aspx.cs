using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;

namespace IntelliJob.User
{
    /// <summary>
    /// Interview session page.
    /// Works for BOTH the new job-linked flow (reached via InterviewAccess.aspx)
    /// AND the old self-service practice flow (reached via Interview.aspx).
    ///
    /// The Vapi voice agent is initialised with the interview's questions so
    /// it can conduct the full conversation; the transcript is saved via
    /// SaveVoiceTranscript.ashx, which then triggers AI feedback generation.
    /// </summary>
    public partial class TakeInterview : System.Web.UI.Page
    {
        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["user"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (Request.QueryString["id"] == null)
            {
                Response.Redirect("Interview.aspx");
                return;
            }

            if (!IsPostBack)
                LoadInterview();
        }

        private void LoadInterview()
        {
            int interviewId = Convert.ToInt32(Request.QueryString["id"]);
            int userId = Convert.ToInt32(Session["userId"]);

            using (SqlConnection con = new SqlConnection(str))
            {
                // ── Load interview + username ─────────────────────────────────
                string query = @"
                    SELECT i.*, u.Username
                    FROM   Interviews i
                    INNER JOIN Users  u ON i.UserId = u.UserId
                    WHERE  i.InterviewId = @InterviewId AND i.UserId = @UserId";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@InterviewId", interviewId);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);

                        if (dt.Rows.Count == 0)
                        {
                            Response.Redirect("Interview.aspx");
                            return;
                        }

                        DataRow row = dt.Rows[0];

                        if (row["Status"].ToString().ToLower() == "completed")
                        {
                            Response.Redirect("InterviewFeedback.aspx?id=" + interviewId);
                            return;
                        }
                        if (row["Status"].ToString().ToLower() == "cancelled")
                        {
                            Response.Redirect("Interview.aspx");
                            return;
                        }

                        // Block direct URL access for company-assigned interviews
                        // whose one-time password has already been consumed.
                        // Exception: if InterviewAccess.aspx just authenticated this
                        // session it stamps AuthorizedInterviewId — we honour that
                        // for exactly one entry and then clear it.
                        using (SqlConnection invCon = new SqlConnection(str))
                        {
                            string checkInvSql = @"SELECT IsPasswordUsed FROM InterviewInvitations
                                                   WHERE InterviewId = @IId";
                            using (SqlCommand invCmd = new SqlCommand(checkInvSql, invCon))
                            {
                                invCmd.Parameters.AddWithValue("@IId", interviewId);
                                invCon.Open();
                                object invResult = invCmd.ExecuteScalar();
                                if (invResult != null && invResult != DBNull.Value)
                                {
                                    bool pwdUsed = Convert.ToBoolean(invResult);
                                    if (pwdUsed)
                                    {
                                        // Check if InterviewAccess.aspx just granted access
                                        object authorizedId = Session["AuthorizedInterviewId"];
                                        if (authorizedId != null && Convert.ToInt32(authorizedId) == interviewId)
                                        {
                                            // Consume the stamp — valid for one pass only
                                            Session.Remove("AuthorizedInterviewId");
                                        }
                                        else
                                        {
                                            // No valid stamp — block re-entry
                                            Response.Redirect("Interview.aspx?err=access_revoked");
                                            return;
                                        }
                                    }
                                }
                            }
                        }

                        litRole.Text = row["Role"].ToString();
                        litLevel.Text = row["Level"].ToString();
                        litType.Text = row["InterviewType"].ToString();
                        litTechStack.Text = string.IsNullOrEmpty(row["TechStack"].ToString())
                                           ? "General" : row["TechStack"].ToString();
                        litUserName.Text = row["Username"].ToString();
                        hdnInterviewId.Value = interviewId.ToString();

                        string username = row["Username"].ToString();
                        litUserAvatar.Text = "<span style='font-size:40px;color:#636e72;'>" +
                                            username.Substring(0, 1).ToUpper() + "</span>";

                        // ── Expose Vapi token to client script ───────────────
                        string vapiToken = ConfigurationManager.AppSettings["VapiWebToken"] ?? "";
                        hdnVapiToken.Value = vapiToken;

                        UpdateInterviewStatus(interviewId, "in-progress");
                    }
                }

                // ── Load questions for voice agent ────────────────────────────
                string qQuery = @"
                    SELECT QuestionText
                    FROM   InterviewQuestions
                    WHERE  InterviewId = @InterviewId
                    ORDER  BY SortOrder";

                using (SqlCommand cmd = new SqlCommand(qQuery, con))
                {
                    cmd.Parameters.AddWithValue("@InterviewId", interviewId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dtQ = new DataTable();
                        sda.Fill(dtQ);

                        var questionTexts = new List<string>();
                        foreach (DataRow qRow in dtQ.Rows)
                            questionTexts.Add(qRow["QuestionText"].ToString());

                        hdnQuestionsJson.Value = JsonConvert.SerializeObject(questionTexts);
                    }
                }
            }
        }

        private void UpdateInterviewStatus(int interviewId, string status)
        {
            using (SqlConnection con = new SqlConnection(str))
            {
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Interviews SET Status = @Status WHERE InterviewId = @Id", con))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@Id", interviewId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void GenerateAIFeedback(int interviewId, int userId)
        {
            try
            {
                var transcript = LoadTranscript(interviewId);
                if (transcript.Count == 0)
                {
                    SaveErrorFeedback(interviewId, userId, "No transcript found.");
                    return;
                }

                string level = "Mid-Level";
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

                var gemini = new GeminiService();
                var feedback = System.Threading.Tasks.Task.Run(
                    () => gemini.GenerateFeedbackAsync(transcript, level)
                ).GetAwaiter().GetResult();

                SaveFeedbackToDb(interviewId, userId, feedback);
            }
            catch (AggregateException aex)
            {
                string msg = aex.InnerException?.Message ?? aex.Message;
                SaveErrorFeedback(interviewId, userId, "AI error: " + msg);
            }
            catch (Exception ex)
            {
                SaveErrorFeedback(interviewId, userId, ex.Message);
            }
        }

        private List<TranscriptMessage> LoadTranscript(int interviewId)
        {
            var messages = new List<TranscriptMessage>();
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
                            messages.Add(new TranscriptMessage
                            {
                                Role = rdr["SpeakerRole"].ToString(),
                                Content = rdr["Content"].ToString()
                            });
                        }
                    }
                }
            }
            return messages;
        }

        private void SaveFeedbackToDb(int interviewId, int userId, InterviewFeedbackResult fb)
        {
            // Resolve JobId for the feedback row (NULL for old practice interviews)
            int? jobId = null;
            using (SqlConnection con = new SqlConnection(str))
            {
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT JobId FROM Interviews WHERE InterviewId = @Id", con))
                {
                    cmd.Parameters.AddWithValue("@Id", interviewId);
                    con.Open();
                    object r = cmd.ExecuteScalar();
                    if (r != null && r != DBNull.Value) jobId = Convert.ToInt32(r);
                }
            }

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
                    cmd.Parameters.AddWithValue("@InterviewId", interviewId);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@JobId", jobId.HasValue ? (object)jobId.Value : DBNull.Value);
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

        private void SaveErrorFeedback(int interviewId, int userId, string detail)
        {
            SaveFeedbackToDb(interviewId, userId, new InterviewFeedbackResult
            {
                TotalScore = 0,
                CommunicationScore = 0,
                CommunicationComment = "Feedback failed.",
                TechnicalScore = 0,
                TechnicalComment = "Feedback failed.",
                ProblemSolvingScore = 0,
                ProblemSolvingComment = "Feedback failed.",
                CulturalFitScore = 0,
                CulturalFitComment = "Feedback failed.",
                ConfidenceScore = 0,
                ConfidenceComment = "Feedback failed.",
                Strengths = new List<string> { "Could not generate feedback" },
                AreasForImprovement = new List<string> { "Please use Regenerate on feedback page" },
                FinalAssessment = "ERROR: " + detail
            });
        }
    }
}
