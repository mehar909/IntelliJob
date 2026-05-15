<%@ Page Title="Job Applicants" Language="C#" MasterPageFile="~/Company/CompanyMaster.Master"
    AutoEventWireup="true" CodeBehind="ViewJobApplicants.aspx.cs"
    Inherits="IntelliJob.Company.ViewJobApplicants" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
<style>
    .gv-wrap { overflow-x: auto; }
    .gv-wrap table { min-width: 900px; }
    th { background: #6c5ce7 !important; color: #fff !important; font-weight: 600; }
    td, th { vertical-align: middle !important; }
    .btn-invite {
        background: linear-gradient(135deg,#6c5ce7,#a29bfe);
        color:#fff; border:none; border-radius:6px;
        padding:5px 12px; font-size:13px; cursor:pointer;
    }
    .btn-invite:hover { opacity:.9; }
    .btn-report {
        background:#00b894; color:#fff; border-radius:6px;
        padding:5px 12px; font-size:13px; text-decoration:none; display:inline-block;
    }
    .btn-report:hover { background:#00a381; color:#fff; text-decoration:none; }
    .status-sent   { color:#6c5ce7; font-size:12px; font-weight:600; }
    .status-used   { color:#00b894; font-size:12px; font-weight:600; }
    .status-none   { color:#b2bec3; font-size:12px; }
</style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

<div class="container-fluid py-4">

    <div class="d-flex justify-content-between align-items-center mb-3">
        <h4 style="color:#2d3436;font-weight:700;">Job Applicants</h4>
        <asp:HyperLink ID="linkBack" runat="server" CssClass="btn btn-outline-secondary btn-sm">
            <i class="fas fa-arrow-left me-1"></i> Back
        </asp:HyperLink>
    </div>

    <asp:Label ID="lblMsg" runat="server" Visible="false" CssClass="alert d-block mb-3" />

    <div class="card shadow-sm">
        <div class="card-body p-0 gv-wrap">
            <asp:GridView ID="GridView1" runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover mb-0"
                AllowPaging="True" PageSize="15"
                DataKeyNames="AppliedJobId,UserId"
                OnPageIndexChanging="GridView1_PageIndexChanging"
                OnRowDataBound="GridView1_RowDataBound">
                <Columns>

                    <asp:BoundField DataField="Sr.No"    HeaderText="#"       ItemStyle-Width="40px" />
                    <asp:BoundField DataField="Username"  HeaderText="Username" />
                    <asp:BoundField DataField="Email"     HeaderText="Email" />
                    <asp:BoundField DataField="Mobile"    HeaderText="Mobile" />
                    <asp:BoundField DataField="Country"   HeaderText="Country" />

                    <%-- View Profile --%>
                    <asp:TemplateField HeaderText="Profile">
                        <ItemTemplate>
                            <asp:HyperLink ID="lnkViewUser" runat="server"
                                NavigateUrl='<%# "ViewUserDetails.aspx?id=" + Eval("UserId") %>'
                                CssClass="btn btn-sm btn-outline-primary">
                                <i class="fas fa-user"></i>
                            </asp:HyperLink>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <%-- Shortlist --%>
                    <asp:TemplateField HeaderText="Shortlist">
                        <ItemTemplate>
                            <asp:ImageButton ID="btnShortlist" runat="server"
                                ImageUrl="~/Images/shortlist.png"
                                OnClick="btnShortlist_Click"
                                ToolTip="Shortlist Candidate"
                                Width="28px" />
                        </ItemTemplate>
                    </asp:TemplateField>

                    <%-- Interview Invite --%>
                    <asp:TemplateField HeaderText="Interview Invite">
                        <ItemTemplate>
                            <div>
                                <%-- Send / Resend button (calls JS → AJAX → handler) --%>
                                <asp:Button ID="btnSendInvite" runat="server"
                                    Text="Send Invite"
                                    CssClass="btn btn-sm btn-invite"
                                    OnClientClick='<%# "sendInvite(" + Eval("AppliedJobId") + "); return false;" %>' />

                                <%-- Status line --%>
                                <div class="mt-1">
                                    <%# GetInviteStatus(Eval("InterviewPassword"), Eval("PasswordUsed"), Eval("InterviewSentAt")) %>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <%-- Interview Report --%>
                    <asp:TemplateField HeaderText="AI Report">
                        <ItemTemplate>
                            <asp:HyperLink ID="lnkViewReport" runat="server"
                                CssClass="btn-report" Visible="false">
                                <i class="fas fa-chart-bar me-1"></i>View Report
                            </asp:HyperLink>
                            <asp:Literal ID="litScore" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>

                </Columns>
                <EmptyDataTemplate>
                    <div class="text-center py-5 text-muted">
                        <i class="fas fa-users fa-2x mb-2 d-block"></i>
                        No applicants found for this job.
                    </div>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>

</div>

<script>
function sendInvite(appliedJobId) {
    if (!confirm('Send interview invitation to this candidate?')) return;

    fetch('<%= ResolveUrl("~/Company/SendInterviewInvite.ashx") %>', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'appliedJobId=' + appliedJobId
    })
    .then(r => r.json())
    .then(data => {
        if (data.success) {
            alert(data.message || 'Invitation sent!');
            location.reload();
        } else {
            alert('Error: ' + (data.error || 'Unknown error'));
        }
    })
    .catch(() => alert('Network error – please try again.'));
}
</script>

</asp:Content>
