using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IntelliJob.User
{
    /// <summary>
    /// Structured resume editor (same card layout / JSON shape as Resume Build and Gemini enhancedResumeDocument).
    /// </summary>
    public partial class ResumeEnhancer
    {
        private void BindEnhancerMonthDropdowns()
        {
            for (int i = 1; i <= 2; i++)
            {
                BindEnhancerMonthDropdown("ddlEnhEdu" + i + "StartMonth");
                BindEnhancerMonthDropdown("ddlEnhEdu" + i + "EndMonth");
            }

            for (int i = 1; i <= 5; i++)
            {
                BindEnhancerMonthDropdown("ddlEnhExp" + i + "StartMonth");
                BindEnhancerMonthDropdown("ddlEnhExp" + i + "EndMonth");
            }
        }

        private void BindEnhancerMonthDropdown(string id)
        {
            var dropDown = FindControlRecursive(this, id) as DropDownList;
            if (dropDown == null || dropDown.Items.Count > 0)
                return;

            dropDown.Items.Add(new ListItem("Month", string.Empty));
            string[] monthNames =
            {
                "January", "February", "March", "April", "May", "June",
                "July", "August", "September", "October", "November", "December"
            };

            for (int month = 1; month <= 12; month++)
                dropDown.Items.Add(new ListItem(monthNames[month - 1], month.ToString("00")));
        }

        private Control FindControlRecursive(Control root, string id)
        {
            if (root == null)
                return null;
            if (string.Equals(root.ID, id, StringComparison.Ordinal))
                return root;
            foreach (Control child in root.Controls)
            {
                Control match = FindControlRecursive(child, id);
                if (match != null)
                    return match;
            }

            return null;
        }

        private void PopulateStructuredPreview(ResumeProfileDocument document)
        {
            document = document ?? new ResumeProfileDocument();
            ResumePersonalInfo pi = document.PersonalInfo ?? new ResumePersonalInfo();

            EnhSetText("txtEnhFullName", !string.IsNullOrWhiteSpace(pi.FullName) ? pi.FullName : document.FullName);
            EnhSetText("txtEnhEmail", !string.IsNullOrWhiteSpace(pi.Email) ? pi.Email : document.Email);
            EnhSetText("txtEnhMobile", !string.IsNullOrWhiteSpace(pi.Mobile) ? pi.Mobile : document.Mobile);
            EnhSetText("txtEnhAddress", !string.IsNullOrWhiteSpace(pi.Address) ? pi.Address : document.Address);
            EnhSetText("txtEnhCountry", pi.Country ?? string.Empty);
            EnhSetText("txtEnhLinkedIn", !string.IsNullOrWhiteSpace(pi.LinkedInUrl) ? pi.LinkedInUrl : document.LinkedInUrl);
            EnhSetText("txtEnhPortfolio", !string.IsNullOrWhiteSpace(pi.PortfolioUrl) ? pi.PortfolioUrl : document.PortfolioUrl);

            string summary = !string.IsNullOrWhiteSpace(document.ProfessionalSummary) ? document.ProfessionalSummary : document.Summary;
            EnhSetText("txtEnhSummary", summary ?? string.Empty);

            PopulateEnhEducationCards(document.EducationDetails, document.Education);
            PopulateEnhExperienceCards(document.ExperienceDetails, document.Experience);
            PopulateEnhProjectCards(document.ProjectDetails, document.Projects);
            PopulateEnhSkillCards(document.SkillGroups, document.Skills);
            EnhSetText("txtEnhResumeCertifications", JoinLines(document.Certifications));
            EnhSetText("txtEnhResumeLanguages", JoinLines(document.Languages));
        }

        private void PopulateEnhEducationCards(IEnumerable<ResumeEducationEntry> entries, IEnumerable<string> fallback)
        {
            List<ResumeEducationEntry> list = entries != null ? entries.Where(en => en != null).Take(2).ToList() : new List<ResumeEducationEntry>();
            if (list.Count == 0 && fallback != null)
            {
                list = fallback.Where(item => !string.IsNullOrWhiteSpace(item))
                    .Select(item => new ResumeEducationEntry { Coursework = item.Trim() })
                    .Take(2)
                    .ToList();
            }

            for (int i = 1; i <= 2; i++)
            {
                ResumeEducationEntry entry = i <= list.Count ? list[i - 1] : null;
                EnhSetText("txtEnhEdu" + i + "SchoolName", entry != null ? entry.SchoolName : string.Empty);
                EnhSetText("txtEnhEdu" + i + "Location", entry != null ? entry.Location : string.Empty);
                EnhSetText("txtEnhEdu" + i + "Degree", entry != null ? entry.Degree : string.Empty);
                SetEnhSelectedMonth("ddlEnhEdu" + i + "StartMonth", entry != null ? entry.StartMonth : null);
                EnhSetText("txtEnhEdu" + i + "StartYear", entry != null && entry.StartYear.HasValue ? entry.StartYear.Value.ToString() : string.Empty);
                SetEnhSelectedMonth("ddlEnhEdu" + i + "EndMonth", entry != null ? entry.EndMonth : null);
                EnhSetText("txtEnhEdu" + i + "EndYear", entry != null && entry.EndYear.HasValue ? entry.EndYear.Value.ToString() : string.Empty);
                EnhSetText("txtEnhEdu" + i + "Grade", entry != null ? entry.Grade : string.Empty);
                EnhSetText("txtEnhEdu" + i + "Coursework", entry != null ? entry.Coursework : string.Empty);
            }
        }

        private void PopulateEnhExperienceCards(IEnumerable<ResumeExperienceEntry> entries, IEnumerable<string> fallback)
        {
            List<ResumeExperienceEntry> list = entries != null ? entries.Where(en => en != null).Take(5).ToList() : new List<ResumeExperienceEntry>();
            if (list.Count == 0 && fallback != null)
            {
                list = fallback.Where(item => !string.IsNullOrWhiteSpace(item))
                    .Select(item => new ResumeExperienceEntry { Bullets = new List<string> { item.Trim() } })
                    .Take(5)
                    .ToList();
            }

            for (int i = 1; i <= 5; i++)
            {
                ResumeExperienceEntry entry = i <= list.Count ? list[i - 1] : null;
                EnhSetText("txtEnhExp" + i + "JobTitle", entry != null ? entry.JobTitle : string.Empty);
                EnhSetText("txtEnhExp" + i + "Company", entry != null ? entry.Company : string.Empty);
                EnhSetText("txtEnhExp" + i + "Location", entry != null ? entry.Location : string.Empty);
                SetEnhSelectedMonth("ddlEnhExp" + i + "StartMonth", entry != null ? entry.StartMonth : null);
                EnhSetText("txtEnhExp" + i + "StartYear", entry != null && entry.StartYear.HasValue ? entry.StartYear.Value.ToString() : string.Empty);
                SetEnhSelectedMonth("ddlEnhExp" + i + "EndMonth", entry != null ? entry.EndMonth : null);
                EnhSetText("txtEnhExp" + i + "EndYear", entry != null && entry.EndYear.HasValue ? entry.EndYear.Value.ToString() : string.Empty);
                EnhSetCheckBox("chkEnhExp" + i + "Current", entry != null && entry.IsCurrent);
                EnhSetText("txtEnhExp" + i + "Description", entry != null ? JoinLines(entry.Bullets) : string.Empty);
            }
        }

        private void PopulateEnhProjectCards(IEnumerable<ResumeProjectEntry> entries, IEnumerable<string> fallback)
        {
            List<ResumeProjectEntry> list = entries != null ? entries.Where(en => en != null).Take(5).ToList() : new List<ResumeProjectEntry>();
            if (list.Count == 0 && fallback != null)
            {
                list = fallback.Where(item => !string.IsNullOrWhiteSpace(item))
                    .Select(item => new ResumeProjectEntry { Description = item.Trim() })
                    .Take(5)
                    .ToList();
            }

            for (int i = 1; i <= 5; i++)
            {
                ResumeProjectEntry entry = i <= list.Count ? list[i - 1] : null;
                EnhSetText("txtEnhProj" + i + "Title", entry != null ? entry.ProjectTitle : string.Empty);
                EnhSetText("txtEnhProj" + i + "TechStack", entry != null ? JoinCommaSeparated(entry.TechStack) : string.Empty);
                EnhSetText("txtEnhProj" + i + "Description", entry != null ? entry.Description : string.Empty);
            }
        }

        private void PopulateEnhSkillCards(ResumeSkillGroups skillGroups, IEnumerable<string> fallbackSkills)
        {
            EnhSetText("txtEnhSkillProgrammingLanguages", skillGroups != null ? JoinCommaSeparated(skillGroups.ProgrammingLanguages) : JoinLines(fallbackSkills));
            EnhSetText("txtEnhSkillFrameworksLibraries", skillGroups != null ? JoinCommaSeparated(skillGroups.FrameworksLibraries) : string.Empty);
            EnhSetText("txtEnhSkillToolsCloudDatabase", skillGroups != null ? JoinCommaSeparated(skillGroups.ToolsCloudDatabaseSkills) : string.Empty);
            EnhSetText("txtEnhSkillSoftSkillsLanguages", skillGroups != null ? JoinCommaSeparated(skillGroups.SoftSkillsLanguages) : string.Empty);
            EnhSetText("txtEnhSkillCustomHeading", skillGroups != null ? skillGroups.CustomHeading : string.Empty);
            EnhSetText("txtEnhSkillCustomItems", skillGroups != null ? JoinCommaSeparated(skillGroups.CustomItems) : string.Empty);
        }

        private void SetEnhSelectedMonth(string id, int? month)
        {
            var dropDown = FindControlRecursive(this, id) as DropDownList;
            if (dropDown != null)
                dropDown.SelectedValue = month.HasValue ? month.Value.ToString("00") : string.Empty;
        }

        private ResumeProfileDocument BuildStructuredDocumentFromForm()
        {
            List<ResumeEducationEntry> educationDetails = ParseEnhEducationEntries();
            List<ResumeExperienceEntry> experienceDetails = ParseEnhExperienceEntries();
            List<ResumeProjectEntry> projectDetails = ParseEnhProjectEntries();
            ResumeSkillGroups skillGroups = BuildEnhSkillGroupsFromForm();

            string summary = EnhGetText("txtEnhSummary").Trim();

            var doc = new ResumeProfileDocument
            {
                FullName = EnhGetText("txtEnhFullName").Trim(),
                Email = EnhGetText("txtEnhEmail").Trim(),
                Mobile = EnhGetText("txtEnhMobile").Trim(),
                Address = EnhGetText("txtEnhAddress").Trim(),
                Headline = string.Empty,
                LinkedInUrl = EnhGetText("txtEnhLinkedIn").Trim(),
                PortfolioUrl = EnhGetText("txtEnhPortfolio").Trim(),
                Summary = summary,
                ProfessionalSummary = summary,
                PersonalInfo = new ResumePersonalInfo
                {
                    FullName = EnhGetText("txtEnhFullName").Trim(),
                    Email = EnhGetText("txtEnhEmail").Trim(),
                    Mobile = EnhGetText("txtEnhMobile").Trim(),
                    Address = EnhGetText("txtEnhAddress").Trim(),
                    Country = EnhGetText("txtEnhCountry").Trim(),
                    LinkedInUrl = EnhGetText("txtEnhLinkedIn").Trim(),
                    PortfolioUrl = EnhGetText("txtEnhPortfolio").Trim()
                },
                EducationDetails = educationDetails,
                ExperienceDetails = experienceDetails,
                ProjectDetails = projectDetails,
                SkillGroups = skillGroups,
                Skills = FlattenSkillGroups(skillGroups),
                Education = FlattenEducationEntries(educationDetails),
                Experience = FlattenExperienceEntries(experienceDetails),
                Projects = FlattenProjectEntries(projectDetails),
                Certifications = SplitLines(EnhGetText("txtEnhResumeCertifications")),
                Languages = SplitLines(EnhGetText("txtEnhResumeLanguages")),
                ParsedAt = DateTime.UtcNow,
                IsValid = true,
                RawText = GetCurrentEditableDocument().RawText,
                OriginalFileName = hfLoadedOriginalFileName.Value,
                StoredFilePath = hfLoadedResumePath.Value
            };

            return doc;
        }

        private List<ResumeEducationEntry> ParseEnhEducationEntries()
        {
            var entries = new List<ResumeEducationEntry>();
            for (int i = 1; i <= 2; i++)
            {
                ResumeEducationEntry entry = BuildEnhEducationEntry(i);
                if (entry != null)
                    entries.Add(entry);
            }

            return entries;
        }

        private ResumeEducationEntry BuildEnhEducationEntry(int index)
        {
            string textPrefix = "txtEnhEdu";
            string monthPrefix = "ddlEnhEdu";
            string schoolName = EnhGetText(textPrefix + index + "SchoolName").Trim();
            string location = EnhGetText(textPrefix + index + "Location").Trim();
            string degree = EnhGetText(textPrefix + index + "Degree").Trim();
            string grade = EnhGetText(textPrefix + index + "Grade").Trim();
            string coursework = EnhGetText(textPrefix + index + "Coursework").Trim();

            if (string.IsNullOrWhiteSpace(schoolName) && string.IsNullOrWhiteSpace(location) && string.IsNullOrWhiteSpace(degree) &&
                string.IsNullOrWhiteSpace(grade) && string.IsNullOrWhiteSpace(coursework))
                return null;

            return new ResumeEducationEntry
            {
                SchoolName = schoolName,
                Location = location,
                Degree = degree,
                StartMonth = ParseEnhMonth(EnhGetDropDownValue(monthPrefix + index + "StartMonth")),
                StartYear = ParseEnhInt(EnhGetText(textPrefix + index + "StartYear")),
                EndMonth = ParseEnhMonth(EnhGetDropDownValue(monthPrefix + index + "EndMonth")),
                EndYear = ParseEnhInt(EnhGetText(textPrefix + index + "EndYear")),
                Grade = grade,
                Coursework = coursework
            };
        }

        private List<ResumeExperienceEntry> ParseEnhExperienceEntries()
        {
            var entries = new List<ResumeExperienceEntry>();
            for (int i = 1; i <= 5; i++)
            {
                ResumeExperienceEntry entry = BuildEnhExperienceEntry(i);
                if (entry != null)
                    entries.Add(entry);
            }

            return entries;
        }

        private ResumeExperienceEntry BuildEnhExperienceEntry(int index)
        {
            string textPrefix = "txtEnhExp";
            string monthPrefix = "ddlEnhExp";
            string checkPrefix = "chkEnhExp";
            string jobTitle = EnhGetText(textPrefix + index + "JobTitle").Trim();
            string company = EnhGetText(textPrefix + index + "Company").Trim();
            string location = EnhGetText(textPrefix + index + "Location").Trim();
            string description = EnhGetText(textPrefix + index + "Description").Trim();

            if (string.IsNullOrWhiteSpace(jobTitle) && string.IsNullOrWhiteSpace(company) && string.IsNullOrWhiteSpace(location) &&
                string.IsNullOrWhiteSpace(description))
                return null;

            return new ResumeExperienceEntry
            {
                JobTitle = jobTitle,
                Company = company,
                Location = location,
                StartMonth = ParseEnhMonth(EnhGetDropDownValue(monthPrefix + index + "StartMonth")),
                StartYear = ParseEnhInt(EnhGetText(textPrefix + index + "StartYear")),
                EndMonth = ParseEnhMonth(EnhGetDropDownValue(monthPrefix + index + "EndMonth")),
                EndYear = ParseEnhInt(EnhGetText(textPrefix + index + "EndYear")),
                IsCurrent = EnhGetCheckBox(checkPrefix + index + "Current"),
                Bullets = SplitBullets(description)
            };
        }

        private List<ResumeProjectEntry> ParseEnhProjectEntries()
        {
            var entries = new List<ResumeProjectEntry>();
            for (int i = 1; i <= 5; i++)
            {
                ResumeProjectEntry entry = BuildEnhProjectEntry(i);
                if (entry != null)
                    entries.Add(entry);
            }

            return entries;
        }

        private ResumeProjectEntry BuildEnhProjectEntry(int index)
        {
            string textPrefix = "txtEnhProj";
            string title = EnhGetText(textPrefix + index + "Title").Trim();
            string techStack = EnhGetText(textPrefix + index + "TechStack").Trim();
            string description = EnhGetText(textPrefix + index + "Description").Trim();

            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(techStack) && string.IsNullOrWhiteSpace(description))
                return null;

            return new ResumeProjectEntry
            {
                ProjectTitle = title,
                TechStack = SplitCommaSeparated(techStack).Take(4).ToList(),
                Description = description
            };
        }

        private ResumeSkillGroups BuildEnhSkillGroupsFromForm()
        {
            string programming = EnhGetText("txtEnhSkillProgrammingLanguages");
            string frameworks = EnhGetText("txtEnhSkillFrameworksLibraries");
            string tools = EnhGetText("txtEnhSkillToolsCloudDatabase");
            string softSkills = EnhGetText("txtEnhSkillSoftSkillsLanguages");
            string customHeading = EnhGetText("txtEnhSkillCustomHeading").Trim();
            string customItems = EnhGetText("txtEnhSkillCustomItems");

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

        private List<string> SplitBullets(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new List<string>();

            return value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToList();
        }

        private int? ParseEnhMonth(string value)
        {
            int month;
            if (int.TryParse(value, out month) && month >= 1 && month <= 12)
                return month;
            return null;
        }

        private int? ParseEnhInt(string value)
        {
            int number;
            if (int.TryParse(value, out number))
                return number;
            return null;
        }

        private List<string> FlattenEducationEntries(IEnumerable<ResumeEducationEntry> entries)
        {
            return entries == null ? new List<string>() : entries.Select(FormatEducationEntryLine).Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
        }

        private List<string> FlattenExperienceEntries(IEnumerable<ResumeExperienceEntry> entries)
        {
            return entries == null ? new List<string>() : entries.Select(FormatExperienceEntryLine).Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
        }

        private List<string> FlattenProjectEntries(IEnumerable<ResumeProjectEntry> entries)
        {
            return entries == null ? new List<string>() : entries.Select(FormatProjectEntryLine).Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
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

        private string FormatEducationEntryLine(ResumeEducationEntry entry)
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

        private string FormatExperienceEntryLine(ResumeExperienceEntry entry)
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

        private string FormatProjectEntryLine(ResumeProjectEntry entry)
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

        private void EnhSetText(string id, string value)
        {
            var textBox = FindControlRecursive(this, id) as TextBox;
            if (textBox != null)
                textBox.Text = value ?? string.Empty;
        }

        private string EnhGetText(string id)
        {
            var textBox = FindControlRecursive(this, id) as TextBox;
            return textBox != null ? textBox.Text : string.Empty;
        }

        private string EnhGetDropDownValue(string id)
        {
            var dropDown = FindControlRecursive(this, id) as DropDownList;
            return dropDown != null ? dropDown.SelectedValue : string.Empty;
        }

        private void EnhSetCheckBox(string id, bool value)
        {
            var checkBox = FindControlRecursive(this, id) as CheckBox;
            if (checkBox != null)
                checkBox.Checked = value;
        }

        private bool EnhGetCheckBox(string id)
        {
            var checkBox = FindControlRecursive(this, id) as CheckBox;
            return checkBox != null && checkBox.Checked;
        }

        private void SetEnhancerEditorsReadOnly(bool readOnly)
        {
            foreach (Control c in EnumerateControlsRecursive(this))
            {
                if (c.ID == null)
                    continue;

                if (!(c.ID.StartsWith("txtEnh", StringComparison.Ordinal) || c.ID.StartsWith("ddlEnh", StringComparison.Ordinal) ||
                      c.ID.StartsWith("chkEnh", StringComparison.Ordinal)))
                    continue;

                if (c is TextBox tb)
                    tb.ReadOnly = readOnly;
                else if (c is DropDownList ddl)
                    ddl.Enabled = !readOnly;
                else if (c is CheckBox chk)
                    chk.Enabled = !readOnly;
            }
        }

        private static IEnumerable<Control> EnumerateControlsRecursive(Control root)
        {
            if (root == null)
                yield break;

            foreach (Control c in root.Controls)
            {
                yield return c;
                foreach (Control nested in EnumerateControlsRecursive(c))
                    yield return nested;
            }
        }
    }
}
