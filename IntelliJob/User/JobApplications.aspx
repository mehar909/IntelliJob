<%@ Page Title="Job Applications" Language="C#" MasterPageFile="~/User/UserMaster.Master" AutoEventWireup="true" CodeBehind="JobApplications.aspx.cs" Inherits="IntelliJob.User.JobApplications" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .applications-container {
            background-color: #f8f9fa;
            padding: 50px 0 80px;
        }

        .page-header-section {
            background: linear-gradient(100deg, #da2461 10%, #011B43 90%);
            padding: 54px 0 28px;
            min-height: 450px;
        }

            .page-header-section .container {
                padding: 100px 32px;
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

            .btn-header-action:hover {
                background-color: #da2461;
                border-color: #da2461;
                color: #fff !important;
                text-decoration: none;
                transform: translateY(-1px);
            }

        .application-card {
            background: #fff;
            border-radius: 18px;
            border: 1px solid #ececec;
            box-shadow: 0 8px 26px rgba(15, 23, 42, 0.06);
            padding: 22px;
            margin-bottom: 18px;
            transition: transform 0.2s ease, box-shadow 0.2s ease;
            max-width: 900px;
        }

            .application-card:hover {
                transform: translateY(-2px);
                box-shadow: 0 12px 30px rgba(15, 23, 42, 0.08);
            }

        .application-logo img {
            width: 64px;
            height: 64px;
            object-fit: cover;
            border-radius: 14px;
            border: 1px solid #eef2f7;
        }

        .application-title h4 {
            margin: 0 0 6px;
            font-size: 20px;
            font-weight: 800;
            color: #111827;
        }

            .application-title h4 a {
                color: inherit;
                text-decoration: none;
            }

                .application-title h4 a:hover {
                    color: #FF4357;
                }

        .application-meta {
            color: #6b7280;
            font-size: 14px;
            line-height: 1.7;
        }

        .application-pills {
            display: flex;
            flex-wrap: wrap;
            gap: 8px;
            margin-top: 12px;
        }

        .application-pill {
            background: #f9fafb;
            border: 1px solid #e8edf3;
            color: #374151;
            border-radius: 999px;
            padding: 8px 12px;
            font-size: 13px;
            font-weight: 600;
        }

        .score-badge {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            min-width: 86px;
            padding: 10px 14px;
            border-radius: 16px;
            font-weight: 800;
            font-size: 15px;
        }

        .score-high {
            background: #d4edda;
            color: #155724;
        }

        .score-mid {
            background: #fff3cd;
            color: #856404;
        }

        .score-low {
            background: #f8d7da;
            color: #721c24;
        }

        .score-none {
            background: #edf2f7;
            color: #6b7280;
        }

        .btn-action {
            align-items: center;
            gap: 8px;
            background-color: #fb246a;
            border: 2px solid #fb246a;
            color: #ffffff !important;
            font-size: 15px;
            transition: all 0.2s ease;
            white-space: nowrap;
            display: inline-block;
            padding: 12px 18px;
            margin-top: 18px;
            border-radius: 10px;
            font-weight: 700;
            text-decoration: none;
            box-shadow: 0 4px 12px rgba(255, 67, 87, 0.25);
        }

            .btn-action:hover {
                background-color: #da2461;
                border-color: #da2461;
                color: #fff !important;
                text-decoration: none;
                transform: translateY(-1px);
            }

        .empty-state {
            text-align: center;
            padding: 60px 20px;
            background: #fff;
            border-radius: 18px;
            border: 1px dashed #dbe3ec;
        }

            .empty-state i {
                font-size: 56px;
                color: #cbd5e1;
                margin-bottom: 18px;
                display: block;
            }

            .empty-state p {
                color: #6b7280;
                font-size: 16px;
                margin-bottom: 18px;
            }

        .top-links {
            display: none;
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
                        <h2>My Job Applications</h2>
                        <p>Track every role you have applied for, open the saved resume report, and jump to the interview history for that job.</p>
                    </div>
                    <div class="col-lg-4 text-right pt-3 d-flex flex-column align-items-end justify-content-center gap-2" style="display: flex; flex-direction: column; align-items: flex-end; gap: 10px;">
                        <a href="JobListing.aspx" class="btn-header-action">
                            <i class="fas fa-search"></i>Browse Jobs
                        </a>
                        <%--<a href="Home.aspx" class="btn-header-action">
                            <i class="fas fa-home"></i> Home
                        </a>--%>
                    </div>
                </div>
            </div>
        </div>

        <div class="applications-container" style="padding-top: 60px;">
            <div class="container">
                <div class="row justify-content-center">

                    <asp:Literal ID="litEmpty" runat="server" />

                    <asp:Repeater ID="rptApplications" runat="server">
                        <ItemTemplate>
                            <div class="application-card">
                                <div class="row align-items-center">
                                    <div class="col-md-2 text-center application-logo">
                                        <img src='<%# GetImageUrl(Eval("CompanyImage")) %>' alt='<%# Eval("CompanyName") %>' />
                                    </div>
                                    <div class="col-md">
                                        <div class="application-title">
                                            <h4><%# Eval("Title") %></h4>
                                        </div>
                                        <div class="application-meta">
                                            <%# Eval("CompanyName") %> - <%# Eval("JobType") %><br />
                                            <%# Server.HtmlEncode(Eval("State") as string) %>, <%# Server.HtmlEncode(Eval("Country") as string).Replace("Â", "") %>
                                        </div>
                                        <div class="application-pills">
                                            <span class="application-pill"><i class="fas fa-calendar-alt"></i><%# Eval("ApplicationDate") %></span>
                                            <span class="application-pill"><i class="fas fa-file-alt"></i>Resume Source: <%# Eval("ResumeSource") %></span>
                                            <span class="application-pill"><i class="fas fa-clipboard"></i>Application Status: <%# (Eval("Shortlisted").ToString().ToLower() == "yes" ? "Shortlisted" : "Under Review") %></span>
                                        </div>
                                        <div class="row text-right" style="margin-top: 12px">
                                            <%# Eval("ShowInterviewFeedbackButton") as string %>
                                            <%# Eval("ShowResumeFeedbackButton") as string %>
                                        </div>
                                    </div>

                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>
    </main>
</asp:Content>
