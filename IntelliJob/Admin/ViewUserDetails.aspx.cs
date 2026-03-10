using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IntelliJob.Admin
{
    public partial class ViewUserDetails : System.Web.UI.Page
    {
        SqlConnection con;
        SqlCommand cmd;
        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;
        string query;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["admin"] == null)
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
                query = @"SELECT u.UserId, u.Username, u.Email, u.Address, u.Country, u.Role,
                                 js.Name, js.Mobile, js.TenthGrade, js.TwelfthGrade, js.GraduationGrade, 
                                 js.PostGraduationGrade, js.Phd, js.WorksOn, js.Experience, 
                                 js.Photo, js.Resume,
                                 c.CompanyName, c.Website, c.Description, c.CompanyLogo, c.CompanySize
                          FROM Users u
                          LEFT JOIN JobSeekers js ON u.UserId = js.ProfileId
                          LEFT JOIN Companies c ON u.UserId = c.CompanyId
                          WHERE u.UserId = @id";
                cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", Request.QueryString["id"]);
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                if (sdr.HasRows)
                {
                    while (sdr.Read())
                    {
                        string role = sdr["Role"] != DBNull.Value ? sdr["Role"].ToString() : "";
                        bool isCompany = role.Equals("Company", StringComparison.OrdinalIgnoreCase);
                        
                        // Show/hide sections based on role
                        pnlJobSeekerFields.Visible = !isCompany;
                        pnlCompanyFields.Visible = isCompany;
                        pnlName.Visible = !isCompany;
                        pnlMobile.Visible = !isCompany;
                        
                        // Common fields
                        lblUsername.Text = sdr["Username"].ToString();
                        lblEmail.Text = sdr["Email"].ToString();
                        lblAddress.Text = sdr["Address"] != DBNull.Value ? sdr["Address"].ToString() : "Not provided";
                        lblCountry.Text = sdr["Country"] != DBNull.Value ? sdr["Country"].ToString() : "Not provided";

                        if (isCompany)
                        {
                            // Company-specific fields
                            lblCompanyName.Text = sdr["CompanyName"] != DBNull.Value ? sdr["CompanyName"].ToString() : "Not provided";
                            lblWebsite.Text = sdr["Website"] != DBNull.Value ? sdr["Website"].ToString() : "Not provided";
                            lblDescription.Text = sdr["Description"] != DBNull.Value ? sdr["Description"].ToString() : "Not provided";
                            lblCompanySize.Text = sdr["CompanySize"] != DBNull.Value ? sdr["CompanySize"].ToString() : "Not provided";
                            
                            // Display company logo
                            if (sdr["CompanyLogo"] != DBNull.Value && !string.IsNullOrEmpty(sdr["CompanyLogo"].ToString()))
                            {
                                imgUserPhoto.ImageUrl = "~/photos/" + sdr["CompanyLogo"].ToString();
                                imgUserPhoto.Visible = true;
                            }
                            else
                            {
                                imgUserPhoto.ImageUrl = "~/Images/No_image.png";
                                imgUserPhoto.Visible = true;
                            }
                        }
                        else
                        {
                            // JobSeeker-specific fields
                            lblName.Text = sdr["Name"] != DBNull.Value ? sdr["Name"].ToString() : "Not provided";
                            lblMobile.Text = sdr["Mobile"] != DBNull.Value ? sdr["Mobile"].ToString() : "Not provided";

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
                            if (sdr["Photo"] != DBNull.Value && !string.IsNullOrEmpty(sdr["Photo"].ToString()))
                        {
                                imgUserPhoto.ImageUrl = "~/photos/" + sdr["Photo"].ToString();
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
                if (!returnUrl.StartsWith("Admin/") && !returnUrl.StartsWith("~/Admin/"))
                {
                    linkBack.NavigateUrl = "~/Admin/" + returnUrl;
                }
                else if (returnUrl.StartsWith("Admin/"))
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
                linkBack.NavigateUrl = "~/Admin/UserList.aspx";
            }
        }
    }
}

