using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace IntelliJob.User
{

    public partial class ResumeBuild : System.Web.UI.Page
    {
        SqlConnection con;
        SqlCommand cmd;
        SqlDataReader sdr;
        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;
        string query;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["user"] == null)
            {
                Response.Redirect("../User/Login.aspx");
            }

            if (!IsPostBack)
            {
                if (Request.QueryString["id"] != null)
                {
                    showUserInfo();

                }
                else
                {
                    Response.Redirect("Login.aspx");
                }
            }
        }

        private void showUserInfo()
        {
            try
            {
                con = new SqlConnection(str);
                string query = @"SELECT u.UserId, u.Username, u.Email, u.Address, u.Country,
                                       js.Name, js.Mobile, js.TenthGrade, js.TwelfthGrade, js.GraduationGrade,
                                       js.PostGraduationGrade, js.Phd, js.WorksOn, js.Experience, js.Resume
                                FROM Users u
                                LEFT JOIN JobSeekers js ON u.UserId = js.ProfileId
                                WHERE u.UserId = @userId";
                cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@userId", Request.QueryString["id"]);
                con.Open();
                sdr = cmd.ExecuteReader();
                if (sdr.HasRows)
                {
                    if (sdr.Read())
                    {
                        txtUserName.Text = sdr["Username"].ToString();
                        txtFullName.Text = sdr["Name"] != DBNull.Value ? sdr["Name"].ToString() : string.Empty;
                        txtEmail.Text = sdr["Email"].ToString();
                        txtMobile.Text = sdr["Mobile"] != DBNull.Value ? sdr["Mobile"].ToString() : string.Empty;
                        txtTenth.Text = sdr["TenthGrade"] != DBNull.Value ? sdr["TenthGrade"].ToString() : string.Empty;
                        txtTwelfth.Text = sdr["TwelfthGrade"] != DBNull.Value ? sdr["TwelfthGrade"].ToString() : string.Empty;
                        txtGraduation.Text = sdr["GraduationGrade"] != DBNull.Value ? sdr["GraduationGrade"].ToString() : string.Empty;
                        txtPostGraduation.Text = sdr["PostGraduationGrade"] != DBNull.Value ? sdr["PostGraduationGrade"].ToString() : string.Empty;
                        txtPhd.Text = sdr["Phd"] != DBNull.Value ? sdr["Phd"].ToString() : string.Empty;
                        txtWork.Text = sdr["WorksOn"] != DBNull.Value ? sdr["WorksOn"].ToString() : string.Empty;
                        txtExperience.Text = sdr["Experience"] != DBNull.Value ? sdr["Experience"].ToString() : string.Empty;
                        txtAddress.Text = sdr["Address"] != DBNull.Value ? sdr["Address"].ToString() : string.Empty;
                        ddlCountry.SelectedValue = sdr["Country"] != DBNull.Value ? sdr["Country"].ToString() : string.Empty;
                    }
                }
                else
                {
                    lblMsg.Visible = true;
                    lblMsg.Text = "User not found!";
                    lblMsg.CssClass = "alert alert-danger";
                }
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "');</script>");
            }
            finally
            {
                con.Close();
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (Request.QueryString["id"] != null)
                {
                    string concatQuery = string.Empty, filePath = string.Empty;
                    //bool isValidToExecute = false;
                    bool isValid = false;
                    con = new SqlConnection(str);
                    if (fuResume.HasFile)
                    {
                        if (Utils.IsValidExtension4Resume(fuResume.FileName))
                        {
                            concatQuery = "Resume=@resume,";
                            isValid = true;
                        }
                        else
                        {
                            concatQuery = string.Empty;
                        }
                    }
                    else
                    {
                        concatQuery = string.Empty;
                    }

                    // Update Users table
                    string userUpdateQuery = @"UPDATE Users SET Username=@Username, Email=@Email, 
                                              Address=@Address, Country=@Country, UpdatedAt=GETDATE() 
                                              WHERE UserId=@UserId";
                    
                    // Update JobSeekers table
                    query = @"UPDATE JobSeekers SET Name=@Name, Mobile=@Mobile, TenthGrade=@TenthGrade,
                             TwelfthGrade=@TwelfthGrade, GraduationGrade=@GraduationGrade, 
                             PostGraduationGrade=@PostGraduationGrade, Phd=@Phd,
                             WorksOn=@WorksOn, Experience=@Experience, " + concatQuery + @"ProfileId=@UserId
                             WHERE ProfileId=@UserId";

                    cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Name", txtFullName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Mobile", txtMobile.Text.Trim());
                    cmd.Parameters.AddWithValue("@TenthGrade", string.IsNullOrWhiteSpace(txtTenth.Text) ? (object)DBNull.Value : txtTenth.Text.Trim());
                    cmd.Parameters.AddWithValue("@TwelfthGrade", string.IsNullOrWhiteSpace(txtTwelfth.Text) ? (object)DBNull.Value : txtTwelfth.Text.Trim());
                    cmd.Parameters.AddWithValue("@GraduationGrade", string.IsNullOrWhiteSpace(txtGraduation.Text) ? (object)DBNull.Value : txtGraduation.Text.Trim());
                    cmd.Parameters.AddWithValue("@PostGraduationGrade", string.IsNullOrWhiteSpace(txtPostGraduation.Text) ? (object)DBNull.Value : txtPostGraduation.Text.Trim());
                    cmd.Parameters.AddWithValue("@Phd", string.IsNullOrWhiteSpace(txtPhd.Text) ? (object)DBNull.Value : txtPhd.Text.Trim());
                    cmd.Parameters.AddWithValue("@WorksOn", string.IsNullOrWhiteSpace(txtWork.Text) ? (object)DBNull.Value : txtWork.Text.Trim());
                    cmd.Parameters.AddWithValue("@Experience", string.IsNullOrWhiteSpace(txtExperience.Text) ? (object)DBNull.Value : txtExperience.Text.Trim());
                    cmd.Parameters.AddWithValue("@UserId", Request.QueryString["id"]);

                    // Get the old resume path before uploading new one (store for deletion after successful update)
                    string oldResumePath = string.Empty;
                    if (fuResume.HasFile)
                    {
                        using (SqlConnection conGetOld = new SqlConnection(str))
                        {
                            conGetOld.Open();
                            SqlCommand cmdGetOldResume = new SqlCommand("SELECT Resume FROM JobSeekers WHERE ProfileId = @UserId", conGetOld);
                            cmdGetOldResume.Parameters.AddWithValue("@UserId", Request.QueryString["id"]);
                            object oldResume = cmdGetOldResume.ExecuteScalar();
                            
                            if (oldResume != null && oldResume != DBNull.Value && !string.IsNullOrEmpty(oldResume.ToString()))
                            {
                                oldResumePath = oldResume.ToString();
                            }
                            conGetOld.Close();
                        }
                    }

                    if (fuResume.HasFile)
                    {
                        if (Utils.IsValidExtension4Resume(fuResume.FileName))
                        {
                            concatQuery = "Resume=@resume,";
                            isValid = true;

                            // Save new resume
                            Guid obj = Guid.NewGuid();
                            filePath = "Resumes/" + obj.ToString() + fuResume.FileName;
                            fuResume.PostedFile.SaveAs(Server.MapPath("~/Resumes/") + obj.ToString() + fuResume.FileName);

                            cmd.Parameters.AddWithValue("@resume", filePath);
                        }
                        else
                        {
                            concatQuery = string.Empty;
                            lblMsg.Visible = true;
                            lblMsg.Text = "Please Select .doc, .docx, .pdf file for resume!";
                            lblMsg.CssClass = "alert alert-danger";
                        }
                    }
                    else
                    {
                        isValid = true;
                    }

                    if (isValid)
                    {
                        con.Open();
                        // First update Users table
                        SqlCommand userCmd = new SqlCommand(userUpdateQuery, con);
                        userCmd.Parameters.AddWithValue("@Username", txtUserName.Text.Trim());
                        userCmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        userCmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());
                        userCmd.Parameters.AddWithValue("@Country", ddlCountry.SelectedValue);
                        userCmd.Parameters.AddWithValue("@UserId", Request.QueryString["id"]);
                        int userRowsAffected = userCmd.ExecuteNonQuery();
                        
                        // Then update JobSeekers table (check if exists first)
                        SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM JobSeekers WHERE ProfileId = @UserId", con);
                        checkCmd.Parameters.AddWithValue("@UserId", Request.QueryString["id"]);
                        int exists = Convert.ToInt32(checkCmd.ExecuteScalar());
                        
                        int r = 0;
                        if (exists > 0)
                        {
                            // Update existing record
                            r = cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            // Insert new record if it doesn't exist
                            string resumeField = concatQuery.Contains("Resume") ? "Resume, " : "";
                            string resumeValue = concatQuery.Contains("Resume") ? "@resume, " : "";
                            string insertQuery = @"INSERT INTO JobSeekers (ProfileId, Name, Mobile, TenthGrade, TwelfthGrade, GraduationGrade,
                                                  PostGraduationGrade, Phd, WorksOn, Experience, " + resumeField + 
                                                  @"Photo) VALUES (@ProfileId, @Name, @Mobile, @TenthGrade, @TwelfthGrade, @GraduationGrade,
                                                  @PostGraduationGrade, @Phd, @WorksOn, @Experience, " + resumeValue +
                                                  @"'avatar.png')";
                            SqlCommand insertCmd = new SqlCommand(insertQuery, con);
                            insertCmd.Parameters.AddWithValue("@ProfileId", Request.QueryString["id"]);
                            insertCmd.Parameters.AddWithValue("@Name", txtFullName.Text.Trim());
                            insertCmd.Parameters.AddWithValue("@Mobile", txtMobile.Text.Trim());
                            insertCmd.Parameters.AddWithValue("@TenthGrade", string.IsNullOrWhiteSpace(txtTenth.Text) ? (object)DBNull.Value : txtTenth.Text.Trim());
                            insertCmd.Parameters.AddWithValue("@TwelfthGrade", string.IsNullOrWhiteSpace(txtTwelfth.Text) ? (object)DBNull.Value : txtTwelfth.Text.Trim());
                            insertCmd.Parameters.AddWithValue("@GraduationGrade", string.IsNullOrWhiteSpace(txtGraduation.Text) ? (object)DBNull.Value : txtGraduation.Text.Trim());
                            insertCmd.Parameters.AddWithValue("@PostGraduationGrade", string.IsNullOrWhiteSpace(txtPostGraduation.Text) ? (object)DBNull.Value : txtPostGraduation.Text.Trim());
                            insertCmd.Parameters.AddWithValue("@Phd", string.IsNullOrWhiteSpace(txtPhd.Text) ? (object)DBNull.Value : txtPhd.Text.Trim());
                            insertCmd.Parameters.AddWithValue("@WorksOn", string.IsNullOrWhiteSpace(txtWork.Text) ? (object)DBNull.Value : txtWork.Text.Trim());
                            insertCmd.Parameters.AddWithValue("@Experience", string.IsNullOrWhiteSpace(txtExperience.Text) ? (object)DBNull.Value : txtExperience.Text.Trim());
                            if (concatQuery.Contains("Resume") && cmd.Parameters.Contains("@resume"))
                            {
                                insertCmd.Parameters.AddWithValue("@resume", cmd.Parameters["@resume"].Value);
                            }
                            r = insertCmd.ExecuteNonQuery();
                        }
                        
                        if (r > 0 || userRowsAffected > 0)
                        {
                            // Delete old resume file AFTER successful database update
                            if (fuResume.HasFile && !string.IsNullOrEmpty(oldResumePath))
                            {
                                try
                                {
                                    // Handle both cases: path with or without ~/
                                    string oldFilePath;
                                    if (oldResumePath.StartsWith("~/"))
                                    {
                                        oldFilePath = Server.MapPath(oldResumePath);
                                    }
                                    else if (oldResumePath.StartsWith("/"))
                                    {
                                        oldFilePath = Server.MapPath("~" + oldResumePath);
                                    }
                                    else
                                    {
                                        oldFilePath = Server.MapPath("~/" + oldResumePath);
                                    }

                                    if (File.Exists(oldFilePath))
                                    {
                                        File.Delete(oldFilePath);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // If deletion fails, show error in message
                                    lblMsg.Text = lblMsg.Text + " (Note: Old resume file could not be deleted: " + ex.Message + ")";
                                }
                            }

                            lblMsg.Visible = true;
                            lblMsg.Text = "Resume details updates successfully!";
                            lblMsg.CssClass = "alert alert-success";
                        }
                        else
                        {
                            lblMsg.Visible = true;
                            lblMsg.Text = "Cannot update the records, Please try after sometime..!";
                            lblMsg.CssClass = "alert alert-danger";
                        }
                    }
                }
                else
                {
                    lblMsg.Visible = true;
                    lblMsg.Text = "Cannot update the records, Please try <b>Relogin</b>!";
                    lblMsg.CssClass = "alert alert-danger";
                }
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("Violation of UNIQUE KEY constraint"))
                {
                    lblMsg.Visible = true;
                    lblMsg.Text = "<b>" + txtUserName.Text.Trim() + "</b> username already exists, try new one..!";
                    lblMsg.CssClass = "alert alert-danger";
                }
                else
                {
                    Response.Write("<script>alert('" + ex.Message + "')</script>");
                }
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "')</script>");
            }
            finally
            {
                con.Close();
            }

        }
    }
}