using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IntelliJob.User
{
    public partial class Login : System.Web.UI.Page
    {
        SqlConnection con;
        SqlCommand cmd;
        SqlDataReader sdr;
        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;
        string username, password = string.Empty;
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                if(ddlLoginType.SelectedValue == "Admin")
                {
                    username = ConfigurationManager.AppSettings["username"];
                    password = ConfigurationManager.AppSettings["password"];
                    if (username == txtUserName.Text.Trim() && password == txtPassword.Text.Trim())
                    {
                        Session["admin"] = username;
                        Response.Redirect("../Admin/Dashboard.aspx",false);
                    }
                    else
                    {
                        showErrorMessage("Admin");
                    }
                }
                else
                {
                    con = new SqlConnection(str);
                    string query = @"SELECT UserId, Username, Role, IsActive, Password 
                 FROM Users 
                 WHERE Username = @Username";

                    cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Username", txtUserName.Text.Trim());
                    con.Open();
                    sdr = cmd.ExecuteReader();
                    if (sdr.Read())
                    {
                        object userIdVal = sdr["UserId"];
                        string usernameVal = sdr["Username"] == DBNull.Value ? string.Empty : sdr["Username"].ToString();
                        string roleVal = sdr["Role"] == DBNull.Value ? string.Empty : sdr["Role"].ToString();
                        bool isActiveVal = sdr["IsActive"] != DBNull.Value && Convert.ToBoolean(sdr["IsActive"]);
                        string stored = sdr["Password"] == DBNull.Value ? string.Empty : sdr["Password"].ToString();

                        bool ok = IntelliJob.Utils.VerifyPassword(stored, txtPassword.Text.Trim());
                        if (!ok)
                        {
                            showErrorMessage("User");
                            sdr.Close();
                            con.Close();
                            return;
                        }

                        bool needsRehash = !string.IsNullOrEmpty(stored) && !stored.Contains(":");
                        sdr.Close();

                        // If legacy plaintext is stored, upgrade it to salted SHA-256.
                        if (needsRehash)
                        {
                            try
                            {
                                string newHash = IntelliJob.Utils.CreateSaltedHash(txtPassword.Text.Trim());
                                using (SqlCommand upd = new SqlCommand("UPDATE Users SET Password = @Password WHERE UserId = @UserId", con))
                                {
                                    upd.Parameters.AddWithValue("@Password", newHash);
                                    upd.Parameters.AddWithValue("@UserId", userIdVal);
                                    upd.ExecuteNonQuery();
                                }
                            }
                            catch
                            {
                                // Ignore rehash failures so login still works.
                            }
                        }

                        // Set sessions from cached values.
                        Session["user"] = usernameVal;
                        Session["userId"] = userIdVal.ToString();
                        Session["role"] = roleVal;

                        if (isActiveVal == false)
                        {
                            lblMsg.Visible = true;
                            lblMsg.Text = "Your account is disabled. Contact support.";
                            lblMsg.CssClass = "alert alert-danger";
                            con.Close();
                            return;
                        }

                        // Redirect based on role
                        if (Session["role"].ToString() == "JobSeeker")
                        {
                            con.Close();
                            Response.Redirect("~/User/Home.aspx", false);
                        }
                        else if (Session["role"].ToString() == "Company")
                        {
                            con.Close();
                            Response.Redirect("~/Company/CompanyDashboard.aspx", false);
                        }
                    }
                    else
                    {
                        showErrorMessage("User");
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "')</script>");
                con.Close();
            }
        }

        private void showErrorMessage(string UserType)
        {
            lblMsg.Visible = true;
            lblMsg.Text = "<b>" + UserType + "</b> credentials are incorrect..!";
            lblMsg.CssClass = "alert alert-danger";
        }
    }
}