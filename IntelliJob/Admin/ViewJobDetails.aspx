<%@ Page Title="" Language="C#" MasterPageFile="~/Admin/AdminMaster.Master" AutoEventWireup="true" CodeBehind="ViewJobDetails.aspx.cs" Inherits="IntelliJob.Admin.ViewJobDetails" %>

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
                    <asp:HyperLink ID="linkViewApplicants" runat="server" CssClass="btn btn-primary">View All Applicants</asp:HyperLink>
                    <asp:HyperLink ID="linkBack" runat="server" NavigateUrl="~/Admin/JobList.aspx" CssClass="btn btn-secondary ml-2"> < Back </asp:HyperLink>
                </div>
            </div>
            <div class="row">
                <div class="col-12 pb-3">
                    <h3 class="text-center">Job Details</h3>
                </div>
            </div>
            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblJobTitle" style="font-weight: 600">Job Title</label>
                    <asp:Label ID="lblJobTitle" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
                <div class="col-md-6 pt-3">
                    <label for="lblNoOfPost" style="font-weight: 600">Number of Positions</label>
                    <asp:Label ID="lblNoOfPost" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
            </div>
            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-12 pt-3">
                    <label for="lblDescription" style="font-weight: 600">Description</label>
                    <asp:Label ID="lblDescription" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6; min-height: 100px; white-space: pre-wrap;"></asp:Label>
                </div>
            </div>
            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblQualification" style="font-weight: 600">Qualification/Education Required</label>
                    <asp:Label ID="lblQualification" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
                <div class="col-md-6 pt-3">
                    <label for="lblExperience" style="font-weight: 600">Experience Required</label>
                    <asp:Label ID="lblExperience" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
            </div>

            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblSpecialization" style="font-weight: 600">Specialization Required</label>
                    <asp:Label ID="lblSpecialization" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6; min-height: 80px; white-space: pre-wrap;"></asp:Label>
                </div>
                <div class="col-md-6 pt-3">
                    <label for="lblLastDate" style="font-weight: 600">Last Date To Apply</label>
                    <asp:Label ID="lblLastDate" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
            </div>
            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblSalary" style="font-weight: 600">Salary</label>
                    <asp:Label ID="lblSalary" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
                <div class="col-md-6 pt-3">
                    <label for="lblJobType" style="font-weight: 600">Job Type</label>
                    <asp:Label ID="lblJobType" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
            </div>

            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblCompany" style="font-weight: 600">Company/Organization Name</label>
                    <asp:Label ID="lblCompany" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
                <div class="col-md-6 pt-3">
                    <label for="imgCompanyLogo" style="font-weight: 600">Company/Organization Logo</label>
                    <div class="pt-2">
                        <asp:Image ID="imgCompanyLogo" runat="server" CssClass="img-thumbnail" style="max-height: 100px; max-width: 200px;" />
                    </div>
                </div>
            </div>

            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblWebsite" style="font-weight: 600">Website</label>
                    <asp:Label ID="lblWebsite" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
                <div class="col-md-6 pt-3">
                    <label for="lblEmail" style="font-weight: 600">Email</label>
                    <asp:Label ID="lblEmail" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
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
                <div class="col-md-6 pt-3">
                    <label for="lblState" style="font-weight: 600">State</label>
                    <asp:Label ID="lblState" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
            </div>

            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblPostedDate" style="font-weight: 600">Posted Date</label>
                    <asp:Label ID="lblPostedDate" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
            </div>

        </div>
    </div>
</asp:Content>

