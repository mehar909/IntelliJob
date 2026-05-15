using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace IntelliJob.User
{
    public partial class ResumeBuild : System.Web.UI.Page
    {
        private readonly string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["user"] == null)
            {
                Response.Redirect("../User/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                if (Request.QueryString["id"] == null)
                {
                    Response.Redirect("Login.aspx");
                    return;
                }

                LoadProfileForEditing();
            }
        }

        private void LoadProfileForEditing()
        {
            int userId = Convert.ToInt32(Request.QueryString["id"]);

            using (SqlConnection con = new SqlConnection(str))
            {
                string query = @"SELECT u.UserId, u.Username, u.Email, u.Address, u.Country,
                                        js.Name, js.Mobile, js.TenthGrade, js.TwelfthGrade, js.GraduationGrade,
                                        js.PostGraduationGrade, js.Phd, js.WorksOn, js.Experience, js.Resume,
                                        js.ResumeOriginalFileName, js.ResumeParseStatus, js.ResumeValidationMessage,
                                        js.ResumeUploadedAt, js.ResumeParsedAt, js.ResumeStructuredJson,
                                        js.ResumeHeadline, js.ResumeSummary, js.ResumeSkills, js.ResumeEducation,
                                        js.ResumeExperienceDetails, js.ResumeProjects, js.ResumeCertifications,
                                        js.ResumeLanguages, js.ResumeRawText
                                 FROM Users u
                                 LEFT JOIN JobSeekers js ON u.UserId = js.ProfileId
                                 WHERE u.UserId = @UserId";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            lblMsg.Visible = true;
                            lblMsg.Text = "User not found!";
                            lblMsg.CssClass = "alert alert-danger";
                            return;
                        }

                        if (!reader.Read())
                            return;

                        ResumeProfileDocument document = BuildDocumentFromReader(reader, userId);
                        PopulateForm(reader, document);
                    }
                }
            }
        }

        private ResumeProfileDocument BuildDocumentFromReader(SqlDataReader reader, int userId)
        {
            ResumeProfileDocument document = null;

            string structuredJson = GetReaderString(reader, "ResumeStructuredJson");
            if (!string.IsNullOrWhiteSpace(structuredJson))
                document = ResumeProfileService.DeserializeDocument(structuredJson);

            string resumePath = GetReaderString(reader, "Resume");
            if (document == null && !string.IsNullOrWhiteSpace(resumePath))
            {
                string physicalPath = ResolveStoredPath(resumePath);
                if (!string.IsNullOrWhiteSpace(physicalPath) && File.Exists(physicalPath))
                {
                    ResumeImportResult legacyImport = ResumeProfileService.ParseExistingResume(
                        physicalPath,
                        resumePath,
                        GetReaderString(reader, "ResumeOriginalFileName"));

                    if (legacyImport != null && legacyImport.Document != null)
                    {
                        document = legacyImport.Document;
                        if (document.IsValid)
                            PersistResumeMetadata(userId, document, resumePath);
                        else
                        {
                            lblMsg.Visible = true;
                            lblMsg.Text = document.ValidationMessage;
                            lblMsg.CssClass = "alert alert-warning";
                        }
                    }
                }
            }

            if (document == null)
                document = new ResumeProfileDocument();

            document.FullName = string.IsNullOrWhiteSpace(document.FullName) ? GetReaderString(reader, "Name") : document.FullName;
            document.Email = string.IsNullOrWhiteSpace(document.Email) ? GetReaderString(reader, "Email") : document.Email;
            document.Mobile = string.IsNullOrWhiteSpace(document.Mobile) ? GetReaderString(reader, "Mobile") : document.Mobile;
            document.Address = string.IsNullOrWhiteSpace(document.Address) ? GetReaderString(reader, "Address") : document.Address;
            document.Headline = string.IsNullOrWhiteSpace(document.Headline) ? GetReaderString(reader, "ResumeHeadline") : document.Headline;
            document.Summary = string.IsNullOrWhiteSpace(document.Summary) ? GetReaderString(reader, "ResumeSummary") : document.Summary;
            document.RawText = string.IsNullOrWhiteSpace(document.RawText) ? GetReaderString(reader, "ResumeRawText") : document.RawText;

            if ((document.Skills == null || document.Skills.Count == 0) && !string.IsNullOrWhiteSpace(GetReaderString(reader, "ResumeSkills")))
                document.Skills = SplitLines(GetReaderString(reader, "ResumeSkills"));

            if ((document.Education == null || document.Education.Count == 0) && !string.IsNullOrWhiteSpace(GetReaderString(reader, "ResumeEducation")))
                document.Education = SplitLines(GetReaderString(reader, "ResumeEducation"));

            if ((document.Experience == null || document.Experience.Count == 0) && !string.IsNullOrWhiteSpace(GetReaderString(reader, "ResumeExperienceDetails")))
                document.Experience = SplitLines(GetReaderString(reader, "ResumeExperienceDetails"));

            if ((document.Projects == null || document.Projects.Count == 0) && !string.IsNullOrWhiteSpace(GetReaderString(reader, "ResumeProjects")))
                document.Projects = SplitLines(GetReaderString(reader, "ResumeProjects"));

            if ((document.Certifications == null || document.Certifications.Count == 0) && !string.IsNullOrWhiteSpace(GetReaderString(reader, "ResumeCertifications")))
                document.Certifications = SplitLines(GetReaderString(reader, "ResumeCertifications"));

            if ((document.Languages == null || document.Languages.Count == 0) && !string.IsNullOrWhiteSpace(GetReaderString(reader, "ResumeLanguages")))
                document.Languages = SplitLines(GetReaderString(reader, "ResumeLanguages"));

            return document;
        }

        private void PopulateForm(SqlDataReader reader, ResumeProfileDocument document)
        {
            txtUserName.Text = GetReaderString(reader, "Username");
            txtFullName.Text = document != null && !string.IsNullOrWhiteSpace(document.FullName) ? document.FullName : GetReaderString(reader, "Name");
            txtEmail.Text = GetReaderString(reader, "Email");
            txtMobile.Text = document != null && !string.IsNullOrWhiteSpace(document.Mobile) ? document.Mobile : GetReaderString(reader, "Mobile");
            txtTenth.Text = GetReaderString(reader, "TenthGrade");
            txtTwelfth.Text = GetReaderString(reader, "TwelfthGrade");
            txtGraduation.Text = GetReaderString(reader, "GraduationGrade");
            txtPostGraduation.Text = GetReaderString(reader, "PostGraduationGrade");
            txtPhd.Text = GetReaderString(reader, "Phd");
            txtWork.Text = !string.IsNullOrWhiteSpace(GetReaderString(reader, "WorksOn")) ? GetReaderString(reader, "WorksOn") : (document != null ? document.Headline : string.Empty);
            txtExperience.Text = !string.IsNullOrWhiteSpace(GetReaderString(reader, "Experience")) ? GetReaderString(reader, "Experience") : (document != null && document.Experience != null && document.Experience.Count > 0 ? document.Experience[0] : string.Empty);
            txtAddress.Text = GetReaderString(reader, "Address");

            string country = GetReaderString(reader, "Country");
            if (!string.IsNullOrWhiteSpace(country) && ddlCountry.Items.FindByValue(country) != null)
                ddlCountry.SelectedValue = country;

            txtResumeHeadline.Text = document != null ? document.Headline : string.Empty;
            txtResumeSummary.Text = document != null ? document.Summary : string.Empty;
            txtResumeSkills.Text = JoinLines(document != null ? document.Skills : null);
            txtResumeEducation.Text = JoinLines(document != null ? document.Education : null);
            txtResumeExperienceDetails.Text = JoinLines(document != null ? document.Experience : null);
            txtResumeProjects.Text = JoinLines(document != null ? document.Projects : null);
            txtResumeCertifications.Text = JoinLines(document != null ? document.Certifications : null);
            txtResumeLanguages.Text = JoinLines(document != null ? document.Languages : null);
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (Request.QueryString["id"] == null)
                {
                    lblMsg.Visible = true;
                    lblMsg.Text = "Cannot update the records, Please try <b>Relogin</b>!";
                    lblMsg.CssClass = "alert alert-danger";
                    return;
                }

                int userId = Convert.ToInt32(Request.QueryString["id"]);
                string currentResumePath = GetCurrentResumePath(userId);
                string resumePathToSave = currentResumePath;
                ResumeProfileDocument importedDocument = null;

                if (fuResume.HasFile)
                {
                    if (!Utils.IsValidExtension4Resume(fuResume.FileName))
                    {
                        lblMsg.Visible = true;
                        lblMsg.Text = "Please Select .doc, .docx, or .pdf file for resume!";
                        lblMsg.CssClass = "alert alert-danger";
                        return;
                    }

                    ResumeImportResult importResult = ResumeProfileService.ImportAndParse(
                        fuResume.PostedFile,
                        Server.MapPath("~/Resumes"),
                        "Resumes");

                    if (!importResult.IsSuccess || importResult.Document == null)
                    {
                        lblMsg.Visible = true;
                        lblMsg.Text = string.IsNullOrWhiteSpace(importResult.ValidationMessage)
                            ? "Please upload a valid resume file."
                            : importResult.ValidationMessage;
                        lblMsg.CssClass = "alert alert-danger";
                        return;
                    }

                    importedDocument = importResult.Document;
                    resumePathToSave = importResult.StoredRelativePath;
                }

                ResumeProfileDocument formDocument = BuildDocumentFromForm();
                if (importedDocument != null)
                    MergeDocumentDefaults(formDocument, importedDocument);

                if (string.IsNullOrWhiteSpace(formDocument.FullName))
                    formDocument.FullName = txtFullName.Text.Trim();

                if (string.IsNullOrWhiteSpace(formDocument.Mobile))
                    formDocument.Mobile = txtMobile.Text.Trim();

                if (string.IsNullOrWhiteSpace(formDocument.Headline))
                    formDocument.Headline = txtWork.Text.Trim();

                if (formDocument.Experience == null || formDocument.Experience.Count == 0)
                {
                    string experienceValue = txtExperience.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(experienceValue))
                        formDocument.Experience = new List<string> { experienceValue };
                }

                formDocument.OriginalFileName = importedDocument != null ? importedDocument.OriginalFileName : GetCurrentResumeOriginalFileName(userId);
                formDocument.StoredFilePath = resumePathToSave;
                formDocument.RawText = importedDocument != null && !string.IsNullOrWhiteSpace(importedDocument.RawText)
                    ? importedDocument.RawText
                    : GetCurrentResumeRawText(userId);
                formDocument.ValidationMessage = string.Empty;
                formDocument.IsValid = !string.IsNullOrWhiteSpace(resumePathToSave) || fuResume.HasFile;

                using (SqlConnection con = new SqlConnection(str))
                {
                    con.Open();
                    using (SqlTransaction tran = con.BeginTransaction())
                    {
                        string oldResumePath = string.Empty;
                        if (fuResume.HasFile)
                            oldResumePath = GetCurrentResumePath(userId, con, tran);

                        try
                        {
                            string userUpdateQuery = @"UPDATE Users SET Username=@Username, Email=@Email, Address=@Address, Country=@Country, UpdatedAt=GETDATE() WHERE UserId=@UserId";
                            using (SqlCommand userCmd = new SqlCommand(userUpdateQuery, con, tran))
                            {
                                userCmd.Parameters.AddWithValue("@Username", txtUserName.Text.Trim());
                                userCmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                                userCmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());
                                userCmd.Parameters.AddWithValue("@Country", ddlCountry.SelectedValue);
                                userCmd.Parameters.AddWithValue("@UserId", userId);
                                userCmd.ExecuteNonQuery();
                            }

                            bool profileExists;
                            using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM JobSeekers WHERE ProfileId = @UserId", con, tran))
                            {
                                checkCmd.Parameters.AddWithValue("@UserId", userId);
                                profileExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                            }

                            string jobSeekerQuery;
                            if (profileExists)
                            {
                                jobSeekerQuery = @"UPDATE JobSeekers SET Name=@Name, Mobile=@Mobile, TenthGrade=@TenthGrade,
                                                    TwelfthGrade=@TwelfthGrade, GraduationGrade=@GraduationGrade, PostGraduationGrade=@PostGraduationGrade,
                                                    Phd=@Phd, WorksOn=@WorksOn, Experience=@Experience,
                                                    Resume=@Resume, ResumeOriginalFileName=@ResumeOriginalFileName, ResumeParseStatus=@ResumeParseStatus,
                                                    ResumeValidationMessage=@ResumeValidationMessage, ResumeUploadedAt=@ResumeUploadedAt, ResumeParsedAt=@ResumeParsedAt,
                                                    ResumeStructuredJson=@ResumeStructuredJson, ResumeRawText=@ResumeRawText, ResumeHeadline=@ResumeHeadline,
                                                    ResumeSummary=@ResumeSummary, ResumeSkills=@ResumeSkills, ResumeEducation=@ResumeEducation,
                                                    ResumeExperienceDetails=@ResumeExperienceDetails, ResumeProjects=@ResumeProjects,
                                                    ResumeCertifications=@ResumeCertifications, ResumeLanguages=@ResumeLanguages
                                                    WHERE ProfileId=@ProfileId";
                            }
                            else
                            {
                                jobSeekerQuery = @"INSERT INTO JobSeekers (ProfileId, Name, Mobile, TenthGrade, TwelfthGrade, GraduationGrade,
                                                    PostGraduationGrade, Phd, WorksOn, Experience, Photo,
                                                    Resume, ResumeOriginalFileName, ResumeParseStatus, ResumeValidationMessage,
                                                    ResumeUploadedAt, ResumeParsedAt, ResumeStructuredJson, ResumeRawText,
                                                    ResumeHeadline, ResumeSummary, ResumeSkills, ResumeEducation,
                                                    ResumeExperienceDetails, ResumeProjects, ResumeCertifications, ResumeLanguages)
                                                    VALUES (@ProfileId, @Name, @Mobile, @TenthGrade, @TwelfthGrade, @GraduationGrade,
                                                    @PostGraduationGrade, @Phd, @WorksOn, @Experience, 'avatar.png',
                                                    @Resume, @ResumeOriginalFileName, @ResumeParseStatus, @ResumeValidationMessage,
                                                    @ResumeUploadedAt, @ResumeParsedAt, @ResumeStructuredJson, @ResumeRawText,
                                                    @ResumeHeadline, @ResumeSummary, @ResumeSkills, @ResumeEducation,
                                                    @ResumeExperienceDetails, @ResumeProjects, @ResumeCertifications, @ResumeLanguages)";
                            }

                            using (SqlCommand jobSeekerCmd = new SqlCommand(jobSeekerQuery, con, tran))
                            {
                                jobSeekerCmd.Parameters.AddWithValue("@ProfileId", userId);
                                jobSeekerCmd.Parameters.AddWithValue("@Name", txtFullName.Text.Trim());
                                jobSeekerCmd.Parameters.AddWithValue("@Mobile", txtMobile.Text.Trim());
                                jobSeekerCmd.Parameters.AddWithValue("@TenthGrade", string.IsNullOrWhiteSpace(txtTenth.Text) ? (object)DBNull.Value : txtTenth.Text.Trim());
                                jobSeekerCmd.Parameters.AddWithValue("@TwelfthGrade", string.IsNullOrWhiteSpace(txtTwelfth.Text) ? (object)DBNull.Value : txtTwelfth.Text.Trim());
                                jobSeekerCmd.Parameters.AddWithValue("@GraduationGrade", string.IsNullOrWhiteSpace(txtGraduation.Text) ? (object)DBNull.Value : txtGraduation.Text.Trim());
                                jobSeekerCmd.Parameters.AddWithValue("@PostGraduationGrade", string.IsNullOrWhiteSpace(txtPostGraduation.Text) ? (object)DBNull.Value : txtPostGraduation.Text.Trim());
                                jobSeekerCmd.Parameters.AddWithValue("@Phd", string.IsNullOrWhiteSpace(txtPhd.Text) ? (object)DBNull.Value : txtPhd.Text.Trim());
                                jobSeekerCmd.Parameters.AddWithValue("@WorksOn", string.IsNullOrWhiteSpace(txtWork.Text) ? (object)DBNull.Value : txtWork.Text.Trim());
                                jobSeekerCmd.Parameters.AddWithValue("@Experience", string.IsNullOrWhiteSpace(txtExperience.Text) ? (object)DBNull.Value : txtExperience.Text.Trim());
                                ResumeProfileService.AddResumeProfileParameters(jobSeekerCmd, formDocument, resumePathToSave);
                                jobSeekerCmd.ExecuteNonQuery();
                            }

                            tran.Commit();

                            if (fuResume.HasFile && !string.IsNullOrWhiteSpace(oldResumePath) && !string.Equals(oldResumePath, resumePathToSave, StringComparison.OrdinalIgnoreCase))
                                DeleteStoredFile(oldResumePath);
                        }
                        catch
                        {
                            tran.Rollback();
                            throw;
                        }
                    }
                }

                lblMsg.Visible = true;
                lblMsg.Text = "Resume details updated successfully!";
                lblMsg.CssClass = "alert alert-success";

                Response.Redirect("Profile.aspx", false);
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("Violation of UNIQUE KEY constraint"))
                {
                    lblMsg.Visible = true;
                    lblMsg.Text = "<b>" + txtUserName.Text.Trim() + "</b> username already exists, try new one..!";
                    lblMsg.CssClass = "alert alert-danger";
                }
                else
                {
                    lblMsg.Visible = true;
                    lblMsg.Text = Server.HtmlEncode(ex.Message);
                    lblMsg.CssClass = "alert alert-danger";
                }
            }
            catch (Exception ex)
            {
                lblMsg.Visible = true;
                lblMsg.Text = Server.HtmlEncode(ex.Message);
                lblMsg.CssClass = "alert alert-danger";
            }
        }

        private ResumeProfileDocument BuildDocumentFromForm()
        {
            return new ResumeProfileDocument
            {
                FullName = txtFullName.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Mobile = txtMobile.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                Headline = txtResumeHeadline.Text.Trim(),
                Summary = txtResumeSummary.Text.Trim(),
                Skills = SplitLines(txtResumeSkills.Text),
                Education = SplitLines(txtResumeEducation.Text),
                Experience = SplitLines(txtResumeExperienceDetails.Text),
                Projects = SplitLines(txtResumeProjects.Text),
                Certifications = SplitLines(txtResumeCertifications.Text),
                Languages = SplitLines(txtResumeLanguages.Text),
                ParsedAt = DateTime.UtcNow,
                IsValid = true
            };
        }

        private void MergeDocumentDefaults(ResumeProfileDocument target, ResumeProfileDocument fallback)
        {
            if (target == null || fallback == null)
                return;

            if (string.IsNullOrWhiteSpace(target.FullName)) target.FullName = fallback.FullName;
            if (string.IsNullOrWhiteSpace(target.Email)) target.Email = fallback.Email;
            if (string.IsNullOrWhiteSpace(target.Mobile)) target.Mobile = fallback.Mobile;
            if (string.IsNullOrWhiteSpace(target.Address)) target.Address = fallback.Address;
            if (string.IsNullOrWhiteSpace(target.Headline)) target.Headline = fallback.Headline;
            if (string.IsNullOrWhiteSpace(target.Summary)) target.Summary = fallback.Summary;
            if (target.Skills == null || target.Skills.Count == 0) target.Skills = fallback.Skills;
            if (target.Education == null || target.Education.Count == 0) target.Education = fallback.Education;
            if (target.Experience == null || target.Experience.Count == 0) target.Experience = fallback.Experience;
            if (target.Projects == null || target.Projects.Count == 0) target.Projects = fallback.Projects;
            if (target.Certifications == null || target.Certifications.Count == 0) target.Certifications = fallback.Certifications;
            if (target.Languages == null || target.Languages.Count == 0) target.Languages = fallback.Languages;
            if (string.IsNullOrWhiteSpace(target.RawText)) target.RawText = fallback.RawText;
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

        private string GetReaderString(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
                return string.Empty;

            return reader.GetValue(ordinal).ToString();
        }

        private string GetCurrentResumePath(int userId)
        {
            using (SqlConnection con = new SqlConnection(str))
            {
                con.Open();
                return GetCurrentResumePath(userId, con, null);
            }
        }

        private string GetCurrentResumePath(int userId, SqlConnection con, SqlTransaction tran)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT Resume FROM JobSeekers WHERE ProfileId = @UserId", con, tran))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                if (con.State != ConnectionState.Open)
                    con.Open();

                object result = cmd.ExecuteScalar();
                return result != null && result != DBNull.Value ? result.ToString() : string.Empty;
            }
        }

        private string GetCurrentResumeOriginalFileName(int userId)
        {
            using (SqlConnection con = new SqlConnection(str))
            using (SqlCommand cmd = new SqlCommand("SELECT ResumeOriginalFileName FROM JobSeekers WHERE ProfileId = @UserId", con))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                con.Open();
                object result = cmd.ExecuteScalar();
                return result != null && result != DBNull.Value ? result.ToString() : string.Empty;
            }
        }

        private string GetCurrentResumeRawText(int userId)
        {
            using (SqlConnection con = new SqlConnection(str))
            using (SqlCommand cmd = new SqlCommand("SELECT ResumeRawText FROM JobSeekers WHERE ProfileId = @UserId", con))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                con.Open();
                object result = cmd.ExecuteScalar();
                return result != null && result != DBNull.Value ? result.ToString() : string.Empty;
            }
        }

        private void PersistResumeMetadata(int userId, ResumeProfileDocument document, string resumePath)
        {
            if (document == null)
                return;

            using (SqlConnection con = new SqlConnection(str))
            {
                string updateQuery = @"UPDATE JobSeekers SET
                    Name = CASE WHEN (Name IS NULL OR LTRIM(RTRIM(Name)) = '') AND @ParsedName <> '' THEN @ParsedName ELSE Name END,
                    Mobile = CASE WHEN (Mobile IS NULL OR LTRIM(RTRIM(Mobile)) = '') AND @ParsedMobile <> '' THEN @ParsedMobile ELSE Mobile END,
                    WorksOn = CASE WHEN (WorksOn IS NULL OR LTRIM(RTRIM(WorksOn)) = '') AND @ParsedHeadline <> '' THEN @ParsedHeadline ELSE WorksOn END,
                    Experience = CASE WHEN (Experience IS NULL OR LTRIM(RTRIM(Experience)) = '') AND @ParsedExperience <> '' THEN @ParsedExperience ELSE Experience END,
                    Resume = @Resume,
                    ResumeOriginalFileName = @ResumeOriginalFileName,
                    ResumeParseStatus = @ResumeParseStatus,
                    ResumeValidationMessage = @ResumeValidationMessage,
                    ResumeUploadedAt = @ResumeUploadedAt,
                    ResumeParsedAt = @ResumeParsedAt,
                    ResumeStructuredJson = @ResumeStructuredJson,
                    ResumeRawText = @ResumeRawText,
                    ResumeHeadline = @ResumeHeadline,
                    ResumeSummary = @ResumeSummary,
                    ResumeSkills = @ResumeSkills,
                    ResumeEducation = @ResumeEducation,
                    ResumeExperienceDetails = @ResumeExperienceDetails,
                    ResumeProjects = @ResumeProjects,
                    ResumeCertifications = @ResumeCertifications,
                    ResumeLanguages = @ResumeLanguages
                    WHERE ProfileId = @UserId";

                using (SqlCommand cmdUpdate = new SqlCommand(updateQuery, con))
                {
                    cmdUpdate.Parameters.AddWithValue("@UserId", userId);
                    cmdUpdate.Parameters.AddWithValue("@ParsedName", string.IsNullOrWhiteSpace(document.FullName) ? string.Empty : document.FullName);
                    cmdUpdate.Parameters.AddWithValue("@ParsedMobile", string.IsNullOrWhiteSpace(document.Mobile) ? string.Empty : document.Mobile);
                    cmdUpdate.Parameters.AddWithValue("@ParsedHeadline", string.IsNullOrWhiteSpace(document.Headline) ? string.Empty : document.Headline);
                    cmdUpdate.Parameters.AddWithValue("@ParsedExperience", document.Experience != null && document.Experience.Count > 0 ? document.Experience[0] : string.Empty);
                    ResumeProfileService.AddResumeProfileParameters(cmdUpdate, document, resumePath);
                    con.Open();
                    cmdUpdate.ExecuteNonQuery();
                }
            }
        }

        private void DeleteStoredFile(string storedPath)
        {
            try
            {
                string physicalPath = ResolveStoredPath(storedPath);
                if (!string.IsNullOrWhiteSpace(physicalPath) && File.Exists(physicalPath))
                    File.Delete(physicalPath);
            }
            catch
            {
            }
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
    }
}