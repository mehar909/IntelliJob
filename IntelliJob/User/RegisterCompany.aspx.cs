using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IntelliJob.User
{
    public partial class RegisterCompany : System.Web.UI.Page
    {
        SqlConnection con;
        SqlCommand cmd;
        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            lbRegisterSwitch.Text = "Register As User";
        }
        protected void btnRegister_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(str);

            try
            {
                // Validate password
                if (txtCompanyPassword.Text.Trim() != txtConfirmCompanyPassword.Text.Trim())
                {
                    lblMsg.Text = "Password & Confirm Password do not match!";
                    lblMsg.CssClass = "alert alert-danger";
                    lblMsg.Visible = true;
                    return;
                }

                // Prepare logo name - default to company_logo.png if no file uploaded
                string logoFileName = "company_logo.jpg";
                bool isValid = false;
                if (fuimage.HasFile)
                {
                    if (Utils.IsValidExtension(fuimage.FileName))
                    {
                        logoFileName = fuimage.FileName;
                        fuimage.PostedFile.SaveAs(Server.MapPath("~/photos/") + logoFileName);
                        isValid = true;
                    }
                    else
                    {
                        lblMsg.Visible = true;
                        lblMsg.Text = "Please Select .png, .jpg, .jpeg file for company logo!";
                        lblMsg.CssClass = "alert alert-danger";
                        return;
                    }
                }
                else
                {
                    isValid = true; // No file uploaded, use default logo
                }

                if (isValid)
                {
                    con.Open();
                    SqlTransaction tran = con.BeginTransaction();

                    try
                    {
                    // Insert into USERS
                    string userSql = @"INSERT INTO Users (Username, Password, Role, Email, Address, Country)
                               VALUES (@Username, @Password, 'Company', @Email, @Address, @Country);
                               SELECT SCOPE_IDENTITY();";

                    SqlCommand userCmd = new SqlCommand(userSql, con, tran);
                    userCmd.Parameters.AddWithValue("@Username", txtUserName.Text.Trim());
                    userCmd.Parameters.AddWithValue("@Password", txtCompanyPassword.Text.Trim());
                    userCmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                    userCmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());
                    userCmd.Parameters.AddWithValue("@Country", ddlCountry.SelectedValue);

                    int newCompanyId = Convert.ToInt32(userCmd.ExecuteScalar());

                    // Insert into COMPANIES
                    string compSql = @"INSERT INTO Companies
                               (CompanyId, CompanyName, Website, Description, CompanyLogo, CompanySize)
                               VALUES
                               (@CompanyId, @CompanyName, @Website, @Description, @CompanyLogo, @CompanySize)";

                    SqlCommand compCmd = new SqlCommand(compSql, con, tran);
                    compCmd.Parameters.AddWithValue("@CompanyId", newCompanyId);
                    compCmd.Parameters.AddWithValue("@CompanyName", txtCompanyName.Text.Trim());
                    compCmd.Parameters.AddWithValue("@Website", txtWebsite.Text.Trim());
                    compCmd.Parameters.AddWithValue("@Description", txtDescription.Text.Trim());
                    compCmd.Parameters.AddWithValue("@CompanyLogo", logoFileName);
                    int companySize = 0;
                    int.TryParse(txtCompanySize.Text.Trim(), out companySize);
                    compCmd.Parameters.AddWithValue("@CompanySize", companySize);

                    compCmd.ExecuteNonQuery();

                    tran.Commit();

                    lblMsg.Text = "Company Registered Successfully!";
                    lblMsg.CssClass = "alert alert-success";
                    lblMsg.Visible = true;
                    Clear();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw;
                    }
                }
            }
            catch (SqlException ex)
            {
                lblMsg.Visible = true;

                if (ex.Number == 2627)
                {
                    string duplicateValue = Utils.ExtractDuplicateValue(ex.Message);
                    if (duplicateValue == txtEmail.Text.Trim())
                    {
                        lblMsg.Text = $"<b>{txtEmail.Text.Trim()}</b> email address already exists, please use a different one!";
                    }
                    else if (duplicateValue == txtUserName.Text.Trim())
                    {
                        lblMsg.Text = $"<b>{txtUserName.Text.Trim()}</b> username already exists, try a new one!";
                    }
                    else
                    {
                        lblMsg.Text = "Username or Email already exists!";
                    }
                }
                else
                {
                    lblMsg.Text = "An error occurred while registering. Please try again later.";
                }

                lblMsg.CssClass = "alert alert-danger";
            }
            catch (Exception ex)
            {
                lblMsg.Visible = true;
                lblMsg.Text = "An error occurred while registering. Please try again later.";
                lblMsg.CssClass = "alert alert-danger";
            }
            finally
            {
                if (con != null && con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }

        private void Clear()
        {
            txtUserName.Text = string.Empty;
            txtAddress.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtCompanyPassword.Text = string.Empty;
            txtConfirmCompanyPassword.Text = string.Empty;
            ddlCountry.ClearSelection();
            txtCompanySize.Text = string.Empty;
            txtCompanyName.Text = string.Empty;
            txtWebsite.Text = string.Empty;
            txtDescription.Text = string.Empty;

        }

        protected void lbRegisterSwitch_Click(object sender, EventArgs e)
        {
            //if (lbRegisterSwitch.Text == "Register As User")
                Response.Redirect("Register.aspx");
           //else
            //{
                //Session.Abandon();
                //Response.Redirect("RegisterCompany.aspx");
            //}
        }
    }
}