<%@ Page Title="" Language="C#" MasterPageFile="~/User/UserMaster.Master" AutoEventWireup="true" CodeBehind="JobDetails.aspx.cs" Inherits="IntelliJob.User.JobDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .job-container {
            background-color: #f8f9fa;
        }
        /*JOB DETAILS SECTION*/
        .job-card {
            background: #fff;
            border-radius: 18px;
            border: 1px solid #ececec;
            box-shadow: 0 8px 26px rgba(15, 23, 42, 0.06);
            padding: 22px;
            margin-bottom: 18px;
            transition: transform 0.2s ease, box-shadow 0.2s ease;
        }

            .job-card:hover {
                transform: translateY(-2px);
                box-shadow: 0 12px 30px rgba(15, 23, 42, 0.08);
            }

        .job-logo img {
            width: 80px;
            height: 80px;
            /*            object-fit: cover;*/
            border-radius: 14px;
            border: 1px solid #eef2f7;
            /*            margin-top:0px;*/
        }

        .job-title h4 {
            margin: 0 0 6px;
            /*font-size: 20px;*/
            font-weight: 800;
            color: #111827;
        }

            .job-title h4 a {
                color: inherit;
                text-decoration: none;
            }

                .job-title h4 a:hover {
                    color: #FF4357;
                }

        .job-meta {
            color: #6b7280;
            font-size: 14px;
            line-height: 1.7;
        }

        .job-pills {
            display: flex;
            flex-wrap: wrap;
            gap: 8px;
            margin-top: 12px;
        }

        .job-pill {
            background: #f9fafb;
            border: 1px solid #e8edf3;
            color: #374151;
            border-radius: 999px;
            padding: 8px 12px;
            font-size: 13px;
            font-weight: 600;
        }

        /*JOB OVERVIEW SECTION*/
        .job-overview-card {
            background: #fff;
            border-radius: 18px;
            border: 1px solid #ececec;
            box-shadow: 0 8px 26px rgba(15, 23, 42, 0.06);
            padding: 22px;
            margin-bottom: 18px;
            transition: transform 0.2s ease, box-shadow 0.2s ease;
        }

        .job-card:hover {
            transform: translateY(-2px);
            box-shadow: 0 12px 30px rgba(15, 23, 42, 0.08);
        }

        .job-overview-header {
            border-bottom: 2px solid #f0f0f0;
            padding-bottom: 20px;
            margin-bottom: 25px;
        }

            .job-overview-header h4 {
                margin: 0;
                font-size: 22px;
                font-weight: 700;
                color: #2d3436;
                display: flex;
                align-items: center;
            }

        /*.job-overview-header h4:before {
            content: "\f0b1";
            font-family: "Font Awesome 6 Free";
            font-weight: 900;
            color: #FF4357;
            margin-right: 12px;
            font-size: 20px;
        }*/

        .h4-icon {
            /*content: "\f0b1";*/
            /*font-family: "Font Awesome 6 Free";*/
            font-weight: 900;
            color: #FF4357;
            margin-right: 12px;
            font-size: 20px;
        }

        .job-overview-list {
            list-style: none;
            padding: 0;
            margin: 0;
        }

            .job-overview-list li {
                display: flex;
                align-items: center;
                justify-content: space-between;
                padding: 16px 0;
                border-bottom: 1px solid #f0f0f0;
                transition: all 0.3s ease;
            }

                .job-overview-list li:last-child {
                    border-bottom: none;
                }

                .job-overview-list li:hover {
                    background-color: #f8f9fa;
                    margin: 0 -15px;
                    padding-left: 15px;
                    padding-right: 15px;
                    border-radius: 8px;
                }

        .job-overview-label {
            display: flex;
            align-items: center;
            font-size: 15px;
            font-weight: 600;
            color: #495057;
            flex: 1;
        }

            .job-overview-label i {
                margin-right: 10px;
                color: #FF4357;
                width: 20px;
                text-align: center;
                font-size: 16px;
            }

        .job-overview-value {
            font-size: 15px;
            font-weight: 500;
            color: #2d3436;
            text-align: right;
        }

        .apply-btn-container {
            margin-top: 30px;
            padding-top: 25px;
            border-top: 2px solid #f0f0f0;
        }

            .apply-btn-container .application-resume-block {
                margin-top: 14px;
                background: #f8fafc;
                border: 1px dashed #dbe3ec;
                border-radius: 12px;
                padding: 14px 14px 16px;
            }

                .apply-btn-container .application-resume-block label {
                    font-weight: 600;
                    color: #374151;
                    margin-bottom: 8px;
                }

                .apply-btn-container .application-resume-block .form-control[type='file'] {
                    display: block;
                    width: 100%;
                    max-width: 100%;
                    overflow: hidden;
                    box-sizing: border-box;
                    padding: 10px 12px;
                    line-height: 1.4;
                }

        .job-alert-popup {
            position: relative;
            z-index: 1;
            max-width: 100%;
            width: 100%;
            padding: 16px 22px;
            border-radius: 14px;
            box-shadow: 0 16px 40px rgba(15, 23, 42, 0.18);
            text-align: center;
            font-weight: 700;
            margin-bottom: 18px;
        }

        .apply-btn-container .btn-action {
            width: 100%;
            padding: 16px 24px;
            background-color: #fb246a;
            color: white !important;
            border: none;
            border-radius: 8px;
            font-size: 16px;
            font-weight: 600;
            /*            text-transform: uppercase;*/
            letter-spacing: 0.5px;
            transition: all 0.3s ease;
            box-shadow: 0 4px 12px rgba(255, 67, 87, 0.3);
            text-decoration: none;
            display: block;
            text-align: center;
        }

            .apply-btn-container .btn-action:hover {
                transform: translateY(-2px);
                box-shadow: 0 6px 16px rgba(255, 67, 87, 0.4);
                text-decoration: none;
                color: white !important;
            }

            .apply-btn-container .btn-action:active {
                transform: translateY(0);
            }

            .apply-btn-container .btn-action:disabled {
                background: #6c757d;
                cursor: not-allowed;
                opacity: 0.7;
                transform: none;
            }

        .company-info-card {
            background: #ffffff;
            border-radius: 12px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
            padding: 30px;
            border: 1px solid #e9ecef;
        }

        .company-info-header {
            border-bottom: 2px solid #f0f0f0;
            padding-bottom: 20px;
            margin-bottom: 25px;
        }

            .company-info-header h4 {
                margin: 0;
                font-size: 22px;
                font-weight: 700;
                color: #2d3436;
                display: flex;
                align-items: center;
            }

        /*        .company-info-header h4:before {
            content: "\f1ad";
            font-family: "Font Awesome 6";
            font-weight: 900;
            color: #FF4357;
            margin-right: 12px;
            font-size: 20px;
        }*/

        .company-info-list {
            list-style: none;
            padding: 0;
            margin: 0;
        }

            .company-info-list li {
                display: flex;
                justify-content: space-between;
                padding: 14px 0;
                border-bottom: 1px solid #f0f0f0;
                font-size: 15px;
            }

                .company-info-list li:last-child {
                    border-bottom: none;
                }

                .company-info-list li strong {
                    color: #495057;
                    font-weight: 600;
                }

                .company-info-list li a {
                    color: #FF4357;
                    text-decoration: none;
                    transition: color 0.3s ease;
                }

                    .company-info-list li a:hover {
                        color: #fb246a;
                        text-decoration: underline;
                    }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <main>

        <!-- Hero Area Start-->
        <div class="slider-area ">
            <div class="single-slider section-overly slider-height2 d-flex align-items-center" data-background="../assets/img/hero/joblisting.jpg">
                <div class="container">
                    <div class="row">
                        <div class="col-xl-12">
                            <div class="hero-cap text-center">
                                <h2><%# jobTitle %></h2>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <!-- Hero Area End -->

        <div>
            <asp:Label ID="lblMsg" runat="server" Visible="false"></asp:Label>
        </div>
        <!-- job post company Start -->
        <div class="job-post-company pt-120 pb-120 job-container">
            <div class="container">
                <div class="row justify-content-center">
                    <asp:DataList ID="DataList1" runat="server" OnItemCommand="DataList1_ItemCommand" OnItemDataBound="DataList1_ItemDataBound">

                        <ItemTemplate>
                            <div class="row justify-content-between">
                                <!-- Left Content -->
                                <div class="col-xl-7 col-lg-8">
                                    <!-- job single -->
                                    <div class="job-card">
                                        <div class="row align-items-center">
                                            <div class="col-md-2 text-center job-logo">
                                                <img width="80" src="<%# GetImageUrl(Eval("DisplayImage")) %>" alt="Company logo">
                                            </div>
                                            <div class="col-md">
                                                <div class="job-title">
                                                    <h4><%# Eval("Title") %></h4>
                                                </div>
                                                <div class="job-meta">
                                                    <i class="fas fa-building" style="margin-right: 4px;"></i><%# Eval("CompanyName") %></br>
                         <i class="fas fa-solid fa-map-marker-alt" style="margin-right: 4px;"></i><%# Server.HtmlEncode(Eval("State") as string) %>, <%# Server.HtmlEncode(Eval("Country") as string).Replace("Â", "") %>
                                                </div>

                                            </div>
                                            <a href="JobDetails.aspx?id=<%# Eval("JobId") %>" class="btn-action" style="margin-left: 10px; margin-right: 20px;">View Details</a>

                                        </div>
                                    </div>
                                    <!-- job single End -->

                                    <div class="job-post-details">
                                        <div class="post-details1 mb-50">
                                            <!-- Small Section Tittle -->
                                            <div class="small-section-tittle">
                                                <h4>Job Description</h4>
                                            </div>
                                            <p><%# Eval("Description") %></p>
                                        </div>
                                        <div class="post-details2  mb-50">
                                            <!-- Small Section Tittle -->
                                            <div class="small-section-tittle">
                                                <h4>Required Knowledge, Skills, and Abilities</h4>
                                            </div>
                                            <ul>
                                                <li><%# Eval("Specialization") %> </li>
                                                <%-- <li>Mobile Applicationin iOS/Android/Tizen or other platform</li>
                             <li>Research and code , libraries, APIs and frameworks</li>
                             <li>Strong knowledge on software development life cycle</li>
                             <li>Strong problem solving and debugging skills</li>--%>
                                            </ul>
                                        </div>
                                        <div class="post-details2  mb-50">
                                            <!-- Small Section Tittle -->
                                            <div class="small-section-tittle">
                                                <h4>Education + Experience</h4>
                                            </div>
                                            <ul>
                                                <li><%# Eval("Qualification") %></li>
                                                <li><%# Eval("Experience") %> Years</li>
                                                <%--<li>Ecommerce website design experience</li>
                             <li>Familiarity with mobile and web apps preferred</li>
                             <li>Experience using Invision a plus</li>--%>
                                            </ul>
                                        </div>
                                    </div>

                                </div>
                                <!-- Right Content -->
                                <div class="col-xl-4 col-lg-4">
                                    <div class="job-card">
                                        <!-- Job Overview Header -->
                                        <div class="job-overview-header">
                                            <h4>
                                                <i class="fas fa-briefcase h4-icon"></i>
                                                Job Overview</h4>
                                        </div>
                                        <!-- Job Overview List -->
                                        <ul class="job-overview-list">
                                            <li>
                                                <span class="job-overview-label">
                                                    <i class="fas fa-calendar-alt"></i>Posted Date
                                                </span>
                                                <span class="job-overview-value">
                                                    <%# DataBinder.Eval(Container.DataItem, "CreateDate", "{0:dd MMMM yyyy}") %>
                                                </span>
                                            </li>
                                            <li>
                                                <span class="job-overview-label">
                                                    <i class="fas fa-building" style="margin-right: 4px;"></i>Location
                                                </span>
                                                <span class="job-overview-value">
                                                    <%# Eval("State") %>
                                                </span>
                                            </li>
                                            <li>
                                                <span class="job-overview-label">
                                                    <i class="fas fa-users"></i>Vacancies
                                                </span>
                                                <span class="job-overview-value">
                                                    <%# Eval("NoOfPost") %>
                                                </span>
                                            </li>
                                            <li>
                                                <span class="job-overview-label">
                                                    <i class="fas fa-briefcase" style="margin-right: 4px;"></i>Job Type
                                                </span>
                                                <span class="job-overview-value">
                                                    <%# Eval("JobType") %>
                                                </span>
                                            </li>
                                            <li>
                                                <span class="job-overview-label">
                                                    <i class="fas fa-dollar-sign"></i>Salary
                                                </span>
                                                <span class="job-overview-value">
                                                    <%# Eval("Salary") %>
                                                </span>
                                            </li>
                                            <li>
                                                <span class="job-overview-label">
                                                    <i class="fas fa-calendar-times"></i>Last date
                                                </span>
                                                <span class="job-overview-value">
                                                    <%# DataBinder.Eval(Container.DataItem, "LastDateToApply", "{0:dd MMMM yyyy}") %>
                                                </span>
                                            </li>
                                        </ul>
                                        <!-- Apply Button -->
                                        <div class="apply-btn-container">
                                            <asp:LinkButton ID="lbApplyJob" runat="server" CssClass="btn-action" Text="Apply Now" CommandName="ApplyJob" OnClientClick="if(!confirm('Please confirm your resume before applying. Once this job is applied, you will not be able to edit the application resume. Continue?')) return false; this.style.pointerEvents='none'; this.innerHTML='Applying...';"></asp:LinkButton>
                                            <asp:Panel ID="pnlApplicationResumeUpload" runat="server" CssClass="application-resume-block mt-3 text-left" Visible='<%# Eval("ShowApplicationResumeUpload") != DBNull.Value && Convert.ToBoolean(Eval("ShowApplicationResumeUpload")) %>'>
                                                <label class="d-block mb-2" style="font-weight: 600; color: #495057;">Optional resume for this application</label>
                                                <asp:FileUpload ID="fuApplicationResume" runat="server" CssClass="form-control" ToolTip=".doc, .docx, .pdf extension only" />
                                                <small class="text-muted d-block mt-2">If you do not upload a resume here, IntelliJob will use the resume from your profile.</small>
                                                <asp:LinkButton ID="lbSaveApplicationResume" runat="server" CssClass="btn-action" CommandName="SaveApplicationResume">Upload Resume</asp:LinkButton>
                                            </asp:Panel>
                                            <asp:Panel ID="pnlApplicationResumeEdit" runat="server" CssClass="application-resume-block mt-3 text-left" Visible='<%# Eval("HasApplicationResumeDraft") != DBNull.Value && Convert.ToBoolean(Eval("HasApplicationResumeDraft")) %>'>
                                                <label class="d-block mb-2" style="font-weight: 600; color: #495057;">Application resume draft</label>
                                                <asp:HyperLink ID="lnkEditApplicationResume" runat="server" CssClass="btn-action" NavigateUrl='<%# Eval("ApplicationResumeEditUrl") %>'>
                                 <i class="fas fa-pen"></i> Edit Resume
                                                </asp:HyperLink>
                                                <small class="text-muted d-block mt-2"><%# Eval("ApplicationResumeNote") %></small>
                                                <small class="d-block mt-1">
                                                    <asp:LinkButton
                                                        ID="lbDeleteAndUseProfileResumeDraft"
                                                        runat="server"
                                                        CommandName="DeleteAndUseProfileResume"
                                                        CssClass="text-danger"
                                                        OnClientClick="return confirm('The profile resume will be attached to the application and you will not be able to attach a job specific resume again. Continue?');">
                                                        Delete and use profile resume
                                                    </asp:LinkButton>
                                                </small>
                                            </asp:Panel>
                                            <asp:Panel ID="pnlAppliedResumeEdit" runat="server" CssClass="application-resume-block mt-3 text-left" Visible='<%# Eval("ShowAppliedResumeEdit") != DBNull.Value && Convert.ToBoolean(Eval("ShowAppliedResumeEdit")) %>'>
                                                <label class="d-block mb-2" style="font-weight: 600; color: #495057;">Applied resume</label>
                                                <asp:HyperLink ID="lnkAppliedResumeEdit" runat="server" CssClass="btn-action" NavigateUrl='<%# Eval("AppliedResumeEditUrl") %>'>
                                 <i class="fas fa-pen"></i> Edit Resume
                                                </asp:HyperLink>
                                                <small class="text-muted d-block mt-2"><%# Eval("AppliedResumeNote") %></small>
                                            </asp:Panel>
                                        </div>
                                    </div>






                                    <div class="job-card">
                                        <!-- Company Information Header -->
                                        <div class="company-info-header">
                                            <h4>
                                                <i class="fas fa-building h4-icon"></i>
                                                Company Information</h4>
                                        </div>
                                        <!-- Company Information List -->
                                        <ul class="company-info-list">
                                            <li>
                                                <span><strong>Name :</strong></span>
                                                <span><%# Eval("CompanyName") %></span>
                                            </li>
                                            <li>
                                                <span><strong>Web :</strong></span>
                                                <span>
                                                    <a href="<%# Eval("Website") %>" target="_blank">
                                                        <%# Eval("Website") %>
                                                    </a>
                                                </span>
                                            </li>
                                            <li>
                                                <span><strong>Email :</strong></span>
                                                <span>
                                                    <a href="mailto:<%# Eval("Email") %>">
                                                        <%# Eval("Email") %>
                                                    </a>
                                                </span>
                                            </li>
                                            <li>
                                                <span><strong>Address :</strong></span>
                                                <span><%# Eval("Address") %></span>
                                            </li>
                                        </ul>
                                    </div>




                                </div>
                            </div>


                        </ItemTemplate>
                    </asp:DataList>
                </div>
            </div>
        </div>
        <!-- job post company End -->

    </main>
</asp:Content>
