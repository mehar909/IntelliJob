using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace IntelliJob.Admin
{
    public partial class UserQuery : System.Web.UI.Page
    {
        SqlConnection con;
        SqlCommand cmd;
        DataTable dt;
        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["admin"] == null)
            {
                Response.Redirect("../User/Login.aspx");
            }

            // Handle delete message postback
            if (Request["__EVENTTARGET"] == "DeleteMessage")
            {
                string eventArgument = Request["__EVENTARGUMENT"];
                if (!string.IsNullOrEmpty(eventArgument))
                {
                    string[] parts = eventArgument.Split('|');
                    if (parts.Length == 2)
                    {
                        int messageId = Convert.ToInt32(parts[0]);
                        string messageType = parts[1];
                        DeleteIndividualMessage(messageId, messageType);
                        return;
                    }
                }
            }

            if (!IsPostBack)
            {
                // Check for success message from Session (after redirect)
                if (Session["AdminChatMessage"] != null)
                {
                    lblMsg.Text = Session["AdminChatMessage"].ToString();
                    string messageType = Session["AdminChatMessageType"] != null ? Session["AdminChatMessageType"].ToString() : "success";
                    lblMsg.CssClass = "alert alert-" + messageType + " mb-0";
                    lblMsg.Visible = true;
                    
                    // Clear Session variables
                    Session["AdminChatMessage"] = null;
                    Session["AdminChatMessageType"] = null;
                    
                    // Auto-scroll and hide message
                    ScriptManager.RegisterStartupScript(this, GetType(), "hideMessage", 
                        "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 3000);", true);
                }
                
                // Check if a user is selected via query string
                if (Request.QueryString["userId"] != null)
                {
                    int userId = Convert.ToInt32(Request.QueryString["userId"]);
                    ShowUserChat(userId);
                    
                    // Auto-scroll to bottom after loading chat
                    ScriptManager.RegisterStartupScript(this, GetType(), "scrollToBottom", 
                        "setTimeout(function() { var container = document.getElementById('chatMessagesContainer'); if(container) container.scrollTop = container.scrollHeight; }, 100);", true);
                }
                else
                {
                    ShowUserList();
                }
            }
        }

        private void ShowUserList()
        {
            // First, clean up orphaned messages from deleted users
            CleanupOrphanedMessages();
            
            con = new SqlConnection(str);
            // Get unique users - one entry per user, only for users that still exist and have messages in AdminContact
            string query = @"SELECT DISTINCT 
                                c.UserId,
                                c.Name, 
                                c.Email
                            FROM AdminContact c
                            INNER JOIN Users u ON c.UserId = u.UserId
                            GROUP BY c.UserId, c.Name, c.Email
                            ORDER BY c.Name ASC";
            cmd = new SqlCommand(query, con);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            // Always create a new DataTable to avoid caching issues
            dt = new DataTable();
            sda.Fill(dt);
            
            // Clear and rebind the repeater
            rptUserList.DataSource = null;
            rptUserList.DataBind();
            
            if (dt.Rows.Count > 0)
            {
                rptUserList.DataSource = dt;
                rptUserList.DataBind();
                lblNoUsers.Visible = false;
            }
            else
            {
                rptUserList.DataSource = null;
                rptUserList.DataBind();
                lblNoUsers.Visible = true;
            }
        }

        private void ShowUserChat(int userId)
        {
            // Get user info - first try from AdminContact, if not found, get from User table
            con = new SqlConnection(str);
            string userName = "";
            string userEmail = "";
            
            // Try to get from AdminContact first
            string userQuery = @"SELECT DISTINCT Name, Email 
                                FROM AdminContact 
                                WHERE UserId = @UserId";
            cmd = new SqlCommand(userQuery, con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            con.Open();
            SqlDataReader userReader = cmd.ExecuteReader();
            if (userReader.Read())
            {
                userName = userReader["Name"].ToString();
                userEmail = userReader["Email"].ToString();
            }
            userReader.Close();
            con.Close();

            // If not found in AdminContact, get from User table
            if (string.IsNullOrEmpty(userName))
            {
                string getUserFromUserTableQuery = "SELECT Username, Email FROM Users WHERE UserId = @UserId";
                SqlCommand getUserFromUserTableCmd = new SqlCommand(getUserFromUserTableQuery, con);
                getUserFromUserTableCmd.Parameters.AddWithValue("@UserId", userId);
                con.Open();
                SqlDataReader userTableReader = getUserFromUserTableCmd.ExecuteReader();
                if (userTableReader.Read())
                {
                    userName = userTableReader["Username"].ToString();
                    userEmail = userTableReader["Email"].ToString();
                }
                userTableReader.Close();
                con.Close();
            }

            if (!string.IsNullOrEmpty(userName))
            {
                lblSelectedUserName.Text = userName + " (" + userEmail + ")";
            }

            // Get chat messages
            DataTable messagesTable = GetChatMessages(userId);
            if (messagesTable.Rows.Count > 0)
            {
                rptChatMessages.DataSource = messagesTable;
                rptChatMessages.DataBind();
            }

            // Store selected user ID in ViewState
            ViewState["SelectedUserId"] = userId;
            
            // Show chat panel, hide user list panel
            pnlChatSelected.Visible = true;
            pnlUserList.Visible = false;
            
            // Auto-scroll to bottom
            ScriptManager.RegisterStartupScript(this, GetType(), "scrollToBottom", 
                "setTimeout(function() { var container = document.getElementById('chatMessagesContainer'); if(container) container.scrollTop = container.scrollHeight; }, 100);", true);
        }

        protected int GetSelectedUserId()
        {
            if (ViewState["SelectedUserId"] != null)
            {
                return Convert.ToInt32(ViewState["SelectedUserId"]);
            }
            if (Request.QueryString["userId"] != null)
            {
                return Convert.ToInt32(Request.QueryString["userId"]);
            }
            return -1;
        }

        protected void rptUserList_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "SelectUser")
            {
                int userId = Convert.ToInt32(e.CommandArgument);
                ShowUserChat(userId);
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            // Clear selected user
            ViewState["SelectedUserId"] = null;
            
            // Show user list, hide chat
            pnlUserList.Visible = true;
            pnlChatSelected.Visible = false;
            
            // Refresh user list
            ShowUserList();
        }

        private void CleanupOrphanedMessages()
        {
            try
            {
                con = new SqlConnection(str);
                
                // Get all ContactIds from AdminContact where UserId doesn't exist in User table
                string getOrphanedContactIdsQuery = @"
                    SELECT c.ContactId 
                    FROM AdminContact c
                    LEFT JOIN Users u ON c.UserId = u.UserId
                    WHERE u.UserId IS NULL";
                
                cmd = new SqlCommand(getOrphanedContactIdsQuery, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                
                // Collect all orphaned contact IDs
                System.Collections.Generic.List<int> orphanedContactIds = new System.Collections.Generic.List<int>();
                while (reader.Read())
                {
                    orphanedContactIds.Add(reader.GetInt32(0));
                }
                reader.Close();
                con.Close();
                
                // Delete replies for orphaned contacts
                if (orphanedContactIds.Count > 0)
                {
                    foreach (int contactId in orphanedContactIds)
                    {
                        con = new SqlConnection(str);
                        string deleteReplyQuery = "DELETE FROM AdminReply WHERE ContactId = @ContactId";
                        cmd = new SqlCommand(deleteReplyQuery, con);
                        cmd.Parameters.AddWithValue("@ContactId", contactId);
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                    
                    // Delete orphaned contacts
                    foreach (int contactId in orphanedContactIds)
                {
                    con = new SqlConnection(str);
                        string deleteContactQuery = "DELETE FROM AdminContact WHERE ContactId = @ContactId";
                        cmd = new SqlCommand(deleteContactQuery, con);
                    cmd.Parameters.AddWithValue("@ContactId", contactId);
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't break the page
                System.Diagnostics.Debug.WriteLine("Error cleaning up orphaned messages: " + ex.Message);
            }
        }

        protected void btnReply_Click(object sender, EventArgs e)
        {
            int userId = GetSelectedUserId();
            if (userId > 0 && !string.IsNullOrEmpty(txtReply.Text.Trim()))
            {
                // Get user info - first try from AdminContact, if not found, get from User table
                con = new SqlConnection(str);
                string userName = "";
                string userEmail = "";
                
                // Try to get from AdminContact first
                string getUserInfoQuery = "SELECT TOP 1 Name, Email FROM AdminContact WHERE UserId = @UserId";
                SqlCommand getUserInfoCmd = new SqlCommand(getUserInfoQuery, con);
                getUserInfoCmd.Parameters.AddWithValue("@UserId", userId);
                con.Open();
                SqlDataReader userReader = getUserInfoCmd.ExecuteReader();
                if (userReader.Read())
                {
                    userName = userReader["Name"].ToString();
                    userEmail = userReader["Email"].ToString();
                }
                userReader.Close();
                con.Close();

                // If not found in AdminContact, get from User table
                if (string.IsNullOrEmpty(userName))
                {
                    string getUserFromUserTableQuery = "SELECT Username, Email FROM Users WHERE UserId = @UserId";
                    SqlCommand getUserFromUserTableCmd = new SqlCommand(getUserFromUserTableQuery, con);
                    getUserFromUserTableCmd.Parameters.AddWithValue("@UserId", userId);
                    con.Open();
                    SqlDataReader userTableReader = getUserFromUserTableCmd.ExecuteReader();
                    if (userTableReader.Read())
                    {
                        userName = userTableReader["Username"].ToString();
                        userEmail = userTableReader["Email"].ToString();
                    }
                    userTableReader.Close();
                    con.Close();
                }

                if (!string.IsNullOrEmpty(userName))
                {
                    // Create a new AdminContact entry for the admin message
                    string insertQuery = @"INSERT INTO AdminContact (UserId, Name, Email, Message, Date) 
                                          VALUES (@UserId, @Name, @Email, @Message, @Date)";
                    cmd = new SqlCommand(insertQuery, con);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Name", userName);
                    cmd.Parameters.AddWithValue("@Email", userEmail);
                    cmd.Parameters.AddWithValue("@Message", "[ADMIN_MESSAGE]" + txtReply.Text.Trim());
                    cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                    con.Open();
                    int r = cmd.ExecuteNonQuery();
                    con.Close();

                    if (r > 0)
                    {
                        // Store success message in Session to display after redirect
                        Session["AdminChatMessage"] = "Reply sent successfully.";
                        Session["AdminChatMessageType"] = "success";
                        
                        // Redirect to prevent duplicate submission on refresh
                        Response.Redirect("UserQuery.aspx?userId=" + userId, false);
                    }
                    else
                    {
                        lblMsg.Text = "Failed to send reply.";
                        lblMsg.CssClass = "alert alert-danger mb-0";
                        lblMsg.Visible = true;
                    }
                }
                else
                {
                    lblMsg.Text = "User not found.";
                    lblMsg.CssClass = "alert alert-warning mb-0";
                    lblMsg.Visible = true;
                    }
                }
                else
                {
                    lblMsg.Text = "Reply message cannot be empty.";
                lblMsg.CssClass = "alert alert-warning mb-0";
                lblMsg.Visible = true;
            }
        }

        protected void btnDeleteReply_Click(object sender, EventArgs e)
        {
            int userId = GetSelectedUserId();
            if (userId > 0)
            {
                try
                {
                    // Get all ContactIds for this user first
                    con = new SqlConnection(str);
                    string getContactIdsQuery = "SELECT ContactId FROM AdminContact WHERE UserId = @UserId";
                    cmd = new SqlCommand(getContactIdsQuery, con);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    
                    // Collect all contact IDs first
                    System.Collections.Generic.List<int> contactIds = new System.Collections.Generic.List<int>();
                    while (reader.Read())
                    {
                        contactIds.Add(reader.GetInt32(0));
                    }
                    reader.Close();
                    con.Close();
                    
                    // Delete old format replies for all contact IDs
                    int deletedReplyCount = 0;
                    if (contactIds.Count > 0)
                    {
                        foreach (int contactId in contactIds)
                        {
                con = new SqlConnection(str);
                string deleteQuery = "DELETE FROM AdminReply WHERE ContactId = @ContactId";
                cmd = new SqlCommand(deleteQuery, con);
                cmd.Parameters.AddWithValue("@ContactId", contactId);
                            con.Open();
                            deletedReplyCount += cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }

                    // Delete all AdminContact entries for this user (includes both user messages and admin messages)
                    con = new SqlConnection(str);
                    string deleteContactQuery = "DELETE FROM AdminContact WHERE UserId = @UserId";
                    cmd = new SqlCommand(deleteContactQuery, con);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                con.Open();
                    int deletedContactCount = cmd.ExecuteNonQuery();
                con.Close();

                    // Always refresh the user list after deletion attempt
                    // Clear selected user
                    ViewState["SelectedUserId"] = null;
                    
                    // Show user list, hide chat
                    pnlUserList.Visible = true;
                    pnlChatSelected.Visible = false;
                    
                    // Refresh user list to remove the user who no longer has messages
                    ShowUserList();

                    if (deletedContactCount > 0 || deletedReplyCount > 0)
                    {
                        lblMsg.Text = "Chat deleted successfully.";
                        lblMsg.CssClass = "alert alert-success mb-0";
                        lblMsg.Visible = true;
                        
                        ScriptManager.RegisterStartupScript(this, GetType(), "hideMessage", 
                            "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 3000);", true);
                }
                else
                {
                        lblMsg.Text = "No chat found to delete.";
                        lblMsg.CssClass = "alert alert-info mb-0";
                        lblMsg.Visible = true;
                    }
                }
                catch (Exception ex)
                {
                    lblMsg.Text = "Error deleting chat: " + ex.Message;
                    lblMsg.CssClass = "alert alert-danger mb-0";
                    lblMsg.Visible = true;
                    
                    // Still refresh the list
                    ViewState["SelectedUserId"] = null;
                    pnlUserList.Visible = true;
                    pnlChatSelected.Visible = false;
                    ShowUserList();
                }
            }
        }

        public DataTable GetChatMessages(int userId)
        {
            con = new SqlConnection(str);
            // Get all messages from this user and admin messages, ordered chronologically
            // Admin messages are prefixed with [ADMIN_MESSAGE]
            // Also include old format replies for backward compatibility
            string query = @"
                SELECT 
                    c.ContactId,
                    c.Message,
                    c.Date,
                    CASE 
                        WHEN c.Message LIKE '[ADMIN_MESSAGE]%' AND LEN(c.Message) > 15 THEN SUBSTRING(c.Message, 16, LEN(c.Message) - 15)
                        WHEN c.Message NOT LIKE '[ADMIN_MESSAGE]%' THEN ISNULL(r.ReplyMessage, '')
                        ELSE ''
                    END AS ReplyMessage,
                    CASE 
                        WHEN c.Message LIKE '[ADMIN_MESSAGE]%' THEN c.Date
                        WHEN c.Message NOT LIKE '[ADMIN_MESSAGE]%' THEN r.ReplyDate
                        ELSE NULL
                    END AS ReplyDate,
                    CASE 
                        WHEN c.Message NOT LIKE '[ADMIN_MESSAGE]%' THEN r.ReplyId
                        ELSE NULL
                    END AS ReplyId,
                    CASE 
                        WHEN c.Message LIKE '[ADMIN_MESSAGE]%' THEN 1
                        ELSE 0
                    END AS IsAdminMessage,
                    ISNULL(u.Username, c.Name) AS Username
                FROM AdminContact c
                LEFT JOIN AdminReply r ON c.ContactId = r.ContactId AND c.Message NOT LIKE '[ADMIN_MESSAGE]%'
                LEFT JOIN Users u ON c.UserId = u.UserId
                WHERE c.UserId = @UserId
                ORDER BY c.Date ASC";
            cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable messagesTable = new DataTable();
            sda.Fill(messagesTable);
            return messagesTable;
        }

        protected string FormatChatMessage(object contactIdObj, object messageObj, object dateObj, object replyMessageObj, object replyDateObj, object replyIdObj, object isAdminMessageObj, object usernameObj)
        {
            int contactId = contactIdObj != null && contactIdObj != DBNull.Value ? Convert.ToInt32(contactIdObj) : 0;
            string message = messageObj != null && messageObj != DBNull.Value ? messageObj.ToString() : "";
            string replyMessage = replyMessageObj != null && replyMessageObj != DBNull.Value ? replyMessageObj.ToString() : "";
            int? replyId = replyIdObj != null && replyIdObj != DBNull.Value ? (int?)Convert.ToInt32(replyIdObj) : null;
            
            // Determine if this is an admin message - ONLY if message starts with [ADMIN_MESSAGE] prefix
            // This is the definitive check - user messages NEVER have this prefix
            bool hasAdminPrefix = !string.IsNullOrEmpty(message) && message.StartsWith("[ADMIN_MESSAGE]");
            // Only treat as admin message if it has the prefix
            // This ensures user messages are never incorrectly identified as admin messages
            bool isAdminMessage = hasAdminPrefix;
            
            // Old format reply: has reply message but is NOT an admin message (from AdminReply table)
            // Only show old format reply if the original message is a user message (no admin prefix)
            bool hasOldFormatReply = !string.IsNullOrEmpty(replyMessage) && !isAdminMessage && replyId.HasValue;
            string username = usernameObj != null && usernameObj != DBNull.Value ? usernameObj.ToString() : "User";
            
            string output = "";
            string deleteButtonStyle = "background: none; border: none; color: #dc3545; cursor: pointer; padding: 2px 5px; font-size: 12px; opacity: 0.7; margin-left: 5px;";
            string deleteButtonHover = "onmouseover=\"this.style.opacity='1'\" onmouseout=\"this.style.opacity='0.7'\"";
            
            if (isAdminMessage)
            {
                // This is an admin message (new format)
                // The query extracts the message into ReplyMessage, so use that first
                // If ReplyMessage is empty, extract from Message field
                string adminMsg = "";
                if (!string.IsNullOrEmpty(replyMessage))
                {
                    // Use the extracted message from the query
                    adminMsg = replyMessage;
                }
                else if (message.StartsWith("[ADMIN_MESSAGE]") && message.Length > 15)
                {
                    // Fallback: extract from Message field if ReplyMessage wasn't populated
                    adminMsg = message.Substring(15); // Remove [ADMIN_MESSAGE] prefix (15 characters)
                }
                else
                {
                    // Use message as-is if no prefix (shouldn't happen for admin messages)
                    adminMsg = message;
                }
                string dateStr = dateObj != null && dateObj != DBNull.Value ? Convert.ToDateTime(dateObj).ToString("MMM dd, HH:mm") : "";
                string deleteButtonStyleWhite = "background: none; border: none; color: #ffffff; cursor: pointer; padding: 2px 5px; font-size: 12px; opacity: 0.9; margin-left: 5px;";
                string deleteBtn = "<button type=\"button\" " + deleteButtonHover + " style=\"" + deleteButtonStyleWhite + "\" onclick=\"deleteMessage('" + contactId + "', 'admin')\" title=\"Delete message\"><i class=\"fas fa-trash\"></i></button>";
                output = "<div class=\"d-flex justify-content-end mb-2\" style=\"width: 100%;\"><div class=\"message-bubble admin-message\" style=\"position: relative;\"><div>" + Server.HtmlEncode(adminMsg) + " " + deleteBtn + "</div><div class=\"message-time text-right\" style=\"color: rgba(255,255,255,0.9);\">" + dateStr + "</div></div></div>";
            }
            else if (hasOldFormatReply)
            {
                // Old format: User message with separate reply (from AdminReply table)
                string dateStr = dateObj != null && dateObj != DBNull.Value ? Convert.ToDateTime(dateObj).ToString("MMM dd, HH:mm") : "";
                string replyDateStr = replyDateObj != null && replyDateObj != DBNull.Value ? Convert.ToDateTime(replyDateObj).ToString("MMM dd, HH:mm") : "";
                string userDeleteBtn = "<button type=\"button\" " + deleteButtonHover + " style=\"" + deleteButtonStyle + "\" onclick=\"deleteMessage('" + contactId + "', 'user')\" title=\"Delete message\"><i class=\"fas fa-trash\"></i></button>";
                string deleteButtonStyleWhite = "background: none; border: none; color: #ffffff; cursor: pointer; padding: 2px 5px; font-size: 12px; opacity: 0.7; margin-left: 5px;";
                string replyDeleteBtn = replyId.HasValue ? "<button type=\"button\" " + deleteButtonHover + " style=\"" + deleteButtonStyleWhite + "\" onclick=\"deleteMessage('" + replyId.Value + "', 'reply')\" title=\"Delete reply\"><i class=\"fas fa-trash\"></i></button>" : "";
                output = "<div class=\"d-flex justify-content-start mb-2\" style=\"width: 100%;\"><div class=\"message-bubble user-message\" style=\"position: relative; margin-left: 0; margin-right: auto;\"><div style=\"font-weight: bold; color: #007bff; margin-bottom: 5px;\">" + Server.HtmlEncode(username) + " " + userDeleteBtn + "</div><div>" + Server.HtmlEncode(message) + "</div><div class=\"message-time\">" + dateStr + "</div></div></div>";
                output += "<div class=\"d-flex justify-content-end mb-2\" style=\"width: 100%;\"><div class=\"message-bubble admin-message\" style=\"position: relative;\"><div>" + Server.HtmlEncode(replyMessage) + " " + replyDeleteBtn + "</div><div class=\"message-time text-right\" style=\"color: rgba(255,255,255,0.9);\">" + replyDateStr + "</div></div></div>";
            }
            else
            {
                // Regular user message (no admin reply)
                string dateStr = dateObj != null && dateObj != DBNull.Value ? Convert.ToDateTime(dateObj).ToString("MMM dd, HH:mm") : "";
                string userDeleteBtn = "<button type=\"button\" " + deleteButtonHover + " style=\"" + deleteButtonStyle + "\" onclick=\"deleteMessage('" + contactId + "', 'user')\" title=\"Delete message\"><i class=\"fas fa-trash\"></i></button>";
                output = "<div class=\"d-flex justify-content-start mb-2\" style=\"width: 100%;\"><div class=\"message-bubble user-message\" style=\"position: relative; margin-left: 0; margin-right: auto;\"><div style=\"font-weight: bold; color: #007bff; margin-bottom: 5px;\">" + Server.HtmlEncode(username) + " " + userDeleteBtn + "</div><div>" + Server.HtmlEncode(message) + "</div><div class=\"message-time\">" + dateStr + "</div></div></div>";
            }
            
            return output;
        }

        protected void rptChatMessages_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            // This method can be used if we switch to server-side buttons
            // For now, we're using client-side JavaScript with __doPostBack
        }

        protected void DeleteIndividualMessage(int messageId, string messageType)
        {
            try
            {
                int userId = GetSelectedUserId();
                if (userId > 0)
                {
                    if (messageType == "admin" || messageType == "user")
                    {
                        // Delete from AdminContact
                        con = new SqlConnection(str);
                        string deleteQuery = "DELETE FROM AdminContact WHERE ContactId = @ContactId";
                        cmd = new SqlCommand(deleteQuery, con);
                        cmd.Parameters.AddWithValue("@ContactId", messageId);
                        con.Open();
                        int deleted = cmd.ExecuteNonQuery();
                        con.Close();

                        // Also delete any associated replies
                        if (deleted > 0)
                        {
                            con = new SqlConnection(str);
                            string deleteReplyQuery = "DELETE FROM AdminReply WHERE ContactId = @ContactId";
                            cmd = new SqlCommand(deleteReplyQuery, con);
                            cmd.Parameters.AddWithValue("@ContactId", messageId);
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                    else if (messageType == "reply")
                    {
                        // Delete from AdminReply only
                        con = new SqlConnection(str);
                        string deleteQuery = "DELETE FROM AdminReply WHERE ReplyId = @ReplyId";
                        cmd = new SqlCommand(deleteQuery, con);
                        cmd.Parameters.AddWithValue("@ReplyId", messageId);
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }

                    // Check if there are any remaining messages for this user
                    DataTable remainingMessages = GetChatMessages(userId);
                    
                    if (remainingMessages.Rows.Count == 0)
                    {
                        // No messages left, close chat and show user list
                        ViewState["SelectedUserId"] = null;
                        pnlUserList.Visible = true;
                        pnlChatSelected.Visible = false;
                        ShowUserList();
                        lblMsg.Text = "Message deleted. User removed from list as no messages remain.";
                        lblMsg.CssClass = "alert alert-info mb-0";
                        lblMsg.Visible = true;
                    }
                    else
                    {
                        // Still have messages, refresh the chat
                        ShowUserChat(userId);
                        lblMsg.Text = "Message deleted successfully.";
                        lblMsg.CssClass = "alert alert-success mb-0";
                        lblMsg.Visible = true;
                    }
                    
                    ScriptManager.RegisterStartupScript(this, GetType(), "hideMessage", 
                        "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 3000);", true);
                }
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Error deleting message: " + ex.Message;
                lblMsg.CssClass = "alert alert-danger mb-0";
                lblMsg.Visible = true;
            }
        }


        protected void btnCleanup_Click(object sender, EventArgs e)
        {
            try
            {
                int deletedCount = 0;
                con = new SqlConnection(str);
                
                // Get all ContactIds from AdminContact where UserId doesn't exist in User table
                string getOrphanedContactIdsQuery = @"
                    SELECT c.ContactId 
                    FROM AdminContact c
                    LEFT JOIN Users u ON c.UserId = u.UserId
                    WHERE u.UserId IS NULL";
                
                cmd = new SqlCommand(getOrphanedContactIdsQuery, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                
                // Collect all orphaned contact IDs
                System.Collections.Generic.List<int> orphanedContactIds = new System.Collections.Generic.List<int>();
                while (reader.Read())
                {
                    orphanedContactIds.Add(reader.GetInt32(0));
                }
                reader.Close();
                con.Close();
                
                // Delete replies for orphaned contacts
                foreach (int contactId in orphanedContactIds)
                {
                    con = new SqlConnection(str);
                    string deleteReplyQuery = "DELETE FROM AdminReply WHERE ContactId = @ContactId";
                    cmd = new SqlCommand(deleteReplyQuery, con);
                    cmd.Parameters.AddWithValue("@ContactId", contactId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
                
                // Delete orphaned contacts
                foreach (int contactId in orphanedContactIds)
                {
                    con = new SqlConnection(str);
                    string deleteContactQuery = "DELETE FROM AdminContact WHERE ContactId = @ContactId";
                    cmd = new SqlCommand(deleteContactQuery, con);
                    cmd.Parameters.AddWithValue("@ContactId", contactId);
                    con.Open();
                    deletedCount += cmd.ExecuteNonQuery();
                    con.Close();
                }
                
                if (deletedCount > 0)
                {
                    lblMsg.Text = $"Successfully removed {deletedCount} chat message(s) from deleted users.";
                    lblMsg.CssClass = "alert alert-success";
                    ShowUserList();
                    
                    ScriptManager.RegisterStartupScript(this, GetType(), "hideMessage", 
                        "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 5000);", true);
                }
                else
                {
                    lblMsg.Text = "No orphaned messages found. All chat messages belong to active users.";
                    lblMsg.CssClass = "alert alert-info";
                    
                    ScriptManager.RegisterStartupScript(this, GetType(), "hideMessage", 
                        "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 5000);", true);
                }
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Error cleaning up orphaned messages: " + ex.Message;
                lblMsg.CssClass = "alert alert-danger";
            }
        }
    }
}
