<%@ Page Title="" Language="C#" MasterPageFile="~/User/UserMaster.Master" AutoEventWireup="true" CodeBehind="JobDetails.aspx.cs" Inherits="IntelliJob.User.JobDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .job-overview-card {
            background: #ffffff;
            border-radius: 12px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
            padding: 30px;
            margin-bottom: 30px;
            border: 1px solid #e9ecef;
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

        .job-overview-header h4:before {
            content: "\f0b1";
            font-family: "Font Awesome 6 Free";
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

        .apply-btn-container .btn {
            width: 100%;
            padding: 16px 24px;
            background: linear-gradient(135deg, #FF4357 0%, #ff6b7a 100%);
            color: white !important;
            border: none;
            border-radius: 8px;
            font-size: 16px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            transition: all 0.3s ease;
            box-shadow: 0 4px 12px rgba(255, 67, 87, 0.3);
            text-decoration: none;
            display: block;
            text-align: center;
        }

        .apply-btn-container .btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 16px rgba(255, 67, 87, 0.4);
            text-decoration: none;
            color: white !important;
        }

        .apply-btn-container .btn:active {
            transform: translateY(0);
        }

        .apply-btn-container .btn:disabled {
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

        .company-info-header h4:before {
            content: "\f1ad";
            font-family: "Font Awesome 6 Free";
            font-weight: 900;
            color: #FF4357;
            margin-right: 12px;
            font-size: 20px;
        }

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
            color: #ff6b7a;
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
                                <h2> <%# jobTitle %></h2>
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
        <div class="job-post-company pt-120 pb-120">
            <div class="container">
                <asp:DataList ID="DataList1" runat="server" OnItemCommand="DataList1_ItemCommand" OnItemDataBound="DataList1_ItemDataBound">

                    <ItemTemplate>

                        <div class="row justify-content-between">
                            <!-- Left Content -->
                            <div class="col-xl-7 col-lg-8">
                                <!-- job single -->
                                <div class="single-job-items mb-50">
                                    <div class="job-items">
                                        <div class="company-img company-img-details">
                                            <a href="#">
                                                <img width="80" src="<%# GetImageUrl(Eval("CompanyImage")) %>" alt=""></a>
                                        </div>
                                        <div class="job-tittle">
                                            <a href="#">
                                                <h4><%# Eval("Title") %> </h4>
                                            </a>
                                            <ul>
                                                <li><%# Eval("CompanyName") %> </li>
                                                <li><i class="fas fa-map-marker-alt"></i><%# Eval("State") %>, <%# Eval("Country") %> </li>
                                                <li><%# Eval("Salary") %> </li>
                                            </ul>
                                        </div>
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
                                <div class="job-overview-card">
                                    <!-- Job Overview Header -->
                                    <div class="job-overview-header">
                                        <h4>Job Overview</h4>
                                    </div>
                                    <!-- Job Overview List -->
                                    <ul class="job-overview-list">
                                        <li>
                                            <span class="job-overview-label">
                                                <i class="fas fa-calendar-alt"></i>Posted date
                                            </span>
                                            <span class="job-overview-value">
                                                <%# DataBinder.Eval(Container.DataItem, "CreateDate", "{0:dd MMMM yyyy}") %>
                                            </span>
                                        </li>
                                        <li>
                                            <span class="job-overview-label">
                                                <i class="fas fa-map-marker-alt"></i>Location
                                            </span>
                                            <span class="job-overview-value">
                                                <%# Eval("State") %>
                                            </span>
                                        </li>
                                        <li>
                                            <span class="job-overview-label">
                                                <i class="fas fa-briefcase"></i>Vacancy
                                            </span>
                                            <span class="job-overview-value">
                                                <%# Eval("NoOfPost") %>
                                            </span>
                                        </li>
                                        <li>
                                            <span class="job-overview-label">
                                                <i class="fas fa-clock"></i>Job nature
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
                                        <asp:LinkButton ID="lbApplyJob" runat="server" CssClass="btn" Text="Apply Now" CommandName="ApplyJob"></asp:LinkButton>
                                    </div>
                                </div>






                                <div class="company-info-card">
                                    <!-- Company Information Header -->
                                    <div class="company-info-header">
                                        <h4>Company Information</h4>
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
        <!-- job post company End -->

    </main>
</asp:Content>
