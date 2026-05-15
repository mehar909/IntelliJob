using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Net;
using System.Net.Mail;


namespace IntelliJob.Company
{
    public partial class ShortlistedCandidates : System.Web.UI.Page
    {

        SqlConnection con;
        SqlCommand cmd;
        DataTable dt;
        String str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["userId"] == null || Session["role"] == null || Session["role"].ToString() != "Company")
            {
                Response.Redirect("../User/Login.aspx");
            }

            if (!IsPostBack)
            {
                ShowAppliedJob();
            }
        }

        private void ShowAppliedJob()
        {
            string query = string.Empty;
            con = new SqlConnection(str);
            // Get the logged-in company's name
            string companyQuery = "SELECT CompanyName FROM Companies WHERE CompanyId = @id";
            SqlCommand cmdCompany = new SqlCommand(companyQuery, con);
            cmdCompany.Parameters.AddWithValue("@id", Session["userId"]);

            con.Open();
            string companyName = Convert.ToString(cmdCompany.ExecuteScalar());
            con.Close();

            if (string.IsNullOrEmpty(companyName))
            {
                lblMsg.Visible = true;
                lblMsg.Text = "Error: Company not found.";
                lblMsg.CssClass = "alert alert-danger";
                return;
            }
            query = @"
                        SELECT 
                        ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS [Sr.No],
                        aj.AppliedJobId,
                        aj.JobId,  
                        j.Title,
                        js.Name AS UserName,
                        u.Email,
                        js.Mobile,
                        js.Resume,
                        j.CompanyName
                    FROM AppliedJobs aj
                    INNER JOIN JobSeekers js ON aj.UserId = js.ProfileId
                    INNER JOIN Users u ON aj.UserId = u.UserId
                    INNER JOIN Jobs j ON aj.JobId = j.JobId
                    WHERE aj.Shortlisted = 'Yes'
                      AND j.CompanyName = @CompanyName";

            cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@CompanyName", companyName);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            dt = new DataTable();
            sda.Fill(dt);
            GridView1.DataSource = dt;
            GridView1.DataBind();
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;
            ShowAppliedJob();
        }

        protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                GridViewRow row = GridView1.Rows[e.RowIndex];
                int AppliedjobId = Convert.ToInt32(GridView1.DataKeys[e.RowIndex].Values[0]);
                con = new SqlConnection(str);
                cmd = new SqlCommand("UPDATE AppliedJobs SET Shortlisted = 'no' WHERE AppliedJobId = @id", con);
                cmd.Parameters.AddWithValue("@id", AppliedjobId);
                con.Open();
                int r = cmd.ExecuteNonQuery();
                if (r > 0)
                {
                    lblMsg.Text = "Candidate removed from shortlisted list successfully!";
                    lblMsg.CssClass = "alert alter-success";
                    lblMsg.Visible = true;
                    // Auto-hide message after 7 seconds
                    ClientScript.RegisterStartupScript(this.GetType(), "hideMessage",
                        "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 7000);", true);

                }
                else
                {
                    lblMsg.Text = "Cannot remove this candidate!";
                    lblMsg.CssClass = "alert alter-success";
                    lblMsg.Visible = true;
                    // Auto-hide message after 7 seconds
                    ClientScript.RegisterStartupScript(this.GetType(), "hideMessage",
                        "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 7000);", true);

                }
                GridView1.EditIndex = -1;
                ShowAppliedJob();
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Error removing candidate: " + ex.Message;
                lblMsg.CssClass = "alert alert-danger";
                lblMsg.Visible = true;
                Response.Write("<script>alert('" + ex.Message + "');</script>");
            }
            finally
            {
                if (con != null && con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            
        }

        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (GridViewRow row in GridView1.Rows)
            {
                if (row.RowIndex == GridView1.SelectedIndex)
                {
                    HiddenField jobId = (HiddenField)row.FindControl("hdnJobId");
                    Response.Redirect("JobList.aspx?id=" + jobId.Value);
                }
                else
                {
                    row.BackColor = ColorTranslator.FromHtml("#FFFFFF");
                    row.ToolTip = "Click to select this row";
                }
            }
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Company/ViewApplications.aspx");
        }

        private void SendInterviewEmail(GridViewRow row)
        {
            int appliedJobId = Convert.ToInt32(GridView1.DataKeys[row.RowIndex].Values["AppliedJobId"]);

            // Load candidate and job info
            string candidateName = "", candidateEmail = "", jobTitle = "", companyName = "";
            using (SqlConnection c = new SqlConnection(str))
            {
                string q = @"SELECT js.Name, u.Email, j.Title, j.CompanyName
                             FROM AppliedJobs aj
                             INNER JOIN Jobs j ON aj.JobId = j.JobId
                             INNER JOIN Users u ON aj.UserId = u.UserId
                             LEFT JOIN JobSeekers js ON aj.UserId = js.ProfileId
                             WHERE aj.AppliedJobId = @Id";
                using (SqlCommand cmd = new SqlCommand(q, c))
                {
                    cmd.Parameters.AddWithValue("@Id", appliedJobId);
                    c.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            candidateName = rdr["Name"] == DBNull.Value ? "" : rdr["Name"].ToString();
                            if (string.IsNullOrWhiteSpace(candidateName)) candidateName = rdr["Email"].ToString().Split('@')[0];
                            candidateEmail = rdr["Email"].ToString();
                            jobTitle = rdr["Title"].ToString();
                            companyName = rdr["CompanyName"].ToString();
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(candidateEmail))
            {
                lblMsg.Text = "Could not find candidate email.";
                lblMsg.CssClass = "alert alert-danger";
                lblMsg.Visible = true;
                return;
            }

            try
            {
                string smtpUser = System.Configuration.ConfigurationManager.AppSettings["SmtpUser"] ?? "";
                string smtpPass = System.Configuration.ConfigurationManager.AppSettings["SmtpPass"] ?? "";
                string smtpHost = System.Configuration.ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
                int smtpPort = int.Parse(System.Configuration.ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
                string smtpFrom = System.Configuration.ConfigurationManager.AppSettings["SmtpFrom"] ?? smtpUser;

                if (string.IsNullOrEmpty(smtpPass))
                {
                    lblMsg.Text = "SMTP password is missing. Set SmtpPass in Web.config appSettings.";
                    lblMsg.CssClass = "alert alert-danger";
                    lblMsg.Visible = true;
                    return;
                }

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(smtpFrom, $"HR Manager - {companyName}");
                mail.To.Add(candidateEmail);
                mail.Subject = $"Congratulations! You've been shortlisted for Round 2 – {jobTitle} at {companyName}";
                mail.Body =
                    $"Dear {candidateName},\n\n" +
                    $"We are pleased to inform you that after reviewing your AI interview performance for the {jobTitle} position at {companyName}, " +
                    "you have been selected to move forward to the second round of our hiring process.\n\n" +
                    "Our team was impressed with your responses and believes you could be a great fit for our team.\n\n" +
                    "Our HR team will be in touch shortly with further details regarding the next steps, including the date, time, and format of the second round.\n\n" +
                    "In the meantime, if you have any questions, please feel free to reach out.\n\n" +
                    $"Congratulations once again, and we look forward to speaking with you!\n\n" +
                    $"Best Regards,\nHiring Manager @ {companyName}\n";
                mail.IsBodyHtml = false;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = smtpHost;
                smtp.Port = smtpPort;
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
                smtp.Send(mail);

                lblMsg.Text = $"Shortlist email sent to {candidateName} ({candidateEmail}) for second round.";
                lblMsg.CssClass = "alert alert-success";
                lblMsg.Visible = true;
                ClientScript.RegisterStartupScript(this.GetType(), "hideMessage",
                    "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 7000);", true);
            }
            catch (Exception ex)
            {
                lblMsg.Text = $"Error sending email to {candidateName} ({candidateEmail}): {ex.Message}";
                lblMsg.CssClass = "alert alert-danger";
                lblMsg.Visible = true;
            }
        }

        private InterviewInvitationEmailData PrepareInterviewInvitation(int appliedJobId, int companyId)
        {
            ApplicationInterviewContext context = LoadApplicationInterviewContext(appliedJobId, companyId);
            if (context == null)
            {
                return null;
            }

            List<string> previousQuestions = LoadPreviousQuestions(context.UserId, context.JobTitle);
            
            string resumeText = null;
            ApplicationResumeSelection selection;
            if (ApplicationDataStore.TryGetApplicationResumeSelection(context.UserId, context.AppliedJobId, out selection))
            {
                if (!string.IsNullOrWhiteSpace(selection.StructuredJson))
                {
                    ResumeProfileDocument document = ResumeProfileService.DeserializeDocument(selection.StructuredJson);
                    if (document != null)
                        resumeText = ResumeProfileService.BuildResumeText(document);
                }
                else if (System.IO.File.Exists(selection.StoredResumePath))
                {
                    resumeText = ResumeTextExtractor.ExtractText(selection.StoredResumePath);
                }
            }

            List<string> questions = GenerateInterviewQuestions(context, previousQuestions, resumeText);
            if (questions == null || questions.Count == 0)
            {
                questions = BuildFallbackQuestions(context.JobTitle, context.JobType, context.QuestionCount);
            }

            int interviewId = UpsertInterviewAndInvitation(context, questions, out string plainPassword, out Guid accessToken);
            if (interviewId <= 0)
            {
                return null;
            }

            string accessUrl = Request.Url.GetLeftPart(UriPartial.Authority) + ResolveUrl("~/User/InterviewAccess.aspx?token=" + accessToken);
            return new InterviewInvitationEmailData
            {
                CandidateName = context.CandidateName,
                CandidateEmail = context.CandidateEmail,
                CompanyName = context.CompanyName,
                JobTitle = context.JobTitle,
                AccessUrl = accessUrl,
                OneTimePassword = plainPassword
            };
        }

        private ApplicationInterviewContext LoadApplicationInterviewContext(int appliedJobId, int companyId)
        {
            using (SqlConnection con = new SqlConnection(str))
            {
                string query = @"SELECT aj.AppliedJobId, aj.JobId, aj.UserId,
                                        j.Title, j.Experience AS JobExperience, j.Specialization, j.Description, j.Qualification, j.JobType, j.CompanyName,
                                        u.Email, u.Username,
                                        js.Name, js.Resume
                                 FROM AppliedJobs aj
                                 INNER JOIN Jobs j ON aj.JobId = j.JobId
                                 INNER JOIN Users u ON aj.UserId = u.UserId
                                 LEFT JOIN JobSeekers js ON aj.UserId = js.ProfileId
                                 WHERE aj.AppliedJobId = @AppliedJobId
                                   AND aj.Shortlisted = 'Yes'
                                   AND j.CompanyName = (SELECT CompanyName FROM Companies WHERE CompanyId = @CompanyId)";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@AppliedJobId", appliedJobId);
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        if (dt.Rows.Count == 0)
                        {
                            lblMsg.Text = "Could not load candidate/job context for interview invitation.";
                            lblMsg.CssClass = "alert alert-danger";
                            lblMsg.Visible = true;
                            return null;
                        }

                        DataRow row = dt.Rows[0];
                        return new ApplicationInterviewContext
                        {
                            AppliedJobId = Convert.ToInt32(row["AppliedJobId"]),
                            JobId = Convert.ToInt32(row["JobId"]),
                            UserId = Convert.ToInt32(row["UserId"]),
                            CompanyId = companyId,
                            JobTitle = row["Title"].ToString(),
                            JobLevel = string.IsNullOrWhiteSpace(row["JobExperience"].ToString()) ? "Mid-Level" : row["JobExperience"].ToString(),
                            JobType = string.IsNullOrWhiteSpace(row["JobType"].ToString()) ? "Mixed" : row["JobType"].ToString(),
                            TechStack = row["Specialization"].ToString(),
                            CompanyCriteria =
                                "Title: " + row["Title"] + "\n" +
                                "Qualification: " + row["Qualification"] + "\n" +
                                "Required Experience: " + row["JobExperience"] + "\n" +
                                "Specialization: " + row["Specialization"] + "\n" +
                                "Job Description: " + row["Description"],
                            ResumeSummary =
                                "Candidate Name: " + (string.IsNullOrWhiteSpace(row["Name"].ToString()) ? row["Username"].ToString() : row["Name"].ToString()) + "\n" +
                                "Uploaded Resume Path: " + row["Resume"],
                            CompanyName = row["CompanyName"].ToString(),
                            CandidateName = string.IsNullOrWhiteSpace(row["Name"].ToString()) ? row["Username"].ToString() : row["Name"].ToString(),
                            CandidateEmail = row["Email"].ToString(),
                            QuestionCount = 8
                        };
                    }
                }
            }
        }

        private List<string> GenerateInterviewQuestions(ApplicationInterviewContext context, List<string> previousQuestions, string resumeText)
        {
            try
            {
                var gemini = new GeminiService();
                // Run on a thread-pool thread (no ASP.NET sync context) with a
                // hard 25-second deadline. If Gemini is slow or unreachable the
                // task is cancelled and the caller falls back to static questions,
                // so the email still sends instead of the page hanging forever.
                using (var cts = new System.Threading.CancellationTokenSource(System.TimeSpan.FromSeconds(25)))
                {
                    var task = System.Threading.Tasks.Task.Run(async () =>
                        await gemini.GenerateQuestionsAsync(
                            context.JobTitle,
                            context.JobLevel,
                            context.JobType,
                            context.TechStack,
                            context.QuestionCount,
                            previousQuestions,
                            resumeText).ConfigureAwait(false),
                        cts.Token);

                    if (task.Wait(System.TimeSpan.FromSeconds(25)))
                        return task.Result;

                    // Timed out — cancel and fall through to fallback questions
                    cts.Cancel();
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private List<string> BuildFallbackQuestions(string role, string interviewType, int questionCount)
        {
            string[] questions =
            {
                "Introduce yourself and explain your relevant experience for this role.",
                "Describe a recent project that best matches this position and your contribution in it.",
                "Which tools and technologies from your resume are you most confident with, and why?",
                "Explain a difficult technical issue you solved and the approach you followed.",
                "How do you prioritize tasks when multiple deadlines are close?",
                "How would you apply your current skillset to the role of " + role + "?",
                "Tell us about one area you are currently improving and how you are improving it.",
                "Why do you think you are a strong fit for this position?"
            };

            return questions.Take(Math.Max(1, Math.Min(questionCount, questions.Length))).ToList();
        }

        private List<string> LoadPreviousQuestions(int userId, string role)
        {
            var questions = new List<string>();
            using (SqlConnection con = new SqlConnection(str))
            {
                string query = @"SELECT TOP 20 q.QuestionText
                                 FROM InterviewQuestions q
                                 INNER JOIN Interviews i ON q.InterviewId = i.InterviewId
                                 WHERE i.UserId = @UserId AND i.Role = @Role
                                 ORDER BY i.CreatedAt DESC";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Role", role);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            questions.Add(reader["QuestionText"].ToString());
                        }
                    }
                }
            }
            return questions;
        }

        private int UpsertInterviewAndInvitation(ApplicationInterviewContext context, List<string> questions, out string plainPassword, out Guid accessToken)
        {
            plainPassword = Utils.GenerateNumericCode(6);
            accessToken = Guid.NewGuid();
            string salt = Utils.GenerateSalt();
            string hash = Utils.ComputeSha256Hash(plainPassword + salt);

            using (SqlConnection con = new SqlConnection(str))
            {
                con.Open();
                SqlTransaction tx = con.BeginTransaction();
                try
                {
                    int interviewId;
                    string existingInterviewQuery = @"SELECT TOP 1 InterviewId
                                                    FROM Interviews
                                                    WHERE AppliedJobId = @AppliedJobId
                                                    ORDER BY CreatedAt DESC";
                    using (SqlCommand cmd = new SqlCommand(existingInterviewQuery, con, tx))
                    {
                        cmd.Parameters.AddWithValue("@AppliedJobId", context.AppliedJobId);
                        object existing = cmd.ExecuteScalar();
                        if (existing == null)
                        {
                            string insertInterview = @"INSERT INTO Interviews
                                                       (UserId, JobId, AppliedJobId, Role, Level, InterviewType, TechStack, QuestionCount, Status)
                                                       VALUES
                                                       (@UserId, @JobId, @AppliedJobId, @Role, @Level, @InterviewType, @TechStack, @QuestionCount, 'pending');
                                                       SELECT SCOPE_IDENTITY();";
                            using (SqlCommand insertCmd = new SqlCommand(insertInterview, con, tx))
                            {
                                insertCmd.Parameters.AddWithValue("@UserId", context.UserId);
                                insertCmd.Parameters.AddWithValue("@JobId", context.JobId);
                                insertCmd.Parameters.AddWithValue("@AppliedJobId", context.AppliedJobId);
                                insertCmd.Parameters.AddWithValue("@Role", context.JobTitle);
                                insertCmd.Parameters.AddWithValue("@Level", context.JobLevel);
                                insertCmd.Parameters.AddWithValue("@InterviewType", context.JobType);
                                insertCmd.Parameters.AddWithValue("@TechStack", string.IsNullOrWhiteSpace(context.TechStack) ? (object)DBNull.Value : context.TechStack);
                                insertCmd.Parameters.AddWithValue("@QuestionCount", context.QuestionCount);
                                interviewId = Convert.ToInt32(insertCmd.ExecuteScalar());
                            }
                        }
                        else
                        {
                            interviewId = Convert.ToInt32(existing);
                            string resetInterview = @"UPDATE Interviews
                                                     SET Role = @Role,
                                                         Level = @Level,
                                                         InterviewType = @InterviewType,
                                                         TechStack = @TechStack,
                                                         QuestionCount = @QuestionCount,
                                                         Status = 'pending',
                                                         CompletedAt = NULL
                                                     WHERE InterviewId = @InterviewId";
                            using (SqlCommand resetCmd = new SqlCommand(resetInterview, con, tx))
                            {
                                resetCmd.Parameters.AddWithValue("@Role", context.JobTitle);
                                resetCmd.Parameters.AddWithValue("@Level", context.JobLevel);
                                resetCmd.Parameters.AddWithValue("@InterviewType", context.JobType);
                                resetCmd.Parameters.AddWithValue("@TechStack", string.IsNullOrWhiteSpace(context.TechStack) ? (object)DBNull.Value : context.TechStack);
                                resetCmd.Parameters.AddWithValue("@QuestionCount", context.QuestionCount);
                                resetCmd.Parameters.AddWithValue("@InterviewId", interviewId);
                                resetCmd.ExecuteNonQuery();
                            }

                            using (SqlCommand deleteTranscript = new SqlCommand("DELETE FROM InterviewTranscripts WHERE InterviewId = @InterviewId", con, tx))
                            {
                                deleteTranscript.Parameters.AddWithValue("@InterviewId", interviewId);
                                deleteTranscript.ExecuteNonQuery();
                            }

                            using (SqlCommand deleteFeedback = new SqlCommand("DELETE FROM InterviewFeedback WHERE InterviewId = @InterviewId", con, tx))
                            {
                                deleteFeedback.Parameters.AddWithValue("@InterviewId", interviewId);
                                deleteFeedback.ExecuteNonQuery();
                            }
                        }
                    }

                    using (SqlCommand delQ = new SqlCommand("DELETE FROM InterviewQuestions WHERE InterviewId = @InterviewId", con, tx))
                    {
                        delQ.Parameters.AddWithValue("@InterviewId", interviewId);
                        delQ.ExecuteNonQuery();
                    }

                    for (int i = 0; i < questions.Count; i++)
                    {
                        string insertQuestion = @"INSERT INTO InterviewQuestions (InterviewId, QuestionText, SortOrder)
                                                  VALUES (@InterviewId, @QuestionText, @SortOrder)";
                        using (SqlCommand qCmd = new SqlCommand(insertQuestion, con, tx))
                        {
                            qCmd.Parameters.AddWithValue("@InterviewId", interviewId);
                            qCmd.Parameters.AddWithValue("@QuestionText", questions[i]);
                            qCmd.Parameters.AddWithValue("@SortOrder", i + 1);
                            qCmd.ExecuteNonQuery();
                        }
                    }

                    string upsertInvitation = @"
                        IF EXISTS (SELECT 1 FROM InterviewInvitations WHERE InterviewId = @InterviewId)
                        BEGIN
                            UPDATE InterviewInvitations
                            SET AppliedJobId = @AppliedJobId,
                                JobId = @JobId,
                                UserId = @UserId,
                                CompanyId = @CompanyId,
                                AccessToken = @AccessToken,
                                PasswordSalt = @PasswordSalt,
                                PasswordHash = @PasswordHash,
                                IsPasswordUsed = 0,
                                PasswordUsedAt = NULL,
                                CreatedAt = GETDATE()
                            WHERE InterviewId = @InterviewId;
                        END
                        ELSE
                        BEGIN
                            INSERT INTO InterviewInvitations
                            (InterviewId, AppliedJobId, JobId, UserId, CompanyId, AccessToken, PasswordSalt, PasswordHash)
                            VALUES
                            (@InterviewId, @AppliedJobId, @JobId, @UserId, @CompanyId, @AccessToken, @PasswordSalt, @PasswordHash);
                        END";

                    using (SqlCommand invitationCmd = new SqlCommand(upsertInvitation, con, tx))
                    {
                        invitationCmd.Parameters.AddWithValue("@InterviewId", interviewId);
                        invitationCmd.Parameters.AddWithValue("@AppliedJobId", context.AppliedJobId);
                        invitationCmd.Parameters.AddWithValue("@JobId", context.JobId);
                        invitationCmd.Parameters.AddWithValue("@UserId", context.UserId);
                        invitationCmd.Parameters.AddWithValue("@CompanyId", context.CompanyId);
                        invitationCmd.Parameters.AddWithValue("@AccessToken", accessToken);
                        invitationCmd.Parameters.AddWithValue("@PasswordSalt", salt);
                        invitationCmd.Parameters.AddWithValue("@PasswordHash", hash);
                        invitationCmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                    return GetInterviewIdByAppliedJob(context.AppliedJobId, con);
                }
                catch (Exception ex)
                {
                    try { tx.Rollback(); } catch { }
                    lblMsg.Text = "Failed to prepare interview invitation: " + ex.Message;
                    lblMsg.CssClass = "alert alert-danger";
                    lblMsg.Visible = true;
                    return 0;
                }
            }
        }

        private int GetInterviewIdByAppliedJob(int appliedJobId, SqlConnection openConnection)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 InterviewId FROM Interviews WHERE AppliedJobId = @AppliedJobId ORDER BY CreatedAt DESC", openConnection))
            {
                cmd.Parameters.AddWithValue("@AppliedJobId", appliedJobId);
                object result = cmd.ExecuteScalar();
                return result == null ? 0 : Convert.ToInt32(result);
            }
        }

        private class ApplicationInterviewContext
        {
            public int AppliedJobId { get; set; }
            public int JobId { get; set; }
            public int UserId { get; set; }
            public int CompanyId { get; set; }
            public string JobTitle { get; set; }
            public string JobLevel { get; set; }
            public string JobType { get; set; }
            public string TechStack { get; set; }
            public string CompanyCriteria { get; set; }
            public string ResumeSummary { get; set; }
            public string CompanyName { get; set; }
            public string CandidateName { get; set; }
            public string CandidateEmail { get; set; }
            public int QuestionCount { get; set; }
        }

        private class InterviewInvitationEmailData
        {
            public string CandidateName { get; set; }
            public string CandidateEmail { get; set; }
            public string CompanyName { get; set; }
            public string JobTitle { get; set; }
            public string AccessUrl { get; set; }
            public string OneTimePassword { get; set; }
        }

        //protected void Button1_Click(object sender, EventArgs e)
        //{
        //    foreach (GridViewRow row in GridView1.Rows)
        //    {
        //        CheckBox chkSelect = row.FindControl("chkSelect") as CheckBox;
        //        if (chkSelect != null && chkSelect.Checked)
        //        {
        //            // Get hidden field values
        //            HiddenField hdnEmail = row.FindControl("hdnEmail") as HiddenField;
        //            HiddenField hdnJobId = row.FindControl("hdnJobId") as HiddenField;

        //            string email = hdnEmail?.Value?.Trim();
        //            string jobId = hdnJobId?.Value;
        //            string candidateName = row.Cells[3].Text.Trim();   // Or make this also a hidden field
        //            string jobTitle = row.Cells[2].Text.Trim();
        //            string company = row.Cells[1].Text.Trim();
        //            // Or make this also a hidden field

        //            // 🧪 DEBUG: Check what email was retrieved
        //            if (string.IsNullOrEmpty(email))
        //            {
        //                lblMsg.Text = $"Email is null or empty for candidate {candidateName}.";
        //                lblMsg.CssClass = "alert alert-danger";
        //                continue;
        //            }

        //            if (!email.Contains("@"))
        //            {
        //                lblMsg.Text = $"Invalid email address: '{email}'";
        //                lblMsg.CssClass = "alert alert-danger";
        //                continue;
        //            }

        //            try
        //            {
        //                MailMessage mail = new MailMessage();
        //                mail.From = new MailAddress("online.jobportal.nuces@gmail.com", $"HR Manager - {company}");
        //                mail.To.Add(email);
        //                mail.Subject = "Onsite Interview Invitation";
        //                mail.Body = $"Dear {candidateName},\n\tWe are pleased to inform you that you have been shortlisted for the {jobTitle} position at {company}. We would like to invite you to attend an onsite interview to discuss your qualifications and skills further.\r\n\r\nHere are the details of the interview:\r\n\r\nDate: 20, May, 2025\r\nTime: 10:00 am\r\n\r\nPlease bring the following documents with you to the interview:\n\n*Your Resume\n*Your original CNIC\n\nBest Regards,\nHiring Manager @ {company}\n";
        //                mail.IsBodyHtml = false;

        //                SmtpClient smtp = new SmtpClient();
        //                smtp.Host = "smtp.gmail.com";
        //                smtp.Port = 587;
        //                smtp.EnableSsl = true;
        //                smtp.Credentials = new NetworkCredential("online.jobportal.nuces@gmail.com", "gvozavnwfiierlug");
        //                smtp.Send(mail);

        //                lblMsg.Text = $"Interview email sent to {candidateName} ({email})!";
        //                lblMsg.CssClass = "alert alert-success";
        //            }
        //            catch (Exception ex)
        //            {
        //                lblMsg.Text = $"Error sending email to {candidateName} ({email}): {ex.Message}";
        //                lblMsg.CssClass = "alert alert-danger";
        //            }
        //        }
        //    }
        //}

        // Updated Button1_Click to use the new helper method
        protected void Button1_Click(object sender, EventArgs e)
        {
            lblMsg.Text = string.Empty; // Clear message at start
            bool anySelected = false;

            foreach (GridViewRow row in GridView1.Rows)
            {
                CheckBox chkSelect = row.FindControl("chkSelect") as CheckBox;
                if (chkSelect != null && chkSelect.Checked)
                {
                    // Call the shared email function
                    SendInterviewEmail(row);
                    anySelected = true;
                    // Note: If multiple emails are sent, the lblMsg will only show the status 
                    // of the last processed row.
                }
            }

            if (!anySelected && string.IsNullOrEmpty(lblMsg.Text))
            {
                lblMsg.Text = "Please select at least one candidate to send an interview mail.";
                lblMsg.CssClass = "alert alert-info";
                lblMsg.Visible = true;
            }
        }


        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string jobId = e.CommandArgument.ToString();
            if (e.CommandName == "ViewJob")
            {
                string url = "JobList.aspx?id=" + jobId;
                url += "&returnUrl=" + Server.UrlEncode("~/Company/ShorlistedCandidates.aspx");
                Response.Redirect(url);
                return;
            }

            if (e.CommandName == "SendSingleMail")
            {
                // 1. Find the row that contains the button that was clicked
                LinkButton btn = (LinkButton)e.CommandSource;
                GridViewRow row = (GridViewRow)btn.NamingContainer;

                // 2. Clear message and call the shared email function for this single row
                lblMsg.Text = string.Empty;
                lblMsg.Visible = false;

                SendInterviewEmail(row);

                return;
            }
        }

    }
}
