<%@ Page Title="" Language="C#" MasterPageFile="~/User/UserMaster.Master" AutoEventWireup="true" CodeBehind="ApplicationResumeBuild.aspx.cs" Inherits="IntelliJob.User.ApplicationResumeBuild" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        body {
            background: linear-gradient(180deg, #fff7f8 0%, #ffffff 18%, #ffffff 100%);
        }

        .page-header-section {
            background: linear-gradient(100deg, #da2461 10%, #011b43 90%);
            padding: 54px 0 28px;
            min-height: 320px;
        }

        .page-header-section .container {
            padding: 80px 32px 0 32px;
        }

        .page-header-section h2 {
            font-size: 40px;
            font-weight: 700;
            text-transform: capitalize;
            font-family: "Muli", sans-serif;
            color: #ffffff;
            margin-bottom: 10px;
        }

        .page-header-section p,
        .page-header-section .job-info-line {
            color: #ffffff;
            font-size: 20px;
            max-width: 720px;
            line-height: 1.6;
        }

        .resume-builder-shell {
            padding: 0 0 80px;
        }

        .resume-builder-main-col {
            max-width: 1180px;
            margin-left: auto !important;
            margin-right: auto !important;
        }

        .interview-setup-card.resume-builder-setup {
            background: rgba(255, 255, 255, 0.96);
            border-radius: 24px;
            box-shadow: 0 18px 40px rgba(15, 23, 42, 0.08);
            padding: 40px;
            margin-bottom: 30px;
            border: 1px solid #edf2f7;
        }

        .resume-draft-note {
            background: #fff8e6;
            border: 1px solid #fde2a5;
            color: #8a5d00;
            border-radius: 16px;
            padding: 14px 16px;
            font-size: 14px;
            line-height: 1.6;
            margin-bottom: 18px;
        }

        .resume-builder-form .form-control {
            border-radius: 0;
            border: 1px solid #fb246a;
            padding: 12px 15px;
            font-size: 15px;
            transition: border-color 0.3s ease, box-shadow 0.3s ease;
            background: #fff;
        }

        .resume-builder-form select.form-control {
            padding: 14px 18px;
            font-size: 16px;
            font-weight: 500;
            min-height: 50px;
        }

        .resume-builder-form .form-control:focus {
            border-color: #011b43;
            box-shadow: 0 0 0 0.15rem rgba(251, 36, 106, 0.25);
        }

        .app-resume-back-link.boxed-btn,
        .app-resume-back-link.boxed-btn:visited {
            background: linear-gradient(135deg, #da2461 0%, #fb246a 100%) !important;
            color: #fff !important;
            border-color: #fb246a !important;
        }
        .app-resume-back-link.boxed-btn:hover,
        .app-resume-back-link.boxed-btn:focus {
            background: #011b43 !important;
            border-color: #011b43 !important;
            color: #fff !important;
        }

        .resume-builder-form textarea.form-control {
            height: 120px !important;
            min-height: 120px !important;
            max-height: 220px !important;
            overflow-y: auto !important;
            resize: vertical;
            width: 100%;
        }

        .resume-builder-form .resume-summary-inner textarea.form-control {
            min-height: 140px !important;
        }

        .resume-summary-inner {
            padding: 4px 4px 0;
        }

        .resume-section-card {
            background: #ffffff;
            border: 1px solid #edf0f5;
            border-radius: 16px;
            padding: 18px 18px 12px;
            margin-bottom: 18px;
            box-shadow: 0 10px 24px rgba(15, 23, 42, 0.04);
            width: 100%;
        }

        .resume-section-card h6 {
            margin-bottom: 10px;
        }

        .resume-section-help {
            color: #6b7280;
            font-size: 13px;
            line-height: 1.6;
            margin-bottom: 14px;
        }

        .resume-builder-form label {
            font-weight: 600;
            color: #374151;
            margin-bottom: 6px;
            font-size: 14px;
        }

        .resume-field-span .form-control {
            width: 100%;
        }

        .resume-builder-actions {
            clear: both;
            margin-top: 24px;
            margin-bottom: 72px;
        }

        .resume-add-card-btn.boxed-btn {
            background: linear-gradient(135deg, #da2461 0%, #fb246a 100%);
            color: #fff !important;
            border: 1px solid transparent;
            letter-spacing: 2px;
        }

        .resume-add-card-btn.boxed-btn:hover,
        .resume-add-card-btn.boxed-btn:focus {
            background: #011b43;
            color: #fff !important;
            border-color: #011b43;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <section class="resume-builder-page">
        <div class="page-header-section">
            <div class="container">
                <div class="row">
                    <div class="col-lg-10">
                        <h2>Application Resume Draft</h2>
                        <p class="job-info-line"><asp:Label ID="lblJobInfo" runat="server" /></p>
                    </div>
                </div>
            </div>
        </div>
        <section class="resume-builder-shell">
            <div class="container">
                <div class="row justify-content-center">
                    <div class="col-12 resume-builder-main-col">
                        <div class="col-12 pt-20 pb-10">
                            <asp:Label ID="lblMsg" runat="server" Visible="false"></asp:Label>
                        </div>
                        <div class="resume-draft-note">
                            <asp:Label ID="lblDraftNote" runat="server" />
                        </div>
                        <div class="interview-setup-card resume-builder-setup">
                    <div class="form-contact contact_form resume-builder-form">
                        <div class="alert alert-info" style="border-radius: 12px; margin-bottom: 20px;">
                            Please confirm these structured resume details carefully before applying. The application resume is saved exactly as you approve it.
                        </div>
                        <div class="row">
                            <div class="col-12">
                                <h6>Structured Resume Sections</h6>
                                <div class="resume-section-help">
                                    Use one line per item. Keep the strongest details first. Education is limited to 2 entries, experience to 5 entries, and projects to 5 entries.
                                </div>
                            </div>

                            <div class="col-12">
                                <div class="resume-section-card">
                                    <div class="form-group mb-0 resume-summary-inner">
                                        <label>Professional Summary</label>
                                        <div class="resume-section-help">Write one paragraph. Focus on role, strengths, and results. Keep it concise and readable.</div>
                                        <asp:TextBox ID="txtAppResumeSummary" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" MaxLength="500" placeholder="Example: Software engineer with strong .NET and React experience, focused on building fast, reliable, user-friendly products."></asp:TextBox>
                                    </div>
                                </div>
                            </div>

                            <div class="col-12">
                                <div class="resume-section-card" style="display:none;">
                                    <h6>Skills</h6>
                                    <asp:TextBox ID="txtAppResumeSkills" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" MaxLength="500"></asp:TextBox>
                                </div>
                            </div>

                            <div class="col-12">
                                <div class="resume-section-card" style="display:none;">
                                    <h6>Education</h6>
                                    <asp:TextBox ID="txtAppResumeEducation" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" MaxLength="500"></asp:TextBox>
                                </div>
                            </div>

                            <div class="col-12">
                                <div class="resume-section-card" style="display:none;">
                                    <h6>Experience</h6>
                                    <asp:TextBox ID="txtAppResumeExperienceDetails" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" MaxLength="1000"></asp:TextBox>
                                </div>
                            </div>

                            <div class="col-12">
                                <div class="resume-section-card" style="display:none;">
                                    <h6>Projects</h6>
                                    <asp:TextBox ID="txtAppResumeProjects" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" MaxLength="250"></asp:TextBox>
                                </div>
                            </div>

                            <div class="col-12">
                                <h6>Education</h6>
                                <div class="resume-section-help">Add up to 2 entries. Fill one card per education record. Leave the second card blank if you only have one.</div>
                            </div>

                            <div class="col-12 resume-card-slot" data-resume-slot="education-1">
                                <div class="resume-section-card">
                                    <h6>Education 1</h6>
                                    <div class="row">
                                        <div class="col-md-4"><div class="form-group"><label>School Name</label><asp:TextBox ID="txtAppEdu1SchoolName" runat="server" CssClass="form-control" MaxLength="100" placeholder="School or university" pattern="^[a-zA-Z\s]+$" title="Only alphabets allowed"></asp:TextBox></div></div>
                                        <div class="col-md-4"><div class="form-group"><label>Location</label><asp:TextBox ID="txtAppEdu1Location" runat="server" CssClass="form-control" MaxLength="50" placeholder="City, Country"></asp:TextBox></div></div>
                                        <div class="col-md-4"><div class="form-group"><label>Degree</label><asp:TextBox ID="txtAppEdu1Degree" runat="server" CssClass="form-control" MaxLength="50" placeholder="BSCS, MSCS, etc."></asp:TextBox></div></div>
                                    </div>
                                    <div class="row">
                                        <div class="col-6 col-md-2"><div class="form-group"><label>Start Month</label><asp:DropDownList ID="ddlAppEdu1StartMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                        <div class="col-6 col-md-2"><div class="form-group"><label>Start Year</label><asp:TextBox ID="txtAppEdu1StartYear" runat="server" CssClass="form-control" MaxLength="4" placeholder="2022" pattern="\d{4}" title="Enter a valid 4-digit year"></asp:TextBox></div></div>
                                        <div class="col-6 col-md-2"><div class="form-group"><label>End Month</label><asp:DropDownList ID="ddlAppEdu1EndMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                        <div class="col-6 col-md-2"><div class="form-group"><label>End Year</label><asp:TextBox ID="txtAppEdu1EndYear" runat="server" CssClass="form-control" MaxLength="4" placeholder="2025" pattern="\d{4}" title="Enter a valid 4-digit year"></asp:TextBox></div></div>
                                        <div class="col-12 col-md-4"><div class="form-group"><label>Final / Current Grade</label><asp:TextBox ID="txtAppEdu1Grade" runat="server" CssClass="form-control" MaxLength="10" placeholder="3.8 / A"></asp:TextBox></div></div>
                                    </div>
                                    <div class="row">
                                        <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Relevant Coursework / Description</label><asp:TextBox ID="txtAppEdu1Coursework" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" MaxLength="500" placeholder="Relevant coursework, honors, thesis, or notes"></asp:TextBox></div></div>
                                    </div>
                                </div>
                            </div>

                            <div class="col-12 resume-card-slot" data-resume-slot="education-2">
                                <div class="resume-section-card">
                                    <h6>Education 2</h6>
                                    <div class="row">
                                        <div class="col-md-4"><div class="form-group"><label>School Name</label><asp:TextBox ID="txtAppEdu2SchoolName" runat="server" CssClass="form-control" MaxLength="100" placeholder="School or university" pattern="^[a-zA-Z\s]+$" title="Only alphabets allowed"></asp:TextBox></div></div>
                                        <div class="col-md-4"><div class="form-group"><label>Location</label><asp:TextBox ID="txtAppEdu2Location" runat="server" CssClass="form-control" MaxLength="50" placeholder="City, Country"></asp:TextBox></div></div>
                                        <div class="col-md-4"><div class="form-group"><label>Degree</label><asp:TextBox ID="txtAppEdu2Degree" runat="server" CssClass="form-control" MaxLength="50" placeholder="BSCS, MSCS, etc."></asp:TextBox></div></div>
                                    </div>
                                    <div class="row">
                                        <div class="col-6 col-md-2"><div class="form-group"><label>Start Month</label><asp:DropDownList ID="ddlAppEdu2StartMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                        <div class="col-6 col-md-2"><div class="form-group"><label>Start Year</label><asp:TextBox ID="txtAppEdu2StartYear" runat="server" CssClass="form-control" MaxLength="4" placeholder="2022" pattern="\d{4}" title="Enter a valid 4-digit year"></asp:TextBox></div></div>
                                        <div class="col-6 col-md-2"><div class="form-group"><label>End Month</label><asp:DropDownList ID="ddlAppEdu2EndMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                        <div class="col-6 col-md-2"><div class="form-group"><label>End Year</label><asp:TextBox ID="txtAppEdu2EndYear" runat="server" CssClass="form-control" MaxLength="4" placeholder="2025" pattern="\d{4}" title="Enter a valid 4-digit year"></asp:TextBox></div></div>
                                        <div class="col-12 col-md-4"><div class="form-group"><label>Final / Current Grade</label><asp:TextBox ID="txtAppEdu2Grade" runat="server" CssClass="form-control" MaxLength="10" placeholder="3.8 / A"></asp:TextBox></div></div>
                                    </div>
                                    <div class="row">
                                        <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Relevant Coursework / Description</label><asp:TextBox ID="txtAppEdu2Coursework" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" MaxLength="500" placeholder="Relevant coursework, honors, thesis, or notes"></asp:TextBox></div></div>
                                    </div>
                                </div>
                            </div>

                            <div id="appResumeAddEducationHost" class="col-12 text-center mb-3">
                                <button id="btnAddAppEducation" type="button" class="btn button-contactForm boxed-btn resume-add-card-btn" onclick="addAppResumeCard('education')">Add Education</button>
                            </div>

                            <div class="col-12">
                                <h6>Experience</h6>
                                <div class="resume-section-help">Add up to 5 experiences. Each card supports bullets inside the description box. Start lines with - and use *bold* or _italic_ if needed.</div>
                            </div>

                            <div class="col-12 resume-card-slot" data-resume-slot="experience-1">
                                <div class="resume-section-card">
                                    <h6>Experience 1</h6>
                                    <div class="row">
                                        <div class="col-md-4"><div class="form-group"><label>Job Title</label><asp:TextBox ID="txtAppExp1JobTitle" runat="server" CssClass="form-control" MaxLength="50" placeholder="Software Engineer"></asp:TextBox></div></div>
                                        <div class="col-md-4"><div class="form-group"><label>Company</label><asp:TextBox ID="txtAppExp1Company" runat="server" CssClass="form-control" MaxLength="50" placeholder="Company name"></asp:TextBox></div></div>
                                        <div class="col-md-4"><div class="form-group"><label>Location</label><asp:TextBox ID="txtAppExp1Location" runat="server" CssClass="form-control" MaxLength="50" placeholder="City, Country"></asp:TextBox></div></div>
                                    </div>
                                    <div class="row">
                                        <div class="col-6 col-md-3"><div class="form-group"><label>Start Month</label><asp:DropDownList ID="ddlAppExp1StartMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                        <div class="col-6 col-md-3"><div class="form-group"><label>Start Year</label><asp:TextBox ID="txtAppExp1StartYear" runat="server" CssClass="form-control" MaxLength="4" placeholder="2023" pattern="\d{4}" title="Enter a valid 4-digit year"></asp:TextBox></div></div>
                                        <div class="col-6 col-md-3"><div class="form-group"><label>End Month</label><asp:DropDownList ID="ddlAppExp1EndMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                        <div class="col-6 col-md-3"><div class="form-group"><label>End Year</label><asp:TextBox ID="txtAppExp1EndYear" runat="server" CssClass="form-control" MaxLength="4" placeholder="2025" pattern="\d{4}" title="Enter a valid 4-digit year"></asp:TextBox></div></div>
                                    </div>
                                    <div class="row">
                                        <div class="col-12"><div class="form-group"><label><asp:CheckBox ID="chkAppExp1Current" runat="server" /> Currently working here</label></div></div>
                                    </div>
                                    <div class="row">
                                        <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Description</label><asp:TextBox ID="txtAppExp1Description" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" MaxLength="1000" placeholder="- Built *mobile apps* using Flutter&#10;- Improved _performance_ by 40%"></asp:TextBox><small class="text-muted d-block mt-2">Each line starting with <code>-</code> becomes one bullet. Use <code>*bold*</code> and <code>_italic_</code> formatting. Sample: <code>- Built *Mobile Apps* with React Native</code></small></div></div>
                                    </div>
                                </div>
                            </div>

                            <div class="col-12 resume-card-slot" data-resume-slot="experience-2">
                                <div class="resume-section-card">
                                    <h6>Experience 2</h6>
                                    <div class="row">
                                        <div class="col-md-4"><div class="form-group"><label>Job Title</label><asp:TextBox ID="txtAppExp2JobTitle" runat="server" CssClass="form-control" MaxLength="50" placeholder="Software Engineer"></asp:TextBox></div></div>
                                        <div class="col-md-4"><div class="form-group"><label>Company</label><asp:TextBox ID="txtAppExp2Company" runat="server" CssClass="form-control" MaxLength="50" placeholder="Company name"></asp:TextBox></div></div>
                                        <div class="col-md-4"><div class="form-group"><label>Location</label><asp:TextBox ID="txtAppExp2Location" runat="server" CssClass="form-control" MaxLength="50" placeholder="City, Country"></asp:TextBox></div></div>
                                    </div>
                                    <div class="row">
                                        <div class="col-6 col-md-3"><div class="form-group"><label>Start Month</label><asp:DropDownList ID="ddlAppExp2StartMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                        <div class="col-6 col-md-3"><div class="form-group"><label>Start Year</label><asp:TextBox ID="txtAppExp2StartYear" runat="server" CssClass="form-control" MaxLength="4" placeholder="2023" pattern="\d{4}" title="Enter a valid 4-digit year"></asp:TextBox></div></div>
                                        <div class="col-6 col-md-3"><div class="form-group"><label>End Month</label><asp:DropDownList ID="ddlAppExp2EndMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                        <div class="col-6 col-md-3"><div class="form-group"><label>End Year</label><asp:TextBox ID="txtAppExp2EndYear" runat="server" CssClass="form-control" MaxLength="4" placeholder="2025" pattern="\d{4}" title="Enter a valid 4-digit year"></asp:TextBox></div></div>
                                    </div>
                                    <div class="row">
                                        <div class="col-12"><div class="form-group"><label><asp:CheckBox ID="chkAppExp2Current" runat="server" /> Currently working here</label></div></div>
                                    </div>
                                    <div class="row">
                                        <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Description</label><asp:TextBox ID="txtAppExp2Description" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" MaxLength="1000" placeholder="- Built *mobile apps* using Flutter&#10;- Improved _performance_ by 40%"></asp:TextBox></div></div>
                                    </div>
                                </div>
                            </div>

                            <div class="col-12 resume-card-slot" data-resume-slot="experience-3">
                                <div class="resume-section-card">
                                    <h6>Experience 3</h6>
                                    <div class="row">
                                        <div class="col-md-4"><div class="form-group"><label>Job Title</label><asp:TextBox ID="txtAppExp3JobTitle" runat="server" CssClass="form-control" MaxLength="50" placeholder="Software Engineer"></asp:TextBox></div></div>
                                        <div class="col-md-4"><div class="form-group"><label>Company</label><asp:TextBox ID="txtAppExp3Company" runat="server" CssClass="form-control" MaxLength="50" placeholder="Company name"></asp:TextBox></div></div>
                                        <div class="col-md-4"><div class="form-group"><label>Location</label><asp:TextBox ID="txtAppExp3Location" runat="server" CssClass="form-control" MaxLength="50" placeholder="City, Country"></asp:TextBox></div></div>
                                    </div>
                                    <div class="row">
                                        <div class="col-6 col-md-3"><div class="form-group"><label>Start Month</label><asp:DropDownList ID="ddlAppExp3StartMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                        <div class="col-6 col-md-3"><div class="form-group"><label>Start Year</label><asp:TextBox ID="txtAppExp3StartYear" runat="server" CssClass="form-control" MaxLength="4" placeholder="2023" pattern="\d{4}" title="Enter a valid 4-digit year"></asp:TextBox></div></div>
                                        <div class="col-6 col-md-3"><div class="form-group"><label>End Month</label><asp:DropDownList ID="ddlAppExp3EndMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                        <div class="col-6 col-md-3"><div class="form-group"><label>End Year</label><asp:TextBox ID="txtAppExp3EndYear" runat="server" CssClass="form-control" MaxLength="4" placeholder="2025" pattern="\d{4}" title="Enter a valid 4-digit year"></asp:TextBox></div></div>
                                    </div>
                                    <div class="row">
                                        <div class="col-12"><div class="form-group"><label><asp:CheckBox ID="chkAppExp3Current" runat="server" /> Currently working here</label></div></div>
                                    </div>
                                    <div class="row">
                                        <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Description</label><asp:TextBox ID="txtAppExp3Description" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" MaxLength="1000" placeholder="- Built *mobile apps* using Flutter&#10;- Improved _performance_ by 40%"></asp:TextBox></div></div>
                                    </div>
                                </div>
                            </div>

                            <div class="col-12 resume-card-slot" data-resume-slot="experience-4">
                                <div class="resume-section-card">
                                    <h6>Experience 4</h6>
                                    <div class="row">
                                        <div class="col-md-4"><div class="form-group"><label>Job Title</label><asp:TextBox ID="txtAppExp4JobTitle" runat="server" CssClass="form-control" MaxLength="50" placeholder="Software Engineer"></asp:TextBox></div></div>
                                        <div class="col-md-4"><div class="form-group"><label>Company</label><asp:TextBox ID="txtAppExp4Company" runat="server" CssClass="form-control" MaxLength="50" placeholder="Company name"></asp:TextBox></div></div>
                                        <div class="col-md-4"><div class="form-group"><label>Location</label><asp:TextBox ID="txtAppExp4Location" runat="server" CssClass="form-control" MaxLength="50" placeholder="City, Country"></asp:TextBox></div></div>
                                    </div>
                                    <div class="row">
                                        <div class="col-6 col-md-3"><div class="form-group"><label>Start Month</label><asp:DropDownList ID="ddlAppExp4StartMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                        <div class="col-6 col-md-3"><div class="form-group"><label>Start Year</label><asp:TextBox ID="txtAppExp4StartYear" runat="server" CssClass="form-control" MaxLength="4" placeholder="2023" pattern="\d{4}" title="Enter a valid 4-digit year"></asp:TextBox></div></div>
                                        <div class="col-6 col-md-3"><div class="form-group"><label>End Month</label><asp:DropDownList ID="ddlAppExp4EndMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                        <div class="col-6 col-md-3"><div class="form-group"><label>End Year</label><asp:TextBox ID="txtAppExp4EndYear" runat="server" CssClass="form-control" MaxLength="4" placeholder="2025" pattern="\d{4}" title="Enter a valid 4-digit year"></asp:TextBox></div></div>
                                    </div>
                                    <div class="row">
                                        <div class="col-12"><div class="form-group"><label><asp:CheckBox ID="chkAppExp4Current" runat="server" /> Currently working here</label></div></div>
                                    </div>
                                    <div class="row">
                                        <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Description</label><asp:TextBox ID="txtAppExp4Description" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" MaxLength="1000" placeholder="- Built *mobile apps* using Flutter&#10;- Improved _performance_ by 40%"></asp:TextBox></div></div>
                                    </div>
                                </div>
                            </div>

                            <div class="col-12 resume-card-slot" data-resume-slot="experience-5">
                                <div class="resume-section-card">
                                    <h6>Experience 5</h6>
                                    <div class="row">
                                        <div class="col-md-4"><div class="form-group"><label>Job Title</label><asp:TextBox ID="txtAppExp5JobTitle" runat="server" CssClass="form-control" MaxLength="50" placeholder="Software Engineer"></asp:TextBox></div></div>
                                        <div class="col-md-4"><div class="form-group"><label>Company</label><asp:TextBox ID="txtAppExp5Company" runat="server" CssClass="form-control" MaxLength="50" placeholder="Company name"></asp:TextBox></div></div>
                                        <div class="col-md-4"><div class="form-group"><label>Location</label><asp:TextBox ID="txtAppExp5Location" runat="server" CssClass="form-control" MaxLength="50" placeholder="City, Country"></asp:TextBox></div></div>
                                    </div>
                                    <div class="row">
                                        <div class="col-6 col-md-3"><div class="form-group"><label>Start Month</label><asp:DropDownList ID="ddlAppExp5StartMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                        <div class="col-6 col-md-3"><div class="form-group"><label>Start Year</label><asp:TextBox ID="txtAppExp5StartYear" runat="server" CssClass="form-control" MaxLength="4" placeholder="2023" pattern="\d{4}" title="Enter a valid 4-digit year"></asp:TextBox></div></div>
                                        <div class="col-6 col-md-3"><div class="form-group"><label>End Month</label><asp:DropDownList ID="ddlAppExp5EndMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                        <div class="col-6 col-md-3"><div class="form-group"><label>End Year</label><asp:TextBox ID="txtAppExp5EndYear" runat="server" CssClass="form-control" MaxLength="4" placeholder="2025" pattern="\d{4}" title="Enter a valid 4-digit year"></asp:TextBox></div></div>
                                    </div>
                                    <div class="row">
                                        <div class="col-12"><div class="form-group"><label><asp:CheckBox ID="chkAppExp5Current" runat="server" /> Currently working here</label></div></div>
                                    </div>
                                    <div class="row">
                                        <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Description</label><asp:TextBox ID="txtAppExp5Description" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" MaxLength="1000" placeholder="- Built *mobile apps* using Flutter&#10;- Improved _performance_ by 40%"></asp:TextBox></div></div>
                                    </div>
                                </div>
                            </div>

                            <div id="appResumeAddExperienceHost" class="col-12 text-center mb-3">
                                <button id="btnAddAppExperience" type="button" class="btn button-contactForm boxed-btn resume-add-card-btn" onclick="addAppResumeCard('experience')">Add Experience</button>
                            </div>

                            <div class="col-12">
                                <h6>Projects</h6>
                                <div class="resume-section-help">Add up to 5 projects. Each project is one card, with one bullet-style description line and a comma-separated tech stack.</div>
                            </div>

                            <div class="col-12 resume-card-slot" data-resume-slot="project-1">
                                <div class="resume-section-card">
                                    <h6>Project 1</h6>
                                    <div class="row">
                                        <div class="col-12 resume-field-span"><div class="form-group"><label>Project Title</label><asp:TextBox ID="txtAppProj1Title" runat="server" CssClass="form-control" MaxLength="50" placeholder="Project title"></asp:TextBox></div></div>
                                        <div class="col-12 resume-field-span"><div class="form-group"><label>Tech Stack</label><asp:TextBox ID="txtAppProj1TechStack" runat="server" CssClass="form-control" MaxLength="100" placeholder="React, Node.js, MongoDB"></asp:TextBox><small class="text-muted d-block mt-2">Comma-separated values, maximum 4 items.</small></div></div>
                                        <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Description</label><asp:TextBox ID="txtAppProj1Description" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" MaxLength="250" placeholder="- Built *ATS optimization* flow for recruiter dashboard"></asp:TextBox><small class="text-muted d-block mt-2">Use one bullet-style line. Start with <code>-</code>, optionally use <code>*bold*</code> and <code>_italic_</code>.</small></div></div>
                                    </div>
                                </div>
                            </div>

                            <div class="col-12 resume-card-slot" data-resume-slot="project-2">
                                <div class="resume-section-card">
                                    <h6>Project 2</h6>
                                    <div class="row">
                                        <div class="col-12 resume-field-span"><div class="form-group"><label>Project Title</label><asp:TextBox ID="txtAppProj2Title" runat="server" CssClass="form-control" MaxLength="50" placeholder="Project title"></asp:TextBox></div></div>
                                        <div class="col-12 resume-field-span"><div class="form-group"><label>Tech Stack</label><asp:TextBox ID="txtAppProj2TechStack" runat="server" CssClass="form-control" MaxLength="100" placeholder="React, Node.js, MongoDB"></asp:TextBox></div></div>
                                        <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Description</label><asp:TextBox ID="txtAppProj2Description" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" MaxLength="250" placeholder="One bullet-style line describing the project and impact."></asp:TextBox></div></div>
                                    </div>
                                </div>
                            </div>

                            <div class="col-12 resume-card-slot" data-resume-slot="project-3">
                                <div class="resume-section-card">
                                    <h6>Project 3</h6>
                                    <div class="row">
                                        <div class="col-12 resume-field-span"><div class="form-group"><label>Project Title</label><asp:TextBox ID="txtAppProj3Title" runat="server" CssClass="form-control" MaxLength="50" placeholder="Project title"></asp:TextBox></div></div>
                                        <div class="col-12 resume-field-span"><div class="form-group"><label>Tech Stack</label><asp:TextBox ID="txtAppProj3TechStack" runat="server" CssClass="form-control" MaxLength="100" placeholder="React, Node.js, MongoDB"></asp:TextBox></div></div>
                                        <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Description</label><asp:TextBox ID="txtAppProj3Description" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" MaxLength="250" placeholder="One bullet-style line describing the project and impact."></asp:TextBox></div></div>
                                    </div>
                                </div>
                            </div>

                            <div class="col-12 resume-card-slot" data-resume-slot="project-4">
                                <div class="resume-section-card">
                                    <h6>Project 4</h6>
                                    <div class="row">
                                        <div class="col-12 resume-field-span"><div class="form-group"><label>Project Title</label><asp:TextBox ID="txtAppProj4Title" runat="server" CssClass="form-control" MaxLength="50" placeholder="Project title"></asp:TextBox></div></div>
                                        <div class="col-12 resume-field-span"><div class="form-group"><label>Tech Stack</label><asp:TextBox ID="txtAppProj4TechStack" runat="server" CssClass="form-control" MaxLength="100" placeholder="React, Node.js, MongoDB"></asp:TextBox></div></div>
                                        <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Description</label><asp:TextBox ID="txtAppProj4Description" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" MaxLength="250" placeholder="One bullet-style line describing the project and impact."></asp:TextBox></div></div>
                                    </div>
                                </div>
                            </div>

                            <div class="col-12 resume-card-slot" data-resume-slot="project-5">
                                <div class="resume-section-card">
                                    <h6>Project 5</h6>
                                    <div class="row">
                                        <div class="col-12 resume-field-span"><div class="form-group"><label>Project Title</label><asp:TextBox ID="txtAppProj5Title" runat="server" CssClass="form-control" MaxLength="50" placeholder="Project title"></asp:TextBox></div></div>
                                        <div class="col-12 resume-field-span"><div class="form-group"><label>Tech Stack</label><asp:TextBox ID="txtAppProj5TechStack" runat="server" CssClass="form-control" MaxLength="100" placeholder="React, Node.js, MongoDB"></asp:TextBox></div></div>
                                        <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Description</label><asp:TextBox ID="txtAppProj5Description" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" MaxLength="250" placeholder="One bullet-style line describing the project and impact."></asp:TextBox></div></div>
                                    </div>
                                </div>
                            </div>

                            <div id="appResumeAddProjectHost" class="col-12 text-center mb-3">
                                <button id="btnAddAppProject" type="button" class="btn button-contactForm boxed-btn resume-add-card-btn" onclick="addAppResumeCard('project')">Add Project</button>
                            </div>

                            <div class="col-12">
                                <h6>Skills</h6>
                                <div class="resume-section-help">Use comma-separated lists. The custom section is optional and the heading can be changed.</div>
                            </div>

                            <div class="col-12">
                                <div class="resume-section-card">
                                    <div class="row">
                                        <div class="col-12 resume-field-span"><div class="form-group"><label>Programming Languages</label><asp:TextBox ID="txtAppSkillProgrammingLanguages" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" MaxLength="300" placeholder="C#, JavaScript, Python"></asp:TextBox></div></div>
                                        <div class="col-12 resume-field-span"><div class="form-group"><label>Frameworks / Libraries</label><asp:TextBox ID="txtAppSkillFrameworksLibraries" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" MaxLength="300" placeholder="ASP.NET, React, Entity Framework"></asp:TextBox></div></div>
                                        <div class="col-12 resume-field-span"><div class="form-group"><label>Tools / Cloud / Database Skills</label><asp:TextBox ID="txtAppSkillToolsCloudDatabase" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" MaxLength="300" placeholder="Git, Azure, SQL Server"></asp:TextBox></div></div>
                                        <div class="col-12 resume-field-span"><div class="form-group"><label>Soft Skills / Languages</label><asp:TextBox ID="txtAppSkillSoftSkillsLanguages" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" MaxLength="300" placeholder="Communication, leadership, English"></asp:TextBox></div></div>
                                        <div class="col-12 resume-field-span"><div class="form-group"><label>User Selection Heading</label><asp:TextBox ID="txtAppSkillCustomHeading" runat="server" CssClass="form-control" MaxLength="50" placeholder="Optional heading"></asp:TextBox></div></div>
                                        <div class="col-12 resume-field-span"><div class="form-group"><label>User Selection Items</label><asp:TextBox ID="txtAppSkillCustomItems" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" MaxLength="300" placeholder="Comma-separated items"></asp:TextBox></div></div>
                                    </div>
                                </div>
                            </div>

                            <div class="col-12">
                                <div class="resume-section-card">
                                    <h6>Certifications</h6>
                                    <asp:TextBox ID="txtAppResumeCertifications" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" MaxLength="300" placeholder="Certification entries"></asp:TextBox>
                                </div>
                            </div>

                            <div class="col-12">
                                <div class="resume-section-card">
                                    <h6>Languages</h6>
                                    <asp:TextBox ID="txtAppResumeLanguages" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" MaxLength="300" placeholder="Language entries"></asp:TextBox>
                                </div>
                            </div>

                            <div class="row resume-builder-actions w-100">
                                <div class="col-md-6 text-center mb-2 mb-md-0">
                                    <asp:HyperLink ID="lnkBackToJob" runat="server" CssClass="btn button-contactForm boxed-btn app-resume-back-link" NavigateUrl="~/User/JobDetails.aspx">Back</asp:HyperLink>
                                </div>
                                <div class="col-md-6 text-center">
                                    <asp:Button ID="btnConfirm" runat="server" Text="Confirm" CssClass="button button-contactForm boxed-btn" OnClick="btnConfirm_Click" CausesValidation="false" OnClientClick="return validateAppResumeForm();" />
                                </div>
                            </div>
                        </div>
                        </div>
                    </div>
                </div>
            </div>
        </section>
    </section>
    <script type="text/javascript">
        (function () {
            var limits = { education: 2, experience: 5, project: 5 };
            var storageKey = "appResumeBuildState:" + window.location.pathname + ":" + window.location.search;
            /** Stable DOM hooks — ASP.NET prefixes ClientID so getElementById("txtAppExp1...") breaks. */
            var slotKeys = {
                education: ['education-1', 'education-2'],
                experience: ['experience-1', 'experience-2', 'experience-3', 'experience-4', 'experience-5'],
                project: ['project-1', 'project-2', 'project-3', 'project-4', 'project-5']
            };
            var addButtonIds = { education: 'btnAddAppEducation', experience: 'btnAddAppExperience', project: 'btnAddAppProject' };
            var hostIds = { education: 'appResumeAddEducationHost', experience: 'appResumeAddExperienceHost', project: 'appResumeAddProjectHost' };
            var formRoot = null;
            var RESUME_DBG = /(?:\?|&)resumeDbg=1(?:&|$)/.test(window.location.search || '');

            function dbg() {
                if (!RESUME_DBG || typeof console === 'undefined' || !console.log) return;
                console.log.apply(console, arguments);
            }

            function getFormRoot() {
                return formRoot || document.querySelector('.resume-builder-form');
            }

            function formFieldBySuffix(suffix) {
                var root = getFormRoot();
                if (!root || !suffix) return null;
                return root.querySelector('[id$="' + suffix + '"]');
            }

            var pfx = {
                eduTxt: 'txtAppEdu',
                eduDdl: 'ddlAppEdu',
                expTxt: 'txtAppExp',
                expDdl: 'ddlAppExp',
                expChk: 'chkAppExp',
                projTxt: 'txtAppProj'
            };

            function getSuffixVal(suffix) {
                var el = formFieldBySuffix(suffix);
                if (!el) return '';
                if (el.type === 'checkbox') return el.checked;
                return el.value || '';
            }

            function setSuffixVal(suffix, raw, isCheckbox) {
                var el = formFieldBySuffix(suffix);
                if (!el) return;
                if (isCheckbox) {
                    el.checked = !!raw;
                    return;
                }
                if (el.tagName === 'SELECT') {
                    if (raw === '' || raw == null) {
                        el.selectedIndex = 0;
                        return;
                    }
                    el.value = String(raw);
                    if (el.selectedIndex < 0 || el.value !== String(raw)) el.selectedIndex = 0;
                    return;
                }
                el.value = raw != null ? String(raw) : '';
            }

            function readEducationBlock(n) {
                var t = pfx.eduTxt;
                var d = pfx.eduDdl;
                return {
                    school: getSuffixVal(t + n + 'SchoolName'),
                    location: getSuffixVal(t + n + 'Location'),
                    degree: getSuffixVal(t + n + 'Degree'),
                    startMonth: getSuffixVal(d + n + 'StartMonth'),
                    startYear: getSuffixVal(t + n + 'StartYear'),
                    endMonth: getSuffixVal(d + n + 'EndMonth'),
                    endYear: getSuffixVal(t + n + 'EndYear'),
                    grade: getSuffixVal(t + n + 'Grade'),
                    coursework: getSuffixVal(t + n + 'Coursework')
                };
            }

            function writeEducationBlock(n, b) {
                var t = pfx.eduTxt;
                var d = pfx.eduDdl;
                if (!b) b = { school: '', location: '', degree: '', startMonth: '', startYear: '', endMonth: '', endYear: '', grade: '', coursework: '' };
                setSuffixVal(t + n + 'SchoolName', b.school || '');
                setSuffixVal(t + n + 'Location', b.location || '');
                setSuffixVal(t + n + 'Degree', b.degree || '');
                setSuffixVal(d + n + 'StartMonth', b.startMonth || '');
                setSuffixVal(t + n + 'StartYear', b.startYear || '');
                setSuffixVal(d + n + 'EndMonth', b.endMonth || '');
                setSuffixVal(t + n + 'EndYear', b.endYear || '');
                setSuffixVal(t + n + 'Grade', b.grade || '');
                setSuffixVal(t + n + 'Coursework', b.coursework || '');
            }

            function readExperienceBlock(n) {
                var t = pfx.expTxt;
                var d = pfx.expDdl;
                var c = pfx.expChk;
                return {
                    jobTitle: getSuffixVal(t + n + 'JobTitle'),
                    company: getSuffixVal(t + n + 'Company'),
                    location: getSuffixVal(t + n + 'Location'),
                    startMonth: getSuffixVal(d + n + 'StartMonth'),
                    startYear: getSuffixVal(t + n + 'StartYear'),
                    endMonth: getSuffixVal(d + n + 'EndMonth'),
                    endYear: getSuffixVal(t + n + 'EndYear'),
                    current: getSuffixVal(c + n + 'Current'),
                    description: getSuffixVal(t + n + 'Description')
                };
            }

            function writeExperienceBlock(n, b) {
                var t = pfx.expTxt;
                var d = pfx.expDdl;
                var c = pfx.expChk;
                if (!b) b = { jobTitle: '', company: '', location: '', startMonth: '', startYear: '', endMonth: '', endYear: '', current: false, description: '' };
                setSuffixVal(t + n + 'JobTitle', b.jobTitle || '');
                setSuffixVal(t + n + 'Company', b.company || '');
                setSuffixVal(t + n + 'Location', b.location || '');
                setSuffixVal(d + n + 'StartMonth', b.startMonth || '');
                setSuffixVal(t + n + 'StartYear', b.startYear || '');
                setSuffixVal(d + n + 'EndMonth', b.endMonth || '');
                setSuffixVal(t + n + 'EndYear', b.endYear || '');
                setSuffixVal(c + n + 'Current', b.current, true);
                setSuffixVal(t + n + 'Description', b.description || '');
            }

            function readProjectBlock(n) {
                var t = pfx.projTxt;
                return {
                    title: getSuffixVal(t + n + 'Title'),
                    techStack: getSuffixVal(t + n + 'TechStack'),
                    description: getSuffixVal(t + n + 'Description')
                };
            }

            function writeProjectBlock(n, b) {
                var t = pfx.projTxt;
                if (!b) b = { title: '', techStack: '', description: '' };
                setSuffixVal(t + n + 'Title', b.title || '');
                setSuffixVal(t + n + 'TechStack', b.techStack || '');
                setSuffixVal(t + n + 'Description', b.description || '');
            }

            function compactSectionBlocks(section, maxSlots, readFn, writeFn) {
                var blocks = [];
                var i;
                for (i = 0; i < maxSlots; i++) {
                    if (hasSlotData(section, i)) {
                        blocks.push(readFn(i + 1));
                    }
                }
                var newCount = Math.max(1, blocks.length);
                for (i = 0; i < maxSlots; i++) {
                    if (i < blocks.length) {
                        writeFn(i + 1, blocks[i]);
                    } else {
                        writeFn(i + 1, null);
                    }
                }
                return newCount;
            }

            function compactAllSections() {
                var root = getFormRoot();
                if (!root) {
                    return { education: 1, experience: 1, project: 1 };
                }
                var stateBefore = getState();
                var desired = {
                    education: Math.max(1, Math.min(limits.education, parseInt(stateBefore.counts && stateBefore.counts.education || 1, 10) || 1)),
                    experience: Math.max(1, Math.min(limits.experience, parseInt(stateBefore.counts && stateBefore.counts.experience || 1, 10) || 1)),
                    project: Math.max(1, Math.min(limits.project, parseInt(stateBefore.counts && stateBefore.counts.project || 1, 10) || 1))
                };
                var e = compactSectionBlocks('education', limits.education, readEducationBlock, writeEducationBlock);
                var x = compactSectionBlocks('experience', limits.experience, readExperienceBlock, writeExperienceBlock);
                var p = compactSectionBlocks('project', limits.project, readProjectBlock, writeProjectBlock);
                var next = {
                    education: Math.min(limits.education, Math.max(e, desired.education)),
                    experience: Math.min(limits.experience, Math.max(x, desired.experience)),
                    project: Math.min(limits.project, Math.max(p, desired.project))
                };
                dbg('[app-resume-cards] compacted', next, 'desired', desired);
                saveFormValues({ counts: next });
                return next;
            }

            function getState() {
                try {
                    return JSON.parse(sessionStorage.getItem(storageKey) || '{}');
                } catch (e) {
                    return {};
                }
            }

            function setState(state) {
                sessionStorage.setItem(storageKey, JSON.stringify(state || {}));
            }

            function saveFormValues(state) {
                state = state || {};
                state.values = {};
                var root = getFormRoot();
                if (!root) return setState(state);
                var fields = root.querySelectorAll('input, textarea, select');
                fields.forEach(function (el) {
                    if (!el.id) return;
                    if (el.type === 'password' || el.type === 'file') return;
                    if (el.type === 'checkbox') {
                        state.values[el.id] = !!el.checked;
                    } else {
                        state.values[el.id] = el.value;
                    }
                });
                setState(state);
            }

            function restoreFormValues(state) {
                if (!state || !state.values) return;
                Object.keys(state.values).forEach(function (id) {
                    var el = document.getElementById(id);
                    if (!el) return;
                    if (el.type === 'checkbox') {
                        el.checked = !!state.values[id];
                    } else {
                        el.value = state.values[id];
                    }
                });
            }

            function cardSlot(section, indexZero) {
                var root = getFormRoot();
                if (!root) return null;
                var keys = slotKeys[section];
                if (!keys || indexZero < 0 || indexZero >= keys.length) return null;
                return root.querySelector('.resume-card-slot[data-resume-slot="' + keys[indexZero] + '"]');
            }

            function hasSlotData(section, indexZero) {
                var slot = cardSlot(section, indexZero);
                if (!slot) return false;
                var fields = slot.querySelectorAll('input, textarea, select');
                for (var i = 0; i < fields.length; i++) {
                    var f = fields[i];
                    if (f.type === 'checkbox' && f.checked) return true;
                    if (f.type !== 'checkbox' && (f.value || '').trim() !== '') return true;
                }
                return false;
            }

            function applyCounts(counts) {
                ['education', 'experience', 'project'].forEach(function (section) {
                    var keys = slotKeys[section];
                    var count = Math.max(1, Math.min(limits[section], parseInt(counts[section] || 1, 10)));
                    for (var i = 0; i < keys.length; i++) {
                        var slotEl = cardSlot(section, i);
                        if (!slotEl) continue;
                        slotEl.style.display = i < count ? '' : 'none';
                        dbg('[app-resume-cards]', section, keys[i], 'display', i < count ? 'show' : 'hide', '(count=' + count + ')');
                    }
                    var host = document.getElementById(hostIds[section]);
                    if (host) host.style.display = count >= limits[section] ? 'none' : '';
                });
            }

            function repositionAddHosts(counts) {
                ['education', 'experience', 'project'].forEach(function (section) {
                    var host = document.getElementById(hostIds[section]);
                    if (!host) return;
                    var keys = slotKeys[section];
                    var count = Math.max(1, Math.min(limits[section], parseInt(counts[section] || 1, 10)));
                    if (count >= limits[section]) return;
                    var slotEl = cardSlot(section, count - 1);
                    var parent = slotEl ? slotEl.parentNode : null;
                    if (!parent) return;
                    if (host.parentNode !== parent || slotEl.nextSibling !== host) {
                        parent.insertBefore(host, slotEl.nextSibling);
                        dbg('[app-resume-cards] reposition host', section, 'after', keys[count - 1]);
                    }
                });
            }

            function splitCsv(value) {
                return (value || '')
                    .split(',')
                    .map(function (item) { return item.trim(); })
                    .filter(function (item) { return item.length > 0; });
            }

            function assertLength(idSuffix, max, label) {
                var el = formFieldBySuffix(idSuffix);
                if (!el) return true;
                if ((el.value || '').trim().length > max) {
                    alert(label + ' must be at most ' + max + ' characters.');
                    el.focus();
                    return false;
                }
                return true;
            }

            function assertPattern(idSuffix, regex, msg) {
                var el = formFieldBySuffix(idSuffix);
                if (!el) return true;
                var value = (el.value || '').trim();
                if (!value) return true;
                if (!regex.test(value)) {
                    alert(msg);
                    el.focus();
                    return false;
                }
                return true;
            }

            function validateSkillsList(idSuffix, label) {
                var el = formFieldBySuffix(idSuffix);
                if (!el) return true;
                var values = splitCsv(el.value);
                if (values.length > 5) {
                    alert(label + ' allows maximum 5 comma-separated items.');
                    el.focus();
                    return false;
                }
                return true;
            }

            function validateProjectTechStack(idSuffix) {
                var el = formFieldBySuffix(idSuffix);
                if (!el) return true;
                var values = splitCsv(el.value);
                if (values.length > 4) {
                    alert('Tech Stack allows maximum 4 comma-separated items.');
                    el.focus();
                    return false;
                }
                return true;
            }

            window.validateAppResumeForm = function () {
                counts = compactAllSections();
                applyCounts(counts);
                repositionAddHosts(counts);
                for (var i = 1; i <= 2; i++) {
                    if (!assertPattern('txtAppEdu' + i + 'SchoolName', /^[A-Za-z\s]{0,100}$/, 'School Name must be alphabets only (max 100).')) return false;
                    if (!assertLength('txtAppEdu' + i + 'Location', 50, 'Education Location')) return false;
                    if (!assertLength('txtAppEdu' + i + 'Degree', 50, 'Education Degree')) return false;
                    if (!assertPattern('txtAppEdu' + i + 'StartYear', /^\s*$|^(19|20)\d{2}$/, 'Start Year must be a valid 4-digit year like 2026.')) return false;
                    if (!assertPattern('txtAppEdu' + i + 'EndYear', /^\s*$|^(19|20)\d{2}$/, 'End Year must be a valid 4-digit year like 2026.')) return false;
                    if (!assertLength('txtAppEdu' + i + 'Grade', 10, 'Final / Current Grade')) return false;
                    if (!assertLength('txtAppEdu' + i + 'Coursework', 500, 'Relevant Coursework / Description')) return false;
                }

                for (var j = 1; j <= 5; j++) {
                    if (!assertLength('txtAppExp' + j + 'JobTitle', 50, 'Experience Job Title')) return false;
                    if (!assertLength('txtAppExp' + j + 'Company', 50, 'Experience Company')) return false;
                    if (!assertLength('txtAppExp' + j + 'Location', 50, 'Experience Location')) return false;
                    if (!assertPattern('txtAppExp' + j + 'StartYear', /^\s*$|^(19|20)\d{2}$/, 'Experience Start Year must be a valid 4-digit year like 2026.')) return false;
                    if (!assertPattern('txtAppExp' + j + 'EndYear', /^\s*$|^(19|20)\d{2}$/, 'Experience End Year must be a valid 4-digit year like 2026.')) return false;
                    if (!assertLength('txtAppExp' + j + 'Description', 1000, 'Experience Description')) return false;
                }

                for (var k = 1; k <= 5; k++) {
                    if (!assertLength('txtAppProj' + k + 'Title', 50, 'Project Title')) return false;
                    if (!validateProjectTechStack('txtAppProj' + k + 'TechStack')) return false;
                    if (!assertLength('txtAppProj' + k + 'Description', 250, 'Project Description')) return false;
                }

                if (!validateSkillsList('txtAppSkillProgrammingLanguages', 'Programming Languages')) return false;
                if (!validateSkillsList('txtAppSkillFrameworksLibraries', 'Frameworks / Libraries')) return false;
                if (!validateSkillsList('txtAppSkillToolsCloudDatabase', 'Tools / Cloud / Database Skills')) return false;
                if (!validateSkillsList('txtAppSkillSoftSkillsLanguages', 'Soft Skills / Languages')) return false;
                if (!validateSkillsList('txtAppSkillCustomItems', 'User Selection Items')) return false;

                var state = getState();
                saveFormValues(state);
                return true;
            };

            window.addAppResumeCard = function (section) {
                var state = getState();
                state.counts = state.counts || counts;
                if ((state.counts[section] || 1) >= limits[section]) {
                    alert('You can add just ' + limits[section] + ' ' + section + ' cards.');
                    return;
                }

                state.counts[section] = (state.counts[section] || 1) + 1;
                saveFormValues(state);
                location.replace(window.location.href);
            };

            function hydrateFromStorage() {
                formRoot = document.querySelector('.resume-builder-form');
                dbg('[app-resume-cards] hydrate, formRoot=', !!formRoot, 'storageKey=', storageKey);
                var initialState = getState();
                dbg('[app-resume-cards] session counts', initialState.counts);
                restoreFormValues(initialState);
                var nextCounts = compactAllSections();
                dbg('[app-resume-cards] counts after compact', nextCounts);
                applyCounts(nextCounts);
                repositionAddHosts(nextCounts);
                return nextCounts;
            }

            var counts = hydrateFromStorage();

            window.addEventListener('pageshow', function (ev) {
                if (ev.persisted) {
                    window.location.reload();
                }
            });

            document.querySelectorAll('input, textarea, select').forEach(function (el) {
                function persistDraft() {
                    var state = getState();
                    state.counts = state.counts || counts;
                    saveFormValues(state);
                }
                el.addEventListener('change', persistDraft);
                el.addEventListener('input', persistDraft);
            });

            window.addEventListener('pagehide', function () {
                formRoot = document.querySelector('.resume-builder-form');
                if (!formRoot) return;
                compactAllSections();
            });
        })();
    </script>
</asp:Content>
