<%@ Page Title="Job Applications" Language="C#" MasterPageFile="~/User/UserMaster.Master" AutoEventWireup="true" CodeBehind="JobApplications.aspx.cs" Inherits="IntelliJob.User.JobApplications" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .applications-container {
            padding: 50px 0 80px;
        }

        .applications-hero {
            background: linear-gradient(135deg, #111827 0%, #1f2937 50%, #FF4357 100%);
            color: #fff;
            border-radius: 24px;
            padding: 28px 30px;
            box-shadow: 0 16px 40px rgba(17, 24, 39, 0.18);
            margin-bottom: 24px;
        }

        .applications-hero h2 {
            color: #fff;
            font-size: 30px;
            font-weight: 800;
            margin-bottom: 8px;
        }

        .applications-hero p {
            margin: 0;
            color: rgba(255, 255, 255, 0.85);
            line-height: 1.7;
        }

        .application-card {
            background: #fff;
            border-radius: 18px;
            border: 1px solid #ececec;
            box-shadow: 0 8px 26px rgba(15, 23, 42, 0.06);
            padding: 22px;
            margin-bottom: 18px;
            transition: transform 0.2s ease, box-shadow 0.2s ease;
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

        .score-high { background: #d4edda; color: #155724; }
        .score-mid { background: #fff3cd; color: #856404; }
        .score-low { background: #f8d7da; color: #721c24; }
        .score-none { background: #edf2f7; color: #6b7280; }

        .btn-action {
            display: inline-block;
            padding: 11px 18px;
            border-radius: 10px;
            background: linear-gradient(135deg, #FF4357 0%, #ff6b7a 100%);
            color: #fff;
            font-weight: 700;
            text-decoration: none;
            box-shadow: 0 4px 12px rgba(255, 67, 87, 0.25);
        }

        .btn-action:hover {
            color: #fff;
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
            display: flex;
            justify-content: flex-end;
            gap: 12px;
            margin-bottom: 18px;
        }

        .top-links a {
            display: inline-block;
            border: 1px solid #FF4357;
            color: #FF4357;
            border-radius: 10px;
            padding: 10px 16px;
            text-decoration: none;
            font-weight: 700;
            background: #fff;
        }

        .top-links a:hover {
            background: #fff5f6;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <main>
        <div class="applications-container">
            <div class="container">
                <div class="applications-hero">
                    <h2>My Job Applications</h2>
                    <p>Track every role you have applied for, open the saved resume report, and jump to the interview history for that job.</p>
                </div>

                <div class="top-links">
                    <a href="JobListing.aspx"><i class="fas fa-search"></i> Browse Jobs</a>
                    <a href="Home.aspx"><i class="fas fa-home"></i> Home</a>
                </div>

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
                                        <span class="application-pill"><i class="fas fa-calendar-alt"></i> <%# Eval("ApplicationDate") %></span>
                                        <span class="application-pill"><i class="fas fa-file-alt"></i> Resume Source: <%# Eval("ResumeSource") %></span>
                                        <span class="application-pill"><i class="fas fa-clipboard"></i> Application Status: <%# (Eval("Shortlisted").ToString().ToLower() == "yes" ? "Shortlisted" : "Under Review") %></span>
                                    </div>
                                 <div class="row text-right" style="margin-top:12px">
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
    </main>
</asp:Content>
