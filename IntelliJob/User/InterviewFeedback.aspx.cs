using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace IntelliJob.User
{
    public partial class InterviewFeedback : System.Web.UI.Page
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
                LoadFeedback();
            }
        }

        private void LoadFeedback()
        {
            int interviewId = Convert.ToInt32(Request.QueryString["id"]);
            int userId = Convert.ToInt32(Session["userId"]);

            using (SqlConnection con = new SqlConnection(str))
            {
                // Load interview + feedback
                string query = @"SELECT mi.Role, mi.Level, mi.InterviewType, mi.TechStack, mi.CreatedAt,
                                 mf.TotalScore, mf.CommunicationScore, mf.CommunicationComment,
                                 mf.TechnicalScore, mf.TechnicalComment,
                                 mf.ProblemSolvingScore, mf.ProblemSolvingComment,
                                 mf.CulturalFitScore, mf.CulturalFitComment,
                                 mf.ConfidenceScore, mf.ConfidenceComment,
                                 mf.Strengths, mf.AreasForImprovement, mf.FinalAssessment
                                 FROM Interviews mi
                                 INNER JOIN InterviewFeedback mf ON mi.InterviewId = mf.InterviewId
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

                        // Header info
                        litRole.Text = row["Role"].ToString();
                        litTotalScore.Text = row["TotalScore"].ToString();
                        litDate.Text = Convert.ToDateTime(row["CreatedAt"]).ToString("MMM d, yyyy h:mm tt");
                        litLevel.Text = row["Level"].ToString();
                        litType.Text = row["InterviewType"].ToString();
                        litFinalAssessment.Text = row["FinalAssessment"].ToString();

                        // Show regenerate button if feedback failed (score 0 or error in assessment)
                        int totalScore = Convert.ToInt32(row["TotalScore"]);
                        string assessment = row["FinalAssessment"].ToString();
                        if (totalScore == 0 || assessment.StartsWith("ERROR:"))
                        {
                            pnlRegenerate.Visible = true;
                            pnlRetake.Visible = true;
                        }

                        // Set interview ID for retake
                        hdnInterviewId.Value = interviewId.ToString();

                        // Score cards
                        litScoreCards.Text = BuildScoreCards(row);

                        // Strengths
                        string strengths = row["Strengths"].ToString();
                        if (!string.IsNullOrEmpty(strengths))
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (string s in strengths.Split('|'))
                            {
                                if (!string.IsNullOrWhiteSpace(s))
                                    sb.AppendFormat("<li>{0}</li>", s.Trim());
                            }
                            litStrengths.Text = sb.ToString();
                        }

                        // Areas for improvement
                        string areas = row["AreasForImprovement"].ToString();
                        if (!string.IsNullOrEmpty(areas))
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (string s in areas.Split('|'))
                            {
                                if (!string.IsNullOrWhiteSpace(s))
                                    sb.AppendFormat("<li>{0}</li>", s.Trim());
                            }
                            litAreas.Text = sb.ToString();
                        }

                        // Set interview ID for retake
                        hdnInterviewId.Value = interviewId.ToString();
                    }
                }
            }
        }

        protected void btnRegenerate_Click(object sender, EventArgs e)
        {
            int interviewId = Convert.ToInt32(Request.QueryString["id"]);
            int userId = Convert.ToInt32(Session["userId"]);

            try
            {
                // Delete old failed feedback
                using (SqlConnection con = new SqlConnection(str))
                {
                    string delQuery = "DELETE FROM InterviewFeedback WHERE InterviewId = @InterviewId AND UserId = @UserId";
                    using (SqlCommand cmd = new SqlCommand(delQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@InterviewId", interviewId);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                // Load transcript
                var transcript = new List<TranscriptMessage>();
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
                                transcript.Add(new TranscriptMessage
                                {
                                    Role = reader["SpeakerRole"].ToString(),
                                    Content = reader["Content"].ToString()
                                });
                            }
                        }
                    }
                }

                if (transcript.Count == 0)
                {
                    SaveErrorAndReload(interviewId, userId, "No transcript found.");
                    return;
                }

                // Call Gemini (with retry built in)
                string level = "Mid-Level";
                using (SqlConnection con2 = new SqlConnection(str))
                {
                    string lvlQuery = "SELECT Level FROM Interviews WHERE InterviewId = @Id";
                    using (SqlCommand cmd2 = new SqlCommand(lvlQuery, con2))
                    {
                        cmd2.Parameters.AddWithValue("@Id", interviewId);
                        con2.Open();
                        object result = cmd2.ExecuteScalar();
                        if (result != null) level = result.ToString();
                    }
                }

                var gemini = new GeminiService();
                InterviewFeedbackResult feedback = System.Threading.Tasks.Task.Run(
                    () => gemini.GenerateFeedbackAsync(transcript, level)
                ).GetAwaiter().GetResult();

                // Save new feedback
                SaveFeedback(interviewId, userId, feedback);

                // Reload page to show new results
                Response.Redirect("InterviewFeedback.aspx?id=" + interviewId, false);
            }
            catch (AggregateException aex)
            {
                string msg = aex.InnerException != null ? aex.InnerException.Message : aex.Message;
                SaveErrorAndReload(interviewId, userId, msg);
            }
            catch (Exception ex)
            {
                SaveErrorAndReload(interviewId, userId, ex.Message);
            }
        }

        private void SaveErrorAndReload(int interviewId, int userId, string errorDetail)
        {
            var fb = new InterviewFeedbackResult
            {
                TotalScore = 0,
                CommunicationScore = 0, CommunicationComment = "Feedback generation failed.",
                TechnicalScore = 0, TechnicalComment = "Feedback generation failed.",
                ProblemSolvingScore = 0, ProblemSolvingComment = "Feedback generation failed.",
                CulturalFitScore = 0, CulturalFitComment = "Feedback generation failed.",
                ConfidenceScore = 0, ConfidenceComment = "Feedback generation failed.",
                Strengths = new List<string> { "Could not generate AI feedback" },
                AreasForImprovement = new List<string> { "Please try again later" },
                FinalAssessment = "ERROR: " + errorDetail
            };
            SaveFeedback(interviewId, userId, fb);
            Response.Redirect("InterviewFeedback.aspx?id=" + interviewId, false);
        }

        private void SaveFeedback(int interviewId, int userId, InterviewFeedbackResult fb)
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

        private string BuildScoreCards(DataRow row)
        {
            StringBuilder sb = new StringBuilder();

            var categories = new[]
            {
                new { Name = "Communication Skills", Score = "CommunicationScore", Comment = "CommunicationComment", Index = 1 },
                new { Name = "Technical Knowledge", Score = "TechnicalScore", Comment = "TechnicalComment", Index = 2 },
                new { Name = "Problem Solving", Score = "ProblemSolvingScore", Comment = "ProblemSolvingComment", Index = 3 },
                new { Name = "Cultural & Role Fit", Score = "CulturalFitScore", Comment = "CulturalFitComment", Index = 4 },
                new { Name = "Confidence & Clarity", Score = "ConfidenceScore", Comment = "ConfidenceComment", Index = 5 },
            };

            foreach (var cat in categories)
            {
                int score = Convert.ToInt32(row[cat.Score]);
                string comment = row[cat.Comment].ToString();
                string level = score >= 70 ? "high" : score >= 40 ? "mid" : "low";

                sb.AppendFormat(@"
                <div class='score-card'>
                    <div class='score-header'>
                        <h5>{0}. {1}</h5>
                        <span class='score-value {2}'>{3}/100</span>
                    </div>
                    <div class='score-bar'>
                        <div class='fill {2}' data-score='{3}' style='width: 0%;'></div>
                    </div>
                    <p class='score-comment'>{4}</p>
                </div>", cat.Index, cat.Name, level, score, comment);
            }

            return sb.ToString();
        }
    }
}
