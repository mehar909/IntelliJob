using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IntelliJob.User
{
    public partial class Profile : System.Web.UI.Page
    {
        private const string ProfileEditModeViewStateKey = "ProfileEditMode";
        private SqlConnection con;
        private SqlCommand cmd;
        private SqlDataAdapter sda;
        private DataTable dt;
        private readonly string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        private bool ProfileEditMode
        {
            get { return ViewState[ProfileEditModeViewStateKey] != null && Convert.ToBoolean(ViewState[ProfileEditModeViewStateKey]); }
            set { ViewState[ProfileEditModeViewStateKey] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["user"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                showUserProfile();
                ShowUserReplies();

                if (Session["ChatMessage"] != null)
                {
                    lblMsg.Text = Session["ChatMessage"].ToString();
                    string messageType = Session["ChatMessageType"] != null ? Session["ChatMessageType"].ToString() : "success";
                    lblMsg.CssClass = "alert alert-" + messageType + " mb-2";
                    lblMsg.Visible = true;

                    Session["ChatMessage"] = null;
                    Session["ChatMessageType"] = null;

                    ScriptManager.RegisterStartupScript(this, GetType(), "scrollAndHide",
                        "setTimeout(function() { var container = document.getElementById('chatMessagesContainer'); if(container) container.scrollTop = container.scrollHeight; var lbl = document.getElementById('" + lblMsg.ClientID + "'); if(lbl) lbl.style.display='none'; }, 3000);", true);
                }
            }
            else
            {
                string eventTarget = Request["__EVENTTARGET"];
                string eventArgument = Request["__EVENTARGUMENT"];

                if (eventTarget == "DeleteMessage" && !string.IsNullOrEmpty(eventArgument))
                {
                    string[] args = eventArgument.Split('|');
                    if (args.Length == 2)
                    {
                        int messageId = Convert.ToInt32(args[0]);
                        string messageType = args[1];
                        DeleteIndividualMessage(messageId, messageType);
                    }
                }
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            ClientScript.RegisterStartupScript(GetType(), "profileEditorToggle", "toggleProfileEditor(" + (ProfileEditMode ? "true" : "false") + ");", true);
        }

        private void showUserProfile()
        {
            con = new SqlConnection(str);
            string query = @"SELECT u.UserId, u.Username, js.Name, u.Address, js.Mobile, u.Email, u.Country, js.Resume, js.Photo,
                                    js.ResumeOriginalFileName, js.ResumeParseStatus, js.ResumeValidationMessage,
                                    js.ResumeUploadedAt, js.ResumeParsedAt, js.ResumeStructuredJson,
                                    js.ResumeRawText
                             FROM Users u
                             LEFT JOIN JobSeekers js ON u.UserId = js.ProfileId
                             WHERE u.Username = @username";

            cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@username", Session["user"]);
            sda = new SqlDataAdapter(cmd);
            dt = new DataTable();
            sda.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                // Always ensure these columns exist so Eval() in the DataList template never throws,
                // even when no resume has been parsed yet.
                EnsureResumeColumns(dt);

                DataRow row = dt.Rows[0];
                ResumeProfileDocument document = EnsureResumeDocument(Convert.ToInt32(row["UserId"]), row, true);
                if (document != null)
                {
                    ApplyDocumentToRow(row, document);
                    hfResumePreviewJson.Value = ResumeProfileService.SerializeDocument(document);
                }
                else
                {
                    hfResumePreviewJson.Value = string.Empty;
                }

                PopulateProfileEditor(row);

                dlProfile.DataSource = dt;
                dlProfile.DataBind();
            }
            else
            {
                Response.Write("<script>alert('Please login again.');</script>");
            }
        }

        private void ShowUserReplies()
        {
            con = new SqlConnection(str);
            string query = @"
                SELECT 
                    c.ContactId,
                    c.Message,
                    c.Date,
                    CASE 
                        WHEN c.Message LIKE '[ADMIN_MESSAGE]%' AND LEN(c.Message) > 15 THEN SUBSTRING(c.Message, 16, LEN(c.Message) - 15)
                        WHEN c.Message NOT LIKE '[ADMIN_MESSAGE]%' THEN ISNULL(r.ReplyMessage, '')
                        ELSE ''
                    END AS ReplyMessage,
                    CASE 
                        WHEN c.Message LIKE '[ADMIN_MESSAGE]%' THEN c.Date
                        WHEN c.Message NOT LIKE '[ADMIN_MESSAGE]%' THEN r.ReplyDate
                        ELSE NULL
                    END AS ReplyDate,
                    CASE 
                        WHEN c.Message NOT LIKE '[ADMIN_MESSAGE]%' THEN r.ReplyId
                        ELSE NULL
                    END AS ReplyId,
                    CASE 
                        WHEN c.Message LIKE '[ADMIN_MESSAGE]%' THEN 1
                        ELSE 0
                    END AS IsAdminMessage,
                    ISNULL(u.Username, c.Name) AS Username
                FROM AdminContact c
                LEFT JOIN AdminReply r ON c.ContactId = r.ContactId AND c.Message NOT LIKE '[ADMIN_MESSAGE]%'
                LEFT JOIN Users u ON c.UserId = u.UserId
                WHERE c.UserId = (SELECT UserId FROM Users WHERE Username = @username)
                ORDER BY c.Date ASC";

            cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@username", Session["user"]);
            sda = new SqlDataAdapter(cmd);
            DataTable repliesTable = new DataTable();
            sda.Fill(repliesTable);

            rptChat.DataSource = null;
            rptChat.DataBind();

            if (repliesTable.Rows.Count > 0)
            {
                rptChat.DataSource = repliesTable;
                rptChat.DataBind();
                noMessages.Visible = false;
            }
            else
            {
                noMessages.Visible = true;
            }

            ScriptManager.RegisterStartupScript(this, GetType(), "scrollToBottom",
                "setTimeout(function() { var container = document.getElementById('chatMessagesContainer'); if(container) container.scrollTop = container.scrollHeight; }, 100);", true);
        }

        protected void dlProfile_ItemCommand(object source, DataListCommandEventArgs e)
        {
            if (e.CommandName == "EditProfile")
            {
                ProfileEditMode = true;
                PopulateProfileEditor(dt != null && dt.Rows.Count > 0 ? dt.Rows[0] : null);
                showUserProfile();
                return;
            }

            if (e.CommandName == "EditResume")
            {
                Response.Redirect("ResumeBuild.aspx?id=" + e.CommandArgument.ToString());
                return;
            }

            if (e.CommandName == "ImportResume")
            {
                int userId = Convert.ToInt32(e.CommandArgument);
                FileUpload fuResumeImport = e.Item.FindControl("fuResumeImport") as FileUpload;

                if (fuResumeImport == null || !fuResumeImport.HasFile)
                {
                    ShowProfileMessage("Please choose a resume file to import.", true);
                    return;
                }

                if (!Utils.IsValidExtension4Resume(fuResumeImport.FileName))
                {
                    ShowProfileMessage("Please upload a .doc, .docx, or .pdf resume.", true);
                    return;
                }

                ResumeImportResult importResult = ResumeProfileService.ImportAndParse(
                    fuResumeImport.PostedFile,
                    Server.MapPath("~/Resumes"),
                    "Resumes");

                if (!importResult.IsSuccess || importResult.Document == null)
                {
                    ShowProfileMessage(string.IsNullOrWhiteSpace(importResult.ValidationMessage)
                        ? "The resume could not be imported."
                        : importResult.ValidationMessage, true);
                    return;
                }

                SaveResumeMetadata(userId, importResult.Document, importResult.StoredRelativePath);
                ShowProfileMessage("Resume imported and profile sections were refreshed.", false);
                showUserProfile();
                return;
            }

            if (e.CommandName == "DeleteResume")
            {
                int userId = Convert.ToInt32(e.CommandArgument);
                DeleteResumeForUser(userId);
                showUserProfile();
            }
        }

        protected void dlProfile_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (e.Item == null || (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem))
                return;

            Literal litProfileHtmlPreviewFrame = e.Item.FindControl("litProfileHtmlPreviewFrame") as Literal;
            if (litProfileHtmlPreviewFrame != null)
                litProfileHtmlPreviewFrame.Text = BuildHtmlPreviewFrameMarkup();
        }

        protected void btnSaveProfile_Click(object sender, EventArgs e)
        {
            try
            {
                int userId = Convert.ToInt32(Session["userId"]);

                if (!string.IsNullOrWhiteSpace(txtProfilePassword.Text) || !string.IsNullOrWhiteSpace(txtProfileConfirmPassword.Text))
                {
                    if (!string.Equals(txtProfilePassword.Text.Trim(), txtProfileConfirmPassword.Text.Trim(), StringComparison.Ordinal))
                    {
                        ShowProfileEditorMessage("Password and confirm password do not match.", true);
                        return;
                    }
                }

                string photoFileName = GetCurrentProfilePhoto(userId);
                if (fuProfilePhoto.HasFile)
                {
                    if (!Utils.IsValidExtension(fuProfilePhoto.FileName))
                    {
                        ShowProfileEditorMessage("Please select a .png, .jpg, or .jpeg file for photo.", true);
                        return;
                    }

                    photoFileName = fuProfilePhoto.FileName;
                    fuProfilePhoto.PostedFile.SaveAs(Server.MapPath("~/photos/") + photoFileName);
                }

                using (SqlConnection profileCon = new SqlConnection(str))
                {
                    profileCon.Open();
                    using (SqlTransaction tran = profileCon.BeginTransaction())
                    {
                        try
                        {
                            string currentPassword = GetCurrentProfilePassword(userId, profileCon, tran);
                            string passwordToSave = currentPassword;
                            if (!string.IsNullOrWhiteSpace(txtProfilePassword.Text))
                            {
                                // Hash new password with salted SHA-256
                                passwordToSave = IntelliJob.Utils.CreateSaltedHash(txtProfilePassword.Text.Trim());
                            }

                            using (SqlCommand userCmd = new SqlCommand(@"UPDATE Users SET Username=@Username, Password=@Password, Email=@Email, Address=@Address, Country=@Country, UpdatedAt=GETDATE() WHERE UserId=@UserId", profileCon, tran))
                            {
                                userCmd.Parameters.AddWithValue("@Username", txtProfileUserName.Text.Trim());
                                userCmd.Parameters.AddWithValue("@Password", passwordToSave);
                                userCmd.Parameters.AddWithValue("@Email", txtProfileEmail.Text.Trim());
                                userCmd.Parameters.AddWithValue("@Address", txtProfileAddress.Text.Trim());
                                userCmd.Parameters.AddWithValue("@Country", ddlProfileCountry.SelectedValue);
                                userCmd.Parameters.AddWithValue("@UserId", userId);
                                userCmd.ExecuteNonQuery();
                            }

                            bool profileExists;
                            using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM JobSeekers WHERE ProfileId = @UserId", profileCon, tran))
                            {
                                checkCmd.Parameters.AddWithValue("@UserId", userId);
                                profileExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                            }

                            string jobSeekerSql = profileExists
                                ? @"UPDATE JobSeekers SET Name=@Name, Mobile=@Mobile, Photo=@Photo WHERE ProfileId=@ProfileId"
                                          : @"INSERT INTO JobSeekers (ProfileId, Name, Mobile, Photo, Resume, ResumeOriginalFileName, ResumeParseStatus, ResumeValidationMessage, ResumeUploadedAt, ResumeParsedAt, ResumeStructuredJson, ResumeRawText)
                                              VALUES (@ProfileId, @Name, @Mobile, @Photo, NULL, NULL, 'none', NULL, NULL, NULL, NULL, NULL)";

                            using (SqlCommand jsCmd = new SqlCommand(jobSeekerSql, profileCon, tran))
                            {
                                jsCmd.Parameters.AddWithValue("@ProfileId", userId);
                                jsCmd.Parameters.AddWithValue("@Name", txtProfileFullName.Text.Trim());
                                jsCmd.Parameters.AddWithValue("@Mobile", txtProfileMobile.Text.Trim());
                                jsCmd.Parameters.AddWithValue("@Photo", string.IsNullOrWhiteSpace(photoFileName) ? "avatar.png" : photoFileName);
                                jsCmd.ExecuteNonQuery();
                            }

                            tran.Commit();
                        }
                        catch
                        {
                            tran.Rollback();
                            throw;
                        }
                    }
                }

                Session["user"] = txtProfileUserName.Text.Trim();
                ProfileEditMode = false;
                ShowProfileEditorMessage("Profile updated successfully.", false);
                showUserProfile();
            }
            catch (Exception ex)
            {
                ProfileEditMode = true;
                ShowProfileEditorMessage("Error updating profile: " + ex.Message, true);
            }
        }

        private void DeleteResumeForUser(int userId)
        {
            string resumePath = string.Empty;
            using (SqlConnection resumeCon = new SqlConnection(str))
            using (SqlCommand resumeCmd = new SqlCommand("SELECT Resume FROM JobSeekers WHERE ProfileId = @UserId", resumeCon))
            {
                resumeCmd.Parameters.AddWithValue("@UserId", userId);
                resumeCon.Open();
                object resumeValue = resumeCmd.ExecuteScalar();
                if (resumeValue != null && resumeValue != DBNull.Value)
                    resumePath = resumeValue.ToString();
            }

            if (!string.IsNullOrWhiteSpace(resumePath))
            {
                string physicalPath = ResolveStoredPath(resumePath);
                if (!string.IsNullOrWhiteSpace(physicalPath) && File.Exists(physicalPath))
                    File.Delete(physicalPath);
            }

            using (SqlConnection conUpdate = new SqlConnection(str))
            using (SqlCommand cmdUpdate = new SqlCommand(@"UPDATE JobSeekers SET 
                                                            Resume = NULL,
                                                            ResumeOriginalFileName = NULL,
                                                            ResumeParseStatus = 'none',
                                                            ResumeValidationMessage = NULL,
                                                            ResumeUploadedAt = NULL,
                                                            ResumeParsedAt = NULL,
                                                            ResumeStructuredJson = NULL,
                                                            ResumeRawText = NULL
                                                            WHERE ProfileId = @UserId", conUpdate))
            {
                cmdUpdate.Parameters.AddWithValue("@UserId", userId);
                conUpdate.Open();
                cmdUpdate.ExecuteNonQuery();
            }
        }

        private ResumeProfileDocument EnsureResumeDocument(int userId, DataRow row, bool persistLegacyData)
        {
            if (row == null)
                return null;

            string structuredJson = GetStringValue(row, "ResumeStructuredJson");
            ResumeProfileDocument document = ResumeProfileService.DeserializeDocument(structuredJson);
            if (document != null)
                return document;

            string resumePath = GetStringValue(row, "Resume");
            if (string.IsNullOrWhiteSpace(resumePath))
                return null;

            string physicalPath = ResolveStoredPath(resumePath);
            if (string.IsNullOrWhiteSpace(physicalPath) || !File.Exists(physicalPath))
                return null;

            ResumeImportResult legacyImport = ResumeProfileService.ParseExistingResume(physicalPath, resumePath, GetStringValue(row, "ResumeOriginalFileName"));
            if (legacyImport == null || legacyImport.Document == null || !legacyImport.Document.IsValid)
                return legacyImport != null ? legacyImport.Document : null;

            if (persistLegacyData)
                SaveResumeMetadata(userId, legacyImport.Document, resumePath);

            return legacyImport.Document;
        }

        private void SaveResumeMetadata(int userId, ResumeProfileDocument document, string resumePath)
        {
            if (document == null)
                return;

            using (SqlConnection resumeCon = new SqlConnection(str))
            {
                string updateQuery = @"UPDATE JobSeekers SET
                    Name = CASE WHEN (Name IS NULL OR LTRIM(RTRIM(Name)) = '') AND @ParsedName <> '' THEN @ParsedName ELSE Name END,
                    Mobile = CASE WHEN (Mobile IS NULL OR LTRIM(RTRIM(Mobile)) = '') AND @ParsedMobile <> '' THEN @ParsedMobile ELSE Mobile END,
                    Resume = @Resume,
                    ResumeOriginalFileName = @ResumeOriginalFileName,
                    ResumeParseStatus = @ResumeParseStatus,
                    ResumeValidationMessage = @ResumeValidationMessage,
                    ResumeUploadedAt = @ResumeUploadedAt,
                    ResumeParsedAt = @ResumeParsedAt,
                    ResumeStructuredJson = @ResumeStructuredJson,
                    ResumeRawText = @ResumeRawText
                    WHERE ProfileId = @UserId";

                using (SqlCommand cmdUpdate = new SqlCommand(updateQuery, resumeCon))
                {
                    cmdUpdate.Parameters.AddWithValue("@UserId", userId);
                    cmdUpdate.Parameters.AddWithValue("@ParsedName", string.IsNullOrWhiteSpace(document.FullName) ? string.Empty : document.FullName);
                    cmdUpdate.Parameters.AddWithValue("@ParsedMobile", string.IsNullOrWhiteSpace(document.Mobile) ? string.Empty : document.Mobile);
                    ResumeProfileService.AddResumeProfileParameters(cmdUpdate, document, resumePath);
                    resumeCon.Open();
                    cmdUpdate.ExecuteNonQuery();
                }
            }
        }

        private void PopulateProfileEditor(DataRow row)
        {
            if (row == null)
                return;

            txtProfileUserName.Text = GetStringValue(row, "Username");
            txtProfileFullName.Text = GetStringValue(row, "Name");
            txtProfileAddress.Text = GetStringValue(row, "Address");
            txtProfileMobile.Text = GetStringValue(row, "Mobile");
            txtProfileEmail.Text = GetStringValue(row, "Email");
            txtProfilePassword.Text = string.Empty;
            txtProfileConfirmPassword.Text = string.Empty;

            string country = GetStringValue(row, "Country");
            if (!string.IsNullOrWhiteSpace(country) && ddlProfileCountry.Items.FindByValue(country) != null)
                ddlProfileCountry.SelectedValue = country;
        }

        private void ApplyDocumentToRow(DataRow row, ResumeProfileDocument document)
        {
            if (row == null || document == null)
                return;

            row["ResumeParseStatus"] = document.IsValid ? "ready" : "failed";
            row["ResumeValidationMessage"] = document.ValidationMessage ?? string.Empty;
            row["ResumeOriginalFileName"] = document.OriginalFileName ?? string.Empty;
            row["ResumeStructuredJson"] = ResumeProfileService.SerializeDocument(document);
            row["ResumeRawText"] = document.RawText ?? string.Empty;

            SetOrAddStringColumn(row, "ResumeHeadline", !string.IsNullOrWhiteSpace(document.Headline) ? document.Headline : string.Empty);
            SetOrAddStringColumn(row, "ResumeSummary", !string.IsNullOrWhiteSpace(document.ProfessionalSummary) ? document.ProfessionalSummary : document.Summary);
            SetOrAddStringColumn(row, "ResumeSkills", JoinLines(document.Skills));
            SetOrAddStringColumn(row, "ResumeEducation", JoinLines(document.Education));
            SetOrAddStringColumn(row, "ResumeExperienceDetails", JoinLines(document.Experience));
            SetOrAddStringColumn(row, "ResumeProjects", JoinLines(document.Projects));
            SetOrAddStringColumn(row, "ResumeCertifications", JoinLines(document.Certifications));
            SetOrAddStringColumn(row, "ResumeLanguages", JoinLines(document.Languages));


        }

        private void SetOrAddStringColumn(DataRow row, string columnName, string value)
        {
            if (row == null || row.Table == null)
                return;

            if (!row.Table.Columns.Contains(columnName))
                row.Table.Columns.Add(columnName, typeof(string));

            row[columnName] = value ?? string.Empty;
        }

        /// <summary>
        /// Guarantees all virtual resume columns exist in the DataTable with empty-string defaults
        /// before DataBind is called, so Eval() in the template never throws even when no resume
        /// has been parsed yet.
        /// </summary>
        private static void EnsureResumeColumns(DataTable table)
        {
            string[] columns = { "ResumeHeadline", "ResumeSummary", "ResumeSkills",
                                  "ResumeEducation", "ResumeExperienceDetails",
                                  "ResumeProjects", "ResumeCertifications", "ResumeLanguages" };
            foreach (string col in columns)
            {
                if (!table.Columns.Contains(col))
                    table.Columns.Add(col, typeof(string));
            }
            // Ensure every row has an empty string default for newly added columns
            foreach (DataRow row in table.Rows)
            {
                foreach (string col in columns)
                {
                    if (row[col] == DBNull.Value || row[col] == null)
                        row[col] = string.Empty;
                }
            }
        }

        protected string BuildResumeOverviewHtml(object statusObj, object validationObj, object fileNameObj, object headlineObj, object summaryObj, object skillsObj, object educationObj, object experienceObj, object projectsObj, object certificationsObj, object languagesObj)
        {
            StringBuilder builder = new StringBuilder();
            string status = statusObj != DBNull.Value && statusObj != null ? statusObj.ToString() : string.Empty;
            string validation = validationObj != DBNull.Value && validationObj != null ? validationObj.ToString() : string.Empty;
            string fileName = fileNameObj != DBNull.Value && fileNameObj != null ? fileNameObj.ToString() : string.Empty;

            builder.Append("<div class='resume-overview' style='padding:12px;border:1px solid #e5e5e5;border-radius:8px;background:#fafafa;'>");
            builder.Append("<p class='mb-1'><strong>Status:</strong> ").Append(Server.HtmlEncode(string.IsNullOrWhiteSpace(status) ? "none" : status)).Append("</p>");

            if (!string.IsNullOrWhiteSpace(fileName))
                builder.Append("<p class='mb-1'><strong>File:</strong> ").Append(Server.HtmlEncode(fileName)).Append("</p>");

            if (!string.IsNullOrWhiteSpace(validation))
                builder.Append("<p class='mb-1 text-danger'><strong>Validation:</strong> ").Append(Server.HtmlEncode(validation)).Append("</p>");

            builder.Append(BuildResumeSectionHtml("Summary", summaryObj));
            builder.Append(BuildResumeSectionHtml("Skills", skillsObj));
            builder.Append(BuildResumeSectionHtml("Education", educationObj));
            builder.Append(BuildResumeSectionHtml("Experience", experienceObj));
            builder.Append(BuildResumeSectionHtml("Projects", projectsObj));
            builder.Append(BuildResumeSectionHtml("Certifications", certificationsObj));
            builder.Append(BuildResumeSectionHtml("Languages", languagesObj));

            builder.Append("</div>");
            return builder.ToString();
        }

        private string BuildResumeSectionHtml(string title, object value)
        {
            string text = value != DBNull.Value && value != null ? value.ToString() : string.Empty;
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return "<div style='margin-top:8px;'><strong>" + Server.HtmlEncode(title) + "</strong><div style='white-space:pre-wrap;'>" + Server.HtmlEncode(text) + "</div></div>";
        }

        private string BuildHtmlPreviewFrameMarkup()
        {
            string json = hfResumePreviewJson != null ? hfResumePreviewJson.Value : string.Empty;
            if (string.IsNullOrWhiteSpace(json))
                return "<div class='muted-box'>No HTML preview data is available yet.</div>";

            string encoded = HttpUtility.UrlEncode(json).Replace("+", "%20");
            string src = ResolveUrl("~/ResumePreview.html") + "#data=" + encoded;
            return "<iframe class='html-preview-frame' src='" + HttpUtility.HtmlAttributeEncode(src) + "' title='HTML Resume Preview'></iframe>";
        }

        private string ResolveStoredPath(string storedPath)
        {
            if (string.IsNullOrWhiteSpace(storedPath))
                return string.Empty;

            if (Path.IsPathRooted(storedPath))
                return storedPath;

            string relativePath = storedPath.Replace('\\', '/').TrimStart('/');
            if (relativePath.StartsWith("~/", StringComparison.Ordinal))
                relativePath = relativePath.Substring(2);

            return Server.MapPath("~/" + relativePath);
        }

        private string GetStringValue(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return string.Empty;

            return row[columnName].ToString();
        }

        private List<string> SplitLines(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new List<string>();

            return value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private string JoinLines(IEnumerable<string> lines)
        {
            if (lines == null)
                return string.Empty;

            return string.Join(Environment.NewLine, lines.Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Trim()));
        }

        private void ShowProfileMessage(string message, bool isDanger)
        {
            lblMsg.Text = message;
            lblMsg.CssClass = isDanger ? "alert alert-danger mb-2" : "alert alert-success mb-2";
            lblMsg.Visible = true;
        }

        private void ShowProfileEditorMessage(string message, bool isDanger)
        {
            lblProfileMsg.Text = message;
            lblProfileMsg.CssClass = isDanger ? "alert alert-danger mb-2" : "alert alert-success mb-2";
            lblProfileMsg.Visible = true;
        }

        private string GetCurrentProfilePassword(int userId, SqlConnection openConnection, SqlTransaction tran)
        {
            using (SqlCommand cmdPassword = new SqlCommand("SELECT Password FROM Users WHERE UserId = @UserId", openConnection, tran))
            {
                cmdPassword.Parameters.AddWithValue("@UserId", userId);
                object result = cmdPassword.ExecuteScalar();
                return result == null || result == DBNull.Value ? string.Empty : result.ToString();
            }
        }

        private string GetCurrentProfilePhoto(int userId)
        {
            using (SqlConnection profileCon = new SqlConnection(str))
            using (SqlCommand cmdPhoto = new SqlCommand("SELECT Photo FROM JobSeekers WHERE ProfileId = @UserId", profileCon))
            {
                cmdPhoto.Parameters.AddWithValue("@UserId", userId);
                profileCon.Open();
                object result = cmdPhoto.ExecuteScalar();
                return result == null || result == DBNull.Value ? string.Empty : result.ToString();
            }
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            if (txtMessage.Text.Trim() == string.Empty)
                return;

            con = new SqlConnection(str);
            string queryProfile = @"SELECT u.UserId, js.Name, u.Email 
                                    FROM Users u 
                                    LEFT JOIN JobSeekers js ON u.UserId = js.ProfileId 
                                    WHERE u.Username = @username";
            cmd = new SqlCommand(queryProfile, con);
            cmd.Parameters.AddWithValue("@username", Session["user"]);
            sda = new SqlDataAdapter(cmd);
            dt = new DataTable();
            sda.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                string queryInsert = "INSERT INTO AdminContact (UserId, Name, Email, Message, Date) VALUES (@UserId, @Name, @Email, @Message, @Date)";
                SqlCommand cmdInsert = new SqlCommand(queryInsert, con);

                cmdInsert.Parameters.AddWithValue("@UserId", row["UserId"]);
                cmdInsert.Parameters.AddWithValue("@Name", row["Name"]);
                cmdInsert.Parameters.AddWithValue("@Email", row["Email"]);
                cmdInsert.Parameters.AddWithValue("@Message", txtMessage.Text.Trim());
                cmdInsert.Parameters.AddWithValue("@Date", DateTime.Now);

                con.Open();
                int r = cmdInsert.ExecuteNonQuery();
                con.Close();

                if (r > 0)
                {
                    Session["ChatMessage"] = "Message sent to admin successfully.";
                    Session["ChatMessageType"] = "success";
                    Response.Redirect("Profile.aspx", false);
                }
                else
                {
                    ShowProfileMessage("Failed to send message. Try again.", true);
                }
            }
            else
            {
                ShowProfileMessage("User data not found.", true);
            }
        }

        protected string FormatChatMessage(object contactIdObj, object messageObj, object dateObj, object replyMessageObj, object replyDateObj, object replyIdObj, object isAdminMessageObj, object usernameObj)
        {
            int contactId = contactIdObj != null && contactIdObj != DBNull.Value ? Convert.ToInt32(contactIdObj) : 0;
            string message = messageObj != null && messageObj != DBNull.Value ? messageObj.ToString() : string.Empty;
            string replyMessage = replyMessageObj != null && replyMessageObj != DBNull.Value ? replyMessageObj.ToString() : string.Empty;
            int? replyId = replyIdObj != null && replyIdObj != DBNull.Value ? (int?)Convert.ToInt32(replyIdObj) : null;

            bool hasAdminPrefix = !string.IsNullOrEmpty(message) && message.StartsWith("[ADMIN_MESSAGE]");
            bool isAdminMessage = hasAdminPrefix;
            bool hasOldFormatReply = !isAdminMessage && !string.IsNullOrEmpty(replyMessage) && replyId.HasValue;

            string output = string.Empty;
            string deleteButtonStyle = "background: none; border: none; color: #ffffff; cursor: pointer; padding: 2px 5px; font-size: 12px; opacity: 0.7; margin-left: 5px;";
            string deleteButtonStyleRed = "background: none; border: none; color: #dc3545; cursor: pointer; padding: 2px 5px; font-size: 12px; opacity: 0.7; margin-left: 5px;";
            string deleteButtonHover = "onmouseover=\"this.style.opacity='1'\" onmouseout=\"this.style.opacity='0.7'\"";

            if (isAdminMessage)
            {
                string adminMsg = string.IsNullOrEmpty(replyMessage) ? (message.StartsWith("[ADMIN_MESSAGE]") && message.Length > 15 ? message.Substring(15) : message) : replyMessage;
                string dateStr = dateObj != null && dateObj != DBNull.Value ? Convert.ToDateTime(dateObj).ToString("MMM dd, HH:mm") : string.Empty;
                string deleteBtn = "<button type=\"button\" " + deleteButtonHover + " style=\"" + deleteButtonStyleRed + "\" onclick=\"deleteMessage('" + contactId + "', 'admin')\" title=\"Delete message\"><i class=\"fas fa-trash\"></i></button>";
                output = "<div class=\"d-flex justify-content-start mb-2\" style=\"width: 100%;\"><div class=\"message-bubble admin-message\" style=\"position: relative; margin-left: 0; margin-right: auto;\"><div style=\"font-weight: bold; color: #7200cf; margin-bottom: 5px; font-size: 14px;\">Admin " + deleteBtn + "</div><div>" + Server.HtmlEncode(adminMsg) + "</div><div class=\"message-time\">" + dateStr + "</div></div></div>";
            }
            else if (hasOldFormatReply)
            {
                string dateStr = dateObj != null && dateObj != DBNull.Value ? Convert.ToDateTime(dateObj).ToString("MMM dd, HH:mm") : string.Empty;
                string replyDateStr = replyDateObj != null && replyDateObj != DBNull.Value ? Convert.ToDateTime(replyDateObj).ToString("MMM dd, HH:mm") : string.Empty;
                string userDeleteBtn = "<button type=\"button\" " + deleteButtonHover + " style=\"" + deleteButtonStyle + "\" onclick=\"deleteMessage('" + contactId + "', 'user')\" title=\"Delete message\"><i class=\"fas fa-trash\"></i></button>";
                string replyDeleteBtn = replyId.HasValue ? "<button type=\"button\" " + deleteButtonHover + " style=\"" + deleteButtonStyleRed + "\" onclick=\"deleteMessage('" + replyId.Value + "', 'reply')\" title=\"Delete reply\"><i class=\"fas fa-trash\"></i></button>" : string.Empty;
                output = "<div class=\"d-flex justify-content-end mb-2\" style=\"width: 100%;\"><div class=\"message-bubble user-message\" style=\"position: relative;\"><div>" + Server.HtmlEncode(message) + " " + userDeleteBtn + "</div><div class=\"message-time text-right\" style=\"color: rgba(255,255,255,0.9);\">" + dateStr + "</div></div></div>";
                output += "<div class=\"d-flex justify-content-start mb-2\" style=\"width: 100%;\"><div class=\"message-bubble admin-message\" style=\"position: relative; margin-left: 0; margin-right: auto;\"><div style=\"font-weight: bold; color: #7200cf; margin-bottom: 5px; font-size: 14px;\">Admin " + replyDeleteBtn + "</div><div>" + Server.HtmlEncode(replyMessage) + "</div><div class=\"message-time\">" + replyDateStr + "</div></div></div>";
            }
            else
            {
                string dateStr = dateObj != null && dateObj != DBNull.Value ? Convert.ToDateTime(dateObj).ToString("MMM dd, HH:mm") : string.Empty;
                string userDeleteBtn = "<button type=\"button\" " + deleteButtonHover + " style=\"" + deleteButtonStyle + "\" onclick=\"deleteMessage('" + contactId + "', 'user')\" title=\"Delete message\"><i class=\"fas fa-trash\"></i></button>";
                output = "<div class=\"d-flex justify-content-end mb-2\" style=\"width: 100%;\"><div class=\"message-bubble user-message\" style=\"position: relative;\"><div>" + Server.HtmlEncode(message) + " " + userDeleteBtn + "</div><div class=\"message-time text-right\" style=\"color: rgba(255,255,255,0.9);\">" + dateStr + "</div></div></div>";
            }

            return output;
        }

        protected void btnDeleteChat_Click(object sender, EventArgs e)
        {
            try
            {
                int userId = GetCurrentUserId();
                if (userId <= 0)
                    return;

                using (SqlConnection connection = new SqlConnection(str))
                {
                    connection.Open();
                    using (SqlCommand deleteReplies = new SqlCommand("DELETE FROM AdminReply WHERE ContactId IN (SELECT ContactId FROM AdminContact WHERE UserId = @UserId)", connection))
                    {
                        deleteReplies.Parameters.AddWithValue("@UserId", userId);
                        deleteReplies.ExecuteNonQuery();
                    }

                    using (SqlCommand deleteContact = new SqlCommand("DELETE FROM AdminContact WHERE UserId = @UserId", connection))
                    {
                        deleteContact.Parameters.AddWithValue("@UserId", userId);
                        int deleted = deleteContact.ExecuteNonQuery();

                        if (deleted > 0)
                            ShowProfileMessage("Chat deleted successfully.", false);
                        else
                            ShowProfileMessage("No chat found to delete.", false);
                    }
                }

                rptChat.DataSource = null;
                rptChat.DataBind();
                ShowUserReplies();
            }
            catch (Exception ex)
            {
                ShowProfileMessage("Error deleting chat: " + ex.Message, true);
            }
        }

        protected void DeleteIndividualMessage(int messageId, string messageType)
        {
            try
            {
                if (messageType == "reply")
                {
                    using (SqlConnection connection = new SqlConnection(str))
                    using (SqlCommand command = new SqlCommand("DELETE FROM AdminReply WHERE ReplyId = @ReplyId", connection))
                    {
                        command.Parameters.AddWithValue("@ReplyId", messageId);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    using (SqlConnection connection = new SqlConnection(str))
                    using (SqlCommand command = new SqlCommand("DELETE FROM AdminReply WHERE ContactId = @ContactId; DELETE FROM AdminContact WHERE ContactId = @ContactId", connection))
                    {
                        command.Parameters.AddWithValue("@ContactId", messageId);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                rptChat.DataSource = null;
                rptChat.DataBind();
                ShowUserReplies();

                if (CountRemainingMessages(GetCurrentUserId()) == 0)
                    ShowProfileMessage("Message deleted. Chat cleared as no messages remain.", false);
                else
                    ShowProfileMessage("Message deleted successfully.", false);
            }
            catch (Exception ex)
            {
                ShowProfileMessage("Error deleting message: " + ex.Message, true);
            }
        }

        private int CountRemainingMessages(int userId)
        {
            using (SqlConnection connection = new SqlConnection(str))
            using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM AdminContact WHERE UserId = @UserId", connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private int GetCurrentUserId()
        {
            using (SqlConnection connection = new SqlConnection(str))
            using (SqlCommand command = new SqlCommand("SELECT UserId FROM Users WHERE Username = @Username", connection))
            {
                command.Parameters.AddWithValue("@Username", Session["user"]);
                connection.Open();
                object result = command.ExecuteScalar();
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
        }


    }
}
