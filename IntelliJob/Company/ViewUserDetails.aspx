<%@ Page Title="" Language="C#" MasterPageFile="~/Company/CompanyMaster.Master" AutoEventWireup="true" CodeBehind="ViewUserDetails.aspx.cs" Inherits="IntelliJob.Company.ViewUserDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div style="background-image: url('../Images/bg.jpg'); width: 100%; height: 720px; background-repeat: no-repeat; background-size: cover; background-attachment: fixed;">
        <div class="container pt-4 pb-4">
            <div>
                <asp:Label ID="lblMsg" runat="server"></asp:Label>
            </div>
            <div class="btn-toolbar justify-content-between mb-3">
                <div class="btn-group">
                    <asp:Label ID="Label1" runat="server"></asp:Label>
                </div>
                <div class="input-group h-25">
                    <asp:HyperLink ID="linkBack" runat="server" NavigateUrl="~/Company/Applicants.aspx" CssClass="btn btn-secondary"> Back </asp:HyperLink>
                </div>
            </div>
            <div class="row">
                <div class="col-12 pb-3">
                    <h3 class="text-center">User Profile Details</h3>
                </div>
            </div>
            
            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-12 pt-3 text-center">
                    <asp:Image ID="imgUserPhoto" runat="server" CssClass="rounded-circle" Width="150" Height="150" style="object-fit: cover;" />
                </div>
            </div>

            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblUsername" style="font-weight: 600">Username</label>
                    <asp:Label ID="lblUsername" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
                <div class="col-md-6 pt-3">
                    <label for="lblName" style="font-weight: 600">Full Name</label>
                    <asp:Label ID="lblName" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
            </div>

            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblEmail" style="font-weight: 600">Email</label>
                    <asp:Label ID="lblEmail" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
                <div class="col-md-6 pt-3">
                    <label for="lblMobile" style="font-weight: 600">Mobile Number</label>
                    <asp:Label ID="lblMobile" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
            </div>

            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-12 pt-3">
                    <label for="lblAddress" style="font-weight: 600">Address</label>
                    <asp:Label ID="lblAddress" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6; min-height: 80px; white-space: pre-wrap;"></asp:Label>
                </div>
            </div>

            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblCountry" style="font-weight: 600">Country</label>
                    <asp:Label ID="lblCountry" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
            </div>

            <h4 class="text-center mt-4 mb-3">Educational Qualifications</h4>

            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblTenthGrade" style="font-weight: 600">10th Grade</label>
                    <asp:Label ID="lblTenthGrade" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
                <div class="col-md-6 pt-3">
                    <label for="lblTwelfthGrade" style="font-weight: 600">12th Grade</label>
                    <asp:Label ID="lblTwelfthGrade" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
            </div>

            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblGraduationGrade" style="font-weight: 600">Graduation Grade</label>
                    <asp:Label ID="lblGraduationGrade" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
                <div class="col-md-6 pt-3">
                    <label for="lblPostGraduationGrade" style="font-weight: 600">Post Graduation Grade</label>
                    <asp:Label ID="lblPostGraduationGrade" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
            </div>

            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblPhd" style="font-weight: 600">Ph.D.</label>
                    <asp:Label ID="lblPhd" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
            </div>

            <h4 class="text-center mt-4 mb-3">Professional Information</h4>

            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblWorksOn" style="font-weight: 600">Works On</label>
                    <asp:Label ID="lblWorksOn" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6; min-height: 80px; white-space: pre-wrap;"></asp:Label>
                </div>
                <div class="col-md-6 pt-3">
                    <label for="lblExperience" style="font-weight: 600">Experience</label>
                    <asp:Label ID="lblExperience" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6; min-height: 80px; white-space: pre-wrap;"></asp:Label>
                </div>
            </div>

            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-12 pt-3">
                    <label for="lblResume" style="font-weight: 600">Resume</label>
                    <div class="pt-2">
                        <asp:HyperLink ID="lnkResume" runat="server" Target="_blank" CssClass="btn" style="background-color: #7200cf; color: white; border: none;" Visible="false">View Resume</asp:HyperLink>
                        <asp:Label ID="lblNoResume" runat="server" Text="Not uploaded" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;" Visible="false"></asp:Label>
                    </div>
                </div>
            </div>

        </div>
    </div>
</asp:Content>

