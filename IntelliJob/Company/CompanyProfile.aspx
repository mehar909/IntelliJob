<%@ Page Title="Company Profile" Language="C#" MasterPageFile="~/Company/CompanyMaster.Master"
    AutoEventWireup="true" CodeBehind="CompanyProfile.aspx.cs"
    Inherits="IntelliJob.Company.CompanyProfile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.12.1/css/all.min.css" />
<style>
    .card {
        background-color: #fff;
        border-radius: 10px;
        border: none;
        position: relative;
        margin-bottom: 30px;
        box-shadow: 0 0.46875rem 2.1875rem rgba(90,97,105,0.1), 0 0.9375rem 1.40625rem rgba(90,97,105,0.1), 0 0.25rem 0.53125rem rgba(90,97,105,0.12), 0 0.125rem 0.1875rem rgba(90,97,105,0.1);
    }

    .l-bg-cherry {
        background: linear-gradient(to right, #493240, #f09) !important;
        color: #fff;
    }

    .l-bg-blue-dark {
        background: linear-gradient(to right, #373b44, #4286f4) !important;
        color: #fff;
    }

    .l-bg-green-dark {
        background: linear-gradient(to right, #0a504a, #38ef7d) !important;
        color: #fff;
    }

    .l-bg-orange-dark {
        background: linear-gradient(to right, #a86008, #ffba56) !important;
        color: #fff;
    }

    .card .card-statistic-3 .card-icon-large .fas, .card .card-statistic-3 .card-icon-large .far, .card .card-statistic-3 .card-icon-large .fab, .card .card-statistic-3 .card-icon-large .fal {
        font-size: 110px;
    }

    .card .card-statistic-3 .card-icon {
        text-align: center;
        line-height: 50px;
        margin-left: 15px;
        color: #000;
        position: absolute;
        right: -5px;
        top: 20px;
        opacity: 0.1;
    }

    .l-bg-cyan {
        background: linear-gradient(135deg, #289cf5, #84c0ec) !important;
        color: #fff;
    }

    .l-bg-green {
        background: linear-gradient(135deg, #23bdb8 0%, #43e794 100%) !important;
        color: #fff;
    }

    .l-bg-red {
        background: linear-gradient(to right, #ff4e50, #f00000) !important;
        color: #fff;
    }

    .l-bg-orange {
        background: linear-gradient(to right, #f9900e, #ffba56) !important;
        color: #fff;
    }

    .l-bg-cyan {
        background: linear-gradient(135deg, #289cf5, #84c0ec) !important;
        color: #fff;
    }

    .l-bg-red-dark {
        background: linear-gradient(to right, #8e0e00, #e52e71) !important;
        color: #fff;
    }
</style>


</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="container pt-5 pb-5">
        <h2 class="text-center">Company Profile</h2>

        <asp:Label ID="lblMsg" runat="server" Visible="false"></asp:Label>

        <div class="card p-4 shadow">

            <div class="card">
                <div class="card-body">
                    <div class="d-flex flex-column align-items-center text-center">
                        <asp:Image ID="Image1" runat="server" Width="150px" Height="150px" CssClass="rounded-circle" />
                        <%--<img src="../photos/<%# Eval("photo") %>" class="rounded-circle" width="150" />--%>
                        <div class="mt-3">
                            <asp:Label ID="txtCompanyNameHeader" runat="server" CssClass="text-capitalize h4"></asp:Label>
                        </div>

                    </div>
                </div>
            </div>


            <!-- Company Name -->
            <div class="form-group">
                <label>Company Name</label>
                <asp:TextBox ID="txtCompanyName" runat="server" CssClass="form-control" required></asp:TextBox>
            </div>

            <!-- Website -->
            <div class="form-group mt-3">
                <label>Website</label>
                <asp:TextBox ID="txtWebsite" runat="server" CssClass="form-control"></asp:TextBox>
            </div>

            <!-- Description -->
            <div class="form-group mt-3">
                <label>Description</label>
                <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="4"
                    CssClass="form-control"></asp:TextBox>
            </div>

            <!-- Company Size -->
            <div class="form-group mt-3">
                <label>Company Size</label>
                <asp:TextBox ID="txtCompanySize" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
            </div>

            <!-- Address -->
            <div class="form-group mt-3">
                <label>Address</label>
                <asp:TextBox ID="txtAddress" runat="server" TextMode="MultiLine" Rows="3"
                    CssClass="form-control"></asp:TextBox>
            </div>

            <!-- Country -->
            <div class="form-group mt-3">
                <label>Country</label>
                <asp:DropDownList ID="ddlCountry" runat="server" CssClass="form-control"
                    DataSourceID="SqlDataSource1" AppendDataBoundItems="true"
                    DataTextField="CountryName" DataValueField="CountryName">
                    <asp:ListItem Value="">Select Country</asp:ListItem>
                </asp:DropDownList>

                <asp:SqlDataSource ID="SqlDataSource1" runat="server"
                    ConnectionString="<%$ ConnectionStrings:cs %>"
                    SelectCommand="SELECT CountryName FROM Country"></asp:SqlDataSource>
            </div>

            <!-- Logo -->
            <div class="form-group mt-3">
                <label>Company Logo</label><br />
                <asp:Image ID="imgLogo" runat="server" Width="120px" Height="120px" CssClass="mb-2" />
                <asp:FileUpload ID="fuLogo" runat="server" CssClass="form-control mt-2" />
            </div>

            <asp:Button ID="btnUpdate" runat="server" Text="Update Profile"
                CssClass="btn btn-primary mt-4" OnClick="btnUpdate_Click" />

        </div>
    </div>

</asp:Content>
