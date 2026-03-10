<%@ Page Title="" Language="C#" MasterPageFile="~/Company/CompanyMaster.Master" AutoEventWireup="true" CodeBehind="JobList.aspx.cs" Inherits="IntelliJob.Company.JobList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function () {
            var backButton = document.getElementById('<%= linkBack.ClientID %>');
            var toolbar = document.getElementById('pnlToolbar');
            if (backButton && toolbar) {
                // Check if back button is hidden
                if (backButton.style.display === 'none' || !backButton.offsetParent) {
                    toolbar.style.display = 'none';
                }
            } else if (toolbar && !backButton) {
                // If no back button exists, hide the toolbar
                toolbar.style.display = 'none';
            } else if (toolbar) {
                // Hide toolbar if it exists and back button is not visible
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
                    <asp:Label ID="Label1" runat="server"></asp:Label>
                </div>
                <div class="input-group h-25">
                    <asp:HyperLink ID="linkBack" runat="server" NavigateUrl="~/Company/ViewApplications.aspx" CssClass="btn btn-secondary" Visible="false"> Back</asp:HyperLink>
                </div>
            </div>
            <div class="row">
                <div class="col-12 pb-3">
                    <h3 class="text-center">Posted Jobs/Details</h3>
                </div>
            </div>

            <div class="row mb-3 pt-sm-3">
                <div class="col-md-12">
                    <asp:GridView ID="GridView1" runat="server" CssClass="table table-hover table-bordered" HeaderStyle-HorizontalAlign="Center"
                        EmptyDataText="No Record to Display..!" AllowPaging="True" PageSize="5"
                        OnPageIndexChanging="GridView1_PageIndexChanging" DataKeyNames="JobId" OnRowDeleting="GridView1_RowDeleting"
                        AutoGenerateColumns="False" OnRowCommand="GridView1_RowCommand" OnRowDataBound="GridView1_RowDataBound">
                        <Columns>

                            <asp:BoundField DataField="Sr.No" HeaderText="Sr.No">
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <asp:BoundField DataField="Title" HeaderText="Job Title">
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <asp:BoundField DataField="CompanyName" HeaderText="Company">
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <asp:BoundField DataField="NoOfPost" HeaderText="No.Of Post">
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <asp:BoundField DataField="CreateDate" HeaderText="Posted Date" DataFormatString="{0:dd MMMM yyyy}">
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <asp:BoundField DataField="LastDateToApply" HeaderText="Valid Till" DataFormatString="{0:dd MMMM yyyy}">
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <asp:TemplateField HeaderText="Edit">
                                <ItemTemplate>
                                    <asp:HyperLink ID="lnkEditJob" runat="server" NavigateUrl='<%# GetEditJobUrl(Eval("JobId").ToString()) %>'>
                                        <asp:Image ID="Img" runat="server" ImageUrl="../assets/img/icon/editPencil.jpg" Height="25px" Width="25px" />
                                    </asp:HyperLink>
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Center" Width="50px" />
                            </asp:TemplateField>

                            <asp:CommandField CausesValidation="false" HeaderText="Delete" ShowDeleteButton="true"
                                DeleteImageUrl="../assets/img/icon/trashIcon.jpg" ButtonType="Image">
                                <ControlStyle Height="25px" Width="25px" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:CommandField>
                           <asp:TemplateField HeaderText="Marked As Featured">
                                <ItemTemplate>
                                    <asp:ImageButton
                                        ID="btnToggleFeatured"
                                        runat="server"
                                        Width="40px"
                                        Height="40px"
                                        ImageUrl='<%# Convert.ToBoolean(Eval("isFeatured")) 
                                                    ? "~/assets/img/icon/yes.png" 
                                                    : "~/assets/img/icon/no.png" %>'
                                        ToolTip='<%# Convert.ToBoolean(Eval("isFeatured")) 
                                                    ? "Mark as Not Featured" 
                                                    : "Mark as Featured" %>'
                                        OnClick="btnToggleFeatured_Click" />
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:TemplateField>



                        </Columns>
                        <HeaderStyle BackColor="#7200cf" ForeColor="White" />
                    </asp:GridView>
                </div>
            </div>


        </div>
    </div>

</asp:Content>
