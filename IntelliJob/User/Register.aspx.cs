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
    public partial class Register : System.Web.UI.Page
    {
        SqlConnection con;
        SqlCommand cmd;
        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            lbRegisterSwitch.Text = "Register As Company";
        }
        protected void btnRegister_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate passwords
                if (txtPassword.Text.Trim() != txtConfirmPassword.Text.Trim())
                {
                    lblMsg.Text = "Password & Confirm Password do not match!";
                    lblMsg.CssClass = "alert alert-danger";
                    lblMsg.Visible = true;
                    return;
                }

                // Prepare photo filename
                string photoFileName = "avatar.png";
                bool IsValid = false;
                if (fuimage.HasFile)
                {
                    if (Utils.IsValidExtension(fuimage.FileName))
                    {
                        photoFileName = fuimage.FileName;
                        fuimage.PostedFile.SaveAs(Server.MapPath("~/photos/") + photoFileName);
                        IsValid = true;
                    }
                    else
                    {
                        lblMsg.Visible = true;
                        lblMsg.Text = "Please Select .png, .jpg, .jpeg file for photo!";
                        lblMsg.CssClass = "alert alert-danger";
                        return;
                    }
                }
                else
                {
                    IsValid = true;
                }

                if (IsValid)
                {
                    con = new SqlConnection(str);
                    con.Open();
                    SqlTransaction tran = con.BeginTransaction();

                    try
                    {
                        // 1) Insert into USERS (main table)
                        string userSql = @"INSERT INTO Users (Username, Password, Role, Email, Address, Country)
                                           VALUES (@Username, @Password, 'JobSeeker', @Email, @Address, @Country);
                                           SELECT SCOPE_IDENTITY();";

                        SqlCommand userCmd = new SqlCommand(userSql, con, tran);
                        userCmd.Parameters.AddWithValue("@Username", txtUserName.Text.Trim());
                        userCmd.Parameters.AddWithValue("@Password", txtConfirmPassword.Text.Trim());
                        userCmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        userCmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());
                        userCmd.Parameters.AddWithValue("@Country", ddlCountry.SelectedValue);

                        int newUserId = Convert.ToInt32(userCmd.ExecuteScalar());

                        // 2) Insert into JOBSEEKERS (child table)
                        string jsSql = @"INSERT INTO JobSeekers (ProfileId, Name, Mobile, Photo)
                                        VALUES (@ProfileId, @Name, @Mobile, @Photo)";

                        SqlCommand jsCmd = new SqlCommand(jsSql, con, tran);
                        jsCmd.Parameters.AddWithValue("@ProfileId", newUserId);
                        jsCmd.Parameters.AddWithValue("@Name", txtName.Text.Trim());
                        jsCmd.Parameters.AddWithValue("@Mobile", txtMobile.Text.Trim());
                        jsCmd.Parameters.AddWithValue("@Photo", photoFileName);

                        jsCmd.ExecuteNonQuery();

                        tran.Commit();

                        lblMsg.Visible = true;
                        lblMsg.Text = "Registered Successfully!";
                        lblMsg.CssClass = "alert alert-success";
                        Clear();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw;
                    }
                    finally
                    {
                        con.Close();
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) // Unique key violation error number
                {
                    // Extract the duplicate value from the error message
                    string duplicateValue = Utils.ExtractDuplicateValue(ex.Message);

                    if (duplicateValue == txtEmail.Text.Trim())
                    {
                        lblMsg.Visible = true;
                        lblMsg.Text = $"<b>{txtEmail.Text.Trim()}</b> email address already exists, please use a different one!";
                        lblMsg.CssClass = "alert alert-danger";
                    }
                    else if (duplicateValue == txtUserName.Text.Trim())
                    {
                        lblMsg.Visible = true;
                        lblMsg.Text = $"<b>{txtUserName.Text.Trim()}</b> username already exists, try a new one!";
                        lblMsg.CssClass = "alert alert-danger";
                    }
                }
                else
                {
                    lblMsg.Visible = true;
                    lblMsg.Text = "An error occurred while registering. Please try again later.";
                    lblMsg.CssClass = "alert alert-danger";
                }
            }
            catch (Exception ex)
            {
                lblMsg.Visible = true;
                lblMsg.Text = "An error occurred while registering. Please try again later.";
                lblMsg.CssClass = "alert alert-danger";
            }
        }

        private void Clear()
        {
            txtUserName.Text = string.Empty;
            txtAddress.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtMobile.Text = string.Empty;
            txtName.Text = string.Empty;
            txtPassword.Text = string.Empty;
            txtConfirmPassword.Text = string.Empty;
            ddlCountry.ClearSelection();
        }

        protected void lbRegisterSwitch_Click(object sender, EventArgs e)
        {
            Response.Redirect("RegisterCompany.aspx");
        }
    }
}