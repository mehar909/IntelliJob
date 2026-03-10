using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IntelliJob.Admin
{
    public partial class ViewJobDetails : System.Web.UI.Page
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
                    LoadJobDetails();
                    // Set the View All Applicants link URL with returnUrl if present
                    string viewApplicantsUrl = "ViewJobApplicants.aspx?jobId=" + Request.QueryString["id"];
                    string returnUrl = Request.QueryString["returnUrl"];
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        viewApplicantsUrl += "&returnUrl=" + Server.UrlEncode(returnUrl);
                    }
                    linkViewApplicants.NavigateUrl = viewApplicantsUrl;
                    // Set the back button URL
                    SetBackButtonUrl();
                }
                else
                {
                    lblMsg.Text = "Job ID not provided.";
                    lblMsg.CssClass = "alert alert-danger";
                }
            }
        }

        private void LoadJobDetails()
        {
            try
            {
                con = new SqlConnection(str);
                query = "Select * from Jobs where JobId = @id";
                cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", Request.QueryString["id"]);
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                if (sdr.HasRows)
                {
                    while (sdr.Read())
                    {
                        lblJobTitle.Text = sdr["Title"].ToString();
                        lblNoOfPost.Text = sdr["NoOfPost"].ToString();
                        lblDescription.Text = sdr["Description"].ToString();
                        lblQualification.Text = sdr["Qualification"].ToString();
                        lblExperience.Text = sdr["Experience"].ToString();
                        lblSpecialization.Text = sdr["Specialization"].ToString();
                        if (sdr["LastDateToApply"] != DBNull.Value)
                        {
                            lblLastDate.Text = Convert.ToDateTime(sdr["LastDateToApply"]).ToString("dd MMMM yyyy");
                        }
                        lblSalary.Text = sdr["Salary"].ToString();
                        lblJobType.Text = sdr["JobType"].ToString();
                        lblCompany.Text = sdr["CompanyName"].ToString();
                        lblWebsite.Text = sdr["Website"].ToString();
                        lblEmail.Text = sdr["Email"].ToString();
                        lblAddress.Text = sdr["Address"].ToString();
                        lblCountry.Text = sdr["Country"].ToString();
                        lblState.Text = sdr["State"].ToString();
                        if (sdr["CreateDate"] != DBNull.Value)
                        {
                            lblPostedDate.Text = Convert.ToDateTime(sdr["CreateDate"]).ToString("dd MMMM yyyy");
                        }

                        // Display company logo if available
                        if (sdr["CompanyImage"] != DBNull.Value && !string.IsNullOrEmpty(sdr["CompanyImage"].ToString()))
                        {
                            imgCompanyLogo.ImageUrl = "~/" + sdr["CompanyImage"].ToString();
                            imgCompanyLogo.Visible = true;
                        }
                        else
                        {
                            imgCompanyLogo.Visible = false;
                        }
                    }
                }
                else
                {
                    lblMsg.Text = "Job Not Found.";
                    lblMsg.CssClass = "alert alert-danger";
                }
                sdr.Close();
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Error loading job details: " + ex.Message;
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
                
                // Build the JobList URL with id and returnUrl parameters
                string jobId = Request.QueryString["id"];
                if (!string.IsNullOrEmpty(jobId))
                {
                    linkBack.NavigateUrl = "JobList.aspx?id=" + jobId + "&returnUrl=" + Server.UrlEncode(returnUrl);
                }
                else
                {
                    // If no jobId, just go to the returnUrl
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
            }
            else
            {
                // Default to JobList.aspx
                linkBack.NavigateUrl = "~/Admin/JobList.aspx";
            }
        }
    }
}

