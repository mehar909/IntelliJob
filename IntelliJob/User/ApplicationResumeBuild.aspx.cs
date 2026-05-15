using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace IntelliJob.User
{
    public partial class ApplicationResumeBuild : Page
    {
        private readonly string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["user"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            if (!int.TryParse(Request.QueryString["jobId"], out int jobId))
            {
                Response.Redirect("JobListing.aspx");
                return;
            }

            lnkBackToJob.NavigateUrl = "JobDetails.aspx?id=" + jobId;

            if (!IsPostBack)
            {
                BindMonthDropDowns();
                LoadDraft(jobId);
            }
        }

        private void BindMonthDropDowns()
        {
            BindMonthDropdown("ddlAppEdu", 1, 2);
            BindMonthDropdown("ddlAppExp", 1, 5);
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

        private void LoadDraft(int jobId)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            ApplicationResumeDraftRecord draft = GetDraft(userId, jobId);
            if (draft == null)
            {
                Response.Redirect("JobDetails.aspx?id=" + jobId);
                return;
            }

            ResumeProfileDocument document = GetDraftDocument(draft);
            if (document == null)
            {
                lblMsg.Visible = true;
                lblMsg.Text = "The application resume could not be loaded.";
                lblMsg.CssClass = "alert alert-danger";
                return;
            }

            lblJobInfo.Text = GetJobInfo(jobId);
            lblDraftNote.Text = draft.IsConfirmed
                ? "This resume draft is already confirmed. You can keep editing the sections below and confirm again when ready."
                : "This is a one-time application resume draft. Edit the sections below and confirm when you are ready to apply.";

            PopulateForm(document);
        }

        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(Request.QueryString["jobId"], out int jobId))
            {
                Response.Redirect("JobListing.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["userId"]);
            ApplicationResumeDraftRecord draft = GetDraft(userId, jobId);
            if (draft == null)
            {
                lblMsg.Visible = true;
                lblMsg.Text = "Your application resume draft could not be found.";
                lblMsg.CssClass = "alert alert-danger";
                return;
            }

            ResumeProfileDocument current = GetDraftDocument(draft) ?? new ResumeProfileDocument();
            string validationMessage;
            if (!ValidateStructuredInputs(out validationMessage))
            {
                lblMsg.Visible = true;
                lblMsg.Text = validationMessage;
                lblMsg.CssClass = "alert alert-danger";
                return;
            }

            ResumeProfileDocument updated = BuildDocumentFromForm();
            MergeDocumentDefaults(updated, current);

            updated.OriginalFileName = draft.OriginalFileName;
            updated.StoredFilePath = draft.StoredResumePath;
            updated.RawText = string.IsNullOrWhiteSpace(current.RawText) ? draft.RawText : current.RawText;
            updated.ValidationMessage = string.Empty;
            updated.IsValid = true;

            ApplicationDataStore.SaveApplicationResumeDraft(userId, jobId, updated, draft.ResumeSource, draft.OriginalFileName);

            Response.Redirect("JobDetails.aspx?id=" + jobId, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private ApplicationResumeDraftRecord GetDraft(int userId, int jobId)
        {
            ApplicationResumeDraftRecord draft;
            if (!ApplicationDataStore.TryGetApplicationResumeDraft(userId, jobId, out draft))
                return null;

            return draft != null && !string.IsNullOrWhiteSpace(draft.StoredResumePath) && File.Exists(draft.StoredResumePath)
                ? draft
                : null;
        }

        private ResumeProfileDocument GetDraftDocument(ApplicationResumeDraftRecord draft)
        {
            if (draft == null)
                return null;

            ResumeProfileDocument document = ResumeProfileService.DeserializeDocument(draft.StructuredJson);
            if (document != null)
                return document;

            return new ResumeProfileDocument();
        }

        private void PopulateForm(ResumeProfileDocument document)
        {
            SetTextBoxValue("txtAppResumeSummary", !string.IsNullOrWhiteSpace(document.ProfessionalSummary) ? document.ProfessionalSummary : document.Summary ?? string.Empty);
            PopulateEducationCards(document.EducationDetails, document.Education);
            PopulateExperienceCards(document.ExperienceDetails, document.Experience);
            PopulateProjectCards(document.ProjectDetails, document.Projects);
            PopulateSkillCards(document.SkillGroups, document.Skills);
            SetTextBoxValue("txtAppResumeCertifications", JoinLines(document.Certifications));
            SetTextBoxValue("txtAppResumeLanguages", JoinLines(document.Languages));
        }

        private ResumeProfileDocument BuildDocumentFromForm()
        {
            List<string> summaryLines = SplitLines(GetTextBoxValue("txtAppResumeSummary"));
            List<ResumeEducationEntry> educationDetails = ParseEducationEntriesFromCards();
            List<ResumeExperienceEntry> experienceDetails = ParseExperienceEntriesFromCards();
            List<ResumeProjectEntry> projectDetails = ParseProjectEntriesFromCards();
            ResumeSkillGroups skillGroups = BuildSkillGroupsFromCards();

            return new ResumeProfileDocument
            {
                Summary = string.Join(Environment.NewLine, summaryLines),
                ProfessionalSummary = string.Join(Environment.NewLine, summaryLines),
                Skills = FlattenSkillGroups(skillGroups),
                Education = FlattenEducationEntries(educationDetails),
                Experience = FlattenExperienceEntries(experienceDetails),
                Projects = FlattenProjectEntries(projectDetails),
                Certifications = SplitLines(GetTextBoxValue("txtAppResumeCertifications")),
                Languages = SplitLines(GetTextBoxValue("txtAppResumeLanguages")),
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
            if (string.IsNullOrWhiteSpace(target.Headline)) target.Headline = fallback.Headline;
            if (string.IsNullOrWhiteSpace(target.Summary)) target.Summary = fallback.Summary;
            if (target.Skills == null || target.Skills.Count == 0) target.Skills = fallback.Skills;
            if (target.Education == null || target.Education.Count == 0) target.Education = fallback.Education;
            if (target.Experience == null || target.Experience.Count == 0) target.Experience = fallback.Experience;
            if (target.Projects == null || target.Projects.Count == 0) target.Projects = fallback.Projects;
            if (target.Certifications == null || target.Certifications.Count == 0) target.Certifications = fallback.Certifications;
            if (target.Languages == null || target.Languages.Count == 0) target.Languages = fallback.Languages;
            if (string.IsNullOrWhiteSpace(target.RawText)) target.RawText = fallback.RawText;
            if (string.IsNullOrWhiteSpace(target.OriginalFileName)) target.OriginalFileName = fallback.OriginalFileName;
            if (string.IsNullOrWhiteSpace(target.StoredFilePath)) target.StoredFilePath = fallback.StoredFilePath;
        }

        private List<string> SplitLines(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new List<string>();

            return value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .Where(item => !string.IsNullOrWhiteSpace(item))
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

        private bool GetCheckBoxValue(string id)
        {
            var checkBox = FindControlRecursive(this, id) as System.Web.UI.WebControls.CheckBox;
            return checkBox != null && checkBox.Checked;
        }

        private void SetCheckBoxValue(string id, bool value)
        {
            var checkBox = FindControlRecursive(this, id) as System.Web.UI.WebControls.CheckBox;
            if (checkBox != null)
                checkBox.Checked = value;
        }

        private string GetDropDownValue(string id)
        {
            var dropDown = FindControlRecursive(this, id) as System.Web.UI.WebControls.DropDownList;
            return dropDown != null ? dropDown.SelectedValue : string.Empty;
        }

        private void SetSelectedMonth(string id, int? month)
        {
            var dropDown = FindControlRecursive(this, id) as System.Web.UI.WebControls.DropDownList;
            if (dropDown != null)
                dropDown.SelectedValue = month.HasValue ? month.Value.ToString("00") : string.Empty;
        }

        private System.Web.UI.Control FindControlRecursive(System.Web.UI.Control root, string id)
        {
            if (root == null)
                return null;

            if (string.Equals(root.ID, id, StringComparison.Ordinal))
                return root;

            foreach (System.Web.UI.Control child in root.Controls)
            {
                var found = FindControlRecursive(child, id);
                if (found != null)
                    return found;
            }

            return null;
        }

        private void PopulateEducationCards(IEnumerable<ResumeEducationEntry> entries, IEnumerable<string> fallback)
        {
            List<ResumeEducationEntry> list = entries != null ? entries.Where(entry => entry != null).Take(2).ToList() : new List<ResumeEducationEntry>();
            if (list.Count == 0 && fallback != null)
                list = fallback.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => new ResumeEducationEntry { Coursework = item.Trim() }).Take(2).ToList();

            for (int i = 1; i <= 2; i++)
            {
                ResumeEducationEntry entry = i <= list.Count ? list[i - 1] : null;
                SetTextBoxValue("txtAppEdu" + i + "SchoolName", entry != null ? entry.SchoolName : string.Empty);
                SetTextBoxValue("txtAppEdu" + i + "Location", entry != null ? entry.Location : string.Empty);
                SetTextBoxValue("txtAppEdu" + i + "Degree", entry != null ? entry.Degree : string.Empty);
                SetSelectedMonth("ddlAppEdu" + i + "StartMonth", entry != null ? entry.StartMonth : null);
                SetTextBoxValue("txtAppEdu" + i + "StartYear", entry != null && entry.StartYear.HasValue ? entry.StartYear.Value.ToString() : string.Empty);
                SetSelectedMonth("ddlAppEdu" + i + "EndMonth", entry != null ? entry.EndMonth : null);
                SetTextBoxValue("txtAppEdu" + i + "EndYear", entry != null && entry.EndYear.HasValue ? entry.EndYear.Value.ToString() : string.Empty);
                SetTextBoxValue("txtAppEdu" + i + "Grade", entry != null ? entry.Grade : string.Empty);
                SetTextBoxValue("txtAppEdu" + i + "Coursework", entry != null ? entry.Coursework : string.Empty);
            }
        }

        private void PopulateExperienceCards(IEnumerable<ResumeExperienceEntry> entries, IEnumerable<string> fallback)
        {
            List<ResumeExperienceEntry> list = entries != null ? entries.Where(entry => entry != null).Take(5).ToList() : new List<ResumeExperienceEntry>();
            if (list.Count == 0 && fallback != null)
                list = fallback.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => new ResumeExperienceEntry { Bullets = new List<string> { item.Trim() } }).Take(5).ToList();

            for (int i = 1; i <= 5; i++)
            {
                ResumeExperienceEntry entry = i <= list.Count ? list[i - 1] : null;
                SetTextBoxValue("txtAppExp" + i + "JobTitle", entry != null ? entry.JobTitle : string.Empty);
                SetTextBoxValue("txtAppExp" + i + "Company", entry != null ? entry.Company : string.Empty);
                SetTextBoxValue("txtAppExp" + i + "Location", entry != null ? entry.Location : string.Empty);
                SetSelectedMonth("ddlAppExp" + i + "StartMonth", entry != null ? entry.StartMonth : null);
                SetTextBoxValue("txtAppExp" + i + "StartYear", entry != null && entry.StartYear.HasValue ? entry.StartYear.Value.ToString() : string.Empty);
                SetSelectedMonth("ddlAppExp" + i + "EndMonth", entry != null ? entry.EndMonth : null);
                SetTextBoxValue("txtAppExp" + i + "EndYear", entry != null && entry.EndYear.HasValue ? entry.EndYear.Value.ToString() : string.Empty);
                SetCheckBoxValue("chkAppExp" + i + "Current", entry != null && entry.IsCurrent);
                SetTextBoxValue("txtAppExp" + i + "Description", entry != null ? JoinLines(entry.Bullets) : string.Empty);
            }
        }

        private void PopulateProjectCards(IEnumerable<ResumeProjectEntry> entries, IEnumerable<string> fallback)
        {
            List<ResumeProjectEntry> list = entries != null ? entries.Where(entry => entry != null).Take(5).ToList() : new List<ResumeProjectEntry>();
            if (list.Count == 0 && fallback != null)
                list = fallback.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => new ResumeProjectEntry { Description = item.Trim() }).Take(5).ToList();

            for (int i = 1; i <= 5; i++)
            {
                ResumeProjectEntry entry = i <= list.Count ? list[i - 1] : null;
                SetTextBoxValue("txtAppProj" + i + "Title", entry != null ? entry.ProjectTitle : string.Empty);
                SetTextBoxValue("txtAppProj" + i + "TechStack", entry != null ? string.Join(", ", entry.TechStack ?? new List<string>()) : string.Empty);
                SetTextBoxValue("txtAppProj" + i + "Description", entry != null ? entry.Description : string.Empty);
            }
        }

        private void PopulateSkillCards(ResumeSkillGroups skillGroups, IEnumerable<string> fallbackSkills)
        {
            SetTextBoxValue("txtAppSkillProgrammingLanguages", skillGroups != null ? string.Join(", ", skillGroups.ProgrammingLanguages ?? new List<string>()) : JoinLines(fallbackSkills));
            SetTextBoxValue("txtAppSkillFrameworksLibraries", skillGroups != null ? string.Join(", ", skillGroups.FrameworksLibraries ?? new List<string>()) : string.Empty);
            SetTextBoxValue("txtAppSkillToolsCloudDatabase", skillGroups != null ? string.Join(", ", skillGroups.ToolsCloudDatabaseSkills ?? new List<string>()) : string.Empty);
            SetTextBoxValue("txtAppSkillSoftSkillsLanguages", skillGroups != null ? string.Join(", ", skillGroups.SoftSkillsLanguages ?? new List<string>()) : string.Empty);
            SetTextBoxValue("txtAppSkillCustomHeading", skillGroups != null ? skillGroups.CustomHeading : string.Empty);
            SetTextBoxValue("txtAppSkillCustomItems", skillGroups != null ? string.Join(", ", skillGroups.CustomItems ?? new List<string>()) : string.Empty);
        }

        private List<ResumeEducationEntry> ParseEducationEntriesFromCards()
        {
            var entries = new List<ResumeEducationEntry>();
            for (int i = 1; i <= 2; i++)
            {
                var entry = BuildEducationEntry(i, "txtAppEdu");
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
                var entry = BuildExperienceEntry(i, "txtAppExp");
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
                var entry = BuildProjectEntry(i, "txtAppProj");
                if (entry != null)
                    entries.Add(entry);
            }
            return entries;
        }

        private ResumeSkillGroups BuildSkillGroupsFromCards()
        {
            string programming = GetTextBoxValue("txtAppSkillProgrammingLanguages");
            string frameworks = GetTextBoxValue("txtAppSkillFrameworksLibraries");
            string tools = GetTextBoxValue("txtAppSkillToolsCloudDatabase");
            string softSkills = GetTextBoxValue("txtAppSkillSoftSkillsLanguages");
            string customHeading = GetTextBoxValue("txtAppSkillCustomHeading");
            string customItems = GetTextBoxValue("txtAppSkillCustomItems");

            return new ResumeSkillGroups
            {
                ProgrammingLanguages = SplitCommaSeparated(programming).Take(5).ToList(),
                FrameworksLibraries = SplitCommaSeparated(frameworks).Take(5).ToList(),
                ToolsCloudDatabaseSkills = SplitCommaSeparated(tools).Take(5).ToList(),
                SoftSkillsLanguages = SplitCommaSeparated(softSkills).Take(5).ToList(),
                CustomHeading = customHeading.Trim(),
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
                string school = GetTextBoxValue("txtAppEdu" + i + "SchoolName").Trim();
                string location = GetTextBoxValue("txtAppEdu" + i + "Location").Trim();
                string degree = GetTextBoxValue("txtAppEdu" + i + "Degree").Trim();
                string startYear = GetTextBoxValue("txtAppEdu" + i + "StartYear").Trim();
                string endYear = GetTextBoxValue("txtAppEdu" + i + "EndYear").Trim();
                string grade = GetTextBoxValue("txtAppEdu" + i + "Grade").Trim();
                string coursework = GetTextBoxValue("txtAppEdu" + i + "Coursework").Trim();

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
                string jobTitle = GetTextBoxValue("txtAppExp" + i + "JobTitle").Trim();
                string company = GetTextBoxValue("txtAppExp" + i + "Company").Trim();
                string location = GetTextBoxValue("txtAppExp" + i + "Location").Trim();
                string startYear = GetTextBoxValue("txtAppExp" + i + "StartYear").Trim();
                string endYear = GetTextBoxValue("txtAppExp" + i + "EndYear").Trim();
                string description = GetTextBoxValue("txtAppExp" + i + "Description").Trim();

                if (jobTitle.Length > 50) { message = "Experience " + i + ": Job Title must be at most 50 characters."; return false; }
                if (company.Length > 50) { message = "Experience " + i + ": Company must be at most 50 characters."; return false; }
                if (location.Length > 50) { message = "Experience " + i + ": Location must be at most 50 characters."; return false; }
                if (!string.IsNullOrWhiteSpace(startYear) && !yearRegex.IsMatch(startYear)) { message = "Experience " + i + ": Start Year must be a valid year like 2026."; return false; }
                if (!string.IsNullOrWhiteSpace(endYear) && !yearRegex.IsMatch(endYear)) { message = "Experience " + i + ": End Year must be a valid year like 2026."; return false; }
                if (description.Length > 1000) { message = "Experience " + i + ": Description must be at most 1000 characters."; return false; }
            }

            for (int i = 1; i <= 5; i++)
            {
                string title = GetTextBoxValue("txtAppProj" + i + "Title").Trim();
                string techStack = GetTextBoxValue("txtAppProj" + i + "TechStack").Trim();
                string description = GetTextBoxValue("txtAppProj" + i + "Description").Trim();

                if (title.Length > 50) { message = "Project " + i + ": Project Title must be at most 50 characters."; return false; }
                if (description.Length > 250) { message = "Project " + i + ": Description must be at most 250 characters."; return false; }
                if (SplitCommaSeparated(techStack).Count > 4) { message = "Project " + i + ": Tech Stack allows maximum 4 comma-separated items."; return false; }
            }

            if (SplitCommaSeparated(GetTextBoxValue("txtAppSkillProgrammingLanguages")).Count > 5) { message = "Programming Languages allows maximum 5 comma-separated items."; return false; }
            if (SplitCommaSeparated(GetTextBoxValue("txtAppSkillFrameworksLibraries")).Count > 5) { message = "Frameworks / Libraries allows maximum 5 comma-separated items."; return false; }
            if (SplitCommaSeparated(GetTextBoxValue("txtAppSkillToolsCloudDatabase")).Count > 5) { message = "Tools / Cloud / Database Skills allows maximum 5 comma-separated items."; return false; }
            if (SplitCommaSeparated(GetTextBoxValue("txtAppSkillSoftSkillsLanguages")).Count > 5) { message = "Soft Skills / Languages allows maximum 5 comma-separated items."; return false; }
            if (SplitCommaSeparated(GetTextBoxValue("txtAppSkillCustomItems")).Count > 5) { message = "User Selection Items allows maximum 5 comma-separated items."; return false; }

            return true;
        }

        private ResumeEducationEntry BuildEducationEntry(int index, string prefix)
        {
            string school = GetTextBoxValue(prefix + index + "SchoolName").Trim();
            string degree = GetTextBoxValue(prefix + index + "Degree").Trim();
            string coursework = GetTextBoxValue(prefix + index + "Coursework").Trim();
            if (string.IsNullOrWhiteSpace(school) && string.IsNullOrWhiteSpace(degree) && string.IsNullOrWhiteSpace(coursework))
                return null;

            return new ResumeEducationEntry
            {
                SchoolName = school,
                Location = GetTextBoxValue(prefix + index + "Location").Trim(),
                Degree = degree,
                StartMonth = ParseMonth(GetDropDownValue("ddlAppEdu" + index + "StartMonth")),
                StartYear = ParseInt(GetTextBoxValue(prefix + index + "StartYear")),
                EndMonth = ParseMonth(GetDropDownValue("ddlAppEdu" + index + "EndMonth")),
                EndYear = ParseInt(GetTextBoxValue(prefix + index + "EndYear")),
                Grade = GetTextBoxValue(prefix + index + "Grade").Trim(),
                Coursework = coursework
            };
        }

        private ResumeExperienceEntry BuildExperienceEntry(int index, string prefix)
        {
            string jobTitle = GetTextBoxValue(prefix + index + "JobTitle").Trim();
            string company = GetTextBoxValue(prefix + index + "Company").Trim();
            string description = GetTextBoxValue(prefix + index + "Description").Trim();
            if (string.IsNullOrWhiteSpace(jobTitle) && string.IsNullOrWhiteSpace(company) && string.IsNullOrWhiteSpace(description))
                return null;

            return new ResumeExperienceEntry
            {
                JobTitle = jobTitle,
                Company = company,
                Location = GetTextBoxValue(prefix + index + "Location").Trim(),
                StartMonth = ParseMonth(GetDropDownValue("ddlAppExp" + index + "StartMonth")),
                StartYear = ParseInt(GetTextBoxValue(prefix + index + "StartYear")),
                EndMonth = ParseMonth(GetDropDownValue("ddlAppExp" + index + "EndMonth")),
                EndYear = ParseInt(GetTextBoxValue(prefix + index + "EndYear")),
                IsCurrent = GetCheckBoxValue("chkAppExp" + index + "Current"),
                Bullets = SplitBullets(description)
            };
        }

        private ResumeProjectEntry BuildProjectEntry(int index, string prefix)
        {
            string title = GetTextBoxValue(prefix + index + "Title").Trim();
            string tech = GetTextBoxValue(prefix + index + "TechStack").Trim();
            string description = GetTextBoxValue(prefix + index + "Description").Trim();
            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(tech) && string.IsNullOrWhiteSpace(description))
                return null;

            return new ResumeProjectEntry
            {
                ProjectTitle = title,
                TechStack = SplitCommaSeparated(tech).Take(4).ToList(),
                Description = description
            };
        }

        private List<string> SplitBullets(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new List<string>();

            return value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
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

            var sections = new List<string>();
            AddSkillLine(sections, "Programming Languages", skillGroups.ProgrammingLanguages);
            AddSkillLine(sections, "Frameworks/Libraries", skillGroups.FrameworksLibraries);
            AddSkillLine(sections, "Tools/Cloud/Database Skills", skillGroups.ToolsCloudDatabaseSkills);
            AddSkillLine(sections, "Soft Skills/Languages", skillGroups.SoftSkillsLanguages);
            if (!string.IsNullOrWhiteSpace(skillGroups.CustomHeading))
                AddSkillLine(sections, skillGroups.CustomHeading, skillGroups.CustomItems);

            return sections;
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
            return SplitEntryBlocks(text).Select(ParseEducationEntry).Where(entry => entry != null).Take(2).ToList();
        }

        private List<ResumeExperienceEntry> ParseExperienceEntries(string text)
        {
            return SplitEntryBlocks(text).Select(ParseExperienceEntry).Where(entry => entry != null).Take(5).ToList();
        }

        private List<ResumeProjectEntry> ParseProjectEntries(string text)
        {
            return SplitEntryBlocks(text).Select(ParseProjectEntry).Where(entry => entry != null).Take(5).ToList();
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

            return string.Join(" | ", new[] { entry.SchoolName, entry.Location, entry.Degree, entry.Grade, entry.Coursework }.Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private string FormatExperienceEntry(ResumeExperienceEntry entry)
        {
            if (entry == null)
                return string.Empty;

            var parts = new List<string> { entry.JobTitle, entry.Company, entry.Location };
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
                AddSkillLine(sections, "Programming Languages", skillGroups.ProgrammingLanguages);
                AddSkillLine(sections, "Frameworks/Libraries", skillGroups.FrameworksLibraries);
                AddSkillLine(sections, "Tools/Cloud/Database Skills", skillGroups.ToolsCloudDatabaseSkills);
                AddSkillLine(sections, "Soft Skills/Languages", skillGroups.SoftSkillsLanguages);
                if (!string.IsNullOrWhiteSpace(skillGroups.CustomHeading))
                    AddSkillLine(sections, skillGroups.CustomHeading, skillGroups.CustomItems);

                if (sections.Count > 0)
                    return string.Join(Environment.NewLine, sections);
            }

            return JoinLines(fallbackSkills);
        }

        private void AddSkillLine(List<string> sections, string heading, IEnumerable<string> values)
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

        private string GetJobInfo(int jobId)
        {
            using (SqlConnection con = new SqlConnection(str))
            using (SqlCommand cmd = new SqlCommand("SELECT Title, CompanyName FROM Jobs WHERE JobId = @JobId", con))
            {
                cmd.Parameters.AddWithValue("@JobId", jobId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return "Application resume draft";

                    return "For " + reader["Title"].ToString() + " at " + reader["CompanyName"].ToString();
                }
            }
        }
    }
}
