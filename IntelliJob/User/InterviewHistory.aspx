<%@ Page Title="Interview History" Language="C#" MasterPageFile="~/User/UserMaster.Master" AutoEventWireup="true" CodeBehind="InterviewHistory.aspx.cs" Inherits="IntelliJob.User.InterviewHistory" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .history-container {
            padding: 50px 0 80px;
        }

        .history-header {
            margin-bottom: 30px;
        }

        .history-header h2 {
            font-size: 28px;
            font-weight: 700;
            color: #2d3436;
        }

        .history-header p {
            color: #636e72;
            font-size: 15px;
        }

        .stats-row {
            display: flex;
            gap: 20px;
            margin-bottom: 30px;
            flex-wrap: wrap;
        }

        .stat-card {
            background: #ffffff;
            border-radius: 12px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
            border: 1px solid #e9ecef;
            padding: 20px 25px;
            flex: 1;
            min-width: 150px;
            text-align: center;
        }

        .stat-card .stat-number {
            font-size: 32px;
            font-weight: 700;
            color: #FF4357;
        }

        .stat-card .stat-label {
            font-size: 13px;
            color: #636e72;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .interview-table-card {
            background: #ffffff;
            border-radius: 12px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
            border: 1px solid #e9ecef;
            overflow: hidden;
        }

        .interview-table-card .table-header {
            background: linear-gradient(135deg, #2d3436, #636e72);
            color: #fff;
            padding: 18px 25px;
            font-size: 16px;
            font-weight: 600;
        }

        .interview-row {
            padding: 18px 25px;
            border-bottom: 1px solid #f0f0f0;
            transition: background 0.2s ease;
        }

        .interview-row:hover {
            background: #f8f9fa;
        }

        .interview-row:last-child {
            border-bottom: none;
        }

        .interview-row .role-name {
            font-size: 16px;
            font-weight: 700;
            color: #2d3436;
        }

        .interview-row .meta {
            color: #636e72;
            font-size: 13px;
            margin-top: 4px;
        }

        .interview-row .meta i {
            color: #FF4357;
            margin-right: 4px;
        }

        .status-badge {
            display: inline-block;
            padding: 4px 12px;
            border-radius: 12px;
            font-size: 12px;
            font-weight: 600;
            text-transform: uppercase;
        }
        .status-pending { background: #fff3cd; color: #856404; }
        .status-completed { background: #d4edda; color: #155724; }
        .status-in-progress { background: #cce5ff; color: #004085; }
        .status-cancelled { background: #f8d7da; color: #721c24; }

        .score-badge {
            display: inline-block;
            padding: 6px 16px;
            border-radius: 20px;
            font-weight: 700;
            font-size: 14px;
        }
        .score-high { background: #d4edda; color: #155724; }
        .score-mid { background: #fff3cd; color: #856404; }
        .score-low { background: #f8d7da; color: #721c24; }

        .btn-action {
            border: 1px solid #FF4357;
            color: #FF4357;
            border-radius: 6px;
            padding: 6px 16px;
            font-size: 13px;
            font-weight: 600;
            text-decoration: none;
            transition: all 0.3s ease;
        }

        .btn-action:hover {
            background: #FF4357;
            color: #fff;
            text-decoration: none;
        }

        .empty-state {
            text-align: center;
            padding: 60px 20px;
        }

        .empty-state i {
            font-size: 64px;
            color: #dee2e6;
            margin-bottom: 20px;
            display: block;
        }

        .empty-state p {
            font-size: 16px;
            color: #636e72;
            margin-bottom: 20px;
        }

        .empty-state a {
            background: linear-gradient(135deg, #FF4357 0%, #ff6b7a 100%);
            color: #fff;
            border-radius: 8px;
            padding: 12px 30px;
            font-weight: 600;
            text-decoration: none;
            display: inline-block;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <main>
        <div class="history-container">
            <div class="container">
                <!-- Header -->
                <div class="row">
                    <div class="col-lg-8">
                        <div class="history-header">
                            <h2><i class="fas fa-history" style="color: #FF4357;"></i> Interview History</h2>
                            <p>View all your past interviews and their results.</p>
                        </div>
                    </div>
                    <div class="col-lg-4 text-right pt-3">
                        <a href="Interview.aspx" class="btn" style="background: linear-gradient(135deg, #FF4357, #ff6b7a); color: #fff; border-radius: 8px; padding: 10px 25px; font-weight: 600;">
                            <i class="fas fa-plus"></i> New Interview
                        </a>
                    </div>
                </div>

                <!-- Stats -->
                <div class="stats-row">
                    <div class="stat-card">
                        <div class="stat-number"><asp:Literal ID="litTotalInterviews" runat="server" /></div>
                        <div class="stat-label">Total Interviews</div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-number"><asp:Literal ID="litCompletedCount" runat="server" /></div>
                        <div class="stat-label">Completed</div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-number"><asp:Literal ID="litAvgScore" runat="server" /></div>
                        <div class="stat-label">Avg Score</div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-number"><asp:Literal ID="litBestScore" runat="server" /></div>
                        <div class="stat-label">Best Score</div>
                    </div>
                </div>

                <!-- Interview List -->
                <div class="interview-table-card">
                    <div class="table-header">
                        <i class="fas fa-list"></i> All Interviews
                    </div>

                    <asp:Repeater ID="rptInterviews" runat="server">
                        <ItemTemplate>
                            <div class="interview-row">
                                <div class="row align-items-center">
                                    <div class="col-md-4">
                                        <div class="role-name"><%# Eval("Role") %> Interview</div>
                                        <div class="meta">
                                            <i class="fas fa-layer-group"></i> <%# Eval("Level") %>
                                            &middot; <i class="fas fa-tag"></i> <%# Eval("InterviewType") %>
                                            &middot; <%# Convert.ToDateTime(Eval("CreatedAt")).ToString("MMM dd, yyyy") %>
                                        </div>
                                    </div>
                                    <div class="col-md-2 text-center">
                                        <%# GetTechStackDisplay(Eval("TechStack")) %>
                                    </div>
                                    <div class="col-md-2 text-center">
                                        <span class='status-badge status-<%# Eval("Status").ToString().ToLower().Replace(" ", "-") %>'>
                                            <%# Eval("Status") %>
                                        </span>
                                    </div>
                                    <div class="col-md-2 text-center">
                                        <%# GetScoreBadge(Eval("TotalScore")) %>
                                    </div>
                                    <div class="col-md-2 text-right">
                                        <a href='<%# GetActionLink(Eval("InterviewId"), Eval("Status")) %>' class="btn-action">
                                            <%# GetActionText(Eval("Status")) %>
                                        </a>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:Literal ID="litEmpty" runat="server" Visible="false" />
                </div>
            </div>
        </div>
    </main>
</asp:Content>
