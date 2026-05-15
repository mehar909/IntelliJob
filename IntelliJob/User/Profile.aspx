<%@ Page Title="" Language="C#" MasterPageFile="~/User/UserMaster.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="IntelliJob.User.Profile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .chat-layout {
            border: 1px solid #dee2e6;
            border-radius: 10px;
            overflow: hidden;
            position: relative;
            display: flex;
            flex-direction: column;
            height: 600px;
        }
        .chat-messages-container {
            flex: 1 1 auto;
            overflow-y: auto !important;
            overflow-x: hidden;
            padding: 20px;
            background-color: #f5f5f5;
            min-height: 0;
            max-height: none;
        }
        .message-bubble {
            padding: 10px 15px;
            border-radius: 18px;
            margin-bottom: 10px;
            max-width: 75%;
            word-wrap: break-word;
            animation: fadeIn 0.3s;
        }
        @keyframes fadeIn {
            from { opacity: 0; transform: translateY(10px); }
            to { opacity: 1; transform: translateY(0); }
        }
        .user-message {
            background-color: #007bff;
            color: white;
            margin-left: auto;
            border-radius: 18px 18px 0 18px;
            box-shadow: 0 2px 5px rgba(0,123,255,0.3);
        }
        .admin-message {
            background-color: white;
            color: #333;
            border-radius: 18px 18px 18px 0;
            box-shadow: 0 2px 5px rgba(0,0,0,0.1);
            margin-left: 0;
            margin-right: auto;
        }
        .message-time {
            font-size: 11px;
            opacity: 0.7;
            margin-top: 5px;
        }
        .reply-section {
            padding: 15px;
            background-color: white;
            border-top: 2px solid #dee2e6;
            flex-shrink: 0;
            flex-grow: 0;
            position: relative;
            z-index: 100;
            min-height: 120px;
            width: 100%;
            box-sizing: border-box;
            display: block !important;
            visibility: visible !important;
            overflow: visible !important;
        }
        .chat-messages-container::-webkit-scrollbar {
            width: 8px;
        }
        .chat-messages-container::-webkit-scrollbar-track {
            background: #f5f5f5;
        }
        .chat-messages-container::-webkit-scrollbar-thumb {
            background: #ccc;
            border-radius: 4px;
        }
        .btn-resume-preview {
            display: inline-flex;
            align-items: center;
            gap: 8px;
            background-color: #fb246a;
            border: 2px solid #fb246a;
            color: #ffffff !important;
            border-radius: 12px;
            padding: 10px 20px;
            font-weight: 600;
            font-size: 14px;
            text-decoration: none;
            transition: all 0.2s ease;
            white-space: nowrap;
            cursor: pointer;
        }
        .btn-resume-preview:hover {
            background-color: #da2461;
            border-color: #da2461;
            transform: translateY(-1px);
        }
        .btn-resume-preview.danger {
            background-color: #fff;
            color: #dc3545 !important;
            border-color: #dc3545;
        }
        .btn-resume-preview.danger:hover {
            background-color: #dc3545;
            color: #fff !important;
        }
    </style>
    <script type="text/javascript">
        // Auto-scroll to bottom when page loads
        window.onload = function() {
            var container = document.getElementById('chatMessagesContainer');
            if (container) {
                container.scrollTop = container.scrollHeight;
            }
        };

        // Delete individual message
        function deleteMessage(messageId, messageType) {
            if (confirm('Are you sure you want to delete this message?')) {
                __doPostBack('DeleteMessage', messageId + '|' + messageType);
            }
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container pt-5 pb-5">
        <div class="main-body">
            <asp:DataList ID="dlProfile" runat="server" Width="100%" OnItemCommand="dlProfile_ItemCommand">
                <ItemTemplate>
                    <div class="row gutters-sm">
                        <div class="col-md-4 mb-3">
                            <div class="card">
                                <div class="card-body">
                                    <div class="d-flex flex-column align-items-center text-center">
                                        <img src="../photos/<%# Eval("photo") %>" class="rounded-circle" width="150" />
                                        <div class="mt-3">
                                            <h4 class="text-capitalize"><%# Eval("Name") %></h4>
                                            <p class="text-secondary mb-1">@<%# Eval("Username") %></p>
                                            <p class="text-muted font-size-sm text-capitalize">
                                                <i class="fas fa-map-marker-alt"></i><%# Eval("Country") %>
                                            </p>
                                        </div>

                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="col-md-8">
                            <div class="card mb-3">
                                <div class="card-body">
                                    <div class="row">
                                        <div class="col-sm-3">
                                            <h6 class="mb-0">Full Name</h6>
                                        </div>
                                        <div class="col-sm-9 text-secondary"><%# Eval("Name") %></div>
                                    </div>
                                    <hr />
                                    <div class="row">
                                        <div class="col-sm-3">
                                            <h6 class="mb-0">Email</h6>
                                        </div>
                                        <a href='mailto:<%# Eval("Email") %>' class="text-secondary"><%# Eval("Email") %></a>
                                    </div>
                                    <hr />
                                    <div class="row">
                                        <div class="col-sm-3">
                                            <h6 class="mb-0">Mobile</h6>
                                        </div>
                                        <div class="col-sm-9 text-secondary"><%# Eval("Mobile") %></div>
                                    </div>
                                    <hr />
                                    <div class="row">
                                        <div class="col-sm-3">
                                            <h6 class="mb-0">Address</h6>
                                        </div>
                                        <div class="col-sm-9 text-secondary"><%# Eval("Address") %></div>
                                    </div>
                                    <hr />
                                    <div class="row">
                                        <div class="col-sm-3">
                                            <h6 class="mb-0">Resume Upload</h6>
                                        </div>
                                        <div class="col-sm-9 text-secondary">
                                            <asp:HyperLink ID="lnkResume" runat="server"
                                                NavigateUrl='<%# Eval("Resume") != DBNull.Value && !string.IsNullOrEmpty(Eval("Resume").ToString()) ? "~/" + Eval("Resume").ToString() : "#" %>'
                                                Target="_blank"
                                                CssClass="text-primary"
                                                style="text-decoration: none;"
                                                Visible='<%# Eval("Resume") != DBNull.Value && !string.IsNullOrEmpty(Eval("Resume").ToString()) %>'>
                                                <i class="fas fa-download"></i> View Resume
                                            </asp:HyperLink>
                                            <asp:Label ID="lblNoResume" runat="server"
                                                Text="Not Uploaded"
                                                Visible='<%# Eval("Resume") == DBNull.Value || string.IsNullOrEmpty(Eval("Resume").ToString()) %>'
                                                CssClass="text-muted"></asp:Label>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Resume Preview Card -->
                    <div class="card mb-4" style="border-radius: 18px; box-shadow: 0 8px 26px rgba(15, 23, 42, 0.06); border: none;">
                        <div class="card-body" style="padding: 25px;">
                            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; flex-wrap: wrap; gap: 10px;">
                                <h3 style="margin: 0; font-size: 20px; font-weight: 800; color: #111827;"><i class="fas fa-file-invoice" style="color:#fb246a;"></i> Resume Preview</h3>
                                <div style="display: flex; gap: 8px;">
                                    <asp:Button ID="btmEdit" runat="server" Text="Edit Resume" CssClass="btn-resume-preview" CommandName="EditUserProfile" CommandArgument='<%# Eval("UserId") %>' Visible='<%# Eval("Resume") != DBNull.Value && !string.IsNullOrEmpty(Eval("Resume").ToString()) %>' />
                                    <asp:LinkButton ID="btnDeleteResume" runat="server" CommandName="DeleteResume" CommandArgument='<%# Eval("UserId") %>' CssClass="btn-resume-preview danger" OnClientClick="return confirm('Are you sure you want to remove your stored resume?');" Visible='<%# Eval("Resume") != DBNull.Value && !string.IsNullOrEmpty(Eval("Resume").ToString()) %>'><i class="fas fa-trash"></i> Delete Resume</asp:LinkButton>
                                </div>
                            </div>

                            <asp:Panel ID="pnlImportResume" runat="server" Visible='<%# Eval("Resume") == DBNull.Value || string.IsNullOrEmpty(Eval("Resume").ToString()) %>'>
                                <div class="alert alert-secondary text-center" style="padding: 40px 20px; border-radius: 12px; border: 1px dashed #ced4da; background-color: #f8f9fa;">
                                    <i class="fas fa-file-upload fa-3x mb-3" style="color: #adb5bd;"></i>
                                    <h5 style="font-weight: 700; color: #495057;">No Resume Uploaded</h5>
                                    <p class="text-muted mb-4">Upload a PDF or DOCX resume to automatically parse and build your profile.</p>
                                    <div class="d-flex justify-content-center align-items-center" style="gap: 10px; flex-wrap: wrap;">
                                        <asp:FileUpload ID="fuResumeImport" runat="server" CssClass="form-control" style="max-width: 300px; height: auto;" />
                                        <asp:LinkButton ID="btnImportResume" runat="server" CommandName="ImportResume" CommandArgument='<%# Eval("UserId") %>' CssClass="btn-resume-preview"><i class="fas fa-file-import"></i> Import & Parse Resume</asp:LinkButton>
                                    </div>
                                </div>
                            </asp:Panel>

                            <asp:Panel ID="pnlResumePreview" runat="server" Visible='<%# Eval("Resume") != DBNull.Value && !string.IsNullOrEmpty(Eval("Resume").ToString()) %>'>
                                <div class="row">
                                    <div class="col-12">
                                        <div class="form-group mb-3">
                                            <label style="font-weight: 600; color: #4b5563; margin-bottom: 6px; display: block;">Headline</label>
                                            <div class="form-control" style="height:auto; min-height:40px; background:#f9fafb; white-space:pre-wrap;"><%# Eval("ResumeHeadline") %></div>
                                        </div>
                                    </div>
                                    <div class="col-12">
                                        <div class="form-group mb-3">
                                            <label style="font-weight: 600; color: #4b5563; margin-bottom: 6px; display: block;">Summary</label>
                                            <div class="form-control" style="height:auto; min-height:40px; background:#f9fafb; white-space:pre-wrap;"><%# Eval("ResumeSummary") %></div>
                                        </div>
                                    </div>
                                    <div class="col-12">
                                        <div class="form-group mb-3">
                                            <label style="font-weight: 600; color: #4b5563; margin-bottom: 6px; display: block;">Skills</label>
                                            <div class="form-control" style="height:auto; min-height:40px; background:#f9fafb; white-space:pre-wrap;"><%# Eval("ResumeSkills") %></div>
                                        </div>
                                    </div>
                                    <div class="col-12">
                                        <div class="form-group mb-3">
                                            <label style="font-weight: 600; color: #4b5563; margin-bottom: 6px; display: block;">Education</label>
                                            <div class="form-control" style="height:auto; min-height:40px; background:#f9fafb; white-space:pre-wrap;"><%# Eval("ResumeEducation") %></div>
                                        </div>
                                    </div>
                                    <div class="col-12">
                                        <div class="form-group mb-3">
                                            <label style="font-weight: 600; color: #4b5563; margin-bottom: 6px; display: block;">Experience</label>
                                            <div class="form-control" style="height:auto; min-height:40px; background:#f9fafb; white-space:pre-wrap;"><%# Eval("ResumeExperienceDetails") %></div>
                                        </div>
                                    </div>
                                    <div class="col-12">
                                        <div class="form-group mb-3">
                                            <label style="font-weight: 600; color: #4b5563; margin-bottom: 6px; display: block;">Projects</label>
                                            <div class="form-control" style="height:auto; min-height:40px; background:#f9fafb; white-space:pre-wrap;"><%# Eval("ResumeProjects") %></div>
                                        </div>
                                    </div>
                                    <div class="col-12">
                                        <div class="form-group mb-3">
                                            <label style="font-weight: 600; color: #4b5563; margin-bottom: 6px; display: block;">Certifications</label>
                                            <div class="form-control" style="height:auto; min-height:40px; background:#f9fafb; white-space:pre-wrap;"><%# Eval("ResumeCertifications") %></div>
                                        </div>
                                    </div>
                                    <div class="col-12">
                                        <div class="form-group mb-3">
                                            <label style="font-weight: 600; color: #4b5563; margin-bottom: 6px; display: block;">Languages</label>
                                            <div class="form-control" style="height:auto; min-height:40px; background:#f9fafb; white-space:pre-wrap;"><%# Eval("ResumeLanguages") %></div>
                                        </div>
                                    </div>
                                </div>
                            </asp:Panel>
                                    </div>
                                </div>
                </ItemTemplate>
            </asp:DataList>

            <div class="card mt-5">
                <div class="card-header bg-primary text-white">
                    <h5 class="mb-0"><i class="fas fa-comments"></i> Chat with Admin</h5>
                </div>
                <div class="chat-layout">
                    <div class="chat-messages-container" id="chatMessagesContainer">
                        <asp:Repeater ID="rptChat" runat="server">
                            <ItemTemplate>
                                <%# FormatChatMessage(Eval("ContactId"), Eval("Message"), Eval("Date"), Eval("ReplyMessage"), Eval("ReplyDate"), Eval("ReplyId"), Eval("IsAdminMessage"), Eval("Username")) %>
                            </ItemTemplate>
                        </asp:Repeater>
                        <div id="noMessages" runat="server" class="text-center text-muted mt-5" style="display: none;">
                            <i class="fas fa-comment-slash fa-3x mb-3"></i>
                            <p>No messages yet. Start a conversation with the admin!</p>
                        </div>
                    </div>
                    <div class="reply-section">
                        <asp:Label ID="lblMsg" runat="server" CssClass="alert mb-2" style="display: none;"></asp:Label>
                        <div class="d-flex align-items-end mb-2">
                            <asp:TextBox ID="txtMessage" runat="server" CssClass="form-control mr-2" placeholder="Type your message here..." TextMode="MultiLine" Rows="2" style="resize: vertical; min-width: 300px;"></asp:TextBox>
                            <asp:Button ID="btnSend" runat="server" Text="Send Message" CssClass="btn btn-primary mr-2" OnClick="btnSend_Click" style="height: fit-content;" />
                            <asp:Button ID="btnDeleteChat" runat="server" Text="Delete Chat" CssClass="btn btn-danger" OnClick="btnDeleteChat_Click" OnClientClick="return confirm('Are you sure you want to delete this entire chat history?');" style="height: fit-content;" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
