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
                    string query = @"SELECT UserId, Username, Role, IsActive 
                 FROM Users 
                 WHERE Username = @Username AND Password = @Password";

                    cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Username", txtUserName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Password", txtPassword.Text.Trim());
                    con.Open();
                    sdr = cmd.ExecuteReader();
                    if (sdr.Read())
                    {
                        Session["user"] = sdr["Username"].ToString();
                        Session["userId"] = sdr["UserId"].ToString();
                        Session["role"] = sdr["Role"].ToString();

                        if (Convert.ToBoolean(sdr["IsActive"]) == false)
                        {
                            lblMsg.Visible = true;
                            lblMsg.Text = "Your account is disabled. Contact support.";
                            lblMsg.CssClass = "alert alert-danger";
                            return;
                        }

                        // Redirect based on role
                        if (Session["role"].ToString() == "JobSeeker")
                        {
                            Response.Redirect("~/User/Home.aspx", false);
                        }
                        else if (Session["role"].ToString() == "Company")
                        {
                            Response.Redirect("~/Company/CompanyDashboard.aspx", false);
                        }


                    }
                    else
                    {
                        showErrorMessage("User");
                    }
                    con.Close();
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