using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IntelliJob.User
{
    public partial class Profile : System.Web.UI.Page
    {
        SqlConnection con;
        SqlCommand cmd;
        SqlDataAdapter sda;
        DataTable dt;
        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        private void showUserProfile()
        {
            con = new SqlConnection(str);
            string query = @"SELECT u.UserId, u.Username, js.Name, u.Address, js.Mobile, u.Email, u.Country, js.Resume, js.Photo 
                             FROM Users u 
                             LEFT JOIN JobSeekers js ON u.UserId = js.ProfileId 
                             WHERE u.Username = @username";
            cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@username", Session["user"]);
            sda = new SqlDataAdapter(cmd);
            dt = new DataTable();
            sda.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                dlProfile.DataSource = dt;
                dlProfile.DataBind();
            }
            else
            {
                Response.Write("<script>alert('Please login again.');</script>");
            }
        }

        private void ShowUserReplies()
        {
            con = new SqlConnection(str);
            // Get all messages - user messages and admin messages (with [ADMIN_MESSAGE] prefix)
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
                WHERE c.UserId = (SELECT UserId FROM Users WHERE Username = @username)
                ORDER BY c.Date ASC";

            cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@username", Session["user"]);
            sda = new SqlDataAdapter(cmd);
            DataTable repliesTable = new DataTable();
            sda.Fill(repliesTable);

            // Clear the Repeater first to ensure fresh data
            rptChat.DataSource = null;
            rptChat.DataBind();

            if (repliesTable.Rows.Count > 0)
            {
                rptChat.DataSource = repliesTable;
                rptChat.DataBind();
                noMessages.Visible = false;
            }
            else
            {
                noMessages.Visible = true;
            }

            // Auto-scroll to bottom after binding
            ScriptManager.RegisterStartupScript(this, GetType(), "scrollToBottom", 
                "setTimeout(function() { var container = document.getElementById('chatMessagesContainer'); if(container) container.scrollTop = container.scrollHeight; }, 100);", true);
        }

        protected void dlProfile_ItemCommand(object source, DataListCommandEventArgs e)
        {
            if (e.CommandName == "EditUserProfile")
            {
                Response.Redirect("ResumeBuild.aspx?id=" + e.CommandArgument.ToString());
            }
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
                if (txtMessage.Text.Trim() != "")
                {
                    con = new SqlConnection(str);
                string queryProfile = @"SELECT u.UserId, js.Name, u.Email 
                                        FROM Users u 
                                        LEFT JOIN JobSeekers js ON u.UserId = js.ProfileId 
                                        WHERE u.Username = @username";
                    cmd = new SqlCommand(queryProfile, con);
                    cmd.Parameters.AddWithValue("@username", Session["user"]);
                    sda = new SqlDataAdapter(cmd);
                    dt = new DataTable();
                    sda.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];

                        string queryInsert = "INSERT INTO AdminContact (UserId, Name, Email, Message, Date) VALUES (@UserId, @Name, @Email, @Message, @Date)";
                        SqlCommand cmdInsert = new SqlCommand(queryInsert, con);

                        cmdInsert.Parameters.AddWithValue("@UserId", row["UserId"]);
                        cmdInsert.Parameters.AddWithValue("@Name", row["Name"]);
                        cmdInsert.Parameters.AddWithValue("@Email", row["Email"]);
                        cmdInsert.Parameters.AddWithValue("@Message", txtMessage.Text.Trim());
                        cmdInsert.Parameters.AddWithValue("@Date", DateTime.Now);

                        con.Open();
                        int r = cmdInsert.ExecuteNonQuery();
                        con.Close();

                        if (r > 0)
                        {
                            // Store success message in Session to display after redirect
                            Session["ChatMessage"] = "Message sent to admin successfully.";
                            Session["ChatMessageType"] = "success";
                            
                            // Redirect to prevent duplicate submission on refresh
                            Response.Redirect("Profile.aspx", false);
                        }
                        else
                        {
                            lblMsg.Text = "Failed to send message. Try again.";
                            lblMsg.CssClass = "alert alert-danger mb-2";
                            lblMsg.Visible = true;
                        }
                    }
                    else
                    {
                        lblMsg.Text = "User data not found.";
                    lblMsg.CssClass = "alert alert-danger mb-2";
                    lblMsg.Visible = true;
                }
            }
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
            // Only show old format reply if:
            // 1. The original message is a user message (no admin prefix)
            // 2. There's actually a reply message
            // 3. The reply has a valid ReplyId (to ensure it's a real reply, not just empty string)
            bool hasOldFormatReply = !isAdminMessage && !string.IsNullOrEmpty(replyMessage) && replyId.HasValue;
            string username = usernameObj != null && usernameObj != DBNull.Value ? usernameObj.ToString() : "User";
            
            string output = "";
            string deleteButtonStyle = "background: none; border: none; color: #ffffff; cursor: pointer; padding: 2px 5px; font-size: 12px; opacity: 0.7; margin-left: 5px;";
            string deleteButtonStyleRed = "background: none; border: none; color: #dc3545; cursor: pointer; padding: 2px 5px; font-size: 12px; opacity: 0.7; margin-left: 5px;";
            string deleteButtonHover = "onmouseover=\"this.style.opacity='1'\" onmouseout=\"this.style.opacity='0.7'\"";
            
            if (isAdminMessage)
            {
                // This is an admin message (new format)
                string adminMsg = "";
                if (!string.IsNullOrEmpty(replyMessage))
                {
                    adminMsg = replyMessage;
                }
                else if (message.StartsWith("[ADMIN_MESSAGE]") && message.Length > 15)
                {
                    adminMsg = message.Substring(15);
                }
                else
                {
                    adminMsg = message;
                }
                string dateStr = dateObj != null && dateObj != DBNull.Value ? Convert.ToDateTime(dateObj).ToString("MMM dd, HH:mm") : "";
                string deleteBtn = "<button type=\"button\" " + deleteButtonHover + " style=\"" + deleteButtonStyleRed + "\" onclick=\"deleteMessage('" + contactId + "', 'admin')\" title=\"Delete message\"><i class=\"fas fa-trash\"></i></button>";
                output = "<div class=\"d-flex justify-content-start mb-2\" style=\"width: 100%;\"><div class=\"message-bubble admin-message\" style=\"position: relative; margin-left: 0; margin-right: auto;\"><div style=\"font-weight: bold; color: #7200cf; margin-bottom: 5px; font-size: 14px;\">Admin " + deleteBtn + "</div><div>" + Server.HtmlEncode(adminMsg) + "</div><div class=\"message-time\">" + dateStr + "</div></div></div>";
            }
            else if (hasOldFormatReply)
            {
                // Old format: User message with separate reply (from AdminReply table)
                string dateStr = dateObj != null && dateObj != DBNull.Value ? Convert.ToDateTime(dateObj).ToString("MMM dd, HH:mm") : "";
                string replyDateStr = replyDateObj != null && replyDateObj != DBNull.Value ? Convert.ToDateTime(replyDateObj).ToString("MMM dd, HH:mm") : "";
                string userDeleteBtn = "<button type=\"button\" " + deleteButtonHover + " style=\"" + deleteButtonStyle + "\" onclick=\"deleteMessage('" + contactId + "', 'user')\" title=\"Delete message\"><i class=\"fas fa-trash\"></i></button>";
                string replyDeleteBtn = replyId.HasValue ? "<button type=\"button\" " + deleteButtonHover + " style=\"" + deleteButtonStyleRed + "\" onclick=\"deleteMessage('" + replyId.Value + "', 'reply')\" title=\"Delete reply\"><i class=\"fas fa-trash\"></i></button>" : "";
                output = "<div class=\"d-flex justify-content-end mb-2\" style=\"width: 100%;\"><div class=\"message-bubble user-message\" style=\"position: relative;\"><div>" + Server.HtmlEncode(message) + " " + userDeleteBtn + "</div><div class=\"message-time text-right\" style=\"color: rgba(255,255,255,0.9);\">" + dateStr + "</div></div></div>";
                output += "<div class=\"d-flex justify-content-start mb-2\" style=\"width: 100%;\"><div class=\"message-bubble admin-message\" style=\"position: relative; margin-left: 0; margin-right: auto;\"><div style=\"font-weight: bold; color: #7200cf; margin-bottom: 5px; font-size: 14px;\">Admin " + replyDeleteBtn + "</div><div>" + Server.HtmlEncode(replyMessage) + "</div><div class=\"message-time\">" + replyDateStr + "</div></div></div>";
            }
            else
            {
                // Regular user message (no admin reply)
                string dateStr = dateObj != null && dateObj != DBNull.Value ? Convert.ToDateTime(dateObj).ToString("MMM dd, HH:mm") : "";
                string userDeleteBtn = "<button type=\"button\" " + deleteButtonHover + " style=\"" + deleteButtonStyle + "\" onclick=\"deleteMessage('" + contactId + "', 'user')\" title=\"Delete message\"><i class=\"fas fa-trash\"></i></button>";
                output = "<div class=\"d-flex justify-content-end mb-2\" style=\"width: 100%;\"><div class=\"message-bubble user-message\" style=\"position: relative;\"><div>" + Server.HtmlEncode(message) + " " + userDeleteBtn + "</div><div class=\"message-time text-right\" style=\"color: rgba(255,255,255,0.9);\">" + dateStr + "</div></div></div>";
            }
            
            return output;
        }

        protected void btnDeleteChat_Click(object sender, EventArgs e)
        {
            try
            {
                int userId = -1;
                con = new SqlConnection(str);
                string getUserQuery = "SELECT UserId FROM Users WHERE Username = @username";
                cmd = new SqlCommand(getUserQuery, con);
                cmd.Parameters.AddWithValue("@username", Session["user"]);
                con.Open();
                object userIdObj = cmd.ExecuteScalar();
                con.Close();

                if (userIdObj != null)
                {
                    userId = Convert.ToInt32(userIdObj);

                    // Get all ContactIds for this user first
                    con = new SqlConnection(str);
                    string getContactIdsQuery = "SELECT ContactId FROM AdminContact WHERE UserId = @UserId";
                    cmd = new SqlCommand(getContactIdsQuery, con);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    
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

                    // Delete all AdminContact entries for this user
                    con = new SqlConnection(str);
                    string deleteContactQuery = "DELETE FROM AdminContact WHERE UserId = @UserId";
                    cmd = new SqlCommand(deleteContactQuery, con);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    con.Open();
                    int deletedContactCount = cmd.ExecuteNonQuery();
                    con.Close();

                    if (deletedContactCount > 0 || deletedReplyCount > 0)
                    {
                        lblMsg.Text = "Chat deleted successfully.";
                        lblMsg.CssClass = "alert alert-success mb-2";
                        lblMsg.Visible = true;
                        
                        // Clear and refresh the chat
                        rptChat.DataSource = null;
                        rptChat.DataBind();
                        ShowUserReplies();
                        
                        ScriptManager.RegisterStartupScript(this, GetType(), "hideMessage", 
                            "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 3000);", true);
                    }
                    else
                    {
                        lblMsg.Text = "No chat found to delete.";
                        lblMsg.CssClass = "alert alert-info mb-2";
                        lblMsg.Visible = true;
                        
                        // Still refresh to ensure UI is updated
                        rptChat.DataSource = null;
                        rptChat.DataBind();
                        ShowUserReplies();
                    }
                }
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Error deleting chat: " + ex.Message;
                lblMsg.CssClass = "alert alert-danger mb-2";
                lblMsg.Visible = true;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["user"] == null)
            {
                Response.Redirect("Login.aspx");
            }

            if (!IsPostBack)
            {
                showUserProfile();
                ShowUserReplies();
                
                // Check for success message from Session (after redirect)
                if (Session["ChatMessage"] != null)
                {
                    lblMsg.Text = Session["ChatMessage"].ToString();
                    string messageType = Session["ChatMessageType"] != null ? Session["ChatMessageType"].ToString() : "success";
                    lblMsg.CssClass = "alert alert-" + messageType + " mb-2";
                    lblMsg.Visible = true;
                    
                    // Clear Session variables
                    Session["ChatMessage"] = null;
                    Session["ChatMessageType"] = null;
                    
                    // Auto-scroll and hide message
                    ScriptManager.RegisterStartupScript(this, GetType(), "scrollAndHide", 
                        "setTimeout(function() { var container = document.getElementById('chatMessagesContainer'); if(container) container.scrollTop = container.scrollHeight; var lbl = document.getElementById('" + lblMsg.ClientID + "'); if(lbl) lbl.style.display='none'; }, 3000);", true);
                }
            }
            else
            {
                // Handle delete message postback
                string eventTarget = Request["__EVENTTARGET"];
                string eventArgument = Request["__EVENTARGUMENT"];
                
                if (eventTarget == "DeleteMessage" && !string.IsNullOrEmpty(eventArgument))
                {
                    string[] args = eventArgument.Split('|');
                    if (args.Length == 2)
                    {
                        int messageId = Convert.ToInt32(args[0]);
                        string messageType = args[1];
                        DeleteIndividualMessage(messageId, messageType);
                    }
                }
            }
        }

        protected void DeleteIndividualMessage(int messageId, string messageType)
        {
            try
            {
                int userId = -1;
                con = new SqlConnection(str);
                string getUserQuery = "SELECT UserId FROM Users WHERE Username = @username";
                cmd = new SqlCommand(getUserQuery, con);
                cmd.Parameters.AddWithValue("@username", Session["user"]);
                con.Open();
                object userIdObj = cmd.ExecuteScalar();
                con.Close();

                if (userIdObj != null)
                {
                    userId = Convert.ToInt32(userIdObj);

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
                        lblMsg.Text = "Message deleted. Chat cleared as no messages remain.";
                        lblMsg.CssClass = "alert alert-info mb-2";
                        lblMsg.Visible = true;
                    }
                    else
                    {
                        lblMsg.Text = "Message deleted successfully.";
                        lblMsg.CssClass = "alert alert-success mb-2";
                        lblMsg.Visible = true;
                    }
                    
                    // Clear and refresh the chat
                    rptChat.DataSource = null;
                    rptChat.DataBind();
                    ShowUserReplies();
                    
                    ScriptManager.RegisterStartupScript(this, GetType(), "hideMessage", 
                        "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 3000);", true);
                }
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Error deleting message: " + ex.Message;
                lblMsg.CssClass = "alert alert-danger mb-2";
                lblMsg.Visible = true;
            }
        }

        public DataTable GetChatMessages(int userId)
        {
            con = new SqlConnection(str);
            string query = @"
                SELECT 
                    c.ContactId,
                    c.Message,
                    c.Date,
                    CASE 
                        WHEN c.Message LIKE '[ADMIN_MESSAGE]%' AND LEN(c.Message) > 15 THEN SUBSTRING(c.Message, 16, LEN(c.Message) - 15)
                        ELSE ISNULL(r.ReplyMessage, '')
                    END AS ReplyMessage,
                    CASE 
                        WHEN c.Message LIKE '[ADMIN_MESSAGE]%' THEN c.Date
                        ELSE r.ReplyDate
                    END AS ReplyDate,
                    r.ReplyId,
                    CASE 
                        WHEN c.Message LIKE '[ADMIN_MESSAGE]%' THEN 1
                        ELSE 0
                    END AS IsAdminMessage,
                    ISNULL(u.Username, c.Name) AS Username
                FROM AdminContact c
                LEFT JOIN AdminReply r ON c.ContactId = r.ContactId
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
    }
}
