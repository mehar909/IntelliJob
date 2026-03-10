using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Newtonsoft.Json;

namespace IntelliJob.User
{
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
            {
                LoadInterview();
            }
        }

        private void LoadInterview()
        {
            int interviewId = Convert.ToInt32(Request.QueryString["id"]);
            int userId = Convert.ToInt32(Session["userId"]);

            using (SqlConnection con = new SqlConnection(str))
            {
                // Load interview details
                string query = @"SELECT mi.*, u.Username FROM Interviews mi 
                                 INNER JOIN Users u ON mi.UserId = u.UserId
                                 WHERE mi.InterviewId = @InterviewId AND mi.UserId = @UserId";
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

                        // If already completed, redirect to feedback
                        if (row["Status"].ToString().ToLower() == "completed")
                        {
                            Response.Redirect("InterviewFeedback.aspx?id=" + interviewId);
                            return;
                        }

                        // If cancelled, redirect to interview page
                        if (row["Status"].ToString().ToLower() == "cancelled")
                        {
                            Response.Redirect("Interview.aspx");
                            return;
                        }

                        litRole.Text = row["Role"].ToString();
                        litLevel.Text = row["Level"].ToString();
                        litType.Text = row["InterviewType"].ToString();
                        litTechStack.Text = string.IsNullOrEmpty(row["TechStack"].ToString()) ? "General" : row["TechStack"].ToString();
                        litUserName.Text = row["Username"].ToString();
                        hdnInterviewId.Value = interviewId.ToString();

                        // User avatar - first letter
                        string username = row["Username"].ToString();
                        litUserAvatar.Text = "<span style='font-size:40px; color:#636e72;'>" + username.Substring(0, 1).ToUpper() + "</span>";

                        // Update status to in-progress
                        UpdateInterviewStatus(interviewId, "in-progress");
                    }
                }

                // Load questions
                string qQuery = @"SELECT QuestionId, QuestionText, SortOrder FROM InterviewQuestions 
                                  WHERE InterviewId = @InterviewId ORDER BY SortOrder";
                using (SqlCommand cmd = new SqlCommand(qQuery, con))
                {
                    cmd.Parameters.AddWithValue("@InterviewId", interviewId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dtQ = new DataTable();
                        sda.Fill(dtQ);
                        rptQuestions.DataSource = dtQ;
                        rptQuestions.DataBind();

                        // Build questions JSON array for voice mode
                        var questionTexts = new List<string>();
                        foreach (DataRow qRow in dtQ.Rows)
                        {
                            questionTexts.Add(qRow["QuestionText"].ToString());
                        }
                        hdnQuestionsJson.Value = JsonConvert.SerializeObject(questionTexts);
                    }
                }
            }
        }

        private void UpdateInterviewStatus(int interviewId, string status)
        {
            using (SqlConnection con = new SqlConnection(str))
            {
                string query = @"UPDATE Interviews SET Status = @Status WHERE InterviewId = @InterviewId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@InterviewId", interviewId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        protected void btnSubmitAnswers_Click(object sender, EventArgs e)
        {
            int interviewId = Convert.ToInt32(hdnInterviewId.Value);
            int userId = Convert.ToInt32(Session["userId"]);
            string answersJson = hdnAnswersJson.Value;

            // Parse answers and save as transcript
            if (!string.IsNullOrEmpty(answersJson))
            {
                var answers = JsonConvert.DeserializeObject<Dictionary<string, string>>(answersJson);
                SaveTranscriptFromAnswers(interviewId, answers);
            }

            // Update interview status
            UpdateInterviewStatus(interviewId, "completed");

            // Mark completion time
            using (SqlConnection con = new SqlConnection(str))
            {
                string query = @"UPDATE Interviews SET CompletedAt = GETDATE() WHERE InterviewId = @InterviewId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@InterviewId", interviewId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            // Phase 2: Generate AI feedback using Gemini (falls back to error feedback if Gemini fails)
            // Wait before calling Gemini to avoid back-to-back rate limit hits
            System.Threading.Thread.Sleep(3000);
            GenerateAIFeedback(interviewId, userId);

            // Redirect to feedback page
            Response.Redirect("InterviewFeedback.aspx?id=" + interviewId);
        }

        private void SaveTranscriptFromAnswers(int interviewId, Dictionary<string, string> answers)
        {
            // Load questions to pair with answers
            using (SqlConnection con = new SqlConnection(str))
            {
                con.Open();
                foreach (var kvp in answers)
                {
                    int questionId = Convert.ToInt32(kvp.Key);
                    string answer = kvp.Value;

                    // Get the question text
                    string qText = "";
                    string qQuery = "SELECT QuestionText FROM InterviewQuestions WHERE QuestionId = @QuestionId";
                    using (SqlCommand qCmd = new SqlCommand(qQuery, con))
                    {
                        qCmd.Parameters.AddWithValue("@QuestionId", questionId);
                        object result = qCmd.ExecuteScalar();
                        if (result != null) qText = result.ToString();
                    }

                    // Save assistant question as transcript
                    string insertQ = @"INSERT INTO InterviewTranscripts (InterviewId, SpeakerRole, Content)
                                       VALUES (@InterviewId, 'assistant', @Content)";
                    using (SqlCommand cmd = new SqlCommand(insertQ, con))
                    {
                        cmd.Parameters.AddWithValue("@InterviewId", interviewId);
                        cmd.Parameters.AddWithValue("@Content", qText);
                        cmd.ExecuteNonQuery();
                    }

                    // Save user answer as transcript
                    string insertA = @"INSERT INTO InterviewTranscripts (InterviewId, SpeakerRole, Content)
                                       VALUES (@InterviewId, 'user', @Content)";
                    using (SqlCommand cmd = new SqlCommand(insertA, con))
                    {
                        cmd.Parameters.AddWithValue("@InterviewId", interviewId);
                        cmd.Parameters.AddWithValue("@Content", answer);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private void GenerateAIFeedback(int interviewId, int userId)
        {
            try
            {
                // Load transcript from database
                var transcript = LoadTranscript(interviewId);
                if (transcript.Count == 0)
                {
                    SaveErrorFeedback(interviewId, userId, "No transcript found in database. Answers may not have been saved.");
                    return;
                }

                // Load interview level for scoring guidance
                string level = "Mid-Level";
                using (SqlConnection con = new SqlConnection(str))
                {
                    string lvlQuery = "SELECT Level FROM Interviews WHERE InterviewId = @Id";
                    using (SqlCommand cmd = new SqlCommand(lvlQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", interviewId);
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null) level = result.ToString();
                    }
                }

                var gemini = new GeminiService();
                // Use Task.Run to avoid ASP.NET synchronization context deadlock
                InterviewFeedbackResult feedback = System.Threading.Tasks.Task.Run(
                    () => gemini.GenerateFeedbackAsync(transcript, level)
                ).GetAwaiter().GetResult();

                SaveFeedbackToDb(interviewId, userId, feedback);
            }
            catch (AggregateException aex)
            {
                // Unwrap AggregateException from Task.Run
                string errorMsg = aex.InnerException != null ? aex.InnerException.Message : aex.Message;
                SaveErrorFeedback(interviewId, userId, "Gemini API error: " + errorMsg);
            }
            catch (Exception ex)
            {
                SaveErrorFeedback(interviewId, userId, "Feedback error: " + ex.Message);
            }
        }

        private List<TranscriptMessage> LoadTranscript(int interviewId)
        {
            var messages = new List<TranscriptMessage>();
            using (SqlConnection con = new SqlConnection(str))
            {
                string query = @"SELECT SpeakerRole, Content FROM InterviewTranscripts 
                                 WHERE InterviewId = @InterviewId ORDER BY TranscriptId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@InterviewId", interviewId);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add(new TranscriptMessage
                            {
                                Role = reader["SpeakerRole"].ToString(),
                                Content = reader["Content"].ToString()
                            });
                        }
                    }
                }
            }
            return messages;
        }

        private void SaveFeedbackToDb(int interviewId, int userId, InterviewFeedbackResult fb)
        {
            using (SqlConnection con = new SqlConnection(str))
            {
                string query = @"INSERT INTO InterviewFeedback 
                    (InterviewId, UserId, TotalScore, 
                     CommunicationScore, CommunicationComment,
                     TechnicalScore, TechnicalComment,
                     ProblemSolvingScore, ProblemSolvingComment,
                     CulturalFitScore, CulturalFitComment,
                     ConfidenceScore, ConfidenceComment,
                     Strengths, AreasForImprovement, FinalAssessment)
                    VALUES 
                    (@InterviewId, @UserId, @TotalScore,
                     @CommScore, @CommComment,
                     @TechScore, @TechComment,
                     @ProblemScore, @ProblemComment,
                     @CulturalScore, @CulturalComment,
                     @ConfidenceScore, @ConfidenceComment,
                     @Strengths, @Areas, @FinalAssessment)";

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

        private void SaveErrorFeedback(int interviewId, int userId, string errorDetail)
        {
            var fallback = new InterviewFeedbackResult
            {
                TotalScore = 0,
                CommunicationScore = 0,
                CommunicationComment = "Feedback generation failed. See details below.",
                TechnicalScore = 0,
                TechnicalComment = "Feedback generation failed. See details below.",
                ProblemSolvingScore = 0,
                ProblemSolvingComment = "Feedback generation failed. See details below.",
                CulturalFitScore = 0,
                CulturalFitComment = "Feedback generation failed. See details below.",
                ConfidenceScore = 0,
                ConfidenceComment = "Feedback generation failed. See details below.",
                Strengths = new List<string> { "Could not generate AI feedback" },
                AreasForImprovement = new List<string> { "Please retake the interview" },
                FinalAssessment = "ERROR: " + errorDetail + "\n\nPlease retake the interview. If this issue persists, check that your Gemini API key is valid in Web.config."
            };
            SaveFeedbackToDb(interviewId, userId, fallback);
        }
    }
}
