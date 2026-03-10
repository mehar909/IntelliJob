<%@ Page Title="User Queries" Language="C#" MasterPageFile="~/Admin/AdminMaster.Master" AutoEventWireup="true" CodeBehind="UserQuery.aspx.cs" Inherits="IntelliJob.Admin.UserQuery" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta name="google" content="notranslate" />
    <meta http-equiv="Content-Language" content="en" />
    <style>
        body {
            overflow: hidden !important;
            height: 100vh !important;
        }
        html {
            overflow: hidden !important;
            height: 100vh !important;
        }
        .main_container {
            display: flex !important;
            flex-direction: column !important;
            height: 100vh !important;
            overflow: hidden !important;
        }
        #ContentPlaceHolder1 {
            overflow: hidden !important;
            flex: 1 !important;
            display: flex !important;
            flex-direction: column !important;
            min-height: 0 !important;
        }
        .chat-layout {
            flex: 1;
            min-height: 0;
            border: 1px solid #dee2e6;
            border-radius: 10px;
            overflow: hidden;
            position: relative;
            display: flex;
            flex-direction: column;
        }
        .user-list-panel {
            width: 100%;
            background-color: #f8f9fa;
            overflow-y: auto;
            height: 100%;
        }
        .user-list-header {
            background: linear-gradient(135deg, #7200cf 0%, #9d4edd 100%);
            color: white;
            padding: 15px;
            font-weight: bold;
            text-align: center;
        }
        .user-list-item {
            padding: 15px;
            border-bottom: 1px solid #dee2e6;
            cursor: pointer;
            transition: background-color 0.2s;
            display: block;
        }
        .user-list-item:hover {
            background-color: #e9ecef;
        }
        .user-name {
            font-weight: bold;
            font-size: 16px;
            margin-bottom: 5px;
        }
        .user-email {
            font-size: 12px;
            opacity: 0.8;
        }
        .chat-panel {
            width: 100%;
            display: flex;
            flex-direction: column;
            background-color: #ffffff;
            height: 100%;
            min-height: 0;
            position: relative;
            overflow: hidden;
            flex: 1;
        }
        .chat-header {
            background: linear-gradient(135deg, #7200cf 0%, #9d4edd 100%);
            color: white;
            padding: 15px 20px;
            font-weight: bold;
            display: flex;
            align-items: center;
            flex-shrink: 0;
            min-height: 60px;
        }
        .chat-header .btn {
            margin-right: 15px;
        }
        .chat-header i {
            margin-right: 10px;
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
            background-color: white;
            color: #333;
            border-radius: 18px 18px 18px 0;
            box-shadow: 0 2px 5px rgba(0,0,0,0.1);
            margin-left: 0;
            margin-right: auto;
        }
        .admin-message {
            background-color: #007bff;
            color: white;
            margin-left: auto;
            border-radius: 18px 18px 0 18px;
            box-shadow: 0 2px 5px rgba(0,123,255,0.3);
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
        .reply-section .form-control {
            flex: 1;
            min-width: 200px;
        }
        .no-chat-selected {
            display: flex;
            align-items: center;
            justify-content: center;
            height: 100%;
            color: #999;
            text-align: center;
        }
        .user-list-panel::-webkit-scrollbar,
        .chat-messages-container::-webkit-scrollbar {
            width: 8px;
        }
        .user-list-panel::-webkit-scrollbar-track,
        .chat-messages-container::-webkit-scrollbar-track {
            background: #f5f5f5;
        }
        .user-list-panel::-webkit-scrollbar-thumb,
        .chat-messages-container::-webkit-scrollbar-thumb {
            background: #ccc;
            border-radius: 4px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container pt-4" style="overflow: hidden; flex: 1; display: flex; flex-direction: column; min-height: 0; height: 100%;">
        <div class="row" style="flex-shrink: 0;">
            <div class="col-12 pb-3" style="position: relative; padding-top: 1rem; padding-bottom: 1rem;">
                <asp:Label ID="lblMsg" runat="server" CssClass="mb-0" style="position: absolute; left: 15px; top: 50%; transform: translateY(-50%);"></asp:Label>
                <h3 class="text-center mb-0">User Queries - Chat Interface</h3>
            </div>
        </div>
        
        <div class="chat-layout" style="flex: 1; min-height: 0; overflow: hidden;">
            <!-- User List View -->
            <asp:Panel ID="pnlUserList" runat="server">
                <div class="user-list-panel">
                    <div class="user-list-header">
                        <i class="fas fa-users"></i> Users
                    </div>
                    <asp:Repeater ID="rptUserList" runat="server" OnItemCommand="rptUserList_ItemCommand">
                        <ItemTemplate>
                            <asp:LinkButton ID="lnkSelectUser" runat="server" 
                                CommandName="SelectUser" 
                                CommandArgument='<%# Eval("UserId") %>'
                                CssClass="user-list-item text-decoration-none d-block"
                                style="color: inherit;">
                                <div class="user-name"><%# Eval("Name") %></div>
                                <div class="user-email"><%# Eval("Email") %></div>
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:Label ID="lblNoUsers" runat="server" Text="No users with messages" CssClass="text-center text-muted d-block p-4" Visible="false"></asp:Label>
                </div>
            </asp:Panel>
            
            <!-- Chat View -->
            <asp:Panel ID="pnlChatSelected" runat="server" Visible="false" style="height: 100%; display: flex; flex-direction: column; flex: 1; min-height: 0;">
                <div class="chat-panel" style="height: 100%; display: flex; flex-direction: column; flex: 1; min-height: 0;">
                    <div class="chat-header" style="flex-shrink: 0;">
                        <asp:Button ID="btnBack" runat="server" Text="← Back" CssClass="btn btn-sm btn-light mr-3" OnClick="btnBack_Click" />
                        <i class="fas fa-user"></i> 
                        <asp:Label ID="lblSelectedUserName" runat="server"></asp:Label>
                    </div>
                    <div class="chat-messages-container" id="chatMessagesContainer" style="flex: 1 1 auto; overflow-y: auto; overflow-x: hidden; min-height: 0;">
                        <asp:Repeater ID="rptChatMessages" runat="server" OnItemCommand="rptChatMessages_ItemCommand">
                            <ItemTemplate>
                                <%# FormatChatMessage(Eval("ContactId"), Eval("Message"), Eval("Date"), Eval("ReplyMessage"), Eval("ReplyDate"), Eval("ReplyId"), Eval("IsAdminMessage"), Eval("Username")) %>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <div class="reply-section" style="flex-shrink: 0; background-color: white !important; border-top: 2px solid #dee2e6 !important; padding: 15px !important; position: relative !important; z-index: 1000 !important; display: block !important;">
                        <div class="d-flex align-items-end mb-2">
                            <asp:TextBox ID="txtReply" runat="server" CssClass="form-control mr-2" placeholder="Type your reply here..." TextMode="MultiLine" Rows="2" style="resize: vertical; min-width: 300px;"></asp:TextBox>
                            <asp:Button ID="btnReply" runat="server" Text="Send Reply" CssClass="btn btn-primary mr-2" OnClick="btnReply_Click" style="height: fit-content;" />
                            <asp:Button ID="btnDeleteReply" runat="server" Text="Delete Chat" CssClass="btn btn-danger" OnClick="btnDeleteReply_Click" OnClientClick="return confirm('Are you sure you want to delete this entire chat history?');" style="height: fit-content;" />
                        </div>
                    </div>
                </div>
            </asp:Panel>
        </div>
    </div>
    
    <script type="text/javascript">
        // Auto-scroll to bottom when chat loads
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
