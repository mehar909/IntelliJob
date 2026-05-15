using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace IntelliJob.User
{
    public partial class JobDetails : System.Web.UI.Page
    {
        private const string StaticInterviewPassword = "123456";

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
            string query = @"SELECT j.*, COALESCE(NULLIF(LTRIM(RTRIM(j.CompanyImage)), ''), NULLIF(LTRIM(RTRIM(c.CompanyLogo)), ''), 'Images/No_image.png') AS DisplayImage
                             FROM Jobs j
                             LEFT JOIN Companies c ON c.CompanyName = j.CompanyName
                             WHERE j.JobId = @id";
            cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@id", Request.QueryString["id"]);
            sda = new SqlDataAdapter(cmd);
            dt = new DataTable();
            sda.Fill(dt);

            ApplyApplicationResumeState(dt);
            DataList1.DataSource = dt;
            DataList1.DataBind();
            jobTitle = dt.Rows[0]["Title"].ToString();
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            if (e.CommandName == "DeleteAndUseProfileResume")
            {
                if (Session["user"] == null)
                {
                    Response.Redirect("Login.aspx");
                    return;
                }

                int userId = Convert.ToInt32(Session["userId"]);
                int jobId = Convert.ToInt32(Request.QueryString["id"]);

                bool removed = ApplicationDataStore.DeleteApplicationResumeDraft(userId, jobId, deleteStoredFile: true);
                ApplicationDataStore.SetApplicationResumeProfileOnly(userId, jobId, true);

                if (!removed)
                    ShowJobMessage("Profile resume is now locked for this application, but existing draft cleanup could not be completed fully.", false);
                else
                    ShowJobMessage("Application draft removed. Your profile resume will be used and job-specific resume upload is now disabled for this job.", true);

                showjobDetails();
                return;
            }

            if (e.CommandName == "SaveApplicationResume")
            {
                if (Session["user"] == null)
                {
                    Response.Redirect("Login.aspx");
                    return;
                }

                try
                {
                    int userId = Convert.ToInt32(Session["userId"]);
                    int jobId = Convert.ToInt32(Request.QueryString["id"]);
                    if (ApplicationDataStore.IsApplicationResumeProfileOnly(userId, jobId))
                    {
                        ShowJobMessage("Profile resume is locked for this application. Job-specific resume upload is disabled.", false);
                        return;
                    }

                    FileUpload fuApplicationResume = e.Item.FindControl("fuApplicationResume") as FileUpload;
                    if (fuApplicationResume == null || !fuApplicationResume.HasFile)
                    {
                        ShowJobMessage("Please choose a resume file to upload.", false);
                        return;
                    }

                    if (!Utils.IsValidExtension4Resume(fuApplicationResume.FileName))
                    {
                        ShowJobMessage("Please select a .doc, .docx, or .pdf file for this application.", false);
                        return;
                    }

                    ApplicationResumeDraftRecord savedDraft = ApplicationDataStore.SaveApplicationResumeDraft(
                        userId,
                        jobId,
                        fuApplicationResume.PostedFile,
                        "application-upload",
                        fuApplicationResume.FileName);

                    if (savedDraft == null || string.IsNullOrWhiteSpace(savedDraft.StoredResumePath))
                    {
                        ShowJobMessage("The application resume could not be saved. Please try again.", false);
                        return;
                    }

                    ApplicationDataStore.SetApplicationResumeProfileOnly(userId, jobId, false);
                    Response.Redirect("ApplicationResumeBuild.aspx?jobId=" + jobId, false);
                    return;
                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('" + HttpUtility.JavaScriptStringEncode(ex.Message) + "');</script>");
                    return;
                }
            }

            if (e.CommandName == "ApplyJob")
            {
                if (Session["user"] != null)
                {
                    try
                    {
                        int userId = Convert.ToInt32(Session["userId"]);
                        int jobId = Convert.ToInt32(Request.QueryString["id"]);
                        ApplicationResumeDraftRecord applicationDraft = GetApplicationResumeDraft(userId, jobId);
                        if (applicationDraft != null && !applicationDraft.IsConfirmed)
                        {
                            ShowJobMessage("Open the application resume editor and confirm your changes before applying.", false);
                            Response.Redirect("ApplicationResumeBuild.aspx?jobId=" + jobId, false);
                            return;
                        }

                        ResumeProfileDocument profileDocument = GetCurrentProfileResumeDocument(userId);
                        if (applicationDraft == null && profileDocument == null)
                        {
                            ShowJobMessage("Please upload a resume in your profile or attach one while applying.", false);
                            return;
                        }

                        int appliedJobId;
                        string siteBaseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                        string applicationPath = Request.ApplicationPath;
                        if (string.IsNullOrWhiteSpace(applicationPath))
                            applicationPath = "/";

                        using (con = new SqlConnection(str))
                        {
                            con.Open();
                            using (SqlTransaction tran = con.BeginTransaction())
                            {
                                using (SqlCommand existingCmd = new SqlCommand(@"SELECT TOP 1 AppliedJobId FROM AppliedJobs WITH (UPDLOCK, HOLDLOCK) WHERE JobId = @JobId AND UserId = @UserId", con, tran))
                                {
                                    existingCmd.Parameters.AddWithValue("@JobId", jobId);
                                    existingCmd.Parameters.AddWithValue("@UserId", userId);
                                    object existingId = existingCmd.ExecuteScalar();
                                    if (existingId != null && existingId != DBNull.Value)
                                    {
                                        tran.Rollback();
                                        ShowJobMessage("You have already applied for this job.", false);
                                        return;
                                    }
                                }

                                string query = @"INSERT INTO AppliedJobs (JobId, UserId, Shortlisted) VALUES (@JobId, @UserId, @Shortlisted); SELECT SCOPE_IDENTITY();";
                                using (SqlCommand insertCmd = new SqlCommand(query, con, tran))
                                {
                                    insertCmd.Parameters.AddWithValue("@JobId", jobId);
                                    insertCmd.Parameters.AddWithValue("@UserId", userId);
                                    insertCmd.Parameters.AddWithValue("@Shortlisted", "no");
                                    appliedJobId = Convert.ToInt32(insertCmd.ExecuteScalar());
                                }

                                ApplicationResumeSelection selection = null;
                                if (applicationDraft != null && File.Exists(applicationDraft.StoredResumePath))
                                {
                                    selection = ApplicationDataStore.FinalizeApplicationResumeDraft(userId, jobId, appliedJobId);
                                }
                                else
                                {
                                    selection = ApplicationDataStore.SaveApplicationResumeSelection(userId, appliedJobId, profileDocument, "profile", profileDocument != null && profileDocument.Metadata != null && !string.IsNullOrWhiteSpace(profileDocument.Metadata.OriginalFileName) ? profileDocument.Metadata.OriginalFileName : "profile-resume.json");
                                }

                                if (selection == null || string.IsNullOrWhiteSpace(selection.StoredResumePath))
                                {
                                    tran.Rollback();
                                    ShowJobMessage("Your application was created, but the resume could not be saved. Please try again.", false);
                                    return;
                                }

                                tran.Commit();
                            }
                        }

                        showjobDetails();
                        ShowJobMessage("Job Applied Successfully. Interview request is being prepared.", true);

                        string candidateEmail = GetCurrentCandidateEmail(userId);
                        if (!IsValidEmail(candidateEmail))
                        {
                            ShowJobPopup("Job applied successfully, but your profile email is invalid so no interview invitation was sent. Please update your email.");
                            return;
                        }

                        QueueInterviewInvite(appliedJobId, userId, siteBaseUrl, applicationPath);
                        ShowJobPopup("Job applied successfully. Interview invitation is being sent to your email.");
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
                    btnApplyJob.OnClientClick = string.Empty;
                }
                else
                {
                    btnApplyJob.Enabled = true;
                    int userId = Convert.ToInt32(Session["userId"]);
                    int jobId = Convert.ToInt32(Request.QueryString["id"]);
                    ApplicationResumeDraftRecord draft = GetApplicationResumeDraft(userId, jobId);
                    if (draft != null && !draft.IsConfirmed)
                    {
                        btnApplyJob.Text = "Confirm Resume";
                        btnApplyJob.OnClientClick = "if(!confirm('Open the application resume editor and confirm your changes first?')) return false;";
                    }
                    else
                    {
                        btnApplyJob.Text = "Apply Now";
                        btnApplyJob.OnClientClick = "if(!confirm('Please confirm your resume before applying. Once this job is applied, you will not be able to edit the application resume. Continue?')) return false; this.style.pointerEvents='none'; this.innerHTML='Applying...';";
                    }
                }

                Panel pnlApplicationResumeUpload = e.Item.FindControl("pnlApplicationResumeUpload") as Panel;
                Panel pnlApplicationResumeEdit = e.Item.FindControl("pnlApplicationResumeEdit") as Panel;
                Panel pnlAppliedResumeEdit = e.Item.FindControl("pnlAppliedResumeEdit") as Panel;
                if (pnlApplicationResumeUpload != null || pnlApplicationResumeEdit != null || pnlAppliedResumeEdit != null)
                {
                    int userId = Convert.ToInt32(Session["userId"]);
                    int jobId = Convert.ToInt32(Request.QueryString["id"]);
                    ApplicationResumeDraftRecord draft = GetApplicationResumeDraft(userId, jobId);
                    bool profileOnlyLock = ApplicationDataStore.IsApplicationResumeProfileOnly(userId, jobId);
                    bool hasDraft = draft != null && !string.IsNullOrWhiteSpace(draft.StoredResumePath) && File.Exists(draft.StoredResumePath);
                    bool applied = isApplied();

                    if (pnlApplicationResumeUpload != null)
                        pnlApplicationResumeUpload.Visible = !applied && !hasDraft && !profileOnlyLock;

                    if (pnlApplicationResumeEdit != null)
                        pnlApplicationResumeEdit.Visible = !applied && hasDraft;

                    if (pnlAppliedResumeEdit != null)
                        pnlAppliedResumeEdit.Visible = false;
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
            if (url == null || url == DBNull.Value)
                return ResolveUrl("~/Images/No_image.png");

            string value = url.ToString().Trim();
            if (string.IsNullOrWhiteSpace(value))
                return ResolveUrl("~/Images/No_image.png");

            if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return value;

            if (value.StartsWith("~/", StringComparison.OrdinalIgnoreCase))
            {
                if (File.Exists(Server.MapPath(value)))
                    return ResolveUrl(value);
            }

            if (value.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                string rooted = "~" + value;
                if (File.Exists(Server.MapPath(rooted)))
                    return ResolveUrl(rooted);
            }

            string[] candidatePaths = new[]
            {
                "~/photos/" + value.TrimStart('~', '/'),
                "~/Images/" + value.TrimStart('~', '/'),
                "~/" + value.TrimStart('~', '/')
            };

            foreach (string candidate in candidatePaths)
            {
                try
                {
                    if (File.Exists(Server.MapPath(candidate)))
                        return ResolveUrl(candidate);
                }
                catch
                {
                }
            }

            return ResolveUrl("~/Images/No_image.png");
        }

        private ResumeProfileDocument GetCurrentProfileResumeDocument(int userId)
        {
            using (SqlConnection c = new SqlConnection(str))
            using (SqlCommand cm = new SqlCommand("SELECT ResumeStructuredJson, Resume FROM JobSeekers WHERE ProfileId = @UserId", c))
            {
                cm.Parameters.AddWithValue("@UserId", userId);
                c.Open();
                using (SqlDataReader reader = cm.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    string structuredJson = reader["ResumeStructuredJson"] != DBNull.Value ? reader["ResumeStructuredJson"].ToString() : string.Empty;
                    if (!string.IsNullOrWhiteSpace(structuredJson))
                        return ResumeProfileService.DeserializeDocument(structuredJson);

                    string resumePath = reader["Resume"] != DBNull.Value ? reader["Resume"].ToString() : string.Empty;
                    if (!string.IsNullOrWhiteSpace(resumePath) && File.Exists(resumePath))
                        return ResumeProfileService.DeserializeDocument(File.ReadAllText(resumePath));
                }

                return null;
            }
        }

        private ApplicationResumeDraftRecord GetApplicationResumeDraft(int userId, int jobId)
        {
            ApplicationResumeDraftRecord draft;
            if (!ApplicationDataStore.TryGetApplicationResumeDraft(userId, jobId, out draft))
                return null;

            return draft != null && !string.IsNullOrWhiteSpace(draft.StoredResumePath) && File.Exists(draft.StoredResumePath)
                ? draft
                : null;
        }

        private void ApplyApplicationResumeState(DataTable table)
        {
            if (table == null || table.Rows.Count == 0)
                return;

            if (!table.Columns.Contains("HasApplicationResumeDraft"))
                table.Columns.Add("HasApplicationResumeDraft", typeof(bool));

            if (!table.Columns.Contains("ShowApplicationResumeUpload"))
                table.Columns.Add("ShowApplicationResumeUpload", typeof(bool));

            if (!table.Columns.Contains("ApplicationResumeEditUrl"))
                table.Columns.Add("ApplicationResumeEditUrl", typeof(string));

            if (!table.Columns.Contains("ApplicationResumeNote"))
                table.Columns.Add("ApplicationResumeNote", typeof(string));

            if (!table.Columns.Contains("ShowAppliedResumeEdit"))
                table.Columns.Add("ShowAppliedResumeEdit", typeof(bool));

            if (!table.Columns.Contains("AppliedResumeEditUrl"))
                table.Columns.Add("AppliedResumeEditUrl", typeof(string));

            if (!table.Columns.Contains("AppliedResumeNote"))
                table.Columns.Add("AppliedResumeNote", typeof(string));

            if (!table.Columns.Contains("ApplicationResumeProfileOnly"))
                table.Columns.Add("ApplicationResumeProfileOnly", typeof(bool));

            int userId = Session["userId"] != null ? Convert.ToInt32(Session["userId"]) : 0;
            int jobId = Convert.ToInt32(table.Rows[0]["JobId"]);
            ApplicationResumeDraftRecord draft = userId > 0 ? GetApplicationResumeDraft(userId, jobId) : null;
            bool isApplied = userId > 0 && IsAppliedForJob(userId, jobId);
            bool profileOnlyLock = userId > 0 && ApplicationDataStore.IsApplicationResumeProfileOnly(userId, jobId);
            bool hasDraft = draft != null;
            string editUrl = hasDraft ? ResolveUrl("~/User/ApplicationResumeBuild.aspx?jobId=" + jobId) : string.Empty;
            string note = hasDraft
                ? (draft.IsConfirmed
                    ? "Your application resume draft is confirmed. Use Edit Resume if you want to change it again."
                    : "Your application resume draft is ready. Use Edit Resume to update it, then confirm it before applying.")
                : (profileOnlyLock
                    ? "Your profile resume is locked for this job application. Job-specific resume upload is disabled."
                    : "Upload once to create an editable application resume draft. If you skip this, your profile resume will be used.");
            string appliedEditUrl = string.Empty;
            string appliedNote = string.Empty;

            foreach (DataRow row in table.Rows)
            {
                row["HasApplicationResumeDraft"] = hasDraft && !isApplied;
                row["ShowApplicationResumeUpload"] = !isApplied && !hasDraft && !profileOnlyLock;
                row["ApplicationResumeEditUrl"] = editUrl;
                row["ApplicationResumeNote"] = isApplied ? string.Empty : note;
                row["ShowAppliedResumeEdit"] = false;
                row["AppliedResumeEditUrl"] = appliedEditUrl;
                row["AppliedResumeNote"] = appliedNote;
                row["ApplicationResumeProfileOnly"] = profileOnlyLock;
            }
        }

        private bool IsAppliedForJob(int userId, int jobId)
        {
            using (SqlConnection c = new SqlConnection(str))
            using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 1 FROM AppliedJobs WHERE UserId = @UserId AND JobId = @JobId", c))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@JobId", jobId);
                c.Open();
                object result = cmd.ExecuteScalar();
                return result != null && result != DBNull.Value;
            }
        }

        private void DeleteAppliedJobRecord(int appliedJobId, int userId)
        {
            using (SqlConnection c = new SqlConnection(str))
            using (SqlCommand cm = new SqlCommand("DELETE FROM AppliedJobs WHERE AppliedJobId = @AppliedJobId AND UserId = @UserId", c))
            {
                cm.Parameters.AddWithValue("@AppliedJobId", appliedJobId);
                cm.Parameters.AddWithValue("@UserId", userId);
                c.Open();
                cm.ExecuteNonQuery();
            }
        }

        private void ShowJobMessage(string message, bool success)
        {
            lblMsg.Visible = true;
            lblMsg.Text = message;
            lblMsg.CssClass = success ? "alert alert-success job-alert-popup" : "alert alert-danger job-alert-popup";
        }

        private void ShowJobPopup(string message)
        {
            string safeMessage = HttpUtility.JavaScriptStringEncode(message);
            ClientScript.RegisterStartupScript(GetType(), Guid.NewGuid().ToString("N"), "alert('" + safeMessage + "');", true);
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

        private void QueueInterviewInvite(int appliedJobId, int userId, string siteBaseUrl, string applicationPath)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    SendAutoInterviewInvite(appliedJobId, userId, siteBaseUrl, applicationPath);
                }
                catch
                {
                    // Fire-and-forget: application already succeeded.
                }
            });
        }

        private void SendAutoInterviewInvite(int appliedJobId, int userId, string siteBaseUrl, string applicationPath)
        {
            // Load job + candidate context
            JobApplicationContext ctx = LoadJobApplicationContext(appliedJobId, userId);
            if (ctx == null) return;

            if (!IsValidEmail(ctx.CandidateEmail))
                return;

            List<string> prevQ = LoadPreviousQuestions(userId, ctx.JobTitle);
            string resumeText = null;
            ApplicationResumeSelection selection;
            if (ApplicationDataStore.TryGetApplicationResumeSelection(ctx.UserId, ctx.AppliedJobId, out selection))
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
            List<string> questions = GenerateInterviewQuestions(ctx, prevQ, resumeText);
            if (questions == null || questions.Count == 0)
                questions = BuildFallbackQuestions(ctx.JobTitle, ctx.JobType, ctx.QuestionCount);

            string plainPassword;
            Guid accessToken;
            int interviewId = UpsertInterviewAndInvitation(ctx, questions, out plainPassword, out accessToken);
            if (interviewId <= 0) return;

            string normalizedAppPath = string.IsNullOrWhiteSpace(applicationPath) ? "/" : applicationPath.TrimEnd('/') + "/";
            string accessUrl = siteBaseUrl.TrimEnd('/') + normalizedAppPath + "User/InterviewAccess.aspx?token=" + accessToken;

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
                                    js.Name, js.Resume
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

        private string GetCurrentCandidateEmail(int userId)
        {
            using (SqlConnection c = new SqlConnection(str))
            using (SqlCommand cmd = new SqlCommand("SELECT Email FROM Users WHERE UserId = @UserId", c))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                c.Open();
                object result = cmd.ExecuteScalar();
                return result == null || result == DBNull.Value ? string.Empty : result.ToString();
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var address = new MailAddress(email.Trim());
                return string.Equals(address.Address, email.Trim(), StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
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

        private List<string> GenerateInterviewQuestions(JobApplicationContext ctx, List<string> prevQ, string resumeText)
        {
            try
            {
                var gemini = new GeminiService();
                using (var cts = new System.Threading.CancellationTokenSource(System.TimeSpan.FromSeconds(25)))
                {
                    var task = System.Threading.Tasks.Task.Run(async () =>
                        await gemini.GenerateQuestionsAsync(ctx.JobTitle, ctx.JobLevel, ctx.JobType, ctx.TechStack, ctx.QuestionCount, prevQ, resumeText).ConfigureAwait(false), cts.Token);
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
            //plainPassword = Utils.GenerateNumericCode(6);
            plainPassword = StaticInterviewPassword;
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
