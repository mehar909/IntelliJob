<%@ Page Title="" Language="C#" MasterPageFile="~/User/UserMaster.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="IntelliJob.User.Home" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">


    <main>

        <!-- slider Area Start-->
        <div class="slider-area ">
            <!-- Mobile Menu -->
            <div class="slider-active">
                <div class="single-slider slider-height d-flex align-items-center" data-background="../assets/img/hero/h1_hero.jpg">
                    <div class="container">
                        <div class="row">
                            <div class="col-xl-6 col-lg-9 col-md-10">
                                <div class="hero__caption">
                                    <h1>Find the most exciting startup jobs</h1>
                                </div>
                            </div>
                        </div>
                        <!-- Search Box -->
                        <div class="row">
                            <div class="col-xl-8">
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <!-- slider Area End-->



        <!-- Our Services End -->
        <!-- Online CV Area Start -->
        <div class="online-cv cv-bg section-overly pt-90 pb-120" data-background="../assets/img/gallery/cv_bg.jpg">
            <div class="container">
                <div class="row justify-content-center">
                    <div class="col-xl-10">
                        <div class="cv-caption text-center">
                            <p class="pera1">FEATURED TOURS Packages</p>
                            <p class="pera2">Make a Difference with Your Online Resume!</p>
                            <%--                            <a href="#" class="border-btn2 border-btn4">Upload your cv</a>--%>

                            <%--                            <div class="main-body">
                                <asp:DataList ID="lbRegisterOrResume" runat="server" Width="100%" OnItemCommand="lbRegisterOrResume_ItemCommand">
                                    <ItemTemplate>
                                    </ItemTemplate>
                                </asp:DataList>
                                <div class="form-group mt-3">
                                    <asp:LinkButton ID="RegorRes" runat="server" CssClass="button button-contactForm boxed-btn" CausesValidation="false" OnClick="RegorRes_Click"></asp:LinkButton>
                                    <%--                                            <asp:Button ID="btnEdit" runat="server" Text="edit" CssClass="button button-contactForm boxed-btn" CommandName="EditUserProfile" CommandArgument='<%# Eval("UserId") %>' />--%>
                            <%--                        </div>
                    </div>--%>
                            <asp:DataList ID="lbRegisterOrResume" runat="server" Width="100%" OnItemCommand="lbRegisterOrResume_ItemCommand">
                                <ItemTemplate>
                                    <asp:Button ID="btnUploadCV" runat="server" Text="Upload Your CV" CssClass="button button-contactForm boxed-btn"
                                        CommandName="EditUserProfile" CommandArgument='<%# Eval("UserId") %>' />
                                    <!-- Add other labels for displaying additional details like country, etc. -->
                                </ItemTemplate>
                            </asp:DataList>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <!-- Online CV Area End-->
        <!-- Featured_job_start -->
        <section class="featured-job-area feature-padding">
            <div class="container">
                <!-- Section Tittle -->
                <div class="row">
                    <div class="col-lg-12">
                        <div class="section-tittle text-center">
                            <span>Recent Job</span>
                            <h2>Featured Jobs</h2>
                        </div>
                    </div>
                </div>
                <div class="row justify-content-center">
                    <div class="col-xl-10">
                        <asp:Repeater ID="RptrFeaturedJobs" runat="server">
                        <ItemTemplate>
                            <div class="single-job-items mb-30">
                                <div class="job-items">
                                    <div class="company-img">
                                        <a href='JobDetails.aspx?id=<%# Eval("JobId") %>'>
                                            <img src='../Images/CompanyLogo/<%# Eval("CompanyImage") %>' 
                                                 alt='<%# Eval("CompanyName") %>' 
                                                 style="max-width: 60px; max-height: 60px;">
                                        </a>
                                    </div>
                                    <div class="job-tittle">
                                        <a href='JobDetails.aspx?id=<%# Eval("JobId") %>'>
                                            <h4><%# Eval("Title") %></h4>
                                        </a>
                                        <ul>
                                            <li><%# Eval("CompanyName") %></li>
                                            <li><i class="fas fa-map-marker-alt"></i><%# Eval("Address") %>, <%# Eval("Country") %></li>
                                            <%-- You can add Salary back here if your SQL fetched it, e.g.: <li>$<%# Eval("Salary") %></li> --%>
                                            <%-- If salary is not needed, remove or replace with another field --%>
                                        </ul>
                                    </div>
                                </div>
                                <div class="items-link f-right">
                                    <a href='JobDetails.aspx?id=<%# Eval("JobId") %>'><%# Eval("JobType") %></a>
                                    <%-- Call the C# helper function GetTimeAgo --%>
                                    <span><%# GetTimeAgo(Eval("CreateDate")) %></span>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:Literal ID="litNoJobsMessage" runat="server" Visible="false" />
                </div>
                   <%--<div class="col-xl-10">
                        <!-- single-job-content -->
                        <div class="single-job-items mb-30">
                            <div class="job-items">
                                <div class="company-img">
                                    <a href="JobDetails.aspx">
                                        <img src="../assets/img/icon/job-list1.png" alt=""></a>
                                </div>
                                <div class="job-tittle">
                                    <a href="JobDetails.aspx">
                                        <h4>Digital Marketer</h4>
                                    </a>
                                    <ul>
                                        <li>Creative Agency</li>
                                        <li><i class="fas fa-map-marker-alt"></i>Athens, Greece</li>
                                        <li>$3500 - $4000</li>
                                    </ul>
                                </div>
                            </div>
                            <div class="items-link f-right">
                                <a href="JobDetails.aspx">Full Time</a>
                                <span>7 hours ago</span>
                            </div>
                        </div>
                        <!-- single-job-content -->
                        <div class="single-job-items mb-30">
                            <div class="job-items">
                                <div class="company-img">
                                    <a href="JobDetails.aspx">
                                        <img src="../assets/img/icon/job-list2.png" alt=""></a>
                                </div>
                                <div class="job-tittle">
                                    <a href="JobDetails.aspx">
                                        <h4>Digital Marketer</h4>
                                    </a>
                                    <ul>
                                        <li>Creative Agency</li>
                                        <li><i class="fas fa-map-marker-alt"></i>Athens, Greece</li>
                                        <li>$3500 - $4000</li>
                                    </ul>
                                </div>
                            </div>
                            <div class="items-link f-right">
                                <a href="JobDetails.aspx">Full Time</a>
                                <span>7 hours ago</span>
                            </div>
                        </div>
                        <!-- single-job-content -->
                        <div class="single-job-items mb-30">
                            <div class="job-items">
                                <div class="company-img">
                                    <a href="JobDetails.aspx">
                                        <img src="../assets/img/icon/job-list3.png" alt=""></a>
                                </div>
                                <div class="job-tittle">
                                    <a href="JobDetails.aspx">
                                        <h4>Digital Marketer</h4>
                                    </a>
                                    <ul>
                                        <li>Creative Agency</li>
                                        <li><i class="fas fa-map-marker-alt"></i>Athens, Greece</li>
                                        <li>$3500 - $4000</li>
                                    </ul>
                                </div>
                            </div>
                            <div class="items-link f-right">
                                <a href="JobDetails.aspx">Full Time</a>
                                <span>7 hours ago</span>
                            </div>
                        </div>
                        <!-- single-job-content -->
                        <div class="single-job-items mb-30">
                            <div class="job-items">
                                <div class="company-img">
                                    <a href="JobDetails.aspx">
                                        <img src="../assets/img/icon/job-list4.png" alt=""></a>
                                </div>
                                <div class="job-tittle">
                                    <a href="JobDetails.aspx">
                                        <h4>Digital Marketer</h4>
                                    </a>
                                    <ul>
                                        <li>Creative Agency</li>
                                        <li><i class="fas fa-map-marker-alt"></i>Athens, Greece</li>
                                        <li>$3500 - $4000</li>
                                    </ul>
                                </div>
                            </div>
                            <div class="items-link f-right">
                                <a href="JobDetails.aspx">Full Time</a>
                                <span>7 hours ago</span>
                            </div>
                        </div>
                    </div>--%>
                </div>
            </div>
            <!-- More Btn -->
            <!-- Section Button -->
            <div class="row">
                <div class="col-lg-12">
                    <div class="browse-btn2 text-center mt-50">
                        <a href="JobListing.aspx" class="border-btn2">Browse All Sectors</a>
                    </div>
                </div>
            </div>
        </section>
        <!-- Featured_job_end -->
       
        <!-- Testimonial Start -->
        <div class="testimonial-area testimonial-padding">
            <div class="container">
                <!-- Testimonial contents -->
                <div class="row d-flex justify-content-center">
                    <div class="col-xl-8 col-lg-8 col-md-10">
                        <div class="h1-testimonial-active dot-style">
                            <!-- Single Testimonial -->
                            <div class="single-testimonial text-center">
                                <!-- Testimonial Content -->
                                <div class="testimonial-caption ">
                                    <!-- founder -->
                                    <div class="testimonial-founder  ">
                                        <div class="founder-img mb-30">
                                            <img src="assets/img/testmonial/testimonial-founder.png" alt="">
                                            <span>Margaret Lawson</span>
                                            <p>Creative Director</p>
                                        </div>
                                    </div>
                                    <div class="testimonial-top-cap">
                                        <p>“I am at an age where I just want to be fit and healthy our bodies are our responsibility! So start caring for your body and it will care for you. Eat clean it will care for you and workout hard.”</p>
                                    </div>
                                </div>
                            </div>
                            <!-- Single Testimonial -->
                            <div class="single-testimonial text-center">
                                <!-- Testimonial Content -->
                                <div class="testimonial-caption ">
                                    <!-- founder -->
                                    <div class="testimonial-founder  ">
                                        <div class="founder-img mb-30">
                                            <img src="../assets/img/testmonial/testimonial-founder.png" alt="">
                                            <span>Margaret Lawson</span>
                                            <p>Creative Director</p>
                                        </div>
                                    </div>
                                    <div class="testimonial-top-cap">
                                        <p>“I am at an age where I just want to be fit and healthy our bodies are our responsibility! So start caring for your body and it will care for you. Eat clean it will care for you and workout hard.”</p>
                                    </div>
                                </div>
                            </div>
                            <!-- Single Testimonial -->
                            <div class="single-testimonial text-center">
                                <!-- Testimonial Content -->
                                <div class="testimonial-caption ">
                                    <!-- founder -->
                                    <div class="testimonial-founder  ">
                                        <div class="founder-img mb-30">
                                            <img src="../assets/img/testmonial/testimonial-founder.png" alt="">
                                            <span>Margaret Lawson</span>
                                            <p>Creative Director</p>
                                        </div>
                                    </div>
                                    <div class="testimonial-top-cap">
                                        <p>“I am at an age where I just want to be fit and healthy our bodies are our responsibility! So start caring for your body and it will care for you. Eat clean it will care for you and workout hard.”</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <!-- Testimonial End -->
        <!-- Support Company Start-->
        <div class="support-company-area support-padding fix">
            <div class="container">
                <div class="row align-items-center">
                    <div class="col-xl-6 col-lg-6">
                        <div class="right-caption">
                            <!-- Section Tittle -->
                            <div class="section-tittle section-tittle2">
                                <span>What we are doing</span>
                                <h2>24k Talented people are getting Jobs</h2>
                            </div>
                            <div class="support-caption">
                                <p class="pera-top">
                                    Welcome to Online Job Portal, where we connect talented job seekers with leading employers for a seamless career journey. Discover your dream job with us today!
                                    <p>At this portal, we offer a comprehensive platform with the latest job listings and company profiles to help you find the perfect match. Whether you're advancing your career or starting fresh, our resources and support are here to guide you every step of the way. Join us and unlock your potential!</p>
                                    <a href="JobListing.aspx" class="btn post-btn">Search for job</a>
                            </div>
                        </div>
                    </div>
                    <div class="col-xl-6 col-lg-6">
                        <div class="support-location-img">
                            <img src="../assets/img/service/support-img.jpg" alt="">
                            <div class="support-img-cap text-center">
                                <p>Since</p>
                                <span>1994</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <!-- Support Company End-->
        <!-- Blog Area Start -->
        <div class="home-blog-area blog-h-padding">
            <div class="container">
                <!-- Section Tittle -->
                <div class="row">
                    <div class="col-lg-12">
                        <div class="section-tittle text-center">
                            <span>Our latest blog</span>
                            <h2>Our recent news</h2>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-xl-6 col-lg-6 col-md-6">
                        <div class="home-blog-single mb-30">
                            <div class="blog-img-cap">
                                <div class="blog-img">
                                    <img src="../assets/img/blog/home-blog1.jpg" alt="">
                                    <!-- Blog date -->
                                    <div class="blog-date text-center">
                                        <span>24</span>
                                        <p>Now</p>
                                    </div>
                                </div>
                                <div class="blog-cap">
                                    <p>|   Properties</p>
                                    <h3><a href="https://earlycareersfoundation.org/?gad_source=1&gclid=CjwKCAjwupGyBhBBEiwA0UcqaFrhsugsNwdHep28A6x89PjsX9JwJ64qdEqfFuwE_WTNYRgIzIfgxxoCkREQAvD_BwE">Early Careers.org</a></h3>
                                    <a href="https://earlycareersfoundation.org/?gad_source=1&gclid=CjwKCAjwupGyBhBBEiwA0UcqaFrhsugsNwdHep28A6x89PjsX9JwJ64qdEqfFuwE_WTNYRgIzIfgxxoCkREQAvD_BwE" class="more-btn">Read more »</a>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-xl-6 col-lg-6 col-md-6">
                        <div class="home-blog-single mb-30">
                            <div class="blog-img-cap">
                                <div class="blog-img">
                                    <img src="../assets/img/blog/home-blog2.jpg" alt="">
                                    <!-- Blog date -->
                                    <div class="blog-date text-center">
                                        <span>24</span>
                                        <p>Now</p>
                                    </div>
                                </div>
                                <div class="blog-cap">
                                    <p>|   Properties</p>
                                    <h3><a href="https://twincitiesrise.org/job-seekers/?gad_source=1&gclid=CjwKCAjwupGyBhBBEiwA0UcqaGZFQW2YXpDTPOh0sFxqjvRWB43hImyu7EePKljY3RdYpEariNEPnRoCdZAQAvD_BwE">Twin City Rise</a></h3>
                                    <a href="https://twincitiesrise.org/job-seekers/?gad_source=1&gclid=CjwKCAjwupGyBhBBEiwA0UcqaGZFQW2YXpDTPOh0sFxqjvRWB43hImyu7EePKljY3RdYpEariNEPnRoCdZAQAvD_BwE" class="more-btn">Read more »</a>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <!-- Blog Area End -->

    </main>

</asp:Content>
