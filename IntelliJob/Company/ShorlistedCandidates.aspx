<%@ Page Title="" Language="C#" MasterPageFile="~/Company/CompanyMaster.Master" EnableEventValidation="false" AutoEventWireup="true" CodeBehind="ShorlistedCandidates.aspx.cs" Inherits="IntelliJob.Company.ShortlistedCandidates" %>

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

    .large-checkbox {
        /* Use scale to increase the size without changing its bounding box, 
           ensuring it stays centered */
        transform: scale(2); /* 1.5 times the original size */
        margin: 5px; /* Add some margin for spacing */
        margin-right: 20px;
        /* For older browsers */
        -ms-transform: scale(1.5);
        -webkit-transform: scale(1.5);
    }

    /* To ensure the increased checkbox is aligned correctly (optional, but good practice) */
    .large-checkbox input[type='checkbox'] {
        vertical-align: middle;
    }
</style>


</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div style="background-image: url('../Images/bg.jpg'); width: 100%; height: 720px; background-repeat: no-repeat; background-size: cover; background-attachment: fixed;">
        <div class="container-fluid pt -4 pb-4">
            <div>
                <asp:Label ID="lblMsg" runat="server"></asp:Label>
            </div>

            <h3 class="text-center">Shortlisted Candidates</h3>

            <div class="row mb-3 pt-sm-3">
                <div class="col-md-12">
                    <asp:GridView ID="GridView1" runat="server" CssClass="table table-hover table-bordered"
                        EmptyDataText="No Record to Display..!" AllowPaging="True" PageSize="5"
                        OnPageIndexChanging="GridView1_PageIndexChanging" DataKeyNames="AppliedJobId" OnRowDeleting="GridView1_RowDeleting"
                        OnRowDataBound="GridView1_RowDataBound" OnSelectedIndexChanged="GridView1_SelectedIndexChanged" OnRowCommand="GridView1_RowCommand" AutoGenerateColumns="False">
                        <Columns>

                            <asp:BoundField DataField="Sr.No" HeaderText="Sr.No">
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>


                            <asp:BoundField DataField="CompanyName" HeaderText="Company Name">
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>


                            <asp:BoundField DataField="Title" HeaderText="Job Title">
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <asp:BoundField DataField="UserName" HeaderText="User Name">
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <asp:TemplateField HeaderText="User Email">
                                <ItemTemplate>
                                    <%# Eval("Email") %>
                                    <asp:HiddenField ID="hdnEmail" runat="server" Value='<%# Eval("Email") %>' />
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:TemplateField>


                            <asp:BoundField DataField="Mobile" HeaderText="User Mobile no.">
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <asp:TemplateField HeaderText="Resume">
                                <ItemTemplate>

                                    <asp:PlaceHolder ID="phResumeLink" runat="server"
                                        Visible='<%# !string.IsNullOrEmpty(Eval("Resume").ToString()) %>'>
                                        <asp:HyperLink 
                                            ID="lnkResume" runat="server"
                                            Text="View Resume"
                                            NavigateUrl='<%# "../" + Eval("Resume") %>' 
                                            Target="_blank"
                                            ForeColor="Blue" />
                                    </asp:PlaceHolder>

                                    <asp:PlaceHolder ID="phNoResume" runat="server"
                                        Visible='<%# string.IsNullOrEmpty(Eval("Resume").ToString()) %>'>
                                        <span style="color: gray;">Not uploaded</span>
                                    </asp:PlaceHolder>

                                    <asp:HiddenField ID="hdnJobId" runat="server" Value='<%# Eval("JobId") %>' />
                                </ItemTemplate>

                                <ItemStyle HorizontalAlign="Center" />
                            </asp:TemplateField>

                           <%-- <asp:TemplateField HeaderText="Select">
                                <ItemTemplate>
                                    <asp:CheckBox ID="chkSelect" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>--%>

                            <asp:TemplateField HeaderText="Mail for Interview">
                                <ItemTemplate>
                                    <div style="display: flex; flex-direction: row; align-items: center; gap: 5px;">
                                        <asp:CheckBox ID="chkSelect" runat="server" CssClass="large-checkbox" />
            
                                        <asp:LinkButton ID="lnkMailSingle" runat="server"
                                            CommandName="SendSingleMail"
                                            CommandArgument='<%# Eval("JobId") %>'
                                            ToolTip="Send Interview Mail to this Candidate"
                                            Style="color: #7200cf; font-weight: bold;">
                                            Mail Now
                                        </asp:LinkButton>
                                    </div>
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Job Details">
                                <ItemTemplate>
                                    <asp:ImageButton 
                                        ID="btnJobDetails" runat="server" Text="Job Details" 
                                        Width="26px"
                                        Height="28px"
                                        ImageUrl="~/assets/img/icon/details.jpg"
                                        CommandName="ViewJob" CommandArgument='<%# Eval("JobId") %>' />
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:TemplateField>

                            <asp:CommandField CausesValidation="false" HeaderText="Remove" ShowDeleteButton="true"
                                DeleteImageUrl="../assets/img/icon/trashIcon.jpg" ButtonType="Image">
                                <ControlStyle Height="25px" Width="25px" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:CommandField>

                        </Columns>

                        <HeaderStyle BackColor="#7200cf" ForeColor="White" />
                    </asp:GridView>
                </div>
            </div>
            <div class="row mr-lg-5 ml-lg-5 mb-3 pt-4">

               <%-- <div class="col-md-3 col-md-offset-2 mb-3">
                    <asp:Button ID="btnAdd" runat="server" CssClass="btn btn-primary btn-block" BackColor="#7200cf" Text="Rest Resumes"
                        OnClick="btnAdd_Click" />
                </div>--%>

                <div class="col-md-9"></div>

                <div class="col-md-3 col-md-offset-2 mb-3">
                    <asp:Button ID="Button1" runat="server" CssClass="btn btn-primary btn-block" BackColor="#7200cf" Text="Mail All Selected"
                        OnClick="Button1_Click" />
                </div>
            </div>

        </div>
    </div>
</asp:Content>
