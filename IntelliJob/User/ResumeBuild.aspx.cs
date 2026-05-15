using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
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

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            if (!IsPostBack)
            {
                if (Request.QueryString["id"] == null)
                {
                    Response.Redirect("Login.aspx");
                    return;
                }

                BindMonthDropDowns();
                LoadProfileForEditing();
            }
        }

        private void BindMonthDropDowns()
        {
            BindMonthDropdown("ddlEdu", 1, 2);
            BindMonthDropdown("ddlExp", 1, 5);
        }

        private void BindMonthDropdown(string prefix, int startIndex, int endIndex)
        {
            for (int index = startIndex; index <= endIndex; index++)
            {
                BindMonthDropdown(prefix + index + "StartMonth");
                BindMonthDropdown(prefix + index + "EndMonth");
            }
        }

        private void BindMonthDropdown(string id)
        {
            var dropDown = FindControlRecursive(this, id) as System.Web.UI.WebControls.DropDownList;
            if (dropDown == null || dropDown.Items.Count > 0)
                return;

            string[] monthNames = new[]
            {
                "January", "February", "March", "April", "May", "June",
                "July", "August", "September", "October", "November", "December"
            };

            dropDown.Items.Add(new System.Web.UI.WebControls.ListItem("Month", string.Empty));
            for (int month = 1; month <= 12; month++)
                dropDown.Items.Add(new System.Web.UI.WebControls.ListItem(monthNames[month - 1], month.ToString("00")));
        }

        private void LoadProfileForEditing()
        {
            int userId = Convert.ToInt32(Request.QueryString["id"]);

            using (SqlConnection con = new SqlConnection(str))
            {
                string query = @"SELECT u.UserId, u.Username, u.Email, u.Address, u.Country,
                                        js.Name, js.Mobile, js.Resume,
                                        js.ResumeOriginalFileName, js.ResumeParseStatus, js.ResumeValidationMessage,
                                        js.ResumeUploadedAt, js.ResumeParsedAt, js.ResumeStructuredJson,
                                        js.ResumeRawText
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
            document.Headline = string.Empty;
            document.RawText = string.IsNullOrWhiteSpace(document.RawText) ? GetReaderString(reader, "ResumeRawText") : document.RawText;

            return document;
        }

        private void PopulateForm(SqlDataReader reader, ResumeProfileDocument document)
        {
            txtUserName.Text = GetReaderString(reader, "Username");
            txtFullName.Text = document != null && !string.IsNullOrWhiteSpace(document.FullName) ? document.FullName : GetReaderString(reader, "Name");
            txtEmail.Text = GetReaderString(reader, "Email");
            txtMobile.Text = document != null && !string.IsNullOrWhiteSpace(document.Mobile) ? document.Mobile : GetReaderString(reader, "Mobile");

            txtAddress.Text = GetReaderString(reader, "Address");

            string country = GetReaderString(reader, "Country");
            if (!string.IsNullOrWhiteSpace(country) && ddlCountry.Items.FindByValue(country) != null)
                ddlCountry.SelectedValue = country;

            SetTextBoxValue("txtResumeSummary", document != null ? !string.IsNullOrWhiteSpace(document.ProfessionalSummary) ? document.ProfessionalSummary : document.Summary : string.Empty);
            PopulateEducationCards(document != null ? document.EducationDetails : null, document != null ? document.Education : null);
            PopulateExperienceCards(document != null ? document.ExperienceDetails : null, document != null ? document.Experience : null);
            PopulateProjectCards(document != null ? document.ProjectDetails : null, document != null ? document.Projects : null);
            PopulateSkillCards(document != null ? document.SkillGroups : null, document != null ? document.Skills : null);
            SetTextBoxValue("txtResumeCertifications", JoinLines(document != null ? document.Certifications : null));
            SetTextBoxValue("txtResumeLanguages", JoinLines(document != null ? document.Languages : null));
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
                string validationMessage;
                if (!ValidateStructuredInputs(out validationMessage))
                {
                    lblMsg.Visible = true;
                    lblMsg.Text = validationMessage;
                    lblMsg.CssClass = "alert alert-danger";
                    return;
                }

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
                            bool profileExists;
                            using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM JobSeekers WHERE ProfileId = @UserId", con, tran))
                            {
                                checkCmd.Parameters.AddWithValue("@UserId", userId);
                                profileExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                            }

                            string jobSeekerQuery;
                            if (profileExists)
                            {
                                jobSeekerQuery = @"UPDATE JobSeekers SET Name=@Name, Mobile=@Mobile,
                                                    Resume=@Resume, ResumeOriginalFileName=@ResumeOriginalFileName, ResumeParseStatus=@ResumeParseStatus,
                                                    ResumeValidationMessage=@ResumeValidationMessage, ResumeUploadedAt=@ResumeUploadedAt, ResumeParsedAt=@ResumeParsedAt,
                                                    ResumeStructuredJson=@ResumeStructuredJson, ResumeRawText=@ResumeRawText
                                                    WHERE ProfileId=@ProfileId";
                            }
                            else
                            {
                                jobSeekerQuery = @"INSERT INTO JobSeekers (ProfileId, Name, Mobile, Photo,
                                                    Resume, ResumeOriginalFileName, ResumeParseStatus, ResumeValidationMessage,
                                                    ResumeUploadedAt, ResumeParsedAt, ResumeStructuredJson, ResumeRawText)
                                                    VALUES (@ProfileId, @Name, @Mobile, 'avatar.png',
                                                    @Resume, @ResumeOriginalFileName, @ResumeParseStatus, @ResumeValidationMessage,
                                                    @ResumeUploadedAt, @ResumeParsedAt, @ResumeStructuredJson, @ResumeRawText)";
                            }

                            using (SqlCommand jobSeekerCmd = new SqlCommand(jobSeekerQuery, con, tran))
                            {
                                jobSeekerCmd.Parameters.AddWithValue("@ProfileId", userId);
                                jobSeekerCmd.Parameters.AddWithValue("@Name", txtFullName.Text.Trim());
                                jobSeekerCmd.Parameters.AddWithValue("@Mobile", txtMobile.Text.Trim());

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
                lblMsg.Visible = true;
                lblMsg.Text = Server.HtmlEncode(ex.Message);
                lblMsg.CssClass = "alert alert-danger";
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
            List<string> summaryLines = SplitLines(GetTextBoxValue("txtResumeSummary"));
            List<ResumeEducationEntry> educationDetails = ParseEducationEntriesFromCards();
            List<ResumeExperienceEntry> experienceDetails = ParseExperienceEntriesFromCards();
            List<ResumeProjectEntry> projectDetails = ParseProjectEntriesFromCards();
            ResumeSkillGroups skillGroups = BuildSkillGroupsFromCards();

            return new ResumeProfileDocument
            {
                FullName = txtFullName.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Mobile = txtMobile.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                Headline = string.Empty,
                Summary = string.Join(Environment.NewLine, summaryLines),
                ProfessionalSummary = string.Join(Environment.NewLine, summaryLines),
                Skills = FlattenSkillGroups(skillGroups),
                Education = FlattenEducationEntries(educationDetails),
                Experience = FlattenExperienceEntries(experienceDetails),
                Projects = FlattenProjectEntries(projectDetails),
                Certifications = SplitLines(GetTextBoxValue("txtResumeCertifications")),
                Languages = SplitLines(GetTextBoxValue("txtResumeLanguages")),
                EducationDetails = educationDetails,
                ExperienceDetails = experienceDetails,
                ProjectDetails = projectDetails,
                SkillGroups = skillGroups,
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
            target.Headline = string.Empty;
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

        private List<string> SplitCommaSeparated(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new List<string>();

            return value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
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

        private string JoinCommaSeparated(IEnumerable<string> values)
        {
            if (values == null)
                return string.Empty;

            return string.Join(", ", values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()));
        }

        private string GetTextBoxValue(string id)
        {
            var textBox = FindControlRecursive(this, id) as System.Web.UI.WebControls.TextBox;
            return textBox != null ? textBox.Text : string.Empty;
        }

        private void SetTextBoxValue(string id, string value)
        {
            var textBox = FindControlRecursive(this, id) as System.Web.UI.WebControls.TextBox;
            if (textBox != null)
                textBox.Text = value ?? string.Empty;
        }

        private string GetDropDownValue(string id)
        {
            var dropDown = FindControlRecursive(this, id) as System.Web.UI.WebControls.DropDownList;
            return dropDown != null ? dropDown.SelectedValue : string.Empty;
        }

        private void SetCheckBoxValue(string id, bool value)
        {
            var checkBox = FindControlRecursive(this, id) as System.Web.UI.WebControls.CheckBox;
            if (checkBox != null)
                checkBox.Checked = value;
        }

        private bool GetCheckBoxValue(string id)
        {
            var checkBox = FindControlRecursive(this, id) as System.Web.UI.WebControls.CheckBox;
            return checkBox != null && checkBox.Checked;
        }

        private System.Web.UI.Control FindControlRecursive(System.Web.UI.Control root, string id)
        {
            if (root == null)
                return null;

            if (string.Equals(root.ID, id, StringComparison.Ordinal))
                return root;

            foreach (System.Web.UI.Control child in root.Controls)
            {
                var match = FindControlRecursive(child, id);
                if (match != null)
                    return match;
            }

            return null;
        }

        private void PopulateEducationCards(IEnumerable<ResumeEducationEntry> entries, IEnumerable<string> fallback)
        {
            List<ResumeEducationEntry> list = entries != null ? entries.Where(entry => entry != null).Take(2).ToList() : new List<ResumeEducationEntry>();
            if (list.Count == 0 && fallback != null)
            {
                list = fallback.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => new ResumeEducationEntry { Coursework = item.Trim() }).Take(2).ToList();
            }

            for (int i = 1; i <= 2; i++)
            {
                ResumeEducationEntry entry = i <= list.Count ? list[i - 1] : null;
                SetTextBoxValue("txtEdu" + i + "SchoolName", entry != null ? entry.SchoolName : string.Empty);
                SetTextBoxValue("txtEdu" + i + "Location", entry != null ? entry.Location : string.Empty);
                SetTextBoxValue("txtEdu" + i + "Degree", entry != null ? entry.Degree : string.Empty);
                SetSelectedMonth("ddlEdu" + i + "StartMonth", entry != null ? entry.StartMonth : null);
                SetTextBoxValue("txtEdu" + i + "StartYear", entry != null && entry.StartYear.HasValue ? entry.StartYear.Value.ToString() : string.Empty);
                SetSelectedMonth("ddlEdu" + i + "EndMonth", entry != null ? entry.EndMonth : null);
                SetTextBoxValue("txtEdu" + i + "EndYear", entry != null && entry.EndYear.HasValue ? entry.EndYear.Value.ToString() : string.Empty);
                SetTextBoxValue("txtEdu" + i + "Grade", entry != null ? entry.Grade : string.Empty);
                SetTextBoxValue("txtEdu" + i + "Coursework", entry != null ? entry.Coursework : string.Empty);
            }
        }

        private void PopulateExperienceCards(IEnumerable<ResumeExperienceEntry> entries, IEnumerable<string> fallback)
        {
            List<ResumeExperienceEntry> list = entries != null ? entries.Where(entry => entry != null).Take(5).ToList() : new List<ResumeExperienceEntry>();
            if (list.Count == 0 && fallback != null)
            {
                list = fallback.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => new ResumeExperienceEntry { Bullets = new List<string> { item.Trim() } }).Take(5).ToList();
            }

            for (int i = 1; i <= 5; i++)
            {
                ResumeExperienceEntry entry = i <= list.Count ? list[i - 1] : null;
                SetTextBoxValue("txtExp" + i + "JobTitle", entry != null ? entry.JobTitle : string.Empty);
                SetTextBoxValue("txtExp" + i + "Company", entry != null ? entry.Company : string.Empty);
                SetTextBoxValue("txtExp" + i + "Location", entry != null ? entry.Location : string.Empty);
                SetSelectedMonth("ddlExp" + i + "StartMonth", entry != null ? entry.StartMonth : null);
                SetTextBoxValue("txtExp" + i + "StartYear", entry != null && entry.StartYear.HasValue ? entry.StartYear.Value.ToString() : string.Empty);
                SetSelectedMonth("ddlExp" + i + "EndMonth", entry != null ? entry.EndMonth : null);
                SetTextBoxValue("txtExp" + i + "EndYear", entry != null && entry.EndYear.HasValue ? entry.EndYear.Value.ToString() : string.Empty);
                SetCheckBoxValue("chkExp" + i + "Current", entry != null && entry.IsCurrent);
                SetTextBoxValue("txtExp" + i + "Description", entry != null ? JoinLines(entry.Bullets) : string.Empty);
            }
        }

        private void PopulateProjectCards(IEnumerable<ResumeProjectEntry> entries, IEnumerable<string> fallback)
        {
            List<ResumeProjectEntry> list = entries != null ? entries.Where(entry => entry != null).Take(5).ToList() : new List<ResumeProjectEntry>();
            if (list.Count == 0 && fallback != null)
            {
                list = fallback.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => new ResumeProjectEntry { Description = item.Trim() }).Take(5).ToList();
            }

            for (int i = 1; i <= 5; i++)
            {
                ResumeProjectEntry entry = i <= list.Count ? list[i - 1] : null;
                SetTextBoxValue("txtProj" + i + "ProjectTitle", entry != null ? entry.ProjectTitle : string.Empty);
                SetTextBoxValue("txtProj" + i + "TechStack", entry != null ? JoinCommaSeparated(entry.TechStack) : string.Empty);
                SetTextBoxValue("txtProj" + i + "Description", entry != null ? entry.Description : string.Empty);
            }
        }

        private void PopulateSkillCards(ResumeSkillGroups skillGroups, IEnumerable<string> fallbackSkills)
        {
            SetTextBoxValue("txtSkillProgrammingLanguages", skillGroups != null ? JoinCommaSeparated(skillGroups.ProgrammingLanguages) : JoinLines(fallbackSkills));
            SetTextBoxValue("txtSkillFrameworksLibraries", skillGroups != null ? JoinCommaSeparated(skillGroups.FrameworksLibraries) : string.Empty);
            SetTextBoxValue("txtSkillToolsCloudDatabase", skillGroups != null ? JoinCommaSeparated(skillGroups.ToolsCloudDatabaseSkills) : string.Empty);
            SetTextBoxValue("txtSkillSoftSkillsLanguages", skillGroups != null ? JoinCommaSeparated(skillGroups.SoftSkillsLanguages) : string.Empty);
            SetTextBoxValue("txtSkillCustomHeading", skillGroups != null ? skillGroups.CustomHeading : string.Empty);
            SetTextBoxValue("txtSkillCustomItems", skillGroups != null ? JoinCommaSeparated(skillGroups.CustomItems) : string.Empty);
        }

        private void SetSelectedMonth(string id, int? month)
        {
            var dropDown = FindControlRecursive(this, id) as System.Web.UI.WebControls.DropDownList;
            if (dropDown != null)
                dropDown.SelectedValue = month.HasValue ? month.Value.ToString("00") : string.Empty;
        }

        private List<ResumeEducationEntry> ParseEducationEntriesFromCards()
        {
            var entries = new List<ResumeEducationEntry>();
            for (int i = 1; i <= 2; i++)
            {
                ResumeEducationEntry entry = BuildEducationEntry(i, "txtEdu", "ddlEdu");
                if (entry != null)
                    entries.Add(entry);
            }

            return entries;
        }

        private List<ResumeExperienceEntry> ParseExperienceEntriesFromCards()
        {
            var entries = new List<ResumeExperienceEntry>();
            for (int i = 1; i <= 5; i++)
            {
                ResumeExperienceEntry entry = BuildExperienceEntry(i, "txtExp", "ddlExp", "chkExp");
                if (entry != null)
                    entries.Add(entry);
            }

            return entries;
        }

        private List<ResumeProjectEntry> ParseProjectEntriesFromCards()
        {
            var entries = new List<ResumeProjectEntry>();
            for (int i = 1; i <= 5; i++)
            {
                ResumeProjectEntry entry = BuildProjectEntry(i, "txtProj");
                if (entry != null)
                    entries.Add(entry);
            }

            return entries;
        }

        private ResumeSkillGroups BuildSkillGroupsFromCards()
        {
            string programming = GetTextBoxValue("txtSkillProgrammingLanguages");
            string frameworks = GetTextBoxValue("txtSkillFrameworksLibraries");
            string tools = GetTextBoxValue("txtSkillToolsCloudDatabase");
            string softSkills = GetTextBoxValue("txtSkillSoftSkillsLanguages");
            string customHeading = GetTextBoxValue("txtSkillCustomHeading").Trim();
            string customItems = GetTextBoxValue("txtSkillCustomItems");

            return new ResumeSkillGroups
            {
                ProgrammingLanguages = SplitCommaSeparated(programming).Take(5).ToList(),
                FrameworksLibraries = SplitCommaSeparated(frameworks).Take(5).ToList(),
                ToolsCloudDatabaseSkills = SplitCommaSeparated(tools).Take(5).ToList(),
                SoftSkillsLanguages = SplitCommaSeparated(softSkills).Take(5).ToList(),
                CustomHeading = customHeading,
                CustomItems = SplitCommaSeparated(customItems).Take(5).ToList()
            };
        }

        private bool ValidateStructuredInputs(out string message)
        {
            message = string.Empty;
            Regex schoolRegex = new Regex(@"^[A-Za-z\s]{0,100}$");
            Regex yearRegex = new Regex(@"^(19|20)\d{2}$");

            for (int i = 1; i <= 2; i++)
            {
                string school = GetTextBoxValue("txtEdu" + i + "SchoolName").Trim();
                string location = GetTextBoxValue("txtEdu" + i + "Location").Trim();
                string degree = GetTextBoxValue("txtEdu" + i + "Degree").Trim();
                string startYear = GetTextBoxValue("txtEdu" + i + "StartYear").Trim();
                string endYear = GetTextBoxValue("txtEdu" + i + "EndYear").Trim();
                string grade = GetTextBoxValue("txtEdu" + i + "Grade").Trim();
                string coursework = GetTextBoxValue("txtEdu" + i + "Coursework").Trim();

                if (!schoolRegex.IsMatch(school))
                {
                    message = "Education " + i + ": School Name must contain only alphabets and spaces, maximum 100 characters.";
                    return false;
                }
                if (location.Length > 50) { message = "Education " + i + ": Location must be at most 50 characters."; return false; }
                if (degree.Length > 50) { message = "Education " + i + ": Degree must be at most 50 characters."; return false; }
                if (!string.IsNullOrWhiteSpace(startYear) && !yearRegex.IsMatch(startYear)) { message = "Education " + i + ": Start Year must be a valid year like 2026."; return false; }
                if (!string.IsNullOrWhiteSpace(endYear) && !yearRegex.IsMatch(endYear)) { message = "Education " + i + ": End Year must be a valid year like 2026."; return false; }
                if (grade.Length > 10) { message = "Education " + i + ": Final / Current Grade must be at most 10 characters."; return false; }
                if (coursework.Length > 500) { message = "Education " + i + ": Relevant Coursework / Description must be at most 500 characters."; return false; }
            }

            for (int i = 1; i <= 5; i++)
            {
                string jobTitle = GetTextBoxValue("txtExp" + i + "JobTitle").Trim();
                string company = GetTextBoxValue("txtExp" + i + "Company").Trim();
                string location = GetTextBoxValue("txtExp" + i + "Location").Trim();
                string startYear = GetTextBoxValue("txtExp" + i + "StartYear").Trim();
                string endYear = GetTextBoxValue("txtExp" + i + "EndYear").Trim();
                string description = GetTextBoxValue("txtExp" + i + "Description").Trim();

                if (jobTitle.Length > 50) { message = "Experience " + i + ": Job Title must be at most 50 characters."; return false; }
                if (company.Length > 50) { message = "Experience " + i + ": Company must be at most 50 characters."; return false; }
                if (location.Length > 50) { message = "Experience " + i + ": Location must be at most 50 characters."; return false; }
                if (!string.IsNullOrWhiteSpace(startYear) && !yearRegex.IsMatch(startYear)) { message = "Experience " + i + ": Start Year must be a valid year like 2026."; return false; }
                if (!string.IsNullOrWhiteSpace(endYear) && !yearRegex.IsMatch(endYear)) { message = "Experience " + i + ": End Year must be a valid year like 2026."; return false; }
                if (description.Length > 1000) { message = "Experience " + i + ": Description must be at most 1000 characters."; return false; }
            }

            for (int i = 1; i <= 5; i++)
            {
                string title = GetTextBoxValue("txtProj" + i + "Title").Trim();
                string techStack = GetTextBoxValue("txtProj" + i + "TechStack").Trim();
                string description = GetTextBoxValue("txtProj" + i + "Description").Trim();

                if (title.Length > 50) { message = "Project " + i + ": Project Title must be at most 50 characters."; return false; }
                if (description.Length > 250) { message = "Project " + i + ": Description must be at most 250 characters."; return false; }
                if (SplitCommaSeparated(techStack).Count > 4) { message = "Project " + i + ": Tech Stack allows maximum 4 comma-separated items."; return false; }
            }

            if (SplitCommaSeparated(GetTextBoxValue("txtSkillProgrammingLanguages")).Count > 5) { message = "Programming Languages allows maximum 5 comma-separated items."; return false; }
            if (SplitCommaSeparated(GetTextBoxValue("txtSkillFrameworksLibraries")).Count > 5) { message = "Frameworks / Libraries allows maximum 5 comma-separated items."; return false; }
            if (SplitCommaSeparated(GetTextBoxValue("txtSkillToolsCloudDatabase")).Count > 5) { message = "Tools / Cloud / Database Skills allows maximum 5 comma-separated items."; return false; }
            if (SplitCommaSeparated(GetTextBoxValue("txtSkillSoftSkillsLanguages")).Count > 5) { message = "Soft Skills / Languages allows maximum 5 comma-separated items."; return false; }
            if (SplitCommaSeparated(GetTextBoxValue("txtSkillCustomItems")).Count > 5) { message = "User Selection Items allows maximum 5 comma-separated items."; return false; }

            return true;
        }

        private ResumeEducationEntry BuildEducationEntry(int index, string textPrefix, string monthPrefix)
        {
            string schoolName = GetTextBoxValue(textPrefix + index + "SchoolName").Trim();
            string location = GetTextBoxValue(textPrefix + index + "Location").Trim();
            string degree = GetTextBoxValue(textPrefix + index + "Degree").Trim();
            string grade = GetTextBoxValue(textPrefix + index + "Grade").Trim();
            string coursework = GetTextBoxValue(textPrefix + index + "Coursework").Trim();

            if (string.IsNullOrWhiteSpace(schoolName) && string.IsNullOrWhiteSpace(location) && string.IsNullOrWhiteSpace(degree) && string.IsNullOrWhiteSpace(grade) && string.IsNullOrWhiteSpace(coursework))
                return null;

            return new ResumeEducationEntry
            {
                SchoolName = schoolName,
                Location = location,
                Degree = degree,
                StartMonth = ParseMonth(GetDropDownValue(monthPrefix + index + "StartMonth")),
                StartYear = ParseInt(GetTextBoxValue(textPrefix + index + "StartYear")),
                EndMonth = ParseMonth(GetDropDownValue(monthPrefix + index + "EndMonth")),
                EndYear = ParseInt(GetTextBoxValue(textPrefix + index + "EndYear")),
                Grade = grade,
                Coursework = coursework
            };
        }

        private ResumeExperienceEntry BuildExperienceEntry(int index, string textPrefix, string monthPrefix, string checkPrefix)
        {
            string jobTitle = GetTextBoxValue(textPrefix + index + "JobTitle").Trim();
            string company = GetTextBoxValue(textPrefix + index + "Company").Trim();
            string location = GetTextBoxValue(textPrefix + index + "Location").Trim();
            string description = GetTextBoxValue(textPrefix + index + "Description").Trim();

            if (string.IsNullOrWhiteSpace(jobTitle) && string.IsNullOrWhiteSpace(company) && string.IsNullOrWhiteSpace(location) && string.IsNullOrWhiteSpace(description))
                return null;

            return new ResumeExperienceEntry
            {
                JobTitle = jobTitle,
                Company = company,
                Location = location,
                StartMonth = ParseMonth(GetDropDownValue(monthPrefix + index + "StartMonth")),
                StartYear = ParseInt(GetTextBoxValue(textPrefix + index + "StartYear")),
                EndMonth = ParseMonth(GetDropDownValue(monthPrefix + index + "EndMonth")),
                EndYear = ParseInt(GetTextBoxValue(textPrefix + index + "EndYear")),
                IsCurrent = GetCheckBoxValue(checkPrefix + index + "Current"),
                Bullets = SplitBullets(description)
            };
        }

        private ResumeProjectEntry BuildProjectEntry(int index, string textPrefix)
        {
            string title = GetTextBoxValue(textPrefix + index + "ProjectTitle").Trim();
            string techStack = GetTextBoxValue(textPrefix + index + "TechStack").Trim();
            string description = GetTextBoxValue(textPrefix + index + "Description").Trim();

            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(techStack) && string.IsNullOrWhiteSpace(description))
                return null;

            return new ResumeProjectEntry
            {
                ProjectTitle = title,
                TechStack = SplitCommaSeparated(techStack).Take(4).ToList(),
                Description = description
            };
        }

        private List<string> SplitBullets(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new List<string>();

            return value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToList();
        }

        private int? ParseMonth(string value)
        {
            int month;
            if (int.TryParse(value, out month) && month >= 1 && month <= 12)
                return month;

            return null;
        }

        private int? ParseInt(string value)
        {
            int number;
            if (int.TryParse(value, out number))
                return number;

            return null;
        }

        private List<string> FlattenEducationEntries(IEnumerable<ResumeEducationEntry> entries)
        {
            return entries == null ? new List<string>() : entries.Select(FormatEducationEntry).Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
        }

        private List<string> FlattenExperienceEntries(IEnumerable<ResumeExperienceEntry> entries)
        {
            return entries == null ? new List<string>() : entries.Select(FormatExperienceEntry).Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
        }

        private List<string> FlattenProjectEntries(IEnumerable<ResumeProjectEntry> entries)
        {
            return entries == null ? new List<string>() : entries.Select(FormatProjectEntry).Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
        }

        private List<string> FlattenSkillGroups(ResumeSkillGroups skillGroups)
        {
            if (skillGroups == null)
                return new List<string>();

            var items = new List<string>();
            AddSkillLine(items, "Programming Languages", skillGroups.ProgrammingLanguages);
            AddSkillLine(items, "Frameworks/Libraries", skillGroups.FrameworksLibraries);
            AddSkillLine(items, "Tools/Cloud/Database Skills", skillGroups.ToolsCloudDatabaseSkills);
            AddSkillLine(items, "Soft Skills/Languages", skillGroups.SoftSkillsLanguages);
            if (!string.IsNullOrWhiteSpace(skillGroups.CustomHeading) && skillGroups.CustomItems != null && skillGroups.CustomItems.Count > 0)
                AddSkillLine(items, skillGroups.CustomHeading, skillGroups.CustomItems);

            return items;
        }

        private void AddSkillLine(List<string> items, string heading, IEnumerable<string> values)
        {
            if (items == null || values == null)
                return;

            List<string> list = values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToList();
            if (list.Count == 0)
                return;

            items.Add(heading + ": " + string.Join(", ", list));
        }

        private ResumeSkillGroups ParseSkillGroups(string text)
        {
            ResumeSkillGroups skillGroups = new ResumeSkillGroups();
            foreach (string line in SplitLines(text))
            {
                string[] parts = line.Split(new[] { ':' }, 2);
                string heading = parts.Length > 1 ? parts[0].Trim() : string.Empty;
                string valuesText = parts.Length > 1 ? parts[1] : line;
                List<string> values = SplitCommaSeparated(valuesText);

                if (heading.IndexOf("framework", StringComparison.OrdinalIgnoreCase) >= 0)
                    skillGroups.FrameworksLibraries = values.Take(5).ToList();
                else if (heading.IndexOf("tool", StringComparison.OrdinalIgnoreCase) >= 0 || heading.IndexOf("cloud", StringComparison.OrdinalIgnoreCase) >= 0 || heading.IndexOf("database", StringComparison.OrdinalIgnoreCase) >= 0)
                    skillGroups.ToolsCloudDatabaseSkills = values.Take(5).ToList();
                else if (heading.IndexOf("soft", StringComparison.OrdinalIgnoreCase) >= 0 || heading.IndexOf("language", StringComparison.OrdinalIgnoreCase) >= 0)
                    skillGroups.SoftSkillsLanguages = values.Take(5).ToList();
                else if (!string.IsNullOrWhiteSpace(heading))
                {
                    skillGroups.CustomHeading = heading;
                    skillGroups.CustomItems = values.Take(10).ToList();
                }
                else
                {
                    skillGroups.ProgrammingLanguages = values.Take(10).ToList();
                }
            }

            return skillGroups;
        }

        private List<ResumeEducationEntry> ParseEducationEntries(string text)
        {
            return SplitEntryBlocks(text)
                .Select(block => ParseEducationEntry(block))
                .Where(entry => entry != null)
                .Take(2)
                .ToList();
        }

        private List<ResumeExperienceEntry> ParseExperienceEntries(string text)
        {
            return SplitEntryBlocks(text)
                .Select(block => ParseExperienceEntry(block))
                .Where(entry => entry != null)
                .Take(5)
                .ToList();
        }

        private List<ResumeProjectEntry> ParseProjectEntries(string text)
        {
            return SplitEntryBlocks(text)
                .Select(block => ParseProjectEntry(block))
                .Where(entry => entry != null)
                .Take(5)
                .ToList();
        }

        private IEnumerable<string> SplitEntryBlocks(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Enumerable.Empty<string>();

            return value.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(block => block.Trim())
                .Where(block => !string.IsNullOrWhiteSpace(block));
        }

        private ResumeEducationEntry ParseEducationEntry(string block)
        {
            if (string.IsNullOrWhiteSpace(block))
                return null;

            string[] parts = block.Split(new[] { '|', '>' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                parts = block.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            return new ResumeEducationEntry
            {
                SchoolName = parts.Length > 0 ? parts[0].Trim() : string.Empty,
                Location = parts.Length > 1 ? parts[1].Trim() : string.Empty,
                Degree = parts.Length > 2 ? parts[2].Trim() : string.Empty,
                Grade = parts.Length > 3 ? parts[3].Trim() : string.Empty,
                Coursework = parts.Length > 4 ? string.Join(" | ", parts.Skip(4).Select(item => item.Trim()).Where(item => !string.IsNullOrWhiteSpace(item))) : (parts.Length == 1 ? parts[0].Trim() : string.Empty)
            };
        }

        private ResumeExperienceEntry ParseExperienceEntry(string block)
        {
            if (string.IsNullOrWhiteSpace(block))
                return null;

            string[] parts = block.Split(new[] { '|', '>' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                parts = block.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var bullets = new List<string>();
            if (parts.Length > 4)
                bullets.AddRange(parts.Skip(4).Select(item => item.Trim()).Where(item => !string.IsNullOrWhiteSpace(item)));
            if (bullets.Count == 0)
                bullets.AddRange(block.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim()).Where(item => !string.IsNullOrWhiteSpace(item)));

            return new ResumeExperienceEntry
            {
                JobTitle = parts.Length > 0 ? parts[0].Trim() : string.Empty,
                Company = parts.Length > 1 ? parts[1].Trim() : string.Empty,
                Location = parts.Length > 2 ? parts[2].Trim() : string.Empty,
                Bullets = bullets.Take(10).ToList()
            };
        }

        private ResumeProjectEntry ParseProjectEntry(string block)
        {
            if (string.IsNullOrWhiteSpace(block))
                return null;

            string[] parts = block.Split(new[] { '|', '>' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                parts = block.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            return new ResumeProjectEntry
            {
                ProjectTitle = parts.Length > 0 ? parts[0].Trim() : string.Empty,
                TechStack = parts.Length > 1 ? SplitCommaSeparated(parts[1]).Take(4).ToList() : new List<string>(),
                Description = parts.Length > 2 ? parts[2].Trim() : block.Trim()
            };
        }

        private string FormatEducationEntry(ResumeEducationEntry entry)
        {
            if (entry == null)
                return string.Empty;

            return string.Join(" | ", new[]
            {
                entry.SchoolName,
                entry.Location,
                entry.Degree,
                FormatMonthYear(entry.StartMonth, entry.StartYear),
                FormatMonthYear(entry.EndMonth, entry.EndYear),
                entry.Grade,
                entry.Coursework
            }.Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private string FormatExperienceEntry(ResumeExperienceEntry entry)
        {
            if (entry == null)
                return string.Empty;

            var parts = new List<string>
            {
                entry.JobTitle,
                entry.Company,
                entry.Location,
                FormatMonthYear(entry.StartMonth, entry.StartYear),
                entry.IsCurrent ? "Present" : FormatMonthYear(entry.EndMonth, entry.EndYear)
            };

            if (entry.Bullets != null && entry.Bullets.Count > 0)
                parts.Add(string.Join("; ", entry.Bullets.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim())));

            return string.Join(" | ", parts.Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private string FormatProjectEntry(ResumeProjectEntry entry)
        {
            if (entry == null)
                return string.Empty;

            return string.Join(" | ", new[]
            {
                entry.ProjectTitle,
                entry.TechStack != null ? string.Join(", ", entry.TechStack.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim())) : string.Empty,
                entry.Description
            }.Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private string FormatSkillGroups(ResumeSkillGroups skillGroups, IEnumerable<string> fallbackSkills)
        {
            if (skillGroups != null)
            {
                List<string> sections = new List<string>();
                AddFormattedSkillSection(sections, "Programming Languages", skillGroups.ProgrammingLanguages);
                AddFormattedSkillSection(sections, "Frameworks/Libraries", skillGroups.FrameworksLibraries);
                AddFormattedSkillSection(sections, "Tools/Cloud/Database Skills", skillGroups.ToolsCloudDatabaseSkills);
                AddFormattedSkillSection(sections, "Soft Skills/Languages", skillGroups.SoftSkillsLanguages);
                if (!string.IsNullOrWhiteSpace(skillGroups.CustomHeading))
                    AddFormattedSkillSection(sections, skillGroups.CustomHeading, skillGroups.CustomItems);

                if (sections.Count > 0)
                    return string.Join(Environment.NewLine, sections);
            }

            return JoinLines(fallbackSkills);
        }

        private void AddFormattedSkillSection(List<string> sections, string heading, IEnumerable<string> values)
        {
            if (sections == null || values == null)
                return;

            List<string> list = values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToList();
            if (list.Count == 0)
                return;

            sections.Add(heading + ": " + string.Join(", ", list));
        }

        private string FormatEducationEntries(IEnumerable<ResumeEducationEntry> entries, IEnumerable<string> fallback)
        {
            if (entries != null)
            {
                List<string> blocks = entries.Select(FormatEducationEntry).Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
                if (blocks.Count > 0)
                    return string.Join(Environment.NewLine + Environment.NewLine, blocks);
            }

            return JoinLines(fallback);
        }

        private string FormatExperienceEntries(IEnumerable<ResumeExperienceEntry> entries, IEnumerable<string> fallback)
        {
            if (entries != null)
            {
                List<string> blocks = entries.Select(FormatExperienceEntry).Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
                if (blocks.Count > 0)
                    return string.Join(Environment.NewLine + Environment.NewLine, blocks);
            }

            return JoinLines(fallback);
        }

        private string FormatProjectEntries(IEnumerable<ResumeProjectEntry> entries, IEnumerable<string> fallback)
        {
            if (entries != null)
            {
                List<string> blocks = entries.Select(FormatProjectEntry).Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
                if (blocks.Count > 0)
                    return string.Join(Environment.NewLine + Environment.NewLine, blocks);
            }

            return JoinLines(fallback);
        }

        private string FormatMonthYear(int? month, int? year)
        {
            if (!month.HasValue && !year.HasValue)
                return string.Empty;

            if (!month.HasValue)
                return year.HasValue ? year.Value.ToString() : string.Empty;

            if (!year.HasValue)
                return month.Value.ToString("00");

            return month.Value.ToString("00") + "/" + year.Value;
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
                    Resume = @Resume,
                    ResumeOriginalFileName = @ResumeOriginalFileName,
                    ResumeParseStatus = @ResumeParseStatus,
                    ResumeValidationMessage = @ResumeValidationMessage,
                    ResumeUploadedAt = @ResumeUploadedAt,
                    ResumeParsedAt = @ResumeParsedAt,
                    ResumeStructuredJson = @ResumeStructuredJson,
                    ResumeRawText = @ResumeRawText
                    WHERE ProfileId = @UserId";

                using (SqlCommand cmdUpdate = new SqlCommand(updateQuery, con))
                {
                    cmdUpdate.Parameters.AddWithValue("@UserId", userId);
                    cmdUpdate.Parameters.AddWithValue("@ParsedName", string.IsNullOrWhiteSpace(document.FullName) ? string.Empty : document.FullName);
                    cmdUpdate.Parameters.AddWithValue("@ParsedMobile", string.IsNullOrWhiteSpace(document.Mobile) ? string.Empty : document.Mobile);
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