using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;

namespace IntelliJob.Company
{
    public partial class JobList : System.Web.UI.Page
    {

        SqlConnection con;
        SqlCommand cmd;
        DataTable dt;
        String str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        //protected void Page_PreRender(object sender, EventArgs e)
        //{
        //    if (Session["userId"] == null || Session["role"] == null || Session["role"].ToString() != "Company")
        //    {
        //        Response.Redirect("../User/Login.aspx");
        //    }

        //    if (!IsPostBack)
        //        ShowJob();
        //}
        protected void Page_Load(object sender, EventArgs e)
        {
           // if (!IsPostBack)
                ShowJob();
        }

        private void ShowJob()
        {
            string query = string.Empty;
            con = new SqlConnection(str);


            // Get company name from DB (safer than trusting textbox)
            string companyQuery = "SELECT CompanyName FROM Companies WHERE CompanyId = @id";
            SqlCommand cmdName = new SqlCommand(companyQuery, con);
            cmdName.Parameters.AddWithValue("@id", Session["userId"].ToString());

            con.Open();
            string companyName = cmdName.ExecuteScalar()?.ToString();
            con.Close();

            if (string.IsNullOrEmpty(companyName))
            {
                lblMsg.Text = "Unable to load company data.";
                lblMsg.CssClass = "alert alert-danger";
                lblMsg.Visible = true;
                return;
            }

            query = @"Select Row_Number() over (Order by (Select 1)) as [Sr.No], j.JobId, j.Title, j.NoOfPost, j.Qualification, j.Experience,
            j.LastDateToApply, j.CompanyName, j.Country, j.State, j.CreateDate, COALESCE(f.isFeatured, 0) as isFeatured from Jobs j left join FeaturedMarks f ON j.JobId = f.JobId where j.CompanyName = @CompanyName";
            cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@CompanyName", companyName);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            dt = new DataTable();
            sda.Fill(dt);
            GridView1.DataSource = dt;
            GridView1.DataBind();
            if (Request.QueryString["id"] != null)
            {
                linkBack.Visible = true;
                SetBackButtonUrl();
            }
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;
            ShowJob();
        }

        protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                GridViewRow row = GridView1.Rows[e.RowIndex];
                int jobId = Convert.ToInt32(GridView1.DataKeys[e.RowIndex].Values[0]);
                con = new SqlConnection(str);
                cmd = new SqlCommand("Delete from Jobs where JobId = @id", con);
                cmd.Parameters.AddWithValue("@id", jobId);
                con.Open();
                int r = cmd.ExecuteNonQuery();
                if (r > 0)
                {
                    lblMsg.Text = "Job delete successfully!";
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
                ShowJob();
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "');</script>");
            }
            finally { con.Close(); }
        }

        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditJob")
            {
                string jobId = e.CommandArgument.ToString();
                if (!string.IsNullOrEmpty(jobId))
                {
                    Response.Redirect("EditJobDetails.aspx?id=" + jobId);
                    return;
                }
            }
        }

        protected void btnToggleFeatured_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                // Get the row of the clicked button
                ImageButton btn = (ImageButton)sender;
                GridViewRow row = (GridViewRow)btn.NamingContainer;

                // Get JobId from DataKeys
                int jobId = Convert.ToInt32(GridView1.DataKeys[row.RowIndex].Values["JobId"]);

                // Open connection
                con = new SqlConnection(str);

                // Get current status
                string getQuery = "SELECT isFeatured FROM FeaturedMarks WHERE JobId = @id";
                cmd = new SqlCommand(getQuery, con);
                cmd.Parameters.AddWithValue("@id", jobId);

                con.Open();
                object result = cmd.ExecuteScalar();
                con.Close();

                bool featured = result != null && Convert.ToBoolean(result); // If NULL, treat as false/not featured
                bool newValue = !featured; // Toggle status


                if (result != null)
                {
                    // Record exists, UPDATE the status
                    string updateQuery = "UPDATE FeaturedMarks SET isFeatured = @val WHERE JobId = @id";
                    cmd = new SqlCommand(updateQuery, con);
                    cmd.Parameters.AddWithValue("@val", newValue);
                    cmd.Parameters.AddWithValue("@id", jobId);
                }
                else
                {
                    // Record does NOT exist, INSERT a new record
                    string insertQuery = "INSERT INTO FeaturedMarks (JobId, isFeatured) VALUES (@id, @val)";
                    cmd = new SqlCommand(insertQuery, con);
                    cmd.Parameters.AddWithValue("@val", newValue);
                    cmd.Parameters.AddWithValue("@id", jobId);
                }

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();

                // Refresh UI
                ShowJob();
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Error: " + ex.Message;
                lblMsg.CssClass = "alert alert-danger";
                lblMsg.Visible = true;
            }
        }


        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.ID = e.Row.RowIndex.ToString();
                // Set default white background for all rows
                e.Row.BackColor = ColorTranslator.FromHtml("#FFFFFF");

                if (Request.QueryString["id"] != null)
                {
                    int jobId = Convert.ToInt32(GridView1.DataKeys[e.Row.RowIndex].Values[0]);
                    if (jobId == Convert.ToInt32(Request.QueryString["id"]))
                    {
                        // Highlight the selected row with light blue
                        e.Row.BackColor = ColorTranslator.FromHtml("#A1DCF2");
                    }
                }
            }
        }

        protected string GetEditJobUrl(string jobId)
        {
            string url = "EditJobDetails.aspx?id=" + jobId;
            string returnUrl = Request.QueryString["returnUrl"];
            if (!string.IsNullOrEmpty(returnUrl))
            {
                url += "&returnUrl=" + Server.UrlEncode(returnUrl);
            }
            return url;
        }

        private void SetBackButtonUrl()
        {
            // Check if returnUrl parameter exists
            string returnUrl = Request.QueryString["returnUrl"];

            if (!string.IsNullOrEmpty(returnUrl))
            {
                // Decode the URL-encoded returnUrl
                returnUrl = Server.UrlDecode(returnUrl);

                    // If returnUrl doesn't start with Company/, add it
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
                // Default behavior - no back button or redirect to a default page
                // Since ViewResume.aspx is deleted, we'll just hide it or redirect to Dashboard
                linkBack.NavigateUrl = "~/Company/CompanyDashboard.aspx";
            }
        }
    }
}