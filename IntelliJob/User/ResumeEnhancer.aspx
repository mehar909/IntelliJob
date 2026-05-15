<%@ Page Title="Resume Enhancer" Language="C#" MasterPageFile="~/User/UserMaster.Master" AutoEventWireup="true" CodeBehind="ResumeEnhancer.aspx.cs" Inherits="IntelliJob.User.ResumeEnhancer" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        /* Edit mode: hide everything except the editor section-card */
        #resumeReportBody.enhancer-editing > *:not(.editor-section) { display: none !important; }
        #resumeReportBody.enhancer-editing > .editor-section { display: block !important; }

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
            border-radius: 14px;
            border-color: #e5e7eb;
        }

        .enhancer-preview-form textarea.form-control {
            min-height: 110px;
            resize: vertical;
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
                    <div class="col-lg-4 text-right pt-3 d-flex flex-column align-items-end justify-content-center gap-2" style="display: flex; flex-direction: column; align-items: flex-end; gap: 10px;">
                        <%--<a href="JobApplications.aspx" class="btn-header-action">
                            <i class="fas fa-briefcase"></i> My Applications
                        </a>--%>
                    </div>
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

                    <div class="section-card editor-section">
                        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px; flex-wrap: wrap; gap: 10px;">
                            <h3 style="margin: 0;">Enhanced Resume Preview</h3>
                            <div class="enhancer-preview-actions" style="margin: 0;">
                                <asp:Button ID="btnToggleEnhPreviewEdit" runat="server" CssClass="btn-resume-preview" Text="Edit Preview" OnClick="btnToggleEnhPreviewEdit_Click" CausesValidation="false" />
                                <asp:Button ID="btnExportResumePdf" runat="server" CssClass="btn-resume-preview" Text="Export PDF" OnClick="btnExportResumePdf_Click" CausesValidation="false" style="margin-left:8px;" />
                            </div>
                        </div>
                        <div class="muted-box" style="margin-bottom:14px;">
                            Click <strong>Edit Preview</strong> to edit the resume fields below, then click <strong>Update</strong> to save changes to your applied resume.
                        </div>
                        <div class="enhancer-preview-form">
                            <div class="row">
                                <div class="col-md-6">
                                    <div class="form-group">
                                        <label>Full Name</label>
                                        <asp:TextBox ID="txtEnhFullName" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="form-group">
                                        <label>Email</label>
                                        <asp:TextBox ID="txtEnhEmail" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="form-group">
                                        <label>Mobile</label>
                                        <asp:TextBox ID="txtEnhMobile" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="form-group">
                                        <label>Address</label>
                                        <asp:TextBox ID="txtEnhAddress" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" ReadOnly="true"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="col-12">
                                    <div class="form-group">
                                        <label>Headline</label>
                                        <asp:TextBox ID="txtEnhHeadline" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="col-12">
                                    <div class="form-group">
                                        <label>Summary</label>
                                        <asp:TextBox ID="txtEnhSummary" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" ReadOnly="true"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="col-12">
                                    <div class="form-group">
                                        <label>Skills</label>
                                        <asp:TextBox ID="txtEnhSkills" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" ReadOnly="true"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="col-12">
                                    <div class="form-group">
                                        <label>Education</label>
                                        <asp:TextBox ID="txtEnhEducation" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" ReadOnly="true"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="col-12">
                                    <div class="form-group">
                                        <label>Experience</label>
                                        <asp:TextBox ID="txtEnhExperience" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" ReadOnly="true"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="col-12">
                                    <div class="form-group">
                                        <label>Projects</label>
                                        <asp:TextBox ID="txtEnhProjects" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" ReadOnly="true"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="col-12">
                                    <div class="form-group">
                                        <label>Certifications</label>
                                        <asp:TextBox ID="txtEnhCertifications" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" ReadOnly="true"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="col-12">
                                    <div class="form-group">
                                        <label>Languages</label>
                                        <asp:TextBox ID="txtEnhLanguages" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" ReadOnly="true"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="form-group">
                                        <label>LinkedIn URL <small style="color:#9ca3af;">(optional)</small></label>
                                        <asp:TextBox ID="txtEnhLinkedIn" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="form-group">
                                        <label>Portfolio / GitHub URL <small style="color:#9ca3af;">(optional)</small></label>
                                        <asp:TextBox ID="txtEnhPortfolio" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                                    </div>
                                </div>
                            </div>

                            <asp:Panel ID="pnlEnhSaveOptions" runat="server" CssClass="enhancer-save-panel" Visible="false">
                                <asp:Button ID="btnSaveEnhancedResume" runat="server" CssClass="btn-resume-preview" Text="Update" OnClick="btnSaveEnhancedResume_Click" CausesValidation="false" />
                            </asp:Panel>

                            <div class="resume-preview" style="display:none;"><asp:Literal ID="litResumePreview" runat="server" /></div>
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
