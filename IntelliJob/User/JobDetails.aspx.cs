using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IntelliJob.User
{
    public partial class JobDetails : System.Web.UI.Page
    {
        SqlConnection con;
        SqlCommand cmd;
        SqlDataAdapter sda;
        DataTable dt, dt1;
        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;
        public string jobTitle = string.Empty;

        protected void Page_Init(object sender, EventArgs e)
        {
            if (Request.QueryString["id"] != null)
            {
                showjobDetails();
                DataBind();
            }
            else
            {
                Response.Redirect("JobListing.aspx");
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        private void showjobDetails()
        {
            con = new SqlConnection(str);
            string query = @"Select * from Jobs where JobId = @id";
            cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@id", Request.QueryString["id"]);
            sda = new SqlDataAdapter(cmd);
            dt = new DataTable();
            sda.Fill(dt);
            DataList1.DataSource = dt;
            DataList1.DataBind();
            jobTitle = dt.Rows[0]["Title"].ToString();
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            if (e.CommandName == "ApplyJob")
            {
                if (Session["user"] != null)
                {
                    try
                    {
                        con = new SqlConnection(str);
                        string query = @"INSERT INTO AppliedJobs (JobId, UserId, Shortlisted) VALUES (@JobId, @UserId, @Shortlisted)";
                        cmd = new SqlCommand(query, con);
                        cmd.Parameters.AddWithValue("@JobId", Request.QueryString["id"]);
                        cmd.Parameters.AddWithValue("@UserId", Session["userId"]);
                        cmd.Parameters.AddWithValue("@Shortlisted", "no");
                        con.Open();
                        int r = cmd.ExecuteNonQuery();
                        if (r > 0)
                        {
                            lblMsg.Visible = true;
                            lblMsg.Text = "Job Applied Successfully.";
                            lblMsg.CssClass = "alert alert-success";
                            showjobDetails();

                            // Auto-send interview invitation email
                            try
                            {
                                int appliedJobId = GetAppliedJobId(Convert.ToInt32(Session["userId"]), Convert.ToInt32(Request.QueryString["id"]));
                                if (appliedJobId > 0)
                                    SendAutoInterviewInvite(appliedJobId, Convert.ToInt32(Session["userId"]));
                            }
                            catch { /* Don't block the user if email fails */ }
                        }
                        else
                        {
                            lblMsg.Visible = true;
                            lblMsg.Text = "Cannot apply the job please try after sometime.";
                            lblMsg.CssClass = "alert alert-danger";
                        }
                    }
                    catch (Exception ex)
                    {
                        Response.Write("<script>alert('" + ex.Message + "');</script>");
                    }
                    finally
                    {
                        con.Close();
                    }
                }
                else
                {
                    Response.Redirect("Login.aspx");

                }
            }
        }

        protected void DataList1_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (Session["user"] != null)
            {
                LinkButton btnApplyJob = e.Item.FindControl("lbApplyJob") as LinkButton;
                if (isApplied())
                {
                    btnApplyJob.Enabled = false;
                    btnApplyJob.Text = "Applied";
                }
                else
                {
                    btnApplyJob.Enabled = true;
                    btnApplyJob.Text = "Apply Now";
                }
            }
        }

        bool isApplied()
        {
            con = new SqlConnection(str);
            string query = @"Select * from AppliedJobs where UserId = @UserId and JobId = @JobId";
            cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserId", Session["userId"]);
            cmd.Parameters.AddWithValue("@JobId", Request.QueryString["id"]);
            sda = new SqlDataAdapter(cmd);
            dt1 = new DataTable();
            sda.Fill(dt1);
            if (dt1.Rows.Count == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        protected string GetImageUrl(Object url)
        {
            string url1 = "";
            if (string.IsNullOrEmpty(url.ToString()) || url == DBNull.Value)
            {
                url1 = "~/Images/No_image.png";
            }
            else
            {
                url1 = string.Format("~/{0}", url);
            }
            return ResolveUrl(url1);
        }

        private int GetAppliedJobId(int userId, int jobId)
        {
            using (SqlConnection c = new SqlConnection(str))
            using (SqlCommand cm = new SqlCommand("SELECT TOP 1 AppliedJobId FROM AppliedJobs WHERE UserId=@u AND JobId=@j ORDER BY AppliedJobId DESC", c))
            {
                cm.Parameters.AddWithValue("@u", userId);
                cm.Parameters.AddWithValue("@j", jobId);
                c.Open();
                object r = cm.ExecuteScalar();
                return r == null ? 0 : Convert.ToInt32(r);
            }
        }

        private void SendAutoInterviewInvite(int appliedJobId, int userId)
        {
            // Load job + candidate context
            JobApplicationContext ctx = LoadJobApplicationContext(appliedJobId, userId);
            if (ctx == null) return;

            List<string> prevQ = LoadPreviousQuestions(userId, ctx.JobTitle);
            List<string> questions = GenerateInterviewQuestions(ctx, prevQ);
            if (questions == null || questions.Count == 0)
                questions = BuildFallbackQuestions(ctx.JobTitle, ctx.JobType, ctx.QuestionCount);

            string plainPassword;
            Guid accessToken;
            int interviewId = UpsertInterviewAndInvitation(ctx, questions, out plainPassword, out accessToken);
            if (interviewId <= 0) return;

            string accessUrl = Request.Url.GetLeftPart(UriPartial.Authority) + ResolveUrl("~/User/InterviewAccess.aspx?token=" + accessToken);

            string smtpUser = ConfigurationManager.AppSettings["SmtpUser"] ?? "";
            string smtpPass = ConfigurationManager.AppSettings["SmtpPass"] ?? "";
            string smtpHost = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
            int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
            string smtpFrom = ConfigurationManager.AppSettings["SmtpFrom"] ?? smtpUser;

            if (string.IsNullOrEmpty(smtpPass)) return;

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(smtpFrom, "IntelliJob - Interview Invite");
            mail.To.Add(ctx.CandidateEmail);
            mail.Subject = $"Your AI Interview Invitation – {ctx.JobTitle} at {ctx.CompanyName}";
            mail.Body =
                $"Dear {ctx.CandidateName},\n\n" +
                $"Thank you for applying for the {ctx.JobTitle} position at {ctx.CompanyName}.\n\n" +
                "You have been invited to complete an AI-powered first-round interview.\n\n" +
                "Please use the link and one-time password below to begin:\n\n" +
                $"Interview Link:\n{accessUrl}\n\n" +
                $"One-Time Password: {plainPassword}\n\n" +
                "Important:\n" +
                "- This password can only be used once.\n" +
                "- Log in with the same account you used to apply.\n\n" +
                $"Best Regards,\nHiring Team @ {ctx.CompanyName}\n";
            mail.IsBodyHtml = false;

            SmtpClient smtp = new SmtpClient();
            smtp.Host = smtpHost;
            smtp.Port = smtpPort;
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
            smtp.Send(mail);
        }

        private JobApplicationContext LoadJobApplicationContext(int appliedJobId, int userId)
        {
            using (SqlConnection c = new SqlConnection(str))
            {
                string q = @"SELECT aj.AppliedJobId, aj.JobId, aj.UserId,
                                    j.Title, j.Experience AS JobExperience, j.Specialization, j.Description, j.Qualification, j.JobType, j.CompanyName,
                                    u.Email, u.Username,
                                    js.Name, js.WorksOn, js.Experience AS CandidateExperience,
                                    js.TenthGrade, js.TwelfthGrade, js.GraduationGrade, js.PostGraduationGrade, js.Phd, js.Resume
                             FROM AppliedJobs aj
                             INNER JOIN Jobs j ON aj.JobId = j.JobId
                             INNER JOIN Users u ON aj.UserId = u.UserId
                             LEFT JOIN JobSeekers js ON aj.UserId = js.ProfileId
                             WHERE aj.AppliedJobId = @AjId AND aj.UserId = @UserId";
                using (SqlCommand cmd = new SqlCommand(q, c))
                {
                    cmd.Parameters.AddWithValue("@AjId", appliedJobId);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        if (dt.Rows.Count == 0) return null;
                        DataRow row = dt.Rows[0];
                        // Resolve companyId from CompanyName
                        int companyId = GetCompanyIdByName(row["CompanyName"].ToString());
                        return new JobApplicationContext
                        {
                            AppliedJobId = Convert.ToInt32(row["AppliedJobId"]),
                            JobId = Convert.ToInt32(row["JobId"]),
                            UserId = Convert.ToInt32(row["UserId"]),
                            CompanyId = companyId,
                            JobTitle = row["Title"].ToString(),
                            JobLevel = string.IsNullOrWhiteSpace(row["JobExperience"].ToString()) ? "Mid-Level" : row["JobExperience"].ToString(),
                            JobType = string.IsNullOrWhiteSpace(row["JobType"].ToString()) ? "Mixed" : row["JobType"].ToString(),
                            TechStack = row["Specialization"].ToString(),
                            CompanyName = row["CompanyName"].ToString(),
                            CandidateName = string.IsNullOrWhiteSpace(row["Name"].ToString()) ? row["Username"].ToString() : row["Name"].ToString(),
                            CandidateEmail = row["Email"].ToString(),
                            QuestionCount = 8
                        };
                    }
                }
            }
        }

        private int GetCompanyIdByName(string companyName)
        {
            using (SqlConnection c = new SqlConnection(str))
            using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 CompanyId FROM Companies WHERE CompanyName=@n", c))
            {
                cmd.Parameters.AddWithValue("@n", companyName);
                c.Open();
                object r = cmd.ExecuteScalar();
                return r == null ? 0 : Convert.ToInt32(r);
            }
        }

        private List<string> LoadPreviousQuestions(int userId, string role)
        {
            var questions = new List<string>();
            using (SqlConnection c = new SqlConnection(str))
            {
                string q = @"SELECT TOP 20 q.QuestionText FROM InterviewQuestions q
                             INNER JOIN Interviews i ON q.InterviewId = i.InterviewId
                             WHERE i.UserId = @UserId AND i.Role = @Role ORDER BY i.CreatedAt DESC";
                using (SqlCommand cmd = new SqlCommand(q, c))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Role", role);
                    c.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                        while (rdr.Read()) questions.Add(rdr["QuestionText"].ToString());
                }
            }
            return questions;
        }

        private List<string> GenerateInterviewQuestions(JobApplicationContext ctx, List<string> prevQ)
        {
            try
            {
                var gemini = new GeminiService();
                using (var cts = new System.Threading.CancellationTokenSource(System.TimeSpan.FromSeconds(25)))
                {
                    var task = System.Threading.Tasks.Task.Run(async () =>
                        await gemini.GenerateQuestionsAsync(ctx.JobTitle, ctx.JobLevel, ctx.JobType, ctx.TechStack, ctx.QuestionCount, prevQ).ConfigureAwait(false), cts.Token);
                    if (task.Wait(System.TimeSpan.FromSeconds(25))) return task.Result;
                    cts.Cancel(); return null;
                }
            }
            catch { return null; }
        }

        private List<string> BuildFallbackQuestions(string role, string type, int count)
        {
            string[] q = {
                "Introduce yourself and explain your relevant experience for this role.",
                "Describe a recent project that best matches this position.",
                "Which tools and technologies are you most confident with, and why?",
                "Explain a difficult technical issue you solved and the approach you followed.",
                "How do you prioritize tasks when multiple deadlines are close?",
                "How would you apply your skillset to the role of " + role + "?",
                "Tell us about one area you are currently improving.",
                "Why do you think you are a strong fit for this position?"
            };
            return q.Take(Math.Max(1, Math.Min(count, q.Length))).ToList();
        }

        private int UpsertInterviewAndInvitation(JobApplicationContext ctx, List<string> questions, out string plainPassword, out Guid accessToken)
        {
            plainPassword = Utils.GenerateNumericCode(6);
            accessToken = Guid.NewGuid();
            string salt = Utils.GenerateSalt();
            string hash = Utils.ComputeSha256Hash(plainPassword + salt);

            using (SqlConnection c = new SqlConnection(str))
            {
                c.Open();
                SqlTransaction tx = c.BeginTransaction();
                try
                {
                    int interviewId;
                    object existing;
                    using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 InterviewId FROM Interviews WHERE AppliedJobId=@AjId ORDER BY CreatedAt DESC", c, tx))
                    {
                        cmd.Parameters.AddWithValue("@AjId", ctx.AppliedJobId);
                        existing = cmd.ExecuteScalar();
                    }

                    if (existing == null)
                    {
                        string ins = @"INSERT INTO Interviews (UserId,JobId,AppliedJobId,Role,Level,InterviewType,TechStack,QuestionCount,Status)
                                       VALUES (@U,@J,@AJ,@R,@L,@IT,@TS,@QC,'pending'); SELECT SCOPE_IDENTITY();";
                        using (SqlCommand cmd = new SqlCommand(ins, c, tx))
                        {
                            cmd.Parameters.AddWithValue("@U", ctx.UserId);
                            cmd.Parameters.AddWithValue("@J", ctx.JobId);
                            cmd.Parameters.AddWithValue("@AJ", ctx.AppliedJobId);
                            cmd.Parameters.AddWithValue("@R", ctx.JobTitle);
                            cmd.Parameters.AddWithValue("@L", ctx.JobLevel);
                            cmd.Parameters.AddWithValue("@IT", ctx.JobType);
                            cmd.Parameters.AddWithValue("@TS", string.IsNullOrWhiteSpace(ctx.TechStack) ? (object)DBNull.Value : ctx.TechStack);
                            cmd.Parameters.AddWithValue("@QC", ctx.QuestionCount);
                            interviewId = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                    else
                    {
                        interviewId = Convert.ToInt32(existing);
                        using (SqlCommand cmd = new SqlCommand("UPDATE Interviews SET Status='pending',CompletedAt=NULL WHERE InterviewId=@Id", c, tx))
                        { cmd.Parameters.AddWithValue("@Id", interviewId); cmd.ExecuteNonQuery(); }
                        using (SqlCommand cmd = new SqlCommand("DELETE FROM InterviewTranscripts WHERE InterviewId=@Id", c, tx))
                        { cmd.Parameters.AddWithValue("@Id", interviewId); cmd.ExecuteNonQuery(); }
                        using (SqlCommand cmd = new SqlCommand("DELETE FROM InterviewFeedback WHERE InterviewId=@Id", c, tx))
                        { cmd.Parameters.AddWithValue("@Id", interviewId); cmd.ExecuteNonQuery(); }
                    }

                    using (SqlCommand cmd = new SqlCommand("DELETE FROM InterviewQuestions WHERE InterviewId=@Id", c, tx))
                    { cmd.Parameters.AddWithValue("@Id", interviewId); cmd.ExecuteNonQuery(); }

                    for (int i = 0; i < questions.Count; i++)
                    {
                        using (SqlCommand cmd = new SqlCommand("INSERT INTO InterviewQuestions(InterviewId,QuestionText,SortOrder) VALUES(@Id,@Q,@S)", c, tx))
                        {
                            cmd.Parameters.AddWithValue("@Id", interviewId);
                            cmd.Parameters.AddWithValue("@Q", questions[i]);
                            cmd.Parameters.AddWithValue("@S", i + 1);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    string upsert = @"IF EXISTS(SELECT 1 FROM InterviewInvitations WHERE InterviewId=@Id)
                        UPDATE InterviewInvitations SET AccessToken=@AT,PasswordSalt=@PS,PasswordHash=@PH,IsPasswordUsed=0,PasswordUsedAt=NULL,CreatedAt=GETDATE() WHERE InterviewId=@Id
                        ELSE
                        INSERT INTO InterviewInvitations(InterviewId,AppliedJobId,JobId,UserId,CompanyId,AccessToken,PasswordSalt,PasswordHash)
                        VALUES(@Id,@AJ,@JId,@UserId,@CId,@AT,@PS,@PH)";
                    using (SqlCommand cmd = new SqlCommand(upsert, c, tx))
                    {
                        cmd.Parameters.AddWithValue("@Id", interviewId);
                        cmd.Parameters.AddWithValue("@AJ", ctx.AppliedJobId);
                        cmd.Parameters.AddWithValue("@JId", ctx.JobId);
                        cmd.Parameters.AddWithValue("@UserId", ctx.UserId);
                        cmd.Parameters.AddWithValue("@CId", ctx.CompanyId);
                        cmd.Parameters.AddWithValue("@AT", accessToken);
                        cmd.Parameters.AddWithValue("@PS", salt);
                        cmd.Parameters.AddWithValue("@PH", hash);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                    return interviewId;
                }
                catch { try { tx.Rollback(); } catch { } return 0; }
            }
        }

        private class JobApplicationContext
        {
            public int AppliedJobId { get; set; }
            public int JobId { get; set; }
            public int UserId { get; set; }
            public int CompanyId { get; set; }
            public string JobTitle { get; set; }
            public string JobLevel { get; set; }
            public string JobType { get; set; }
            public string TechStack { get; set; }
            public string CompanyName { get; set; }
            public string CandidateName { get; set; }
            public string CandidateEmail { get; set; }
            public int QuestionCount { get; set; }
        }
    }
}
