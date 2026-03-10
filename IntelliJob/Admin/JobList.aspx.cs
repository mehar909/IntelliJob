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

namespace IntelliJob.Admin
{
    public partial class JobList : System.Web.UI.Page
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
                ShowJob();
            }
        }

        private void ShowJob()
        {
            string query = string.Empty;
            con = new SqlConnection(str);
            query = @"Select Row_Number() over(Order by (Select 1)) as[Sr.No], JobId, Title, NoOfPost, Qualification, Experience,
            LastDateToApply, CompanyName, Country, State, CreateDate from Jobs";
            cmd = new SqlCommand(query, con);
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
                }
                else
                {
                    lblMsg.Text = "Cannot delete this record!";
                    lblMsg.CssClass = "alert alter-success";
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
            if (e.CommandName == "ViewJob")
            {
                string jobId = e.CommandArgument.ToString();
                if (!string.IsNullOrEmpty(jobId))
                {
                    Response.Redirect("ViewJobDetails.aspx?id=" + jobId);
                }
            }
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if(e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.ID = e.Row.RowIndex.ToString();
                // Set default white background for all rows
                e.Row.BackColor = ColorTranslator.FromHtml("#FFFFFF");
                
                if (Request.QueryString["id"] != null)
                {
                    int jobId = Convert.ToInt32(GridView1.DataKeys[e.Row.RowIndex].Values[0]);
                    if(jobId == Convert.ToInt32(Request.QueryString["id"]))
                    {
                        // Highlight the selected row with light blue
                        e.Row.BackColor = ColorTranslator.FromHtml("#A1DCF2");
                    }
                }
            }
        }

        protected string GetViewJobUrl(string jobId)
        {
            string url = "ViewJobDetails.aspx?id=" + jobId;
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
                // Default behavior - no back button or redirect to a default page
                // Since ViewResume.aspx is deleted, we'll just hide it or redirect to Dashboard
                linkBack.NavigateUrl = "~/Admin/Dashboard.aspx";
            }
        }
    }
}