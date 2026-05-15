<%@ Page Title="Resume Enhancer" Language="C#" MasterPageFile="~/User/UserMaster.Master" AutoEventWireup="true" CodeBehind="ResumeEnhancer.aspx.cs" Inherits="IntelliJob.User.ResumeEnhancer" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .enhancer-shell {
            padding: 45px 0 80px;
            background: linear-gradient(180deg, #fff7f8 0%, #ffffff 100%);
        }

        .enhancer-hero {
            background: linear-gradient(135deg, #111827 0%, #1f2937 50%, #FF4357 100%);
            color: #fff;
            border-radius: 24px;
            padding: 28px 30px;
            box-shadow: 0 16px 40px rgba(17, 24, 39, 0.18);
            margin-bottom: 22px;
        }

        .enhancer-hero h2 {
            font-size: 30px;
            font-weight: 800;
            margin-bottom: 8px;
            color: #ffffff;
            text-shadow: 0 2px 8px rgba(0, 0, 0, 0.25);
        }

        .enhancer-hero p {
            margin: 0;
            color: rgba(255, 255, 255, 0.85);
            line-height: 1.7;
        }

        .hero-chips {
            display: flex;
            gap: 10px;
            flex-wrap: wrap;
            margin-top: 18px;
        }

        .hero-actions {
            display: flex;
            gap: 12px;
            flex-wrap: wrap;
            margin-top: 18px;
        }

        .hero-action-btn {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            gap: 8px;
            padding: 12px 18px;
            border-radius: 999px;
            text-decoration: none;
            font-size: 14px;
            font-weight: 700;
            border: 1px solid rgba(255,255,255,0.2);
            transition: transform 0.2s ease, box-shadow 0.2s ease, background 0.2s ease;
        }

        .hero-action-btn.primary {
            background: #ffffff;
            color: #111827;
        }

        .hero-action-btn.secondary {
            background: rgba(255,255,255,0.12);
            color: #ffffff;
        }

        .hero-action-btn:hover {
            transform: translateY(-1px);
            text-decoration: none;
        }

        .hero-chip {
            background: rgba(255, 255, 255, 0.12);
            border: 1px solid rgba(255, 255, 255, 0.18);
            padding: 8px 14px;
            border-radius: 999px;
            font-size: 13px;
            font-weight: 600;
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
        <div class="enhancer-shell">
            <div class="container">
                <div class="enhancer-hero">
                    <h2>Resume Enhancer</h2>
                    <p>
                        A job-specific review built from the uploaded resume, the target role, the job description, and the interview feedback already collected inside IntelliJob.
                    </p>
                    <div class="hero-chips">
                        <span class="hero-chip"><asp:Literal ID="litRole" runat="server" /></span>
                        <span class="hero-chip"><asp:Literal ID="litCompany" runat="server" /></span>
                        <span class="hero-chip"><asp:Literal ID="litLevel" runat="server" /></span>
                        <span class="hero-chip"><asp:Literal ID="litInterviewType" runat="server" /></span>
                    </div>
                    <div class="hero-actions">
                        <a href='ResumeBuild.aspx?id=<%= Session["userId"] %>' class="hero-action-btn primary">
                            <i class="fas fa-file-upload"></i> Update Resume
                        </a>
                        <a href="JobApplications.aspx" class="hero-action-btn secondary">
                            <i class="fas fa-briefcase"></i> My Applications
                        </a>
                    </div>
                </div>

                <asp:Literal ID="litStatus" runat="server" />
                <asp:HiddenField ID="hfLoadedAppliedJobId" runat="server" />
                <asp:HiddenField ID="hfLoadedInterviewId" runat="server" />
                <asp:HiddenField ID="hfLoadedResumePath" runat="server" />
                <asp:HiddenField ID="hfLoadedResumeSource" runat="server" />
                <asp:HiddenField ID="hfLoadedOriginalFileName" runat="server" />

                <div id="resumeReportBody" class="report-body">
                    <div class="score-grid">
                        <div class="metric-card">
                            <div class="label">Overall Match</div>
                            <div class="value"><asp:Literal ID="litOverallScore" runat="server" />%</div>
                            <div class="hint">How strongly the resume fits the job</div>
                        </div>
                        <div class="metric-card">
                            <div class="label">ATS Fit</div>
                            <div class="value"><asp:Literal ID="litAtsScore" runat="server" />%</div>
                            <div class="hint">Formatting and recruiter-system friendliness</div>
                        </div>
                        <div class="metric-card">
                            <div class="label">Semantic Fit</div>
                            <div class="value"><asp:Literal ID="litSemanticScore" runat="server" />%</div>
                            <div class="hint">Meaning match with the role and feedback</div>
                        </div>
                        <div class="metric-card">
                            <div class="label">Keyword Fit</div>
                            <div class="value"><asp:Literal ID="litKeywordScore" runat="server" />%</div>
                            <div class="hint">Soft keyword alignment, not a strict rejection rule</div>
                        </div>
                    </div>

                    <div class="section-card">
                        <h3>Resume Summary</h3>
                        <p><asp:Literal ID="litResumeSummary" runat="server" /></p>
                    </div>

                    <div class="row">
                        <div class="col-lg-6">
                            <div class="section-card">
                                <h3>Strengths</h3>
                                <ul class="bullet-list">
                                    <asp:Literal ID="litStrengths" runat="server" />
                                </ul>
                            </div>
                        </div>
                        <div class="col-lg-6">
                            <div class="section-card">
                                <h3>Gaps To Improve</h3>
                                <ul class="bullet-list">
                                    <asp:Literal ID="litGaps" runat="server" />
                                </ul>
                            </div>
                        </div>
                    </div>

                    <div class="section-card">
                        <h3>Enhanced Resume Preview</h3>
                        <div class="muted-box" style="margin-bottom:14px;">
                            Click Edit Preview to unlock the boxes below. Save as a job resume or profile resume when you are done.
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
                            </div>

                            <div class="enhancer-preview-actions">
                                <asp:Button ID="btnToggleEnhPreviewEdit" runat="server" CssClass="btn btn-post" Text="Edit Preview" OnClick="btnToggleEnhPreviewEdit_Click" CausesValidation="false" />
                            </div>

                            <asp:Panel ID="pnlEnhSaveOptions" runat="server" CssClass="enhancer-save-panel" Visible="false">
                                <h4 style="font-size:18px; font-weight:800; color:#111827; margin-bottom:6px;">Save Enhanced Resume</h4>
                                <div class="muted-box" style="margin-bottom:14px;">Choose where this edited version should be saved.</div>
                                <asp:RadioButtonList ID="rblEnhSaveTarget" runat="server" CssClass="radio-list-inline" RepeatDirection="Horizontal" RepeatLayout="Flow">
                                    <asp:ListItem Text="Save as Job Resume" Value="job" Selected="True"></asp:ListItem>
                                    <asp:ListItem Text="Save as Profile Resume" Value="profile"></asp:ListItem>
                                </asp:RadioButtonList>
                                <asp:Button ID="btnSaveEnhancedResume" runat="server" CssClass="button button-contactForm boxed-btn" Text="Save Enhanced Resume" OnClick="btnSaveEnhancedResume_Click" CausesValidation="false" />
                            </asp:Panel>

                            <div class="resume-preview" style="display:none;"><asp:Literal ID="litResumePreview" runat="server" /></div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-lg-6">
                            <div class="section-card">
                                <h3>Suggested Rewrites</h3>
                                <asp:Literal ID="litRewriteSuggestions" runat="server" />
                            </div>
                        </div>
                        <div class="col-lg-6">
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
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
</asp:Content>
