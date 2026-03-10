using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;

namespace IntelliJob.Company
{
    public partial class ViewApplications : System.Web.UI.Page
    {

        SqlConnection con;
        SqlCommand cmd;
        DataTable dt;
        String str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            // Company authentication
            if (Session["userId"] == null || Session["role"].ToString() != "Company")
            {
                Response.Redirect("../User/Login.aspx");
            }

            if (!IsPostBack)
            {
                ShowAppliedJob();
            }
        }


        private void ShowAppliedJob()
        {
            string query = string.Empty;
            con = new SqlConnection(str);
            // First get the logged-in company's name
            string companyNameQuery = "SELECT CompanyName FROM Companies WHERE CompanyId = @id";
            SqlCommand cmdName = new SqlCommand(companyNameQuery, con);
            cmdName.Parameters.AddWithValue("@id", Session["userId"]);
            con.Open();
            string companyName = cmdName.ExecuteScalar()?.ToString();
            con.Close();

            if (string.IsNullOrEmpty(companyName))
            {
                lblMsg.Text = "Company name not found.";
                lblMsg.CssClass = "alert alert-danger";
                lblMsg.Visible = true;
                return;
            }
            // Now load ONLY applications for this company
            query = @"
                SELECT 
                    ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS [Sr.No],
                    aj.AppliedJobId,
                    j.CompanyName,
                    aj.JobId,
                    j.Title,
                    js.Mobile,
                    js.Name AS UserName,
                    u.Email,
                    js.Resume
                FROM AppliedJobs aj
                INNER JOIN JobSeekers js ON aj.UserId = js.ProfileId
                INNER JOIN Users u ON aj.UserId = u.UserId
                INNER JOIN Jobs j ON aj.JobId = j.JobId
                WHERE aj.Shortlisted = 'no'
                  AND j.CompanyName = @CompanyName";

            cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@CompanyName", companyName);

            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            dt = new DataTable();
            sda.Fill(dt);
            GridView1.DataSource = dt;
            GridView1.DataBind();
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;
            ShowAppliedJob();
        }

        protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                GridViewRow row = GridView1.Rows[e.RowIndex];
                int AppliedjobId = Convert.ToInt32(GridView1.DataKeys[e.RowIndex].Values[0]);
                con = new SqlConnection(str);
                cmd = new SqlCommand("Delete from AppliedJobs where AppliedJobId = @id", con);
                cmd.Parameters.AddWithValue("@id", AppliedjobId);
                con.Open();
                int r = cmd.ExecuteNonQuery();
                if (r > 0)
                {
                    lblMsg.Text = "Resume delete successfully!";
                    lblMsg.CssClass = "alert alter-success";
                    // Auto-hide message after 7 seconds
                    ClientScript.RegisterStartupScript(this.GetType(), "hideMessage",
                        "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 7000);", true);

                }
                else
                {
                    lblMsg.Text = "Cannot delete this record!";
                    lblMsg.CssClass = "alert alter-success";
                    // Auto-hide message after 7 seconds
                    ClientScript.RegisterStartupScript(this.GetType(), "hideMessage",
                        "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 7000);", true);

                }
                GridView1.EditIndex = -1;
                ShowAppliedJob();
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

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //e.Row.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(GridView1, "Select$" + e.Row.RowIndex);
            //e.Row.ToolTip = "Click to view job details";
            // We removed row click functionality entirely.
            // Keep this method empty unless you add another UI effect later.
        }

        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (GridViewRow row in GridView1.Rows)
            {
                if (row.RowIndex == GridView1.SelectedIndex)
                {
                    HiddenField jobId = (HiddenField)row.FindControl("hdnJobId");
                    Response.Redirect("JobList.aspx?id=" + jobId.Value);
                }
                else
                {
                    row.BackColor = ColorTranslator.FromHtml("#FFFFFF");
                    row.ToolTip = "Click to select this row";
                }
            }
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            Response.Redirect("ShorlistedCandidates.aspx");
        }

        protected void btnShortlist_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                // Get the GridView row where the button was clicked
                ImageButton btn = (ImageButton)sender;
                GridViewRow row = (GridViewRow)btn.NamingContainer;

                // Retrieve the AppliedJobId or UserId from the GridView's DataKey or another field
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

                if (rowsAffected > 0)
                {
                    // Update successful, show a success message
                    lblMsg.Visible = true;
                    lblMsg.Text = "Candidate shortlisted successfully!";
                    lblMsg.CssClass = "alert alert-success";
                    // Auto-hide message after 7 seconds
                    ClientScript.RegisterStartupScript(this.GetType(), "hideMessage",
                        "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 7000);", true);

                }
                else
                {
                    // Update failed, show a failure message
                    lblMsg.Visible = true;
                    lblMsg.Text = "Failed to shortlist the candidate. Please try again later.";
                    lblMsg.CssClass = "alert alert-danger";
                }

                // Optionally, refresh the GridView to reflect the changes
                ShowAppliedJob();
            }
            catch (Exception ex)
            {
                // Handle exceptions
                lblMsg.Visible = true;
                lblMsg.Text = "Error: " + ex.Message;
                lblMsg.CssClass = "alert alert-danger";
            }
            finally
            {
                // Always close the connection
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Create a new SqlConnection
                con = new SqlConnection(str);

                // Define the SQL query to delete all AppliedJobs where Shortlisted is 'no'
                string deleteQuery = "DELETE FROM AppliedJobs WHERE Shortlisted = 'no'";

                // Initialize the SqlCommand object
                cmd = new SqlCommand(deleteQuery, con);

                // Open connection to the database
                con.Open();

                // Execute the query
                int rowsAffected = cmd.ExecuteNonQuery();

                // Check if any rows were affected
                if (rowsAffected > 0)
                {
                    lblMsg.Visible = true;
                    lblMsg.Text = "All non-shortlisted applications have been deleted successfully!";
                    lblMsg.CssClass = "alert alert-success";
                    // Auto-hide message after 7 seconds
                    ClientScript.RegisterStartupScript(this.GetType(), "hideMessage",
                        "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 7000);", true);

                }
                else
                {
                    lblMsg.Visible = true;
                    lblMsg.Text = "No non-shortlisted applications found to delete.";
                    lblMsg.CssClass = "alert alert-warning";
                }

                // Optionally, refresh the GridView to reflect the changes
                ShowAppliedJob();
            }
            catch (Exception ex)
            {
                // Handle any errors that may occur during the process
                lblMsg.Visible = true;
                lblMsg.Text = "Error: " + ex.Message;
                lblMsg.CssClass = "alert alert-danger";
            }
            finally
            {
                // Ensure that the connection is closed
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }

        //protected string GetJobListUrl(string jobId)
        //{
        //    string url = "JobList.aspx?id=" + jobId;
        //    string returnUrl = Request.QueryString["returnUrl"];
        //    if (!string.IsNullOrEmpty(returnUrl))
        //    {
        //        url += "&returnUrl=" + Server.UrlEncode(returnUrl);
        //    }
        //    return url;
        //}

        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string jobId = e.CommandArgument.ToString();
            if (e.CommandName == "ViewJob")
            {
                string url = "JobList.aspx?id=" + jobId;
                url += "&returnUrl=" + Server.UrlEncode("~/Company/ViewApplications.aspx");
                //Response.Redirect(GetJobListUrl(jobId));
                Response.Redirect(url);
                return;
            }
        }

    }
}