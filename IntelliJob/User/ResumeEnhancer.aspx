<%@ Page Title="Resume Enhancer" Language="C#" MasterPageFile="~/User/UserMaster.Master" AutoEventWireup="true" CodeBehind="ResumeEnhancer.aspx.cs" Inherits="IntelliJob.User.ResumeEnhancer" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        /* Edit mode: hide everything except the editor section-card */
        #resumeReportBody.enhancer-editing > *:not(.editor-section) { display: none !important; }
        #resumeReportBody.enhancer-editing > .editor-section { display: block !important; }
        #resumeReportBody.enhancer-previewing .resume-empty-preview { display: none !important; }

        .html-preview-frame {
            width: 100%;
            min-height: 980px;
            border: 1px solid #e5e7eb;
            border-radius: 16px;
            background: #f8fafc;
        }

        .enhancer-shell {
            padding: 45px 0 80px;
            background: linear-gradient(180deg, #fff7f8 0%, #ffffff 100%);
        }

        .enhancer-preview-form textarea {
            resize: none;
            overflow: hidden;
            min-height: 40px;
        }

        .page-header-section {
            background: linear-gradient(100deg, #da2461 10%, #011B43  90%);
            padding: 54px 0 28px;
            min-height:450px;
        }

        .page-header-section .container {
            padding: 50px 32px 0 32px;
        }

        .page-header-section h2 {
            font-size: 40px;
            font-weight: 700;
            text-transform: capitalize;
            font-family: "Muli", sans-serif;
            color: #ffffff;
            margin-bottom: 10px;
        }

        .page-header-section p {
            color: #ffffff;
            font-size: 20px;
            max-width: 600px;
        }

        .btn-header-action {
            display: inline-flex;
            align-items: center;
            gap: 8px;
            padding: 12px 20px;
            background: rgba(255, 255, 255, 0.1);
            border: 1px solid rgba(255, 255, 255, 0.3);
            border-radius: 999px;
            color: #fff !important;
            font-weight: 600;
            font-size: 14px;
            transition: all 0.2s ease;
            white-space: nowrap;
        }
        .btn-header-action:hover {
            background-color: #da2461;
            border-color: #da2461;
            color: #fff !important;
            text-decoration: none;
            transform: translateY(-1px);
        }

        .hero-action-btn.secondary-action {
            background: rgba(255, 255, 255, 0.18);
        }

        .hero-action-btn.secondary-action:hover {
            background: rgba(255, 255, 255, 0.28);
        }

        .hero-chips {
            display: flex;
            gap: 10px;
            flex-wrap: wrap;
            margin-top: 18px;
        }

        .hero-chip {
            background: rgba(255, 255, 255, 0.12);
            border: 1px solid rgba(255, 255, 255, 0.18);
            padding: 8px 14px;
            border-radius: 999px;
            font-size: 13px;
            font-weight: 600;
            color: #fff;
        }

        .score-grid {
            display: grid;
            grid-template-columns: repeat(4, minmax(0, 1fr));
            gap: 16px;
            margin-bottom: 22px;
        }

        .metric-card {
            background: #fff;
            border-radius: 18px;
            border: 1px solid #ececec;
            box-shadow: 0 8px 26px rgba(15, 23, 42, 0.06);
            padding: 20px;
        }

        .metric-card .label {
            color: #6b7280;
            font-size: 13px;
            text-transform: uppercase;
            letter-spacing: 0.08em;
            margin-bottom: 10px;
            font-weight: 700;
        }

        .metric-card .value {
            font-size: 30px;
            font-weight: 800;
            color: #111827;
            line-height: 1;
        }

        .metric-card .hint {
            color: #6b7280;
            font-size: 13px;
            margin-top: 8px;
        }

        .content-grid {
            display: grid;
            grid-template-columns: minmax(0, 1.05fr) minmax(0, 0.95fr);
            gap: 22px;
        }

        .section-card {
            background: #fff;
            border-radius: 20px;
            border: 1px solid #ececec;
            box-shadow: 0 8px 26px rgba(15, 23, 42, 0.06);
            padding: 24px;
            margin-bottom: 18px;
        }

        .section-card h3 {
            font-size: 20px;
            font-weight: 800;
            color: #111827;
            margin-bottom: 14px;
        }

        .section-card p,
        .section-card li {
            color: #4b5563;
            font-size: 14px;
            line-height: 1.7;
        }

        .pill-list,
        .bullet-list {
            list-style: none;
            padding: 0;
            margin: 0;
        }

        .pill-list li,
        .bullet-list li {
            margin-bottom: 10px;
            padding: 12px 14px;
            border-radius: 14px;
            background: #f9fafb;
            border: 1px solid #eef2f7;
        }

        .pill-list {
            display: flex;
            flex-wrap: wrap;
            gap: 8px;
        }

        .pill-list li {
            display: inline-flex;
            align-items: center;
            padding: 8px 12px;
            margin-bottom: 0;
            border-radius: 999px;
            white-space: nowrap;
        }

        .bullet-list li {
            padding-left: 18px;
            position: relative;
        }

        .bullet-list li:before {
            content: "";
            width: 8px;
            height: 8px;
            border-radius: 50%;
            background: #FF4357;
            position: absolute;
            left: 0;
            top: 18px;
        }

        .rewrite-card {
            background: linear-gradient(180deg, #ffffff 0%, #fff8f9 100%);
            border: 1px solid #ffd6db;
            border-radius: 18px;
            padding: 18px;
            margin-bottom: 14px;
        }

        .rewrite-card h4 {
            font-size: 15px;
            font-weight: 800;
            color: #111827;
            margin-bottom: 10px;
        }

        .rewrite-card .current,
        .rewrite-card .suggested {
            font-size: 14px;
            line-height: 1.7;
            color: #4b5563;
            margin-bottom: 10px;
        }

        .rewrite-card .current span,
        .rewrite-card .suggested span {
            display: block;
            font-size: 12px;
            font-weight: 800;
            text-transform: uppercase;
            letter-spacing: 0.08em;
            margin-bottom: 4px;
        }

        .resume-preview {
            background: #0f172a;
            color: #e5e7eb;
            border-radius: 18px;
            padding: 20px;
            max-height: 520px;
            overflow: auto;
            white-space: pre-wrap;
            font-size: 13px;
            line-height: 1.75;
        }

        /* View resume-preview header button */
        .btn-resume-preview {
            display: inline-flex;
            align-items: center;
            gap: 8px;
            background-color: #fb246a;
            border: 2px solid #fb246a;
            color: #ffffff !important;
            border-radius: 12px;
            padding: 12px 28px;
            font-weight: 600;
            font-size: 15px;
            text-decoration: none;
            transition: all 0.2s ease;
            white-space: nowrap;
        }
        .btn-resume-preview:hover {
            background-color: #da2461;
            border-color: #da2461;
            color: #fff !important;
            text-decoration: none;
            transform: translateY(-1px);
        }

        .enhancer-preview-form textarea.form-control,
        .enhancer-preview-form input.form-control {
            border-radius: 12px;
            border: 1px solid #dee2e6;
            padding: 12px 15px;
            font-size: 15px;
            background: #fff;
            transition: border-color 0.3s ease, box-shadow 0.3s ease;
        }

        .enhancer-preview-form textarea.form-control:focus,
        .enhancer-preview-form input.form-control:focus {
            border-color: #ff4357;
            box-shadow: 0 0 0 0.2rem rgba(255, 67, 87, 0.15);
        }

        .enhancer-preview-form textarea.form-control {
            min-height: 110px;
            resize: vertical;
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

        .resume-builder-form select.form-control {
            padding: 14px 18px;
            font-size: 16px;
            font-weight: 500;
            min-height: 50px;
        }

        .enhancer-preview-actions {
            display: flex;
            gap: 12px;
            flex-wrap: wrap;
            align-items: center;
            justify-content: space-between;
            margin-top: 16px;
        }

        .enhancer-save-panel {
            margin-top: 18px;
            padding: 18px;
            border-radius: 18px;
            border: 1px solid #e9eef5;
            background: #fbfcfe;
        }

        .enhancer-save-panel .radio-list-inline {
            display: flex;
            gap: 18px;
            flex-wrap: wrap;
            margin: 10px 0 14px;
        }

        .enhancer-save-panel .radio-list-inline label {
            margin-left: 6px;
            font-weight: 600;
            color: #2d3436;
        }

        .status-note {
            background: #fff8e6;
            border: 1px solid #fde2a5;
            color: #8a5d00;
            border-radius: 16px;
            padding: 14px 16px;
            font-size: 14px;
            line-height: 1.6;
            margin-bottom: 18px;
            margin-top: 18px;
        }

        .muted-box {
            background: #f8fafc;
            border: 1px dashed #dbe3ec;
            color: #5b6472;
            border-radius: 16px;
            padding: 14px 16px;
            font-size: 14px;
            line-height: 1.7;
        }

        .report-body {
            display: none;
        }

        .visual-score-circle {
            position: relative;
            width: 90px;
            height: 90px;
            margin: 0 auto 10px;
        }

        .visual-score-circle svg {
            transform: rotate(-90deg);
            width: 100%;
            height: 100%;
        }

        .visual-score-circle .circle-bg {
            fill: none;
            stroke: #e5e7eb;
            stroke-width: 8;
        }

        .visual-score-circle .circle-progress {
            fill: none;
            stroke: url(#grad);
            stroke-width: 8;
            stroke-linecap: round;
            transition: stroke-dashoffset 1s ease-in-out;
        }

        .visual-score-circle .score-text {
            position: absolute;
            inset: 0;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 22px;
            font-weight: 800;
            color: #111827;
        }

        .metric-card.overall-card {
            background: linear-gradient(180deg, #fff7f8 0%, #ffffff 100%);
            border: 1px solid #ffd6db;
        }


        @media (max-width: 992px) {
            .score-grid,
            .content-grid {
                grid-template-columns: 1fr 1fr;
            }
        }

        @media (max-width: 768px) {
            .score-grid,
            .content-grid {
                grid-template-columns: 1fr;
            }

            .enhancer-hero {
                padding: 22px;
            }
        }
    </style>
    <script type="text/javascript">
        function exportHTMLAsPDF() {
            var payload = document.getElementById('<%= hfResumePreviewJson.ClientID %>');
            if (!payload || !payload.value) {
                alert('No enhanced resume JSON is available for the HTML preview yet.');
                return false;
            }

            var previewUrl = '<%= ResolveUrl("~/ResumePreview.html") %>';
            var encoded = encodeURIComponent(payload.value);
            var previewWindow = window.open(previewUrl + '#data=' + encoded, '_blank', 'noopener');
            if (!previewWindow) {
                alert('The preview window was blocked by the browser. Please allow popups for this site.');
                return false;
            }

            previewWindow.focus();
            return false;
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <main>
        <!-- Page Header -->
        <div class="page-header-section">
            <div class="container">
                <div class="row">
                    <div class="col-lg-8">
                        <h2>Resume Enhancer</h2>
                        <p>A job-specific review built from the uploaded resume, the target role, the job description, and the interview feedback already collected inside IntelliJob.</p>
                        <div class="hero-chips" style="margin-top: 15px;">
                            <span class="hero-chip"><asp:Literal ID="litRole" runat="server" /></span>
                            <span class="hero-chip"><asp:Literal ID="litCompany" runat="server" /></span>
                            <span class="hero-chip"><asp:Literal ID="litLevel" runat="server" /></span>
                            <span class="hero-chip"><asp:Literal ID="litInterviewType" runat="server" /></span>
                            <span class="hero-chip"><i class="fas fa-file-alt" style="margin-right:4px;"></i><asp:Literal ID="litResumeSource" runat="server" /></span>
                        </div>
                    </div>
<%--                    <div class="col-lg-4 text-right pt-3 d-flex flex-column align-items-end justify-content-center gap-2" style="display: flex; flex-direction: column; align-items: flex-end; gap: 10px;">
                        <a href="#" class="hero-action-btn secondary-action" onclick="return exportHTMLAsPDF();">
                            <i class="fas fa-file-code"></i> HTML Preview
                        </a>
                    </div>--%>
                </div>
            </div>
        </div>

        <div class="enhancer-shell" style="padding-top: 0;">
            <div class="container">
                <div class="row justify-content-center">
                    <div class="col-xl-8 col-lg-10">

                <asp:Literal ID="litStatus" runat="server" />
                <asp:HiddenField ID="hfLoadedAppliedJobId" runat="server" />
                <asp:HiddenField ID="hfLoadedInterviewId" runat="server" />
                <asp:HiddenField ID="hfLoadedResumePath" runat="server" />
                <asp:HiddenField ID="hfLoadedResumeSource" runat="server" />
                <asp:HiddenField ID="hfLoadedOriginalFileName" runat="server" />
                <asp:HiddenField ID="hfResumePreviewJson" runat="server" />

                <div id="resumeReportBody" class="report-body">
                    <div class="score-grid">
                        <div class="metric-card overall-card" style="grid-column: 1 / -1; text-align: center;">
                            <div class="label" style="font-size: 16px;">Overall Match</div>
                            <%--<div class="value" style="font-size: 42px; color: #FF4357;"><asp:Literal ID="litOverallScore" runat="server" />/100</div>--%>
                            <div style="margin-top: 15px; display: flex; justify-content: center;">
                                <asp:Literal ID="litOverallVisual" runat="server" />
                            </div>
                            <div class="hint" style="font-size: 14px; margin-top:0;">How strongly the resume fits the job</div>
                        </div>
                        <div class="metric-card" style="text-align: center;">
                            <div class="label">ATS Fit</div>
                            <%--<div class="value"><asp:Literal ID="litAtsScore" runat="server" />/100</div>--%>
                            <div style="margin-top: 15px;">
                                <asp:Literal ID="litAtsVisual" runat="server" />
                            </div>
                            <div class="hint">Formatting & parsing</div>
                        </div>
                        <div class="metric-card" style="text-align: center;">
                            <div class="label">Semantic Fit</div>
                            <%--<div class="value"><asp:Literal ID="litSemanticScore" runat="server" />/100</div>--%>
                            <div style="margin-top: 15px;">
                                <asp:Literal ID="litSemanticVisual" runat="server" />
                            </div>
                            <div class="hint">Meaning & context match</div>
                        </div>
                        <div class="metric-card" style="text-align: center;">
                            <div class="label">Keyword Fit</div>
                            <%--<div class="value"><asp:Literal ID="litKeywordScore" runat="server" />/100</div>--%>
                            <div style="margin-top: 15px;">
                                <asp:Literal ID="litKeywordVisual" runat="server" />
                            </div>
                            <div class="hint">Soft keyword alignment</div>
                        </div>
                    </div>

                    <div class="section-card">
                        <h3>Resume Summary</h3>
                        <p><asp:Literal ID="litResumeSummary" runat="server" /></p>
                    </div>

                    <div class="section-card">
                        <h3>Strengths</h3>
                        <ul class="bullet-list">
                            <asp:Literal ID="litStrengths" runat="server" />
                        </ul>
                    </div>

                    <div class="section-card">
                        <h3>Gaps To Improve</h3>
                        <ul class="bullet-list">
                            <asp:Literal ID="litGaps" runat="server" />
                        </ul>
                    </div>

                    <div class="section-card">
                        <h3>Suggested Rewrites</h3>
                        <asp:Literal ID="litRewriteSuggestions" runat="server" />
                    </div>

                    <div class="section-card">
                        <h3>Priority Keywords</h3>
                        <ul class="pill-list">
                            <asp:Literal ID="litPriorityKeywords" runat="server" />
                        </ul>
                    </div>

                    <div class="section-card">
                        <h3>Final Assessment</h3>
                        <p><asp:Literal ID="litFinalAssessment" runat="server" /></p>
                    </div>

                    <div class="section-card editor-section" style="display:none;">
                        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px; flex-wrap: wrap; gap: 10px;">
                            <h3 style="margin: 0;">Enhanced Resume Editing</h3>
                            <div class="enhancer-preview-actions" style="margin: 0;">
                                <asp:Button ID="btnToggleEnhPreviewEdit" runat="server" CssClass="btn-resume-preview" Text="Edit Preview" OnClick="btnToggleEnhPreviewEdit_Click" CausesValidation="false" />
<%--                                <asp:Button ID="btnExportResumePdf" runat="server" CssClass="btn-resume-preview" Text="Export PDF" OnClick="btnExportResumePdf_Click" CausesValidation="false" style="margin-left:8px;" />--%>
<%--                                <asp:Button ID="btnDeleteEnhancementHistory" runat="server" CssClass="btn-resume-preview" Text="Reset Enhancement" OnClick="btnDeleteEnhancementHistory_Click" CausesValidation="false" OnClientClick="return confirm('This will delete the saved AI enhancement so it can be regenerated. Continue?');" style="margin-left:8px; background:#6b7280; border-color:#6b7280;" />--%>
                            </div>
                        </div>
                        <div class="muted-box" style="margin-bottom:14px;">
                            Click <strong>Edit Preview</strong> to edit the resume fields below, then click <strong>Update</strong> to save changes to your applied resume.
                        </div>
                                                <div class="enhancer-preview-form resume-builder-form">
                            <div class="col-12"><h6 style="margin-top:8px;">Personal Information</h6></div>
                            <div class="row">
                                <div class="col-md-6"><div class="form-group"><label>Full Name</label><asp:TextBox ID="txtEnhFullName" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox></div></div>
                                <div class="col-md-6"><div class="form-group"><label>Email</label><asp:TextBox ID="txtEnhEmail" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox></div></div>
                                <div class="col-md-6"><div class="form-group"><label>Mobile</label><asp:TextBox ID="txtEnhMobile" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox></div></div>
                                <div class="col-md-6"><div class="form-group"><label>Country</label><asp:TextBox ID="txtEnhCountry" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox></div></div>
                                <div class="col-12"><div class="form-group"><label>Address</label><asp:TextBox ID="txtEnhAddress" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" ReadOnly="true"></asp:TextBox></div></div>
                                <div class="col-md-6"><div class="form-group"><label>LinkedIn URL</label><asp:TextBox ID="txtEnhLinkedIn" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox></div></div>
                                <div class="col-md-6"><div class="form-group"><label>Portfolio URL</label><asp:TextBox ID="txtEnhPortfolio" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox></div></div>
                            </div>
                            <div class="col-12"><div class="form-group"><label>Professional Summary</label><asp:TextBox ID="txtEnhSummary" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" ReadOnly="true"></asp:TextBox></div></div>
                            <div class="col-12"><h6>Education</h6><div class="resume-section-help">Up to two entries.</div></div>
                        <div class="col-12 resume-card-slot" data-resume-slot="education-1">
                            <div class="resume-section-card">
                                <h6>Education 1</h6>
                                <div class="row">
                                    <div class="col-md-4"><div class="form-group"><label>School Name</label><asp:TextBox ID="txtEnhEdu1SchoolName" runat="server" CssClass="form-control" MaxLength="100" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-md-4"><div class="form-group"><label>Location</label><asp:TextBox ID="txtEnhEdu1Location" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-md-4"><div class="form-group"><label>Degree</label><asp:TextBox ID="txtEnhEdu1Degree" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                                <div class="row">
                                    <div class="col-6 col-md-2"><div class="form-group"><label>Start Month</label><asp:DropDownList ID="ddlEnhEdu1StartMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                    <div class="col-6 col-md-2"><div class="form-group"><label>Start Year</label><asp:TextBox ID="txtEnhEdu1StartYear" runat="server" CssClass="form-control" MaxLength="4" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-6 col-md-2"><div class="form-group"><label>End Month</label><asp:DropDownList ID="ddlEnhEdu1EndMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                    <div class="col-6 col-md-2"><div class="form-group"><label>End Year</label><asp:TextBox ID="txtEnhEdu1EndYear" runat="server" CssClass="form-control" MaxLength="4" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-12 col-md-4"><div class="form-group"><label>Grade</label><asp:TextBox ID="txtEnhEdu1Grade" runat="server" CssClass="form-control" MaxLength="10" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                                <div class="row">
                                    <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Coursework</label><asp:TextBox ID="txtEnhEdu1Coursework" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                            </div>
                        </div>
                        <div class="col-12 resume-card-slot" data-resume-slot="education-2">
                            <div class="resume-section-card">
                                <h6>Education 2</h6>
                                <div class="row">
                                    <div class="col-md-4"><div class="form-group"><label>School Name</label><asp:TextBox ID="txtEnhEdu2SchoolName" runat="server" CssClass="form-control" MaxLength="100" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-md-4"><div class="form-group"><label>Location</label><asp:TextBox ID="txtEnhEdu2Location" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-md-4"><div class="form-group"><label>Degree</label><asp:TextBox ID="txtEnhEdu2Degree" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                                <div class="row">
                                    <div class="col-6 col-md-2"><div class="form-group"><label>Start Month</label><asp:DropDownList ID="ddlEnhEdu2StartMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                    <div class="col-6 col-md-2"><div class="form-group"><label>Start Year</label><asp:TextBox ID="txtEnhEdu2StartYear" runat="server" CssClass="form-control" MaxLength="4" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-6 col-md-2"><div class="form-group"><label>End Month</label><asp:DropDownList ID="ddlEnhEdu2EndMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                    <div class="col-6 col-md-2"><div class="form-group"><label>End Year</label><asp:TextBox ID="txtEnhEdu2EndYear" runat="server" CssClass="form-control" MaxLength="4" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-12 col-md-4"><div class="form-group"><label>Grade</label><asp:TextBox ID="txtEnhEdu2Grade" runat="server" CssClass="form-control" MaxLength="10" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                                <div class="row">
                                    <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Coursework</label><asp:TextBox ID="txtEnhEdu2Coursework" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                            </div>
                        </div>
                            <div class="col-12"><h6>Experience</h6><div class="resume-section-help">Up to five roles.</div></div>
                        <div class="col-12 resume-card-slot" data-resume-slot="experience-1">
                            <div class="resume-section-card">
                                <h6>Experience 1</h6>
                                <div class="row">
                                    <div class="col-md-4"><div class="form-group"><label>Job Title</label><asp:TextBox ID="txtEnhExp1JobTitle" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-md-4"><div class="form-group"><label>Company</label><asp:TextBox ID="txtEnhExp1Company" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-md-4"><div class="form-group"><label>Location</label><asp:TextBox ID="txtEnhExp1Location" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                                <div class="row">
                                    <div class="col-6 col-md-3"><div class="form-group"><label>Start Month</label><asp:DropDownList ID="ddlEnhExp1StartMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                    <div class="col-6 col-md-3"><div class="form-group"><label>Start Year</label><asp:TextBox ID="txtEnhExp1StartYear" runat="server" CssClass="form-control" MaxLength="4" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-6 col-md-3"><div class="form-group"><label>End Month</label><asp:DropDownList ID="ddlEnhExp1EndMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                    <div class="col-6 col-md-3"><div class="form-group"><label>End Year</label><asp:TextBox ID="txtEnhExp1EndYear" runat="server" CssClass="form-control" MaxLength="4" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                                <div class="row">
                                    <div class="col-12"><div class="form-group"><label><asp:CheckBox ID="chkEnhExp1Current" runat="server" Enabled="false" /> Currently working here</label></div></div>
                                </div>
                                <div class="row">
                                    <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Bullets (one per line)</label><asp:TextBox ID="txtEnhExp1Description" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                            </div>
                        </div>
                        <div class="col-12 resume-card-slot" data-resume-slot="experience-2">
                            <div class="resume-section-card">
                                <h6>Experience 2</h6>
                                <div class="row">
                                    <div class="col-md-4"><div class="form-group"><label>Job Title</label><asp:TextBox ID="txtEnhExp2JobTitle" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-md-4"><div class="form-group"><label>Company</label><asp:TextBox ID="txtEnhExp2Company" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-md-4"><div class="form-group"><label>Location</label><asp:TextBox ID="txtEnhExp2Location" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                                <div class="row">
                                    <div class="col-6 col-md-3"><div class="form-group"><label>Start Month</label><asp:DropDownList ID="ddlEnhExp2StartMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                    <div class="col-6 col-md-3"><div class="form-group"><label>Start Year</label><asp:TextBox ID="txtEnhExp2StartYear" runat="server" CssClass="form-control" MaxLength="4" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-6 col-md-3"><div class="form-group"><label>End Month</label><asp:DropDownList ID="ddlEnhExp2EndMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                    <div class="col-6 col-md-3"><div class="form-group"><label>End Year</label><asp:TextBox ID="txtEnhExp2EndYear" runat="server" CssClass="form-control" MaxLength="4" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                                <div class="row">
                                    <div class="col-12"><div class="form-group"><label><asp:CheckBox ID="chkEnhExp2Current" runat="server" Enabled="false" /> Currently working here</label></div></div>
                                </div>
                                <div class="row">
                                    <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Bullets (one per line)</label><asp:TextBox ID="txtEnhExp2Description" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                            </div>
                        </div>
                        <div class="col-12 resume-card-slot" data-resume-slot="experience-3">
                            <div class="resume-section-card">
                                <h6>Experience 3</h6>
                                <div class="row">
                                    <div class="col-md-4"><div class="form-group"><label>Job Title</label><asp:TextBox ID="txtEnhExp3JobTitle" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-md-4"><div class="form-group"><label>Company</label><asp:TextBox ID="txtEnhExp3Company" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-md-4"><div class="form-group"><label>Location</label><asp:TextBox ID="txtEnhExp3Location" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                                <div class="row">
                                    <div class="col-6 col-md-3"><div class="form-group"><label>Start Month</label><asp:DropDownList ID="ddlEnhExp3StartMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                    <div class="col-6 col-md-3"><div class="form-group"><label>Start Year</label><asp:TextBox ID="txtEnhExp3StartYear" runat="server" CssClass="form-control" MaxLength="4" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-6 col-md-3"><div class="form-group"><label>End Month</label><asp:DropDownList ID="ddlEnhExp3EndMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                    <div class="col-6 col-md-3"><div class="form-group"><label>End Year</label><asp:TextBox ID="txtEnhExp3EndYear" runat="server" CssClass="form-control" MaxLength="4" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                                <div class="row">
                                    <div class="col-12"><div class="form-group"><label><asp:CheckBox ID="chkEnhExp3Current" runat="server" Enabled="false" /> Currently working here</label></div></div>
                                </div>
                                <div class="row">
                                    <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Bullets (one per line)</label><asp:TextBox ID="txtEnhExp3Description" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                            </div>
                        </div>
                        <div class="col-12 resume-card-slot" data-resume-slot="experience-4">
                            <div class="resume-section-card">
                                <h6>Experience 4</h6>
                                <div class="row">
                                    <div class="col-md-4"><div class="form-group"><label>Job Title</label><asp:TextBox ID="txtEnhExp4JobTitle" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-md-4"><div class="form-group"><label>Company</label><asp:TextBox ID="txtEnhExp4Company" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-md-4"><div class="form-group"><label>Location</label><asp:TextBox ID="txtEnhExp4Location" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                                <div class="row">
                                    <div class="col-6 col-md-3"><div class="form-group"><label>Start Month</label><asp:DropDownList ID="ddlEnhExp4StartMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                    <div class="col-6 col-md-3"><div class="form-group"><label>Start Year</label><asp:TextBox ID="txtEnhExp4StartYear" runat="server" CssClass="form-control" MaxLength="4" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-6 col-md-3"><div class="form-group"><label>End Month</label><asp:DropDownList ID="ddlEnhExp4EndMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                    <div class="col-6 col-md-3"><div class="form-group"><label>End Year</label><asp:TextBox ID="txtEnhExp4EndYear" runat="server" CssClass="form-control" MaxLength="4" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                                <div class="row">
                                    <div class="col-12"><div class="form-group"><label><asp:CheckBox ID="chkEnhExp4Current" runat="server" Enabled="false" /> Currently working here</label></div></div>
                                </div>
                                <div class="row">
                                    <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Bullets (one per line)</label><asp:TextBox ID="txtEnhExp4Description" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                            </div>
                        </div>
                        <div class="col-12 resume-card-slot" data-resume-slot="experience-5">
                            <div class="resume-section-card">
                                <h6>Experience 5</h6>
                                <div class="row">
                                    <div class="col-md-4"><div class="form-group"><label>Job Title</label><asp:TextBox ID="txtEnhExp5JobTitle" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-md-4"><div class="form-group"><label>Company</label><asp:TextBox ID="txtEnhExp5Company" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-md-4"><div class="form-group"><label>Location</label><asp:TextBox ID="txtEnhExp5Location" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                                <div class="row">
                                    <div class="col-6 col-md-3"><div class="form-group"><label>Start Month</label><asp:DropDownList ID="ddlEnhExp5StartMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                    <div class="col-6 col-md-3"><div class="form-group"><label>Start Year</label><asp:TextBox ID="txtEnhExp5StartYear" runat="server" CssClass="form-control" MaxLength="4" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-6 col-md-3"><div class="form-group"><label>End Month</label><asp:DropDownList ID="ddlEnhExp5EndMonth" runat="server" CssClass="form-control w-100"></asp:DropDownList></div></div>
                                    <div class="col-6 col-md-3"><div class="form-group"><label>End Year</label><asp:TextBox ID="txtEnhExp5EndYear" runat="server" CssClass="form-control" MaxLength="4" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                                <div class="row">
                                    <div class="col-12"><div class="form-group"><label><asp:CheckBox ID="chkEnhExp5Current" runat="server" Enabled="false" /> Currently working here</label></div></div>
                                </div>
                                <div class="row">
                                    <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Bullets (one per line)</label><asp:TextBox ID="txtEnhExp5Description" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                            </div>
                        </div>
                            <div class="col-12"><h6>Projects</h6><div class="resume-section-help">Up to five projects.</div></div>
                        <div class="col-12 resume-card-slot" data-resume-slot="project-1">
                            <div class="resume-section-card">
                                <h6>Project 1</h6>
                                <div class="row">
                                    <div class="col-12 resume-field-span"><div class="form-group"><label>Title</label><asp:TextBox ID="txtEnhProj1Title" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-12 resume-field-span"><div class="form-group"><label>Tech Stack</label><asp:TextBox ID="txtEnhProj1TechStack" runat="server" CssClass="form-control" MaxLength="100" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Description</label><asp:TextBox ID="txtEnhProj1Description" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                            </div>
                        </div>
                        <div class="col-12 resume-card-slot" data-resume-slot="project-2">
                            <div class="resume-section-card">
                                <h6>Project 2</h6>
                                <div class="row">
                                    <div class="col-12 resume-field-span"><div class="form-group"><label>Title</label><asp:TextBox ID="txtEnhProj2Title" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-12 resume-field-span"><div class="form-group"><label>Tech Stack</label><asp:TextBox ID="txtEnhProj2TechStack" runat="server" CssClass="form-control" MaxLength="100" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Description</label><asp:TextBox ID="txtEnhProj2Description" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                            </div>
                        </div>
                        <div class="col-12 resume-card-slot" data-resume-slot="project-3">
                            <div class="resume-section-card">
                                <h6>Project 3</h6>
                                <div class="row">
                                    <div class="col-12 resume-field-span"><div class="form-group"><label>Title</label><asp:TextBox ID="txtEnhProj3Title" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-12 resume-field-span"><div class="form-group"><label>Tech Stack</label><asp:TextBox ID="txtEnhProj3TechStack" runat="server" CssClass="form-control" MaxLength="100" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Description</label><asp:TextBox ID="txtEnhProj3Description" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                            </div>
                        </div>
                        <div class="col-12 resume-card-slot" data-resume-slot="project-4">
                            <div class="resume-section-card">
                                <h6>Project 4</h6>
                                <div class="row">
                                    <div class="col-12 resume-field-span"><div class="form-group"><label>Title</label><asp:TextBox ID="txtEnhProj4Title" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-12 resume-field-span"><div class="form-group"><label>Tech Stack</label><asp:TextBox ID="txtEnhProj4TechStack" runat="server" CssClass="form-control" MaxLength="100" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Description</label><asp:TextBox ID="txtEnhProj4Description" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                            </div>
                        </div>
                        <div class="col-12 resume-card-slot" data-resume-slot="project-5">
                            <div class="resume-section-card">
                                <h6>Project 5</h6>
                                <div class="row">
                                    <div class="col-12 resume-field-span"><div class="form-group"><label>Title</label><asp:TextBox ID="txtEnhProj5Title" runat="server" CssClass="form-control" MaxLength="50" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-12 resume-field-span"><div class="form-group"><label>Tech Stack</label><asp:TextBox ID="txtEnhProj5TechStack" runat="server" CssClass="form-control" MaxLength="100" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-12 resume-field-span"><div class="form-group mb-0"><label>Description</label><asp:TextBox ID="txtEnhProj5Description" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" ReadOnly="true"></asp:TextBox></div></div>
                                </div>
                            </div>
                        </div>
                            <div class="col-12"><h6>Skills</h6></div>
                        <div class="col-12"><div class="resume-section-card"><div class="row">
                                    <div class="col-12 resume-field-span"><div class="form-group"><label>Programming Languages</label><asp:TextBox ID="txtEnhSkillProgrammingLanguages" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-12 resume-field-span"><div class="form-group"><label>Frameworks / Libraries</label><asp:TextBox ID="txtEnhSkillFrameworksLibraries" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-12 resume-field-span"><div class="form-group"><label>Tools / Cloud / Database</label><asp:TextBox ID="txtEnhSkillToolsCloudDatabase" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-12 resume-field-span"><div class="form-group"><label>Soft Skills / Languages</label><asp:TextBox ID="txtEnhSkillSoftSkillsLanguages" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-12 resume-field-span"><div class="form-group"><label>Custom heading</label><asp:TextBox ID="txtEnhSkillCustomHeading" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox></div></div>
                                    <div class="col-12 resume-field-span"><div class="form-group"><label>Custom items</label><asp:TextBox ID="txtEnhSkillCustomItems" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" ReadOnly="true"></asp:TextBox></div></div>
                                </div></div></div>                        <div class="col-12 resume-preview-card"><div class="resume-section-card"><h6>Certifications</h6><asp:TextBox ID="txtEnhResumeCertifications" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" ReadOnly="true"></asp:TextBox></div></div>
                            <div class="col-12 resume-preview-card"><div class="resume-section-card"><h6>Languages</h6><asp:TextBox ID="txtEnhResumeLanguages" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" ReadOnly="true"></asp:TextBox></div></div>

                            <asp:Panel ID="pnlEnhSaveOptions" runat="server" CssClass="enhancer-save-panel" Visible="false">
                                <asp:Button ID="btnSaveEnhancedResume" runat="server" CssClass="btn-resume-preview" Text="Update" OnClick="btnSaveEnhancedResume_Click" CausesValidation="false" />
                            </asp:Panel>

                            <div class="resume-preview" style="display:none;"><asp:Literal ID="litResumePreview" runat="server" /></div>
                        </div>
                    </div>

                    </div>
                </div>

                <div class="row justify-content-center mt-4">
                    <div class="col-12">
                        <div class="section-card" style="min-width:900px">
                            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px; flex-wrap: wrap; gap: 10px;">
                                <h3 style="margin: 0;">Enhanced Resume Preview</h3>
                                <div class="enhancer-preview-actions" style="margin: 0;">
                                <asp:Button ID="Button1" runat="server" CssClass="btn-resume-preview" Text="Edit Preview" OnClick="btnToggleEnhPreviewEdit_Click" CausesValidation="false" />
<%--                                <asp:Button ID="Button2" runat="server" CssClass="btn-resume-preview" Text="Export PDF" OnClick="return exportPDFasHTML();" CausesValidation="false" style="margin-left:8px;" />--%>
                                                        <%--<div class="col-lg-4 text-right pt-3 d-flex flex-column align-items-end justify-content-center gap-2" style="display: flex; flex-direction: column; align-items: flex-end; gap: 10px;">--%>
                        <a href="#" class="btn-resume-preview" onclick="return exportHTMLAsPDF();">
                            <i class="fas fa-file-code"></i> Preview in New Tab
                        </a>
                    <%--</div>--%>
                            </div>
                                <%--<div class="muted-box" style="margin: 0; padding: 8px 12px;">Rendered from ResumePreview.html using the same preview data.</div>--%>
                            </div>
                            <asp:Literal ID="litHtmlPreviewFrame" runat="server" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
   </div>
            </main>

    <script>
        function resizeTextareas() {
            document.querySelectorAll('.enhancer-preview-form textarea').forEach(function(el) {
                el.style.height = 'auto';
                el.style.height = (el.scrollHeight + 5) + 'px';
            });
        }
        window.addEventListener('load', resizeTextareas);
        document.addEventListener('input', function (e) {
            if (e.target.tagName.toLowerCase() === 'textarea') {
                e.target.style.height = 'auto';
                e.target.style.height = (e.target.scrollHeight + 5) + 'px';
            }
        }, false);
    </script>
</asp:Content>
