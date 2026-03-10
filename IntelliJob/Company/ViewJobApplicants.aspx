<%@ Page Title="" Language="C#" MasterPageFile="~/Company/CompanyMaster.Master" AutoEventWireup="true" CodeBehind="ViewJobApplicants.aspx.cs" Inherits="IntelliJob.Company.ViewJobApplicants" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta name="google" content="notranslate" />
    <meta http-equiv="Content-Language" content="en" />
    <script type="text/javascript">
        function hideMessage() {
            var lblMsg = document.getElementById('<%= lblMsg.ClientID %>');
            if (lblMsg && lblMsg.textContent.trim() !== '') {
                setTimeout(function () {
                    if (lblMsg) {
                        lblMsg.style.display = 'none';
                    }
                }, 5000); // Hide after 5 seconds
            }
        }
        window.onload = hideMessage;
        // Also call after postback
        if (window.addEventListener) {
            window.addEventListener('load', hideMessage, false);
        } else if (window.attachEvent) {
            window.attachEvent('onload', hideMessage);
        }
    </script>
    <style>
        .row.mb-3.pt-sm-3 .table-hover tbody tr td {
            padding: 8px !important;
            vertical-align: middle !important;
        }
        .row.mb-3.pt-sm-3 .table-hover tbody tr {
            height: auto !important;
            line-height: 1.2 !important;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div style="background-image: url('../Images/bg.jpg'); width: 100%; height: 720px; background-repeat: no-repeat; background-size: cover; background-attachment: fixed;">
        <div class="container-fluid pt-4 pb-4">
            <div class="mb-0">
                <asp:Label ID="lblMsg" runat="server"></asp:Label>
            </div>
            <div class="btn-toolbar justify-content-between mb-3">
                <div class="btn-group">
                    <asp:Label ID="Label1" runat="server"></asp:Label>
                </div>
                <div class="input-group h-25">
                    <asp:HyperLink ID="linkBack" runat="server" CssClass="btn btn-secondary"> Back </asp:HyperLink>
                </div>
            </div>
            <div class="row">
                <div class="col-12 pb-3">
                    <h3 class="text-center">Job Applicants</h3>
                </div>
            </div>

            <div class="row mb-3 pt-sm-3">
                <div class="col-md-12">
                    <asp:GridView ID="GridView1" runat="server" CssClass="table table-hover table-bordered" HeaderStyle-HorizontalAlign="Center"
                        EmptyDataText="No Applicants Found..!" AllowPaging="True" PageSize="5"
                        OnPageIndexChanging="GridView1_PageIndexChanging" OnRowDataBound="GridView1_RowDataBound" DataKeyNames="UserId,AppliedJobId" AutoGenerateColumns="False">
                        <Columns>

                            <asp:BoundField DataField="Sr.No" HeaderText="Sr.No">
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <asp:BoundField DataField="UserName" HeaderText="User Name">
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <asp:BoundField DataField="Email" HeaderText="Email">
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <asp:BoundField DataField="Mobile" HeaderText="Mobile No.">
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <asp:BoundField DataField="Country" HeaderText="Country">
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <asp:TemplateField HeaderText="View">
                                <ItemTemplate>
                                    <asp:HyperLink ID="lnkViewUser" runat="server" NavigateUrl='<%# "ViewUserDetails.aspx?id=" + Eval("UserId") %>'>
                                        <asp:Image ID="Img" runat="server" ImageUrl="../assets/img/icon/view.png" Height="22px" Width="22px" />
                                    </asp:HyperLink>
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Center" Width="50px" />
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Shortlist Candidate">
                                <ItemTemplate>
                                    <asp:ImageButton
                                        ID="btnShortlist"
                                        runat="server"
                                        ImageUrl="../assets/img/icon/tick.jpg"
                                        Width="25px"
                                        Height="25px"
                                        CommandName="Shortlist"
                                        OnClick="btnShortlist_Click"
                                        ToolTip="Shortlist this candidate" />
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Center" Width="50px" />
                            </asp:TemplateField>

                        </Columns>

                        <HeaderStyle BackColor="#7200cf" ForeColor="White" />
                    </asp:GridView>
                </div>
            </div>


        </div>
    </div>
</asp:Content>

