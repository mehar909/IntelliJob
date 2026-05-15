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
            font-size: inherit;
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
        .profile-editor-form textarea.form-control {
            height: 120px !important;
            min-height: 120px !important;
            max-height: 220px !important;
            overflow-y: auto !important;
            resize: vertical;
        }
        .profile-editor-actions {
            clear: both;
            margin-top: 24px;
            margin-bottom: 72px;
        }
        .profile-edit-sheet {
            max-width: 920px;
            margin-left: auto;
            margin-right: auto;
        }
        .html-preview-frame {
            width: 100%;
            min-height: 980px;
            border: 1px solid #e5e7eb;
            border-radius: 16px;
            background: #f8fafc;
        }
    </style>
    <script type="text/javascript">
        // Auto-scroll to bottom when page loads
        window.onload = function() {
            var editor = document.getElementById('profileEditor');
            if (editor && editor.style.display === 'block') {
                return;
            }
            var container = document.getElementById('chatMessagesContainer');
            if (container) {
                container.scrollTop = container.scrollHeight;
            }
        };

        function toggleProfileEditor(showEditor) {
            var editor = document.getElementById('profileEditor');
            var body = document.getElementById('profilePageBody');
            if (!editor) return;
            editor.style.display = showEditor ? 'block' : 'none';
            if (body) body.style.display = showEditor ? 'none' : 'block';
            if (showEditor) {
                var password = document.getElementById('<%= txtProfilePassword.ClientID %>');
                var confirmPassword = document.getElementById('<%= txtProfileConfirmPassword.ClientID %>');
                if (password) password.value = '';
                if (confirmPassword) confirmPassword.value = '';
                setTimeout(function () {
                    editor.scrollIntoView({ behavior: 'smooth', block: 'start' });
                }, 50);
            }
        }

        // Delete individual message
        function deleteMessage(messageId, messageType) {
            if (confirm('Are you sure you want to delete this message?')) {
                __doPostBack('DeleteMessage', messageId + '|' + messageType);
            }
        }

        function exportHTMLAsPDF() {
            var payload = document.getElementById('<%= hfResumePreviewJson.ClientID %>');
            if (!payload || !payload.value) {
                alert('No enhanced resume JSON is available for the HTML preview yet.');
                return false;
            }

            var previewUrl = '<%= ResolveUrl("~/ResumePreview.html") %>';
            var encoded = encodeURIComponent(payload.value);
            var previewWindow = window.open(previewUrl + '#data=' + encoded, '_blank', 'noopener');
            if (!previewWindow) {
                alert('The preview window was blocked by the browser. Please allow popups for this site.');
                return false;
            }

            previewWindow.focus();
            return false;
        }

    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container pt-5 pb-5">
        <div class="main-body">
        <asp:HiddenField ID="hfResumePreviewJson" runat="server" />

            <div class="profile-edit-sheet">
            <div id="profileEditor" class="card mb-4" style="display:none; border-radius: 18px; box-shadow: 0 8px 26px rgba(15, 23, 42, 0.06); border: none;">
                <div class="card-body" style="padding: 25px;">
                    <div class="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-3">
                        <h3 style="margin: 0; font-size: 20px; font-weight: 800; color: #111827;"><i class="fas fa-user-edit" style="color:#fb246a;"></i> Edit Profile</h3>
                        <small class="text-muted">Your profile details are stored separately from the resume preview below.</small>
                    </div>
                    <asp:Label ID="lblProfileMsg" runat="server" Visible="false"></asp:Label>

                    <div class="form-contact contact_form profile-editor-form">
                        <div class="row">
                            <div class="col-12">
                                <h6>Login Information</h6>
                            </div>
                            <div class="col-12">
                                <div class="form-group">
                                    <label>Username</label>
                                    <asp:TextBox ID="txtProfileUserName" runat="server" CssClass="form-control" placeholder="Enter Unique Username"></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-sm-6">
                                <div class="form-group">
                                    <label>New Password</label>
                                    <asp:TextBox ID="txtProfilePassword" runat="server" CssClass="form-control" placeholder="Leave blank to keep current password" TextMode="Password"></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-sm-6">
                                <div class="form-group">
                                    <label>Confirm Password</label>
                                    <asp:TextBox ID="txtProfileConfirmPassword" runat="server" CssClass="form-control" placeholder="Confirm new password" TextMode="Password"></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-12">
                                <h6>Personal Information</h6>
                            </div>
                            <div class="col-12">
                                <div class="form-group">
                                    <label>Full Name</label>
                                    <asp:TextBox ID="txtProfileFullName" runat="server" CssClass="form-control" placeholder="Enter Full Name"></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-12">
                                <div class="form-group">
                                    <label>Address</label>
                                    <asp:TextBox ID="txtProfileAddress" runat="server" CssClass="form-control" placeholder="Enter Address" TextMode="MultiLine"></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-12">
                                <div class="form-group">
                                    <label>Mobile Number</label>
                                    <asp:TextBox ID="txtProfileMobile" runat="server" CssClass="form-control" placeholder="Enter Mobile Number"></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-12">
                                <div class="form-group">
                                    <label>Email</label>
                                    <asp:TextBox ID="txtProfileEmail" runat="server" CssClass="form-control" placeholder="Enter Email" TextMode="Email"></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-12">
                                <div class="form-group">
                                    <label>Upload Photo</label>
                                    <asp:FileUpload ID="fuProfilePhoto" runat="server" CssClass="form-control" ToolTip=".png, .jpg, .jpeg extension only" />
                                </div>
                            </div>
                            <div class="col-12">
                                <div class="form-group">
                                    <label>Country</label>
                                    <asp:DropDownList ID="ddlProfileCountry" runat="server" DataSourceID="SqlDataSourceProfileCountry" CssClass="form-control w-100" AppendDataBoundItems="true" DataTextField="CountryName" DataValueField="CountryName">
                                        <asp:ListItem Value="0">Select Country</asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:SqlDataSource ID="SqlDataSourceProfileCountry" runat="server" ConnectionString="<%$ ConnectionStrings:cs %>" SelectCommand="SELECT [CountryName] FROM [Country]"></asp:SqlDataSource>
                                </div>
                            </div>
                        </div>

                        <div class="form-group mt-3 profile-editor-actions text-right">
                            <asp:Button ID="btnCancelProfile" runat="server" Text="Cancel" CssClass="btn btn-light" OnClientClick="toggleProfileEditor(false); return false;" CausesValidation="false" />
                            <asp:Button ID="btnSaveProfile" runat="server" Text="Update Profile" CssClass="button button-contactForm boxed-btn" OnClick="btnSaveProfile_Click" />
                        </div>
                    </div>
                </div>
            </div>
            </div>

            <div id="profilePageBody">
            <asp:DataList ID="dlProfile" runat="server" Width="100%" OnItemCommand="dlProfile_ItemCommand" OnItemDataBound="dlProfile_ItemDataBound">
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
                                                <i class="fas fa-map-marker-alt"></i> <%# Eval("Country") %>
                                            </p>
                                        </div>

                                    </div>
                                </div>
                            </div>
                            <div class="card border-0">
                                <div class="card-body">
                                    <div class="d-flex flex-column align-items-center text-center">
                                <div style="display: flex; gap: 8px;">
                                    <asp:Button ID="btnEditProfile" runat="server" Text="Edit Profile" CssClass="btn-resume-preview" CommandName="EditProfile" CommandArgument='<%# Eval("UserId") %>' />
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
                            <div style="display: flex; flex:1; align-items: center; margin-bottom: 20px;">
                                <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; flex-wrap: wrap; gap: 10px;">
                                    <h3 style="margin: 0; font-size: 20px; font-weight: 800; color: #111827;"><i class="fas fa-file-invoice" style="color: #fb246a;"></i>Resume Preview</h3>
                                </div>
                                <div style="display: flex; flex:1; justify-content: flex-end; gap: 8px;">
                                    <asp:Button ID="Button2" runat="server" Text="Edit Resume" CssClass="btn-resume-preview" CommandName="EditResume" CommandArgument='<%# Eval("UserId") %>' Visible='<%# Eval("Resume") != DBNull.Value && !string.IsNullOrEmpty(Eval("Resume").ToString()) %>' />
                                    <asp:LinkButton ID="LinkButton1" runat="server" CommandName="DeleteResume" CommandArgument='<%# Eval("UserId") %>' CssClass="btn-resume-preview danger" OnClientClick="return confirm('Are you sure you want to remove your stored resume?');" Visible='<%# Eval("Resume") != DBNull.Value && !string.IsNullOrEmpty(Eval("Resume").ToString()) %>'><i class="fas fa-trash"></i> Delete Resume</asp:LinkButton>
                                </div>
                            </div>

                            <asp:Panel ID="pnlImportResume" runat="server" Visible='<%# Eval("Resume") == DBNull.Value || string.IsNullOrEmpty(Eval("Resume").ToString()) %>'>
                                <div class="alert alert-secondary text-center" style="padding: 40px 20px; border-radius: 12px; border: 1px dashed #ced4da; background-color: #f8f9fa;">
                                    <i class="fas fa-file-upload fa-3x mb-3" style="color: #adb5bd;"></i>
                                    <h5 style="font-weight: 700; color: #495057;">No Resume Uploaded</h5>
                                    <p class="text-muted mb-4">Upload a PDF or DOCX resume to automatically parse and build your profile.</p>
                                    <div class="d-flex justify-content-center align-items-center" style="gap: 10px; flex-wrap: wrap;">
                                        <asp:FileUpload ID="fuResumeImport" runat="server" CssClass="form-control" style="max-width: 300px; height: auto;" />
                                        <asp:LinkButton ID="btnImportResume" runat="server" CommandName="ImportResume" CommandArgument='<%# Eval("UserId") %>' CssClass="btn-resume-preview"><i class="fas fa-file-import"></i>Upload Resume</asp:LinkButton>
                                    </div>
                                </div>
                            </asp:Panel>

                            <asp:Panel ID="pnlResumePreview" runat="server" Visible="false">
                                <div class="row">
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

                    <div class="row justify-content-center mt-4">
                        <div class="col-12">
                            <div class="card mb-4" style="border-radius: 18px; box-shadow: 0 8px 26px rgba(15, 23, 42, 0.06); border: none;">
                                <div class="card-body" style="padding: 25px;">
                                    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; flex-wrap: wrap; gap: 10px;">
                                        <h3 style="margin: 0; font-size: 20px; font-weight: 800; color: #111827;">Profile Resume Preview</h3>
                                        <%--<small class="text-muted">Rendered from ResumePreview.html using your current profile resume data.</small>--%>
                                    </div>
                                    <asp:Literal ID="litProfileHtmlPreviewFrame" runat="server" />
                                </div>
                            </div>
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
    </div>
</asp:Content>
