using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IntelliJob.Admin
{
    public partial class ViewJobApplicants : System.Web.UI.Page
    {
        SqlConnection con;
        SqlCommand cmd;
        DataTable dt;
        String str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["admin"] == null)
            {
                Response.Redirect("../User/Login.aspx");
            }

            if (!IsPostBack)
            {
                if (Request.QueryString["jobId"] != null)
                {
                    ShowApplicants();
                    // Set the back link to return to the job details page with returnUrl if present
                    SetBackButtonUrl();
                }
                else
                {
                    lblMsg.Text = "Job ID not provided.";
                    lblMsg.CssClass = "alert alert-danger";
                    linkBack.NavigateUrl = "JobList.aspx";
                }
            }
        }

        private void ShowApplicants()
        {
            try
            {
                string query = string.Empty;
                con = new SqlConnection(str);
                // Join AppliedJobs with User table to get only users who applied for this specific job and are NOT shortlisted yet
                query = @"Select Row_Number() over(Order by (Select 1)) as[Sr.No], u.UserId, u.Username, u.Email, js.Mobile, u.Country, aj.AppliedJobId
                          from AppliedJobs aj 
                          inner join Users u on aj.UserId = u.UserId 
                          LEFT JOIN JobSeekers js ON u.UserId = js.ProfileId
                          where aj.JobId = @JobId AND (aj.Shortlisted IS NULL OR LOWER(aj.Shortlisted) <> 'yes')";
                cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@JobId", Request.QueryString["jobId"]);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                dt = new DataTable();
                sda.Fill(dt);
                GridView1.DataSource = dt;
                GridView1.DataBind();
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Error loading applicants: " + ex.Message;
                lblMsg.CssClass = "alert alert-danger";
            }
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;
            ShowApplicants();
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                HyperLink lnkViewUser = e.Row.FindControl("lnkViewUser") as HyperLink;
                if (lnkViewUser != null && Request.QueryString["jobId"] != null)
                {
                    // Add returnUrl parameter to navigate back to ViewJobApplicants with the jobId
                    string returnUrl = "ViewJobApplicants.aspx?jobId=" + Request.QueryString["jobId"];
                    // Preserve the original returnUrl if it exists
                    string originalReturnUrl = Request.QueryString["returnUrl"];
                    if (!string.IsNullOrEmpty(originalReturnUrl))
                    {
                        returnUrl += "&returnUrl=" + Server.UrlEncode(originalReturnUrl);
                    }
                    lnkViewUser.NavigateUrl += "&returnUrl=" + Server.UrlEncode(returnUrl);
                }
            }
        }

        private void SetBackButtonUrl()
        {
            string jobId = Request.QueryString["jobId"];
            string returnUrl = Request.QueryString["returnUrl"];
            
            if (!string.IsNullOrEmpty(jobId))
            {
                // Build the ViewJobDetails URL with jobId
                string backUrl = "ViewJobDetails.aspx?id=" + jobId;
                
                // If returnUrl exists, preserve it
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    backUrl += "&returnUrl=" + Server.UrlEncode(returnUrl);
                }
                
                linkBack.NavigateUrl = backUrl;
            }
            else
            {
                linkBack.NavigateUrl = "JobList.aspx";
            }
        }

        protected void btnShortlist_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                // Get the GridView row where the button was clicked
                ImageButton btn = (ImageButton)sender;
                GridViewRow row = (GridViewRow)btn.NamingContainer;

                // Retrieve the AppliedJobId from the GridView's DataKey
                int appliedJobId = Convert.ToInt32(GridView1.DataKeys[row.RowIndex].Values["AppliedJobId"]);

                // Define your connection string (from your config)
                con = new SqlConnection(str);

                // SQL query to update the Shortlisted field to 'Yes' for the selected AppliedJobId
                string updateQuery = "UPDATE AppliedJobs SET Shortlisted = 'Yes' WHERE AppliedJobId = @AppliedJobId";

                // Initialize the SqlCommand object
                cmd = new SqlCommand(updateQuery, con);
                cmd.Parameters.AddWithValue("@AppliedJobId", appliedJobId);

                // Open connection and execute the query
                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                con.Close();

                if (rowsAffected > 0)
                {
                    // Update successful, show a success message
                    lblMsg.Visible = true;
                    lblMsg.Text = "Candidate shortlisted successfully!";
                    lblMsg.CssClass = "alert alert-success";
                    
                    // Auto-hide message after 5 seconds
                    ClientScript.RegisterStartupScript(this.GetType(), "hideMessage", 
                        "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 5000);", true);
                    
                    // Refresh the GridView to reflect the changes
                    ShowApplicants();
                }
                else
                {
                    // Update failed, show a failure message
                    lblMsg.Visible = true;
                    lblMsg.Text = "Failed to shortlist the candidate. Please try again later.";
                    lblMsg.CssClass = "alert alert-danger";
                }
            }
            catch (Exception ex)
            {
                lblMsg.Visible = true;
                lblMsg.Text = "Error shortlisting candidate: " + ex.Message;
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
    }
}

