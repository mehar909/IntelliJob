using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;

namespace IntelliJob.User
{
    public partial class Home : System.Web.UI.Page
    {
        SqlConnection con;
        SqlCommand cmd;
        SqlDataAdapter sda;
        DataTable dt;
        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        // ADDED: Declare the Repeater and Literal controls
        protected Repeater RptrFeaturedJobs;
        protected Literal litNoJobsMessage;
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["user"] == null)
            //{
            //    Response.Redirect("Login.aspx");
            //}
            //if (!IsPostBack)
            //{
            //    showUserProfile();
            //}
            if (!IsPostBack)
            {
                // ADDED: Call the function to load featured jobs
                ShowFeaturedJobs();
            }
        }

        // ADDED: Method to fetch and bind featured jobs
        private void ShowFeaturedJobs()
        {
            string query = @"
                SELECT TOP 4
                    J.JobId,
                    J.Title,
                    J.CompanyName,
                    J.CompanyImage,
                    J.JobType,
                    J.Salary, -- Include Salary if you plan to use it (was in the hardcoded example)
                    J.Address,
                    J.Country,
                    J.State,
                    J.CreateDate
                FROM
                    Jobs J
                INNER JOIN
                    FeaturedMarks FM ON J.JobId = FM.JobId
                WHERE
                    FM.isFeatured = 1
                ORDER BY
                    J.CreateDate DESC";

            using (SqlConnection con = new SqlConnection(str))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);

                        RptrFeaturedJobs.DataSource = dt;
                        RptrFeaturedJobs.DataBind();

                        // Check if no jobs were found and display message
                        if (dt.Rows.Count == 0)
                        {
                            litNoJobsMessage.Text = "<div style='text-align: center; padding: 20px; border: 1px solid #eee; background-color: #f9f9f9; margin-top: 20px;'><p style='font-size: 1.2em; color: #555;'>No jobs featured yet.</p></div>";
                            litNoJobsMessage.Visible = true;
                        }
                    }
                }
            }
        }

        // ADDED: Public helper method to calculate "time ago" (e.g., "7 hours ago")
        public string GetTimeAgo(object date)
        {
            if (date == DBNull.Value || date == null) return string.Empty;

            DateTime postDate = Convert.ToDateTime(date);
            TimeSpan timeSpan = DateTime.Now.Subtract(postDate);

            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalHours < 1)
                return (int)timeSpan.TotalMinutes + " minutes ago";
            if (timeSpan.TotalDays < 1)
                return (int)timeSpan.TotalHours + " hours ago";
            if (timeSpan.TotalDays < 30)
                return (int)timeSpan.TotalDays + " days ago";

            return postDate.ToString("MMM dd, yyyy");
        }
        private void showUserProfile()
        {
            con = new SqlConnection(str);
            string query = @"SELECT u.UserId, u.Username, js.Name as FullName, u.Address, js.Mobile, u.Email, u.Country, js.Resume 
                             FROM Users u 
                             LEFT JOIN JobSeekers js ON u.UserId = js.ProfileId 
                             WHERE u.username=@username";
            cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@username", Session["user"]);
            sda = new SqlDataAdapter(cmd);
            dt = new DataTable();
            sda.Fill(dt);
            lbRegisterOrResume.DataSource = dt;
            lbRegisterOrResume.DataBind();
        }

        protected void lbRegisterOrResume_ItemCommand(object source, DataListCommandEventArgs e)
        {
            if (e.CommandName == "EditUserProfile")
            {
                Response.Redirect("ResumeBuild.aspx?id=" + e.CommandArgument.ToString());
            }
        }
    }
}