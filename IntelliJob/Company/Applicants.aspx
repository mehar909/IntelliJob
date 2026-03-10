<%@ Page Title="" Language="C#" MasterPageFile="~/Company/CompanyMaster.Master" AutoEventWireup="true" CodeBehind="Applicants.aspx.cs" Inherits="IntelliJob.Company.UserList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
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
                    <h3 class="text-center">User List/Details</h3>
                </div>
            </div>

            <div class="row mb-3 pt-sm-3">
                <div class="col-md-12">
                    <asp:GridView ID="GridView1" runat="server" CssClass="table table-hover table-bordered" HeaderStyle-HorizontalAlign="Center"
                        EmptyDataText="No Record to Display..!" AllowPaging="True" PageSize="5"
                        OnPageIndexChanging="GridView1_PageIndexChanging" DataKeyNames="UserId" OnRowDeleting="GridView1_RowDeleting" AutoGenerateColumns="False">
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
                                    <asp:HyperLink ID="lnkViewUser" runat="server" NavigateUrl='<%# "ViewUserDetails.aspx?id=" + Eval("UserId") + "&returnUrl=Applicants.aspx" %>'>
                                        <asp:Image ID="Img" runat="server" ImageUrl="../assets/img/icon/view.png" Height="25px" Width="25px" />
                                    </asp:HyperLink>
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Center" Width="50px" />
                            </asp:TemplateField>

                            <asp:CommandField CausesValidation="false" HeaderText="Delete" ShowDeleteButton="true"
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
