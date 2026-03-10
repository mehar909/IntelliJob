using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IntelliJob.Company
{
    public partial class ViewUserDetails : System.Web.UI.Page
    {
        SqlConnection con;
        SqlCommand cmd;
        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;
        string query;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["userId"] == null || Session["role"] == null || Session["role"].ToString() != "Company")
            {
                Response.Redirect("../User/Login.aspx");
            }

            if (!IsPostBack)
            {
                if (Request.QueryString["id"] != null)
                {
                    LoadUserDetails();
                    SetBackButtonUrl();
                }
                else
                {
                    lblMsg.Text = "User ID not provided.";
                    lblMsg.CssClass = "alert alert-danger";
                }
            }
        }

        private void LoadUserDetails()
        {
            try
            {
                con = new SqlConnection(str);
                query = "Select * from Users u inner join JobSeekers j on u.UserId = j.ProfileId where u.UserId = @id";
                cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", Request.QueryString["id"]);
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                if (sdr.HasRows)
                {
                    while (sdr.Read())
                    {
                        lblUsername.Text = sdr["Username"].ToString();
                        lblName.Text = sdr["Name"].ToString();
                        lblEmail.Text = sdr["Email"].ToString();
                        lblMobile.Text = sdr["Mobile"].ToString();
                        lblAddress.Text = sdr["Address"].ToString();
                        lblCountry.Text = sdr["Country"].ToString();

                        // Educational Qualifications
                        if (sdr["TenthGrade"] != DBNull.Value && !string.IsNullOrWhiteSpace(sdr["TenthGrade"].ToString()))
                        {
                            lblTenthGrade.Text = sdr["TenthGrade"].ToString();
                        }
                        else
                        {
                            lblTenthGrade.Text = "Not provided";
                        }

                        if (sdr["TwelfthGrade"] != DBNull.Value && !string.IsNullOrWhiteSpace(sdr["TwelfthGrade"].ToString()))
                        {
                            lblTwelfthGrade.Text = sdr["TwelfthGrade"].ToString();
                        }
                        else
                        {
                            lblTwelfthGrade.Text = "Not provided";
                        }

                        if (sdr["GraduationGrade"] != DBNull.Value && !string.IsNullOrWhiteSpace(sdr["GraduationGrade"].ToString()))
                        {
                            lblGraduationGrade.Text = sdr["GraduationGrade"].ToString();
                        }
                        else
                        {
                            lblGraduationGrade.Text = "Not provided";
                        }

                        if (sdr["PostGraduationGrade"] != DBNull.Value && !string.IsNullOrWhiteSpace(sdr["PostGraduationGrade"].ToString()))
                        {
                            lblPostGraduationGrade.Text = sdr["PostGraduationGrade"].ToString();
                        }
                        else
                        {
                            lblPostGraduationGrade.Text = "Not provided";
                        }

                        if (sdr["Phd"] != DBNull.Value && !string.IsNullOrWhiteSpace(sdr["Phd"].ToString()))
                        {
                            lblPhd.Text = sdr["Phd"].ToString();
                        }
                        else
                        {
                            lblPhd.Text = "Not provided";
                        }

                        // Professional Information
                        if (sdr["WorksOn"] != DBNull.Value && !string.IsNullOrWhiteSpace(sdr["WorksOn"].ToString()))
                        {
                            lblWorksOn.Text = sdr["WorksOn"].ToString();
                        }
                        else
                        {
                            lblWorksOn.Text = "Not provided";
                        }

                        if (sdr["Experience"] != DBNull.Value && !string.IsNullOrWhiteSpace(sdr["Experience"].ToString()))
                        {
                            lblExperience.Text = sdr["Experience"].ToString();
                        }
                        else
                        {
                            lblExperience.Text = "Not provided";
                        }

                        // Display user photo if available
                        if (sdr["photo"] != DBNull.Value && !string.IsNullOrEmpty(sdr["photo"].ToString()))
                        {
                            imgUserPhoto.ImageUrl = "~/photos/" + sdr["photo"].ToString();
                            imgUserPhoto.Visible = true;
                        }
                        else
                        {
                            imgUserPhoto.ImageUrl = "~/Images/No_image.png";
                            imgUserPhoto.Visible = true;
                        }

                        // Display resume link if available
                        if (sdr["Resume"] != DBNull.Value && !string.IsNullOrEmpty(sdr["Resume"].ToString()))
                        {
                            lnkResume.NavigateUrl = "~/" + sdr["Resume"].ToString();
                            lnkResume.Visible = true;
                            lblNoResume.Visible = false;
                        }
                        else
                        {
                            lnkResume.Visible = false;
                            lblNoResume.Visible = true;
                        }
                    }
                }
                else
                {
                    lblMsg.Text = "User Not Found.";
                    lblMsg.CssClass = "alert alert-danger";
                }
                sdr.Close();
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Error loading user details: " + ex.Message;
                lblMsg.CssClass = "alert alert-danger";
            }
            finally
            {
                con.Close();
            }
        }

        private void SetBackButtonUrl()
        {
            // Check if returnUrl parameter exists
            string returnUrl = Request.QueryString["returnUrl"];
            
            if (!string.IsNullOrEmpty(returnUrl))
            {
                // Decode the URL-encoded returnUrl
                returnUrl = Server.UrlDecode(returnUrl);
                
                // If returnUrl doesn't start with Admin/, add it
                if (!returnUrl.StartsWith("Company/") && !returnUrl.StartsWith("~/Company/"))
                {
                    linkBack.NavigateUrl = "~/Company/" + returnUrl;
                }
                else if (returnUrl.StartsWith("Company/"))
                {
                    linkBack.NavigateUrl = "~/" + returnUrl;
                }
                else
                {
                    linkBack.NavigateUrl = returnUrl;
                }
            }
            else
            {
                // Default to UserList.aspx if no returnUrl is provided
                linkBack.NavigateUrl = "~/Company/Applicants.aspx";
            }
        }
    }
}

