using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace IntelliJob.User
{
    /// <summary>
    /// Dynamic resume editor cards for Education, Experience, and Projects.
    ///
    /// How it works
    /// ─────────────
    /// Controls are built ONCE in Page_Init, never again.
    /// Page_Init reads Request.Form to figure out the FINAL count for this
    /// request (including any Add or Delete action in progress) and builds
    /// exactly that many controls — so ViewState restores existing card values
    /// correctly, the new slot is empty, and deleted slots are gone.
    ///
    ///  Add    → Page_Init detects the Add button in the form, increments the
    ///           count, builds N+1 controls.  ViewState fills cards 1..N
    ///           automatically; card N+1 is empty.  Click handler only sets
    ///           edit-mode flag.
    ///
    ///  Delete → Page_Init detects the Delete button, reads ALL current card
    ///           values directly from Request.Form (controls don't exist yet),
    ///           drops the target slot, shifts the rest, stores them in
    ///           _pendingXxxData, builds N-1 controls.  After ViewState loads
    ///           ApplyPendingCardData() overwrites control values with the
    ///           shifted data.  Click handler only sets edit-mode flag.
    /// </summary>
    public partial class ResumeEnhancer
    {
        // ── Month names ────────────────────────────────────────────────────────
        private static readonly string[] MonthNames =
        {
            "January","February","March","April","May","June",
            "July","August","September","October","November","December"
        };

        // Pending data written after ViewState loads (delete path only)
        private List<EduCardData>  _pendingEduData;
        private List<ExpCardData>  _pendingExpData;
        private List<ProjCardData> _pendingProjData;

        // Final counts decided in Page_Init — must be re-applied in Page_Load
        // because IPostBackDataHandler overwrites HiddenField.Value after Page_Init.
        private int _finalEduCount  = 1;
        private int _finalExpCount  = 1;
        private int _finalProjCount = 1;

        // ══════════════════════════════════════════════════════════════════════
        // Page_Init — runs before ViewState; sets final control count
        // ══════════════════════════════════════════════════════════════════════
        protected void Page_Init(object sender, EventArgs e)
        {
            // Current counts from the submitted form
            int eduCount  = ReadFormCount("hfEduCount",  1);
            int expCount  = ReadFormCount("hfExpCount",  1);
            int projCount = ReadFormCount("hfProjCount", 1);

            // Detect which action button fired this postback
            if (IsButtonInForm("btnAddEdu"))
            {
                eduCount++;
            }
            else if (IsButtonInForm("btnAddExp"))
            {
                expCount++;
            }
            else if (IsButtonInForm("btnAddProj"))
            {
                projCount++;
            }
            else
            {
                // Check for a delete button (IDs: btnDel_edu_3, btnDel_exp_2, etc.)
                string delKey = FindDeleteButtonKey();
                if (!string.IsNullOrEmpty(delKey))
                {
                    // delKey local part: "btnDel_edu_3"
                    string[] parts = delKey.Split('_');   // ["btnDel","edu","3"]
                    if (parts.Length == 3)
                    {
                        string section = parts[1];
                        int removeIdx;
                        if (int.TryParse(parts[2], out removeIdx))
                        {
                            switch (section)
                            {
                                case "edu":
                                    _pendingEduData = CollectEduFromForm(eduCount, removeIdx);
                                    eduCount = _pendingEduData.Count;
                                    break;
                                case "exp":
                                    _pendingExpData = CollectExpFromForm(expCount, removeIdx);
                                    expCount = _pendingExpData.Count;
                                    break;
                                case "proj":
                                    _pendingProjData = CollectProjFromForm(projCount, removeIdx);
                                    projCount = _pendingProjData.Count;
                                    break;
                            }
                        }
                    }
                }
            }

            // Store final counts in instance fields — Page_Load will re-apply
            // them to the HiddenFields after IPostBackDataHandler has run.
            _finalEduCount  = eduCount;
            _finalExpCount  = expCount;
            _finalProjCount = projCount;

            // Build controls exactly once — ViewState will restore existing values
            BuildEduCards(eduCount);
            BuildExpCards(expCount);
            BuildProjCards(projCount);
        }

        /// <summary>
        /// Called from Page_Load AFTER ViewState and IPostBackDataHandler have run.
        /// 1. Re-applies the final card counts to the HiddenFields (IPostBackDataHandler
        ///    overwrites them with the old submitted value after Page_Init sets them).
        /// 2. Writes shifted card values into controls on delete.
        /// </summary>
        internal void ApplyPendingCardData()
        {
            // Re-stamp the counts that Page_Init computed — IPostBackDataHandler
            // has already overwritten HiddenField.Value with the submitted (old) value.
            if (hfEduCount  != null) hfEduCount.Value  = _finalEduCount.ToString();
            if (hfExpCount  != null) hfExpCount.Value  = _finalExpCount.ToString();
            if (hfProjCount != null) hfProjCount.Value = _finalProjCount.ToString();

            // Write shifted values into controls on delete
            if (_pendingEduData != null)
                for (int i = 0; i < _pendingEduData.Count; i++)
                    WriteEduCardToControls(i + 1, _pendingEduData[i]);

            if (_pendingExpData != null)
                for (int i = 0; i < _pendingExpData.Count; i++)
                    WriteExpCardToControls(i + 1, _pendingExpData[i]);

            if (_pendingProjData != null)
                for (int i = 0; i < _pendingProjData.Count; i++)
                    WriteProjCardToControls(i + 1, _pendingProjData[i]);
        }

        // ── Add / Delete click handlers — counts already adjusted in Page_Init ──
        protected void btnAddEdu_Click(object sender, EventArgs e)    { PreviewEditMode = true; }
        protected void btnAddExp_Click(object sender, EventArgs e)    { PreviewEditMode = true; }
        protected void btnAddProj_Click(object sender, EventArgs e)   { PreviewEditMode = true; }
        protected void btnDeleteCard_Click(object sender, EventArgs e) { PreviewEditMode = true; }

        // ══════════════════════════════════════════════════════════════════════
        // Card builders — called ONLY from Page_Init (and PopulateStructuredPreview)
        // ══════════════════════════════════════════════════════════════════════
        private void BuildEduCards(int count)
        {
            phEduCards.Controls.Clear();
            for (int i = 1; i <= count; i++)
                phEduCards.Controls.Add(CreateEduCard(i));
            // Place Add button next to the section heading, outside any card
            if (phEduAddBtn != null)
            {
                phEduAddBtn.Controls.Clear();
                phEduAddBtn.Controls.Add(BuildSectionAddButton("btnAddEdu", "Education", btnAddEdu_Click));
            }
        }

        private void BuildExpCards(int count)
        {
            phExpCards.Controls.Clear();
            for (int i = 1; i <= count; i++)
                phExpCards.Controls.Add(CreateExpCard(i));
            if (phExpAddBtn != null)
            {
                phExpAddBtn.Controls.Clear();
                phExpAddBtn.Controls.Add(BuildSectionAddButton("btnAddExp", "Experience", btnAddExp_Click));
            }
        }

        private void BuildProjCards(int count)
        {
            phProjCards.Controls.Clear();
            for (int i = 1; i <= count; i++)
                phProjCards.Controls.Add(CreateProjCard(i));
            if (phProjAddBtn != null)
            {
                phProjAddBtn.Controls.Clear();
                phProjAddBtn.Controls.Add(BuildSectionAddButton("btnAddProj", "Project", btnAddProj_Click));
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // Card HTML factories
        // ══════════════════════════════════════════════════════════════════════
        private Control CreateEduCard(int idx)
        {
            var outer = Div("row");
            var col   = Div("col-12");
            var card  = Div("resume-section-card");

            card.Controls.Add(BuildCardHeader("Education " + idx, "edu", idx));
            card.Controls.Add(BuildRow(
                BuildCol("col-md-4",  "School Name", MakeText("txtEnhEdu" + idx + "SchoolName", 100)),
                BuildCol("col-md-4",  "Location",    MakeText("txtEnhEdu" + idx + "Location",    50)),
                BuildCol("col-md-4",  "Degree",      MakeText("txtEnhEdu" + idx + "Degree",      50))
            ));
            card.Controls.Add(BuildRow(
                BuildCol("col-6 col-md-3", "Start Month", MakeMonthDdl("ddlEnhEdu" + idx + "StartMonth")),
                BuildCol("col-6 col-md-3", "Start Year",  MakeText("txtEnhEdu" + idx + "StartYear", 4)),
                BuildCol("col-6 col-md-3", "End Month",   MakeMonthDdl("ddlEnhEdu" + idx + "EndMonth")),
                BuildCol("col-6 col-md-3", "End Year",    MakeText("txtEnhEdu" + idx + "EndYear",   4))
            ));

            col.Controls.Add(card);
            outer.Controls.Add(col);
            return outer;
        }

        private Control CreateExpCard(int idx)
        {
            var outer = Div("row");
            var col   = Div("col-12");
            var card  = Div("resume-section-card");

            card.Controls.Add(BuildCardHeader("Experience " + idx, "exp", idx));
            card.Controls.Add(BuildRow(
                BuildCol("col-md-4", "Job Title", MakeText("txtEnhExp" + idx + "JobTitle", 50)),
                BuildCol("col-md-4", "Company",   MakeText("txtEnhExp" + idx + "Company",  50)),
                BuildCol("col-md-4", "Location",  MakeText("txtEnhExp" + idx + "Location", 50))
            ));
            card.Controls.Add(BuildRow(
                BuildCol("col-6 col-md-3", "Start Month", MakeMonthDdl("ddlEnhExp" + idx + "StartMonth")),
                BuildCol("col-6 col-md-3", "Start Year",  MakeText("txtEnhExp" + idx + "StartYear", 4)),
                BuildCol("col-6 col-md-3", "End Month",   MakeMonthDdl("ddlEnhExp" + idx + "EndMonth")),
                BuildCol("col-6 col-md-3", "End Year",    MakeText("txtEnhExp" + idx + "EndYear",   4))
            ));

            // "Currently working here" checkbox
            var rowCur = Div("row");
            var colCur = Div("col-12");
            var fg     = Div("form-group");
            var lbl    = new HtmlGenericControl("label");
            var chk    = new CheckBox { ID = "chkEnhExp" + idx + "Current", Enabled = false };
            lbl.Controls.Add(chk);
            lbl.Controls.Add(new LiteralControl(" Currently working here"));
            fg.Controls.Add(lbl);
            colCur.Controls.Add(fg);
            rowCur.Controls.Add(colCur);
            card.Controls.Add(rowCur);

            card.Controls.Add(BuildRow(
                BuildCol("col-12", "Bullets (one per line)", MakeArea("txtEnhExp" + idx + "Description", 5))
            ));

            col.Controls.Add(card);
            outer.Controls.Add(col);
            return outer;
        }

        private Control CreateProjCard(int idx)
        {
            var outer = Div("row");
            var col   = Div("col-12");
            var card  = Div("resume-section-card");

            card.Controls.Add(BuildCardHeader("Project " + idx, "proj", idx));
            card.Controls.Add(BuildRow(
                BuildCol("col-12", "Title",       MakeText("txtEnhProj" + idx + "Title",      50)),
                BuildCol("col-12", "Tech Stack",  MakeText("txtEnhProj" + idx + "TechStack",  100)),
                BuildCol("col-12", "Bullets (one per line)", MakeArea("txtEnhProj" + idx + "Description", 3))
            ));

            col.Controls.Add(card);
            outer.Controls.Add(col);
            return outer;
        }

        private Control BuildCardHeader(string title, string section, int idx,
                                        bool showAdd = false, string addBtnId = null,
                                        string addLabel = null, EventHandler addHandler = null)
        {
            var header = Div("card-header-row");
            var h6 = new HtmlGenericControl("h6") { InnerText = title };
            header.Controls.Add(h6);

            // Right-side button group (delete only — Add button lives in section heading row)
            var btnGroup = Div("card-header-actions");
            header.Controls.Add(btnGroup);

            // Delete button — always present
            var del = new Button
            {
                ID               = "btnDel_" + section + "_" + idx,
                Text             = "🗑 Delete",
                CssClass         = "card-delete-btn",
                CausesValidation = false,
                CommandArgument  = section + "|" + idx,
                ToolTip          = "Delete this entry"
            };
            del.Click += btnDeleteCard_Click;
            btnGroup.Controls.Add(del);

            return header;
        }

        /// <summary>
        /// Builds a standalone Add button for a section heading row.
        /// Called from BuildEduCards / BuildExpCards / BuildProjCards and
        /// placed into the phXxxAddBtn PlaceHolder in the ASPX markup.
        /// </summary>
        private Button BuildSectionAddButton(string addBtnId, string addLabel, EventHandler addHandler)
        {
            var add = new Button
            {
                ID               = addBtnId,
                Text             = "＋ Add " + (addLabel ?? string.Empty),
                CssClass         = "section-add-btn",
                CausesValidation = false,
                ToolTip          = "Add another " + (addLabel ?? "entry")
            };
            add.Click += addHandler;
            return add;
        }

        // ── Tiny builders ──────────────────────────────────────────────────────
        private HtmlGenericControl Div(string css) { var d = new HtmlGenericControl("div"); d.Attributes["class"] = css; return d; }

        private HtmlGenericControl BuildRow(params Control[] cols)
        { var r = Div("row"); foreach (var c in cols) r.Controls.Add(c); return r; }

        private HtmlGenericControl BuildCol(string css, string label, Control input)
        {
            var col = Div(css);
            var fg  = Div("form-group");
            fg.Controls.Add(new HtmlGenericControl("label") { InnerText = label });
            fg.Controls.Add(input);
            col.Controls.Add(fg);
            return col;
        }

        private TextBox MakeText(string id, int max) =>
            new TextBox { ID = id, CssClass = "form-control", MaxLength = max, ReadOnly = true };

        private TextBox MakeArea(string id, int rows) =>
            new TextBox { ID = id, CssClass = "form-control", TextMode = TextBoxMode.MultiLine, Rows = rows, ReadOnly = true };

        private DropDownList MakeMonthDdl(string id)
        {
            var ddl = new DropDownList { ID = id, CssClass = "form-control w-100", Enabled = false };
            ddl.Items.Add(new ListItem("Month", string.Empty));
            for (int m = 1; m <= 12; m++)
                ddl.Items.Add(new ListItem(MonthNames[m - 1], m.ToString("00")));
            return ddl;
        }

        // ══════════════════════════════════════════════════════════════════════
        // Read card values from Request.Form (Page_Init — controls don't exist yet)
        // ══════════════════════════════════════════════════════════════════════
        private List<EduCardData> CollectEduFromForm(int count, int skipIdx)
        {
            var list = new List<EduCardData>();
            for (int i = 1; i <= count; i++)
            {
                if (i == skipIdx) continue;
                list.Add(new EduCardData
                {
                    SchoolName = FormVal("txtEnhEdu" + i + "SchoolName"),
                    Location   = FormVal("txtEnhEdu" + i + "Location"),
                    Degree     = FormVal("txtEnhEdu" + i + "Degree"),
                    StartMonth = ParseEnhMonth(FormVal("ddlEnhEdu" + i + "StartMonth")),
                    StartYear  = FormVal("txtEnhEdu" + i + "StartYear"),
                    EndMonth   = ParseEnhMonth(FormVal("ddlEnhEdu" + i + "EndMonth")),
                    EndYear    = FormVal("txtEnhEdu" + i + "EndYear"),

                });
            }
            return list;
        }

        private List<ExpCardData> CollectExpFromForm(int count, int skipIdx)
        {
            var list = new List<ExpCardData>();
            for (int i = 1; i <= count; i++)
            {
                if (i == skipIdx) continue;
                list.Add(new ExpCardData
                {
                    JobTitle    = FormVal("txtEnhExp" + i + "JobTitle"),
                    Company     = FormVal("txtEnhExp" + i + "Company"),
                    Location    = FormVal("txtEnhExp" + i + "Location"),
                    StartMonth  = ParseEnhMonth(FormVal("ddlEnhExp" + i + "StartMonth")),
                    StartYear   = FormVal("txtEnhExp" + i + "StartYear"),
                    EndMonth    = ParseEnhMonth(FormVal("ddlEnhExp" + i + "EndMonth")),
                    EndYear     = FormVal("txtEnhExp" + i + "EndYear"),
                    IsCurrent   = FormCheckBox("chkEnhExp" + i + "Current"),
                    Description = FormVal("txtEnhExp" + i + "Description")
                });
            }
            return list;
        }

        private List<ProjCardData> CollectProjFromForm(int count, int skipIdx)
        {
            var list = new List<ProjCardData>();
            for (int i = 1; i <= count; i++)
            {
                if (i == skipIdx) continue;
                list.Add(new ProjCardData
                {
                    Title       = FormVal("txtEnhProj" + i + "Title"),
                    TechStack   = FormVal("txtEnhProj" + i + "TechStack"),
                    Description = FormVal("txtEnhProj" + i + "Description")
                });
            }
            return list;
        }

        // ── Request.Form reader — handles both plain and naming-container IDs ──
        private string FormVal(string id)
        {
            string v = Request.Form[id] ?? Request.Form["ctl00$ContentPlaceHolder1$" + id];
            return (v ?? string.Empty).Trim();
        }

        private bool FormCheckBox(string id)
        {
            string v = Request.Form[id] ?? Request.Form["ctl00$ContentPlaceHolder1$" + id];
            return !string.IsNullOrWhiteSpace(v);
        }

        // ── Detect which button fired this postback ────────────────────────────
        private bool IsButtonInForm(string buttonId)
        {
            string target = Request.Form["__EVENTTARGET"] ?? string.Empty;
            if (target.EndsWith(buttonId, StringComparison.Ordinal)) return true;
            return Request.Form[buttonId] != null
                || Request.Form["ctl00$ContentPlaceHolder1$" + buttonId] != null;
        }

        private string FindDeleteButtonKey()
        {
            // Check __EVENTTARGET first (LinkButton path)
            string target = Request.Form["__EVENTTARGET"] ?? string.Empty;
            if (!string.IsNullOrEmpty(target))
            {
                int d = target.LastIndexOf('$');
                string local = d >= 0 ? target.Substring(d + 1) : target;
                if (local.StartsWith("btnDel_", StringComparison.Ordinal)) return local;
            }

            // Regular Button: its UniqueID appears as a form key
            foreach (string key in Request.Form.AllKeys)
            {
                if (key == null) continue;
                int d = key.LastIndexOf('$');
                string local = d >= 0 ? key.Substring(d + 1) : key;
                if (local.StartsWith("btnDel_", StringComparison.Ordinal)) return local;
            }
            return null;
        }

        // ── Set hidden field value before ViewState overwrites it ──────────────
        private void SetHiddenCountEarly(string hfId, int count)
        {
            var hf = FindControlRecursive(this, hfId) as HiddenField;
            if (hf != null) hf.Value = count.ToString();
        }

        private int ReadFormCount(string hfId, int def)
        {
            string raw = Request.Form[hfId] ?? Request.Form["ctl00$ContentPlaceHolder1$" + hfId];
            int v;
            return (!string.IsNullOrWhiteSpace(raw) && int.TryParse(raw.Trim(), out v) && v >= 0) ? v : def;
        }

        // ══════════════════════════════════════════════════════════════════════
        // PopulateStructuredPreview — fills controls from a loaded document
        // (called on first load, not on add/delete postbacks)
        // ══════════════════════════════════════════════════════════════════════
        private void PopulateStructuredPreview(ResumeProfileDocument document)
        {
            document = document ?? new ResumeProfileDocument();
            ResumePersonalInfo pi = document.PersonalInfo ?? new ResumePersonalInfo();

            EnhSetText("txtEnhFullName",  Coalesce(pi.FullName,     document.FullName));
            EnhSetText("txtEnhEmail",     Coalesce(pi.Email,        document.Email));
            EnhSetText("txtEnhMobile",    Coalesce(pi.Mobile,       document.Mobile));
            EnhSetText("txtEnhAddress",   Coalesce(pi.Address,      document.Address));
            EnhSetText("txtEnhCountry",   pi.Country ?? string.Empty);
            EnhSetText("txtEnhLinkedIn",  Coalesce(pi.LinkedInUrl,  document.LinkedInUrl));
            EnhSetText("txtEnhPortfolio", Coalesce(pi.PortfolioUrl, document.PortfolioUrl));
            EnhSetText("txtEnhSummary",   Coalesce(document.ProfessionalSummary, document.Summary));

            // Education
            var eduList   = NormaliseEdu(document.EducationDetails, document.Education);
            int eduCount  = eduList.Count;
            _finalEduCount = eduCount;
            SetHiddenCountEarly("hfEduCount", eduCount);
            BuildEduCards(eduCount);
            for (int i = 0; i < eduList.Count; i++)
                WriteEduCardToControls(i + 1, EduCardData.From(eduList[i]));

            // Experience
            var expList   = NormaliseExp(document.ExperienceDetails, document.Experience);
            int expCount  = expList.Count;
            _finalExpCount = expCount;
            SetHiddenCountEarly("hfExpCount", expCount);
            BuildExpCards(expCount);
            for (int i = 0; i < expList.Count; i++)
                WriteExpCardToControls(i + 1, ExpCardData.From(expList[i]));

            // Projects
            var projList  = NormaliseProj(document.ProjectDetails, document.Projects);
            int projCount = projList.Count;
            _finalProjCount = projCount;
            SetHiddenCountEarly("hfProjCount", projCount);
            BuildProjCards(projCount);
            for (int i = 0; i < projList.Count; i++)
                WriteProjCardToControls(i + 1, ProjCardData.From(projList[i]));

            // Skills / Certs / Languages
            PopulateEnhSkillCards(document.SkillGroups, document.Skills);
            EnhSetText("txtEnhResumeCertifications", JoinLines(document.Certifications));
            EnhSetText("txtEnhResumeLanguages",       JoinLines(document.Languages));
        }

        private static string Coalesce(string a, string b) =>
            !string.IsNullOrWhiteSpace(a) ? a : (b ?? string.Empty);

        // ── Write card values into controls ────────────────────────────────────
        private void WriteEduCardToControls(int i, EduCardData d)
        {
            EnhSetText("txtEnhEdu" + i + "SchoolName", d.SchoolName);
            EnhSetText("txtEnhEdu" + i + "Location",   d.Location);
            EnhSetText("txtEnhEdu" + i + "Degree",     d.Degree);
            SetEnhSelectedMonth("ddlEnhEdu" + i + "StartMonth", d.StartMonth);
            EnhSetText("txtEnhEdu" + i + "StartYear",  d.StartYear);
            SetEnhSelectedMonth("ddlEnhEdu" + i + "EndMonth",   d.EndMonth);
            EnhSetText("txtEnhEdu" + i + "EndYear",    d.EndYear);

        }

        private void WriteExpCardToControls(int i, ExpCardData d)
        {
            EnhSetText("txtEnhExp" + i + "JobTitle",    d.JobTitle);
            EnhSetText("txtEnhExp" + i + "Company",     d.Company);
            EnhSetText("txtEnhExp" + i + "Location",    d.Location);
            SetEnhSelectedMonth("ddlEnhExp" + i + "StartMonth", d.StartMonth);
            EnhSetText("txtEnhExp" + i + "StartYear",   d.StartYear);
            SetEnhSelectedMonth("ddlEnhExp" + i + "EndMonth",   d.EndMonth);
            EnhSetText("txtEnhExp" + i + "EndYear",     d.EndYear);
            EnhSetCheckBox("chkEnhExp" + i + "Current", d.IsCurrent);
            EnhSetText("txtEnhExp" + i + "Description", d.Description);
        }

        private void WriteProjCardToControls(int i, ProjCardData d)
        {
            EnhSetText("txtEnhProj" + i + "Title",       d.Title);
            EnhSetText("txtEnhProj" + i + "TechStack",   d.TechStack);
            EnhSetText("txtEnhProj" + i + "Description", d.Description);
        }

        // ══════════════════════════════════════════════════════════════════════
        // BuildStructuredDocumentFromForm
        // ══════════════════════════════════════════════════════════════════════
        private ResumeProfileDocument BuildStructuredDocumentFromForm()
        {
            var edu    = ParseEnhEducationEntries();
            var exp    = ParseEnhExperienceEntries();
            var proj   = ParseEnhProjectEntries();
            var skills = BuildEnhSkillGroupsFromForm();
            string sum = EnhGetText("txtEnhSummary").Trim();

            return new ResumeProfileDocument
            {
                FullName            = EnhGetText("txtEnhFullName").Trim(),
                Email               = EnhGetText("txtEnhEmail").Trim(),
                Mobile              = EnhGetText("txtEnhMobile").Trim(),
                Address             = EnhGetText("txtEnhAddress").Trim(),
                Headline            = string.Empty,
                LinkedInUrl         = EnhGetText("txtEnhLinkedIn").Trim(),
                PortfolioUrl        = EnhGetText("txtEnhPortfolio").Trim(),
                Summary             = sum,
                ProfessionalSummary = sum,
                PersonalInfo = new ResumePersonalInfo
                {
                    FullName     = EnhGetText("txtEnhFullName").Trim(),
                    Email        = EnhGetText("txtEnhEmail").Trim(),
                    Mobile       = EnhGetText("txtEnhMobile").Trim(),
                    Address      = EnhGetText("txtEnhAddress").Trim(),
                    Country      = EnhGetText("txtEnhCountry").Trim(),
                    LinkedInUrl  = EnhGetText("txtEnhLinkedIn").Trim(),
                    PortfolioUrl = EnhGetText("txtEnhPortfolio").Trim()
                },
                EducationDetails  = edu,
                ExperienceDetails = exp,
                ProjectDetails    = proj,
                SkillGroups       = skills,
                Skills            = FlattenSkillGroups(skills),
                Education         = FlattenEducationEntries(edu),
                Experience        = FlattenExperienceEntries(exp),
                Projects          = FlattenProjectEntries(proj),
                Certifications    = SplitLines(EnhGetText("txtEnhResumeCertifications")),
                Languages         = SplitLines(EnhGetText("txtEnhResumeLanguages")),
                ParsedAt          = DateTime.UtcNow,
                IsValid           = true,
                RawText           = GetCurrentEditableDocument().RawText,
                OriginalFileName  = hfLoadedOriginalFileName.Value,
                StoredFilePath    = hfLoadedResumePath.Value
            };
        }

        private List<ResumeEducationEntry> ParseEnhEducationEntries()
        {
            int count = GetHiddenCount(hfEduCount);
            var list  = new List<ResumeEducationEntry>();
            for (int i = 1; i <= count; i++) { var e = BuildEnhEducationEntry(i); if (e != null) list.Add(e); }
            return list;
        }

        private ResumeEducationEntry BuildEnhEducationEntry(int i)
        {
            string school = EnhGetText("txtEnhEdu" + i + "SchoolName").Trim();
            string loc    = EnhGetText("txtEnhEdu" + i + "Location").Trim();
            string deg    = EnhGetText("txtEnhEdu" + i + "Degree").Trim();
            if (string.IsNullOrWhiteSpace(school) && string.IsNullOrWhiteSpace(loc) &&
                string.IsNullOrWhiteSpace(deg)) return null;
            return new ResumeEducationEntry
            {
                SchoolName = school, Location = loc, Degree = deg,
                StartMonth = ParseEnhMonth(EnhGetDropDownValue("ddlEnhEdu" + i + "StartMonth")),
                StartYear  = ParseEnhInt(EnhGetText("txtEnhEdu" + i + "StartYear")),
                EndMonth   = ParseEnhMonth(EnhGetDropDownValue("ddlEnhEdu" + i + "EndMonth")),
                EndYear    = ParseEnhInt(EnhGetText("txtEnhEdu" + i + "EndYear"))
            };
        }

        private List<ResumeExperienceEntry> ParseEnhExperienceEntries()
        {
            int count = GetHiddenCount(hfExpCount);
            var list  = new List<ResumeExperienceEntry>();
            for (int i = 1; i <= count; i++) { var e = BuildEnhExperienceEntry(i); if (e != null) list.Add(e); }
            return list;
        }

        private ResumeExperienceEntry BuildEnhExperienceEntry(int i)
        {
            string title = EnhGetText("txtEnhExp" + i + "JobTitle").Trim();
            string co    = EnhGetText("txtEnhExp" + i + "Company").Trim();
            string loc   = EnhGetText("txtEnhExp" + i + "Location").Trim();
            string desc  = EnhGetText("txtEnhExp" + i + "Description").Trim();
            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(co) &&
                string.IsNullOrWhiteSpace(loc)   && string.IsNullOrWhiteSpace(desc)) return null;
            return new ResumeExperienceEntry
            {
                JobTitle  = title, Company = co, Location = loc,
                StartMonth = ParseEnhMonth(EnhGetDropDownValue("ddlEnhExp" + i + "StartMonth")),
                StartYear  = ParseEnhInt(EnhGetText("txtEnhExp" + i + "StartYear")),
                EndMonth   = ParseEnhMonth(EnhGetDropDownValue("ddlEnhExp" + i + "EndMonth")),
                EndYear    = ParseEnhInt(EnhGetText("txtEnhExp" + i + "EndYear")),
                IsCurrent  = EnhGetCheckBox("chkEnhExp" + i + "Current"),
                Bullets    = SplitBullets(desc)
            };
        }

        private List<ResumeProjectEntry> ParseEnhProjectEntries()
        {
            int count = GetHiddenCount(hfProjCount);
            var list  = new List<ResumeProjectEntry>();
            for (int i = 1; i <= count; i++) { var e = BuildEnhProjectEntry(i); if (e != null) list.Add(e); }
            return list;
        }

        private ResumeProjectEntry BuildEnhProjectEntry(int i)
        {
            string title = EnhGetText("txtEnhProj" + i + "Title").Trim();
            string tech  = EnhGetText("txtEnhProj" + i + "TechStack").Trim();
            string desc  = EnhGetText("txtEnhProj" + i + "Description").Trim();
            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(tech) &&
                string.IsNullOrWhiteSpace(desc)) return null;
            return new ResumeProjectEntry
            {
                ProjectTitle = title,
                TechStack    = SplitCommaSeparated(tech).Take(10).ToList(),
                Bullets      = SplitBullets(desc),
                Description  = desc
            };
        }

        // ── Skills ─────────────────────────────────────────────────────────────
        private void PopulateEnhSkillCards(ResumeSkillGroups sg, IEnumerable<string> fallback)
        {
            EnhSetText("txtEnhSkillProgrammingLanguages", sg != null ? JoinCommaSeparated(sg.ProgrammingLanguages)     : JoinLines(fallback));
            EnhSetText("txtEnhSkillFrameworksLibraries",  sg != null ? JoinCommaSeparated(sg.FrameworksLibraries)      : string.Empty);
            EnhSetText("txtEnhSkillToolsCloudDatabase",   sg != null ? JoinCommaSeparated(sg.ToolsCloudDatabaseSkills) : string.Empty);
            EnhSetText("txtEnhSkillSoftSkillsLanguages",  sg != null ? JoinCommaSeparated(sg.SoftSkillsLanguages)      : string.Empty);
            EnhSetText("txtEnhSkillCustomHeading",        sg != null ? sg.CustomHeading                                : string.Empty);
            EnhSetText("txtEnhSkillCustomItems",          sg != null ? JoinCommaSeparated(sg.CustomItems)              : string.Empty);
        }

        private ResumeSkillGroups BuildEnhSkillGroupsFromForm() => new ResumeSkillGroups
        {
            ProgrammingLanguages     = SplitCommaSeparated(EnhGetText("txtEnhSkillProgrammingLanguages")).Take(10).ToList(),
            FrameworksLibraries      = SplitCommaSeparated(EnhGetText("txtEnhSkillFrameworksLibraries")).Take(10).ToList(),
            ToolsCloudDatabaseSkills = SplitCommaSeparated(EnhGetText("txtEnhSkillToolsCloudDatabase")).Take(10).ToList(),
            SoftSkillsLanguages      = SplitCommaSeparated(EnhGetText("txtEnhSkillSoftSkillsLanguages")).Take(10).ToList(),
            CustomHeading            = EnhGetText("txtEnhSkillCustomHeading").Trim(),
            CustomItems              = SplitCommaSeparated(EnhGetText("txtEnhSkillCustomItems")).Take(10).ToList()
        };

        // ══════════════════════════════════════════════════════════════════════
        // SetEnhancerEditorsReadOnly
        // ══════════════════════════════════════════════════════════════════════
        private void SetEnhancerEditorsReadOnly(bool readOnly)
        {
            foreach (Control c in EnumerateControlsRecursive(this))
            {
                if (c.ID == null) continue;
                if (!(c.ID.StartsWith("txtEnh") || c.ID.StartsWith("ddlEnh") || c.ID.StartsWith("chkEnh"))) continue;
                if      (c is TextBox tb)      tb.ReadOnly  = readOnly;
                else if (c is DropDownList ddl) ddl.Enabled = !readOnly;
                else if (c is CheckBox chk)    chk.Enabled  = !readOnly;
            }

            foreach (Control c in EnumerateControlsRecursive(this))
                if (c is Button b && b.ID != null && (b.ID.StartsWith("btnDel_") || b.ID == "btnAddEdu" || b.ID == "btnAddExp" || b.ID == "btnAddProj"))
                    b.Visible = !readOnly;
        }

        // ══════════════════════════════════════════════════════════════════════
        // Normalise / flatten helpers
        // ══════════════════════════════════════════════════════════════════════
        private static List<ResumeEducationEntry> NormaliseEdu(IEnumerable<ResumeEducationEntry> entries, IEnumerable<string> fb)
        {
            if (entries != null) { var l = entries.Where(e => e != null).ToList(); if (l.Count > 0) return l; }
            return fb == null ? new List<ResumeEducationEntry>()
                : fb.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => new ResumeEducationEntry { Coursework = s.Trim() }).ToList();
        }

        private static List<ResumeExperienceEntry> NormaliseExp(IEnumerable<ResumeExperienceEntry> entries, IEnumerable<string> fb)
        {
            if (entries != null) { var l = entries.Where(e => e != null).ToList(); if (l.Count > 0) return l; }
            return fb == null ? new List<ResumeExperienceEntry>()
                : fb.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => new ResumeExperienceEntry { Bullets = new List<string> { s.Trim() } }).ToList();
        }

        private static List<ResumeProjectEntry> NormaliseProj(IEnumerable<ResumeProjectEntry> entries, IEnumerable<string> fb)
        {
            if (entries != null) { var l = entries.Where(e => e != null).ToList(); if (l.Count > 0) return l; }
            return fb == null ? new List<ResumeProjectEntry>()
                : fb.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => new ResumeProjectEntry { Description = s.Trim() }).ToList();
        }

        private List<string> FlattenEducationEntries(IEnumerable<ResumeEducationEntry> entries) =>
            entries == null ? new List<string>() : entries.Select(FormatEducationEntryLine).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

        private List<string> FlattenExperienceEntries(IEnumerable<ResumeExperienceEntry> entries) =>
            entries == null ? new List<string>() : entries.Select(FormatExperienceEntryLine).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

        private List<string> FlattenProjectEntries(IEnumerable<ResumeProjectEntry> entries) =>
            entries == null ? new List<string>() : entries.Select(FormatProjectEntryLine).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

        private List<string> FlattenSkillGroups(ResumeSkillGroups sg)
        {
            if (sg == null) return new List<string>();
            var items = new List<string>();
            AddSkillLine(items, "Programming Languages",       sg.ProgrammingLanguages);
            AddSkillLine(items, "Frameworks/Libraries",        sg.FrameworksLibraries);
            AddSkillLine(items, "Tools/Cloud/Database Skills", sg.ToolsCloudDatabaseSkills);
            AddSkillLine(items, "Soft Skills/Languages",       sg.SoftSkillsLanguages);
            if (!string.IsNullOrWhiteSpace(sg.CustomHeading) && sg.CustomItems?.Count > 0)
                AddSkillLine(items, sg.CustomHeading, sg.CustomItems);
            return items;
        }

        private void AddSkillLine(List<string> items, string heading, IEnumerable<string> values)
        {
            if (values == null) return;
            var l = values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).ToList();
            if (l.Count > 0) items.Add(heading + ": " + string.Join(", ", l));
        }

        private string FormatEducationEntryLine(ResumeEducationEntry e)
        {
            if (e == null) return string.Empty;
            return string.Join(" | ", new[] { e.SchoolName, e.Location, e.Degree,
                FormatMonthYear(e.StartMonth, e.StartYear), FormatMonthYear(e.EndMonth, e.EndYear)
                }.Where(v => !string.IsNullOrWhiteSpace(v)));
        }

        private string FormatExperienceEntryLine(ResumeExperienceEntry e)
        {
            if (e == null) return string.Empty;
            var p = new List<string> { e.JobTitle, e.Company, e.Location,
                FormatMonthYear(e.StartMonth, e.StartYear),
                e.IsCurrent ? "Present" : FormatMonthYear(e.EndMonth, e.EndYear) };
            if (e.Bullets?.Count > 0)
                p.Add(string.Join("; ", e.Bullets.Where(b => !string.IsNullOrWhiteSpace(b))));
            return string.Join(" | ", p.Where(v => !string.IsNullOrWhiteSpace(v)));
        }

        private string FormatProjectEntryLine(ResumeProjectEntry e)
        {
            if (e == null) return string.Empty;
            string descPart = (e.Bullets != null && e.Bullets.Count > 0)
                ? string.Join("; ", e.Bullets.Where(b => !string.IsNullOrWhiteSpace(b)).Select(b => b.Trim()))
                : (e.Description ?? string.Empty);
            return string.Join(" | ", new[] { e.ProjectTitle,
                e.TechStack != null ? string.Join(", ", e.TechStack.Where(t => !string.IsNullOrWhiteSpace(t))) : string.Empty,
                descPart }.Where(v => !string.IsNullOrWhiteSpace(v)));
        }

        // ══════════════════════════════════════════════════════════════════════
        // Control finder / getter / setter
        // ══════════════════════════════════════════════════════════════════════
        private Control FindControlRecursive(Control root, string id)
        {
            if (root == null) return null;
            if (string.Equals(root.ID, id, StringComparison.Ordinal)) return root;
            foreach (Control c in root.Controls) { var m = FindControlRecursive(c, id); if (m != null) return m; }
            return null;
        }

        private static IEnumerable<Control> EnumerateControlsRecursive(Control root)
        {
            if (root == null) yield break;
            foreach (Control c in root.Controls)
            {
                yield return c;
                foreach (Control n in EnumerateControlsRecursive(c)) yield return n;
            }
        }

        private void EnhSetText(string id, string value)
        { var tb = FindControlRecursive(this, id) as TextBox; if (tb != null) tb.Text = value ?? string.Empty; }

        private string EnhGetText(string id)
        { var tb = FindControlRecursive(this, id) as TextBox; return tb != null ? tb.Text : string.Empty; }

        private string EnhGetDropDownValue(string id)
        { var ddl = FindControlRecursive(this, id) as DropDownList; return ddl != null ? ddl.SelectedValue : string.Empty; }

        private void EnhSetCheckBox(string id, bool value)
        { var chk = FindControlRecursive(this, id) as CheckBox; if (chk != null) chk.Checked = value; }

        private bool EnhGetCheckBox(string id)
        { var chk = FindControlRecursive(this, id) as CheckBox; return chk != null && chk.Checked; }

        private void SetEnhSelectedMonth(string id, int? month)
        {
            var ddl = FindControlRecursive(this, id) as DropDownList;
            if (ddl != null) ddl.SelectedValue = month.HasValue ? month.Value.ToString("00") : string.Empty;
        }

        private int GetHiddenCount(HiddenField hf)
        { int v; return (hf != null && int.TryParse(hf.Value, out v) && v >= 0) ? v : 0; }

        // ── Parse helpers ──────────────────────────────────────────────────────
        private int? ParseEnhMonth(string v) { int m; return (int.TryParse(v, out m) && m >= 1 && m <= 12) ? (int?)m : null; }
        private int? ParseEnhInt(string v)   { int n; return int.TryParse(v, out n) ? (int?)n : null; }

        private List<string> SplitBullets(string v) =>
            string.IsNullOrWhiteSpace(v) ? new List<string>()
            : v.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
               .Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

        private List<string> SplitCommaSeparated(string v) =>
            string.IsNullOrWhiteSpace(v) ? new List<string>()
            : v.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
               .Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s))
               .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        private void BindEnhancerMonthDropdowns() { /* no-op: dropdowns built inline in MakeMonthDdl */ }

        // ══════════════════════════════════════════════════════════════════════
        // Inner data structs
        // ══════════════════════════════════════════════════════════════════════
        private struct EduCardData
        {
            public string SchoolName, Location, Degree, StartYear, EndYear;
            public int?   StartMonth, EndMonth;
            public static EduCardData From(ResumeEducationEntry e) => new EduCardData
            {
                SchoolName = e.SchoolName ?? string.Empty, Location = e.Location ?? string.Empty,
                Degree     = e.Degree     ?? string.Empty,
                StartMonth = e.StartMonth, StartYear = e.StartYear.HasValue ? e.StartYear.Value.ToString() : string.Empty,
                EndMonth   = e.EndMonth,   EndYear   = e.EndYear.HasValue   ? e.EndYear.Value.ToString()   : string.Empty
            };
        }

        private struct ExpCardData
        {
            public string JobTitle, Company, Location, StartYear, EndYear, Description;
            public int?   StartMonth, EndMonth;
            public bool   IsCurrent;
            public static ExpCardData From(ResumeExperienceEntry e) => new ExpCardData
            {
                JobTitle    = e.JobTitle ?? string.Empty, Company  = e.Company  ?? string.Empty,
                Location    = e.Location ?? string.Empty,
                StartMonth  = e.StartMonth, StartYear = e.StartYear.HasValue ? e.StartYear.Value.ToString() : string.Empty,
                EndMonth    = e.EndMonth,   EndYear   = e.EndYear.HasValue   ? e.EndYear.Value.ToString()   : string.Empty,
                IsCurrent   = e.IsCurrent,
                Description = e.Bullets != null
                    ? string.Join(Environment.NewLine, e.Bullets.Where(b => !string.IsNullOrWhiteSpace(b)))
                    : string.Empty
            };
        }

        private struct ProjCardData
        {
            public string Title, TechStack, Description;
            public static ProjCardData From(ResumeProjectEntry e) => new ProjCardData
            {
                Title       = e.ProjectTitle ?? string.Empty,
                TechStack   = e.TechStack != null ? string.Join(", ", e.TechStack.Where(t => !string.IsNullOrWhiteSpace(t))) : string.Empty,
                Description = (e.Bullets != null && e.Bullets.Count > 0)
                    ? string.Join(Environment.NewLine, e.Bullets.Where(b => !string.IsNullOrWhiteSpace(b)))
                    : (e.Description ?? string.Empty)
            };
        }
    }
}
