<%@ Page Title="" Language="C#" MasterPageFile="~/Company/CompanyMaster.Master" AutoEventWireup="true" CodeBehind="EditJobDetails.aspx.cs" Inherits="IntelliJob.Company.EditJobDetails" %>

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
                    <asp:HyperLink ID="linkBack" runat="server" NavigateUrl="~/Company/JobList.aspx" CssClass="btn btn-secondary ml-2"> Back </asp:HyperLink>
                </div>
            </div>
            <div class="row">
                <div class="col-12 pb-3">
                    <h3 class="text-center">Edit Job Details</h3>
                </div>
            </div>
            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblJobTitle" style="font-weight: 600">Job Title</label>
<%--                    <asp:Label ID="lblJobTitle" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>--%>
                    <asp:TextBox ID="txtJobTitle" runat="server" CssClass="form-control" placeholder="E.g. Web Developer, App Developer" required></asp:TextBox>
                </div>
                <div class="col-md-6 pt-3">
                    <label for="lblNoOfPost" style="font-weight: 600">Number of Positions</label>
<%--                    <asp:Label ID="lblNoOfPost" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>--%>
                    <asp:TextBox ID="txtNoOfPost" runat="server" CssClass="form-control" placeholder="Enter Number of Position" TextMode="Number" required></asp:TextBox>
                    <asp:RangeValidator ID="RangeValidator1" runat="server" ControlToValidate="txtNoOfPost" Type="Integer" MinimumValue="0" MaximumValue="999999" ErrorMessage="Number of positions cannot be negative." ForeColor="Red"></asp:RangeValidator>

                </div>
            </div>
            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-12 pt-3">
                    <label for="lblDescription" style="font-weight: 600">Description</label>
<%--                    <asp:Label ID="lblDescription" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6; min-height: 100px; white-space: pre-wrap;"></asp:Label>--%>
                    <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" placeholder="Enter Job Description" TextMode="MultiLine" required></asp:TextBox>

                </div>
            </div>
            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblQualification" style="font-weight: 600">Qualification/Education Required</label>
<%--                    <asp:Label ID="lblQualification" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>--%>
                     <asp:TextBox ID="txtQualification" runat="server" CssClass="form-control" placeholder="E.g. BSCS, BSIT, BSSE" required></asp:TextBox>

                    </div>
                <div class="col-md-6 pt-3">
                    <label for="lblExperience" style="font-weight: 600">Experience Required</label>
<%--                    <asp:Label ID="lblExperience" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>--%>
                     <asp:TextBox ID="txtExperience" runat="server" CssClass="form-control" placeholder="E.g. 2 Years, 1.5 Years" required></asp:TextBox>

                    </div>
            </div>

            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblSpecialization" style="font-weight: 600">Specialization Required</label>
<%--                    <asp:Label ID="lblSpecialization" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6; min-height: 80px; white-space: pre-wrap;"></asp:Label>--%>
                      <asp:TextBox ID="txtSpecialization" runat="server" CssClass="form-control" placeholder="Enter Specialization"
                        TextMode="Multiline" required></asp:TextBox>

                    </div>
                <div class="col-md-6 pt-3">
                    <label for="lblLastDate" style="font-weight: 600">Last Date To Apply</label>
<%--                    <asp:Label ID="lblLastDate" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>--%>
                    <asp:TextBox ID="txtLastDate" runat="server" CssClass="form-control" placeholder="Enter Last Date To Apply"
                        TextMode="Date" required></asp:TextBox>

                    </div>
            </div>
            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblSalary" style="font-weight: 600">Salary</label>
<%--                    <asp:Label ID="lblSalary" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>--%>
                    <asp:TextBox ID="txtSalary" runat="server" CssClass="form-control" placeholder="E.g. 25000/Month, 7L/Year" required></asp:TextBox>

                </div>
                <div class="col-md-6 pt-3">
                    <label for="lblJobType" style="font-weight: 600">Job Type</label>
