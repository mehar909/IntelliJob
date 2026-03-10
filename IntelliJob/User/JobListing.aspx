<%@ Page Title="" Language="C#" Async="true" MasterPageFile="~/User/UserMaster.Master" AutoEventWireup="true" CodeBehind="JobListing.aspx.cs" Inherits="IntelliJob.User.JobListing" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet" />
    
    <style type="text/css">
        /* Modern Filter Panel Styles */
        .filter-panel {
            background: #ffffff;
            border-radius: 12px;
            box-shadow: 0 2px 12px rgba(0,0,0,0.08);
            padding: 24px;
            position: sticky;
            top: 20px;
        }

        .filter-header {
            display: flex;
            align-items: center;
            margin-bottom: 28px;
            padding-bottom: 16px;
            border-bottom: 2px solid #f0f0f0;
        }

        .filter-header i {
            font-size: 20px;
            color: #FF4357;
            margin-right: 12px;
        }

        .filter-header h4 {
            margin: 0;
            font-size: 20px;
            font-weight: 700;
            color: #2d3436;
        }

        /* Form Groups */
        .filter-group {
            margin-bottom: 24px;
        }

        .filter-label {
            display: block;
            font-size: 14px;
            font-weight: 600;
            color: #2d3436;
            margin-bottom: 10px;
            letter-spacing: 0.3px;
        }

        .filter-label i {
            margin-right: 8px;
            color: #FF4357;
            width: 16px;
        }

        /* Input Styles */
        .modern-input {
            width: 100%;
            padding: 12px 16px;
            border: 2px solid #e9ecef;
            border-radius: 8px;
            font-size: 14px;
            transition: all 0.3s ease;
            background: #f8f9fa;
            box-sizing: border-box;
        }

        .modern-input:focus {
            outline: none;
            border-color: #FF4357;
            background: #ffffff;
            box-shadow: 0 0 0 3px rgba(255, 67, 87, 0.1);
        }

        .modern-input::placeholder {
            color: #adb5bd;
        }

        /* Dropdown Styles - Updated with centered text */
        .modern-select {
            width: 100%;
            padding: 12px 16px;
            border: 2px solid #e9ecef;
            border-radius: 8px;
            font-size: 14px;
            background: #f8f9fa;
            cursor: pointer;
            transition: all 0.3s ease;
            box-sizing: border-box;
            text-align: center;
            text-align-last: center;
            line-height: normal;
            height: 46px;
            appearance: none;
            -webkit-appearance: none;
            -moz-appearance: none;
            background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 12 12'%3E%3Cpath fill='%23adb5bd' d='M6 9L1 4h10z'/%3E%3C/svg%3E");
            background-repeat: no-repeat;
            background-position: center right 16px;
            background-size: 12px;
        }

        .modern-select:focus {
            outline: none;
            border-color: #FF4357;
            background-color: #ffffff;
            box-shadow: 0 0 0 3px rgba(255, 67, 87, 0.1);
        }

        .modern-select option {
            text-align: center;
        }

        /* Radio Button List Styling - Job Type */
        #RadioButtonListJobType {
            display: flex;
            flex-direction: column;
            gap: 10px;
        }

        #RadioButtonListJobType tr {
            display: block;
            margin-bottom: 0;
        }

        #RadioButtonListJobType td {
            display: flex;
            align-items: center;
            padding: 10px 14px;
            border: 2px solid #e9ecef;
            border-radius: 8px;
            transition: all 0.3s ease;
            background: #f8f9fa;
            cursor: pointer;
        }

        #RadioButtonListJobType td:hover {
            border-color: #FF4357;
            background: #fff5f6;
        }

        #RadioButtonListJobType input[type="radio"] {
            width: 18px;
            height: 18px;
            margin: 0 10px 0 0;
            cursor: pointer;
            accent-color: #FF4357;
        }

        #RadioButtonListJobType label {
            margin: 0;
            cursor: pointer;
            font-size: 14px;
            color: #495057;
            font-weight: 500;
            flex: 1;
        }

        #RadioButtonListJobType input[type="radio"]:checked ~ label,
        #RadioButtonListJobType input[type="radio"]:checked + label {
            color: #FF4357;
            font-weight: 600;
        }

        #RadioButtonListJobType td:has(input[type="radio"]:checked) {
            border-color: #FF4357;
            background: #fff5f6;
        }

        /* Radio Button List Styling - Posted Within (Pill Style) */
        #RadioButtonList1 {
            display: flex;
            flex-direction: column;
            gap: 8px;
        }

        #RadioButtonList1 tr {
            display: block;
            margin-bottom: 0;
        }

        #RadioButtonList1 td {
            display: block;
        }

        #RadioButtonList1 label {
            display: inline-block;
            padding: 8px 16px;
            border: 2px solid #e9ecef;
            border-radius: 20px;
            font-size: 13px;
            font-weight: 500;
            color: #495057;
            cursor: pointer;
            transition: all 0.3s ease;
            background: #f8f9fa;
            white-space: nowrap;
            margin: 0;
            width: 100%;
            text-align: center;
        }

        #RadioButtonList1 label:hover {
            border-color: #FF4357;
            background: #fff5f6;
        }

        #RadioButtonList1 input[type="radio"] {
            position: absolute;
            opacity: 0;
            pointer-events: none;
        }

        #RadioButtonList1 input[type="radio"]:checked + label {
            background: #FF4357;
            color: white;
            border-color: #FF4357;
            font-weight: 600;
            box-shadow: 0 2px 8px rgba(255, 67, 87, 0.3);
        }

        /* Action Buttons */
        .filter-actions {
            display: flex;
            flex-direction: column;
            gap: 12px;
            margin-top: 28px;
            padding-top: 20px;
            border-bottom: 2px solid #f0f0f0;
        }

        .btn-filter {
            width: 100%;
            padding: 14px 24px;
            border: none;
            border-radius: 8px;
            font-size: 15px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            text-decoration: none;
            display: inline-block;
            text-align: center;
        }

        .btn-filter-primary {
            background: linear-gradient(135deg, #FF4357 0%, #ff6b7a 100%);
            color: white !important;
            box-shadow: 0 4px 12px rgba(255, 67, 87, 0.3);
        }

        .btn-filter-primary:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 16px rgba(255, 67, 87, 0.4);
            text-decoration: none;
        }

        .btn-filter-primary:active {
            transform: translateY(0);
        }

        .btn-filter-secondary {
            background: #f8f9fa;
            color: #495057 !important;
            border: 2px solid #e9ecef;
        }

        .btn-filter-secondary:hover {
            background: #e9ecef;
            border-color: #dee2e6;
            text-decoration: none;
        }

        /* Input Icon Wrapper */
        .input-with-icon {
            position: relative;
        }

        .input-with-icon i {
            position: absolute;
            left: 16px;
            top: 50%;
            transform: translateY(-50%);
            color: #adb5bd;
            pointer-events: none;
        }

        .input-with-icon .modern-input {
            padding-left: 44px;
        }

        /* Responsive */
        @media (max-width: 768px) {
            .filter-panel {
                border-radius: 8px;
                padding: 20px;
            }
        }

        /* Remove default table styling from RadioButtonList */
        #RadioButtonListJobType table,
        #RadioButtonList1 table {
            border-collapse: separate;
            border-spacing: 0;
            width: 100%;
        }

        #RadioButtonListJobType tbody,
        #RadioButtonList1 tbody {
            display: contents;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <main>
        <div class="slider-area">
            <div class="single-slider section-overly slider-height2 d-flex align-items-center" data-background="../assets/img/hero/joblisting.jpg">
                <div class="container">
                    <div class="row">
                        <div class="col-xl-12">
                            <div class="hero-cap text-center">
                                <h2>Get your job</h2>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="job-listing-area pt-50 pb-120">
            <div class="container">
                <div class="row">
                    <!-- Left Filter Panel -->
                    <div class="col-xl-3 col-lg-4 col-md-4">
                        <div class="filter-panel">
                            <!-- Filter Header -->
                            <div class="filter-header">
                                <i class="fas fa-filter"></i>
                                <h4>Filter Jobs</h4>
                            </div>

                            <!-- Keyword Search -->
                            <div class="filter-group">
                                <label class="filter-label">
                                    <i class="fas fa-search"></i>Keyword
                                </label>
                                <div class="input-with-icon">
                                    <i class="fas fa-briefcase"></i>
                                    <asp:TextBox ID="txtKeyword" runat="server" CssClass="modern-input" placeholder="e.g. Developer, Designer" />
                                </div>
                            </div>

                            <!-- Job Location -->
                            <div class="filter-group">
                                <label class="filter-label">
                                    <i class="fas fa-map-marker-alt"></i>Job Location
                                </label>
                                <asp:DropDownList ID="ddlCountry" runat="server" CssClass="modern-select"
                                    DataSourceID="SqlDataSource1" AppendDataBoundItems="True" 
                                    DataTextField="CountryName" DataValueField="CountryName" 
                                    OnSelectedIndexChanged="ddlCountry_SelectedIndexChanged">
                                    <asp:ListItem Value="0">Select Country</asp:ListItem>
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="SqlDataSource1" runat="server" 
                                    ConnectionString="<%$ ConnectionStrings:cs %>"
                                    SelectCommand="SELECT [CountryName] FROM [Country]">
                                </asp:SqlDataSource>
                            </div>

                            <!-- Job Type -->
                            <div class="filter-group">
                                <label class="filter-label">
                                    <i class="fas fa-building"></i>Job Type
                                </label>
                                <asp:RadioButtonList ID="RadioButtonListJobType" runat="server"
                                    RepeatDirection="Vertical" RepeatLayout="Table">
                                    <asp:ListItem Value="Any" Selected="True">Any Type</asp:ListItem>
                                    <asp:ListItem Value="Full Time">Full Time</asp:ListItem>
                                    <asp:ListItem Value="Part Time">Part Time</asp:ListItem>
                                    <asp:ListItem Value="Remote">Remote</asp:ListItem>
                                    <asp:ListItem Value="Freelance">Freelance</asp:ListItem>
                                </asp:RadioButtonList>
                            </div>

                            <!-- Posted Within -->
                            <div class="filter-group">
                                <label class="filter-label">
                                    <i class="fas fa-clock"></i>Posted Within
                                </label>
                                <asp:RadioButtonList ID="RadioButtonList1" runat="server"
                                    RepeatLayout="Table"
                                    RepeatDirection="Vertical">
                                    <asp:ListItem Value="Any" Selected="True">Any</asp:ListItem>
                                    <asp:ListItem Value="1">Today</asp:ListItem>
                                    <asp:ListItem Value="2">Last 2 days</asp:ListItem>
                                    <asp:ListItem Value="3">Last 7 days</asp:ListItem>
                                    <asp:ListItem Value="4">Last 30 days</asp:ListItem>
                                </asp:RadioButtonList>
                            </div>

                            <!-- Action Buttons -->
                            <div class="filter-actions">
                                <asp:LinkButton ID="lbFilter" runat="server" CssClass="btn-filter btn-filter-primary"
                                    OnClick="lbFilter_Click">
                                    <i class="fas fa-search"></i> Apply Filters
                                </asp:LinkButton>
                                <asp:LinkButton ID="lbReset" runat="server" CssClass="btn-filter btn-filter-secondary"
                                    OnClick="lbReset_Click">
                                    <i class="fas fa-redo"></i> Reset All
                                </asp:LinkButton>
                            </div>
                        </div>
                    </div>

                    <!-- Right Content -->
                    <div class="col-xl-9 col-lg-8 col-md-8">
                        <section class="featured-job-area">
                            <div class="container">
                                <!-- Job Count -->
                                <div class="row">
                                    <div class="col-lg-12">
                                        <div class="count-job mb-35">
                                            <asp:Label ID="lbljobCount" runat="server"></asp:Label>
                                        </div>
                                    </div>
                                </div>

                                <!-- Local Jobs -->
                                <asp:DataList ID="DataList1" runat="server" RepeatLayout="Flow">
                                    <ItemTemplate>
                                        <div class="single-job-items mb-30" style="border: 1px solid #ddd; padding: 15px; border-radius: 8px; display: flex; justify-content: space-between; align-items: center;">
                                            <div class="job-items">
                                                <div class="company-img">
                                                    <a href="JobDetails.aspx?id=<%# Eval("JobId") %>">
                                                        <img width="80" src="<%# GetImageUrl( Eval("CompanyImage")) %>" alt="">
                                                    </a>
                                                </div>
                                                <div class="job-tittle job-tittle2">
                                                    <a href="JobDetails.aspx?id=<%# Eval("JobId") %>">
                                                        <h5><%# Eval("Title") %></h5>
                                                    </a>
                                                    <ul>
                                                        <li><%# Eval("CompanyName") %></li>
                                                        <li><i class="fas fa-map-marker-alt"></i><%# Eval("State") %>, <%# Eval("Country") %></li>
                                                        <li><%# Eval("Salary") %></li>
                                                    </ul>
                                                </div>
                                            </div>
                                            <div class="items-link items-link2 f-right">
                                                <a href="JobDetails.aspx?id=<%# Eval("JobId") %>"><%# Eval("JobType") %></a>
                                                <span class="text-secondary">
                                                    <i class="fas fa-clock pr-1"></i>
                                                    <%# RelativeDate(Convert.ToDateTime(Eval("CreateDate"))) %>
                                                </span>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:DataList>
                            </div>
                        </section>
                    </div>
                </div>
            </div>
        </div>
    </main>
</asp:Content>