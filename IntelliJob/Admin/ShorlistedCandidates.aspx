<%@ Page Title="" Language="C#" MasterPageFile="~/Admin/AdminMaster.Master" EnableEventValidation="false" AutoEventWireup="true" CodeBehind="ShorlistedCandidates.aspx.cs" Inherits="IntelliJob.Admin.ShortlistedCandidates" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
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
        
        // Hide toolbar to match Dashboard spacing
        document.addEventListener('DOMContentLoaded', function() {
            var toolbar = document.getElementById('pnlToolbar');
            if (toolbar) {
                toolbar.style.display = 'none';
            }
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div style="background-image: url('../Images/bg.jpg'); width: 100%; height: 720px; background-repeat: no-repeat; background-size: cover; background-attachment: fixed;">
        <div class="container-fluid pt-4 pb-4">
            <div>
                <asp:Label ID="lblMsg" runat="server"></asp:Label>
            </div>
            <div id="pnlToolbar" class="btn-toolbar justify-content-between" style="margin-bottom: 0;">
                <div class="btn-group">
                </div>
                <div class="input-group h-25">
                </div>
            </div>
            <div class="row">
                <div class="col-12 pb-3">
                    <h3 class="text-center">Shortlisted Candidates</h3>
                </div>
            </div>

            <div class="row mb-3 pt-sm-3">
                <div class="col-md-12">
                    <asp:GridView ID="GridView1" runat="server" CssClass="table table-hover table-bordered" HeaderStyle-HorizontalAlign="Center"
                        EmptyDataText="No Record to Display..!" AllowPaging="True" PageSize="5"
                        OnPageIndexChanging="GridView1_PageIndexChanging" DataKeyNames="AppliedJobId" OnRowDeleting="GridView1_RowDeleting"
                        OnRowDataBound="GridView1_RowDataBound" OnSelectedIndexChanged="GridView1_SelectedIndexChanged" OnRowCommand="GridView1_RowCommand" AutoGenerateColumns="False">
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

                            <asp:TemplateField HeaderText="Call for Interview">
                                <ItemTemplate>
                                    <asp:ImageButton
                                        ID="btnCallForInterview"
                                        runat="server"
                                        ImageUrl="../assets/img/icon/tick.jpg"
                                        Width="22px"
                                        Height="22px"
                                        CommandName="CallForInterview"
                                        OnClick="btnCallForInterview_Click"
                                        ToolTip="Call for Interview" />
                                    <asp:HiddenField ID="hdnEmail" runat="server" Value='<%# Eval("Email") %>' />
                                    <asp:HiddenField ID="hdnJobId" runat="server" Value='<%# Eval("JobId") %>' />
                                    <asp:HiddenField ID="hdnUserName" runat="server" Value='<%# Eval("UserName") %>' />
                                    <asp:HiddenField ID="hdnJobTitle" runat="server" Value='<%# Eval("Title") %>' />
                                    <asp:HiddenField ID="hdnCompanyName" runat="server" Value='<%# Eval("CompanyName") %>' />
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Center" Width="50px" />
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <asp:Button ID="btnJobDetails" runat="server" Text="Job Details"
                                        CommandName="ViewJob" CommandArgument='<%# Eval("JobId") %>'
                                        CssClass="btn btn-sm btn-link" style="cursor: pointer; text-decoration: underline;" />
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

        </div>
    </div>
</asp:Content>
