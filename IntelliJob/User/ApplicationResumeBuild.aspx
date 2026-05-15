<%@ Page Title="" Language="C#" MasterPageFile="~/User/UserMaster.Master" AutoEventWireup="true" CodeBehind="ApplicationResumeBuild.aspx.cs" Inherits="IntelliJob.User.ApplicationResumeBuild" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .application-resume-shell {
            padding: 45px 0 80px;
            background: linear-gradient(180deg, #fff7f8 0%, #ffffff 100%);
        }

        .application-resume-hero {
            background: linear-gradient(135deg, #111827 0%, #1f2937 50%, #FF4357 100%);
            color: #fff;
            border-radius: 24px;
            padding: 28px 30px;
            box-shadow: 0 16px 40px rgba(17, 24, 39, 0.18);
            margin-bottom: 22px;
        }

        .application-resume-hero h2 {
            font-size: 30px;
            font-weight: 800;
            margin-bottom: 8px;
            color: #fff;
        }

        .application-resume-hero p {
            margin: 0;
            color: rgba(255, 255, 255, 0.85);
            line-height: 1.7;
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

        .resume-builder-form textarea.form-control {
            height: 120px !important;
            min-height: 120px !important;
            max-height: 220px !important;
            overflow-y: auto !important;
            resize: vertical;
        }

        .resume-builder-actions {
            clear: both;
            margin-top: 24px;
            margin-bottom: 72px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <section class="application-resume-shell">
        <div class="container pt-20 pb-20">
            <div class="row">
                <div class="col-12 pb-20">
                    <asp:Label ID="lblMsg" runat="server" Visible="false"></asp:Label>
                </div>
                <div class="col-12">
                    <div class="application-resume-hero">
                        <h2>Application Resume Draft</h2>
                        <p><asp:Label ID="lblJobInfo" runat="server" /></p>
                    </div>
                </div>
                <div class="col-lg-8 mx-auto">
                    <div class="resume-draft-note">
                        <asp:Label ID="lblDraftNote" runat="server" />
                    </div>
                    <div class="form-contact contact_form resume-builder-form">
                        <div class="row">
                            <div class="col-12">
                                <h6>Structured Resume Sections</h6>
                            </div>

                            <div class="col-12">
                                <div class="form-group">
                                    <label>Headline</label>
                                    <asp:TextBox ID="txtAppResumeHeadline" runat="server" CssClass="form-control" placeholder="Resume headline / title"></asp:TextBox>
                                </div>
                            </div>

                            <div class="col-12">
                                <div class="form-group">
                                    <label>Summary</label>
                                    <asp:TextBox ID="txtAppResumeSummary" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" placeholder="Professional summary"></asp:TextBox>
                                </div>
                            </div>

                            <div class="col-12">
                                <div class="form-group">
                                    <label>Skills</label>
                                    <asp:TextBox ID="txtAppResumeSkills" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" placeholder="One skill per line"></asp:TextBox>
                                </div>
                            </div>

                            <div class="col-12">
                                <div class="form-group">
                                    <label>Education Details</label>
                                    <asp:TextBox ID="txtAppResumeEducation" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" placeholder="Education entries"></asp:TextBox>
                                </div>
                            </div>

                            <div class="col-12">
                                <div class="form-group">
                                    <label>Experience Details</label>
                                    <asp:TextBox ID="txtAppResumeExperienceDetails" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" placeholder="Experience entries"></asp:TextBox>
                                </div>
                            </div>

                            <div class="col-12">
                                <div class="form-group">
                                    <label>Projects</label>
                                    <asp:TextBox ID="txtAppResumeProjects" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" placeholder="Project entries"></asp:TextBox>
                                </div>
                            </div>

                            <div class="col-12">
                                <div class="form-group">
                                    <label>Certifications</label>
                                    <asp:TextBox ID="txtAppResumeCertifications" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" placeholder="Certification entries"></asp:TextBox>
                                </div>
                            </div>

                            <div class="col-12">
                                <div class="form-group">
                                    <label>Languages</label>
                                    <asp:TextBox ID="txtAppResumeLanguages" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" placeholder="Language entries"></asp:TextBox>
                                </div>
                            </div>

                            <div class="row resume-builder-actions w-100">
                                <div class="col-md-6 text-center mb-2 mb-md-0">
                                    <asp:HyperLink ID="lnkBackToJob" runat="server" CssClass="btn button-contactForm boxed-btn" NavigateUrl="~/User/JobDetails.aspx">Back</asp:HyperLink>
                                </div>
                                <div class="col-md-6 text-center">
                                    <asp:Button ID="btnConfirm" runat="server" Text="Confirm" CssClass="button button-contactForm boxed-btn" OnClick="btnConfirm_Click" CausesValidation="false" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </section>
</asp:Content>
