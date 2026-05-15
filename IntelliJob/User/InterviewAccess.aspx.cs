using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.SessionState;

namespace IntelliJob.User
{
    public partial class InterviewAccess : System.Web.UI.Page
    {

        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["user"] == null || Session["role"]?.ToString() != "JobSeeker")
            {
                Response.Redirect("Login.aspx?returnUrl=" + Server.UrlEncode(Request.RawUrl));
                return;
            }

            if (!IsPostBack)
            {
                string token = Request.QueryString["token"];
                if (string.IsNullOrEmpty(token))
                {
                    ShowError("Invalid invitation link. Please use the link from your email.");
                    return;
                }

                // Store token in hidden field so it survives postback
                // GUIDs are exactly 36 chars — trim any junk appended by email clients
                hdnJobId.Value = token.Length > 36 ? token.Substring(0, 36) : token;

                if (!string.IsNullOrEmpty(Request.QueryString["pwd"]))
                    txtPassword.Text = Request.QueryString["pwd"];
            }
        }

        protected void btnEnter_Click(object sender, EventArgs e)
        {
            string enteredPwd = txtPassword.Text.Trim();
            string token      = hdnJobId.Value;
            int    userId     = Convert.ToInt32(Session["userId"]);

            if (string.IsNullOrEmpty(enteredPwd))
            {
                ShowError("Please enter your interview password.");
                return;
            }

            if (string.IsNullOrEmpty(token))
            {
                ShowError("Invalid invitation link. Please use the link from your email.");
                return;
            }

            int    interviewId  = 0;
            int    ownerUserId  = 0;
            string salt         = "";
            string storedHash   = "";
            bool   isUsed       = false;
            bool   found        = false;

            using (SqlConnection con = new SqlConnection(str))
            {
                con.Open();

                // Look up invitation by AccessToken — InterviewId is the PK
                string lookupSql = @"
                    SELECT  ii.InterviewId,
                            ii.UserId,
                            ii.PasswordSalt,
                            ii.PasswordHash,
                            ii.IsPasswordUsed
                    FROM    InterviewInvitations ii
                    WHERE   ii.AccessToken = @Token";

                using (SqlCommand cmd = new SqlCommand(lookupSql, con))
                {
                    cmd.Parameters.AddWithValue("@Token", token);
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            found       = true;
                            interviewId = Convert.ToInt32(rdr["InterviewId"]);
                            ownerUserId = Convert.ToInt32(rdr["UserId"]);
                            salt        = rdr["PasswordSalt"].ToString();
                            storedHash  = rdr["PasswordHash"].ToString();
                            isUsed      = Convert.ToBoolean(rdr["IsPasswordUsed"]);
                        }
                    }
                }

                if (!found)
                {
                    ShowError("Invalid invitation link. Please use the link from your email.");
                    return;
                }

                if (ownerUserId != userId)
                {
                    ShowError("This invitation does not belong to your account.");
                    return;
                }

                if (isUsed)
                {
                    ShowError("This interview password has already been used. Please contact the company if you believe this is an error.");
                    return;
                }

                // Verify password using the stored per-invitation hash.
                string enteredHash = Utils.ComputeSha256Hash(enteredPwd + salt);
                if (!string.Equals(enteredHash, storedHash, StringComparison.OrdinalIgnoreCase))
                {
                    ShowError("Invalid password. Please check your invitation email and try again.");
                    return;
                }

                Session["AuthorizedInterviewId"] = interviewId;
            }

            Response.Redirect("TakeInterview.aspx?id=" + interviewId);
        }

        private void ShowError(string msg)
        {
            lblError.Text    = msg;
            lblError.Visible = true;
        }
    }
}