<%--                    <asp:Label ID="lblJobType" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>--%>
                    <asp:DropDownList ID="ddlJobType" runat="server" CssClass="form-control">
                        <asp:ListItem Value="0">Select Job Type</asp:ListItem>
                        <asp:ListItem>Full Time</asp:ListItem>
                        <asp:ListItem>Part Time</asp:ListItem>
                        <asp:ListItem>Remote</asp:ListItem>
                        <asp:ListItem>Freelance</asp:ListItem>
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator" runat="server" ErrorMessage="JobType Is Required" ForeColor="Red"
                        ControlToValidate="ddlJobType" InitialValue="0" Display="Dynamic" SetFocusOnError="true"></asp:RequiredFieldValidator>

                </div>
            </div>

            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblCompany" style="font-weight: 600">Company/Organization Name</label>
<%--                    <asp:Label ID="lblCompany" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>--%>
                     <asp:TextBox ID="txtCompany" runat="server" CssClass="form-control" placeholder="Enter Company/Organization Name" required></asp:TextBox>

                    </div>
                <div class="col-md-6 pt-3">
                    <label for="imgCompanyLogo" style="font-weight: 600">Company/Organization Logo</label>
                    <div class="pt-2">
                        <asp:Image ID="imgCompanyLogo" runat="server" CssClass="img-thumbnail" style="max-height: 100px; max-width: 200px;" />
                      <asp:FileUpload ID="fuCompanyLogo" runat="server" CssClass="form-control" ToolTip=".jpg, .jpeg, .png extension only" />

                        </div>
                </div>
            </div>

            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblWebsite" style="font-weight: 600">Website</label>
<%--                    <asp:Label ID="lblWebsite" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>--%>
                    <asp:TextBox ID="txtWebsite" runat="server" CssClass="form-control" placeholder="Enter Website"
                        TextMode="Url"></asp:TextBox>

                </div>
                <div class="col-md-6 pt-3">
                    <label for="lblEmail" style="font-weight: 600">Email</label>
<%--                    <asp:Label ID="lblEmail" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>--%>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" placeholder="Enter Email"
                        TextMode="Email"></asp:TextBox>

                </div>
            </div>

            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-12 pt-3">
                    <label for="lblAddress" style="font-weight: 600">Address</label>
<%--                    <asp:Label ID="lblAddress" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6; min-height: 80px; white-space: pre-wrap;"></asp:Label>--%>
                    <asp:TextBox ID="txtAddress" runat="server" CssClass="form-control" placeholder="Enter Work Location" TextMode="MultiLine" required></asp:TextBox>

                </div>
            </div>

            <div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblCountry" style="font-weight: 600">Country</label>
<%--                    <asp:Label ID="lblCountry" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>--%>
                    <asp:DropDownList ID="ddlCountry" runat="server" DataSourceID="SqlDataSource1" CssClass="form-control w-100" AppendDataBoundItems="true" DataTextField="CountryName" DataValueField="CountryName">
                        <asp:ListItem Value="0">Select Country</asp:ListItem>
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="Country is required" ForeColor="Red" Display="Dynamic" SetFocusOnError="true" Font-Size="Small" InitialValue="0" ControlToValidate="ddlCountry"></asp:RequiredFieldValidator>
                    <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:cs %>" SelectCommand="SELECT [CountryName] FROM [Country]"></asp:SqlDataSource>

                </div>
                <div class="col-md-6 pt-3">
                    <label for="lblState" style="font-weight: 600">State</label>
<%--                    <asp:Label ID="lblState" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>--%>
                    <asp:TextBox ID="txtState" runat="server" CssClass="form-control" placeholder="Enter State" required></asp:TextBox>

                </div>
            </div>

            <%--<div class="row mr-lg-5 ml-lg-5 mb-3">
                <div class="col-md-6 pt-3">
                    <label for="lblPostedDate" style="font-weight: 600">Posted Date</label>
                    <asp:Label ID="lblPostedDate" runat="server" CssClass="form-control" style="background-color: #f8f9fa; border: 1px solid #dee2e6;"></asp:Label>
                </div>
            </div>--%>
        </div>

        <div class="row mr-lg-5 ml-lg-5 mb-3 pt-4">
            <div class="col-md-3 col-md-offset-2 mb-3">
                <asp:Button ID="btnAdd" runat="server" CssClass="btn btn-primary btn-block" BackColor="#7200cf" Text="Update Job" 
                    onclick="btnAdd_Click"
                    />
    </div>
</div>
    </div>
</asp:Content>

