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
                int jobId = Convert.ToInt32(GridView1.DataKeys[e.RowIndex].Values[0]);
                con = new SqlConnection(str);
                con.Open();
                using (SqlTransaction tran = con.BeginTransaction())
                {
                    using (SqlCommand deleteFeatured = new SqlCommand("DELETE FROM FeaturedMarks WHERE JobId = @id", con, tran))
                    {
                        deleteFeatured.Parameters.AddWithValue("@id", jobId);
                        deleteFeatured.ExecuteNonQuery();
                    }

                    DataTable applications = new DataTable();
                    using (SqlCommand loadApplications = new SqlCommand("SELECT AppliedJobId FROM AppliedJobs WHERE JobId = @id", con, tran))
                    {
                        loadApplications.Parameters.AddWithValue("@id", jobId);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(loadApplications))
                        {
                            adapter.Fill(applications);
                        }
                    }

                    foreach (DataRow applicationRow in applications.Rows)
                    {
                        int appliedJobId = Convert.ToInt32(applicationRow[0]);

                        using (SqlCommand deleteInvitations = new SqlCommand("DELETE FROM InterviewInvitations WHERE AppliedJobId = @AppliedJobId", con, tran))
                        {
                            deleteInvitations.Parameters.AddWithValue("@AppliedJobId", appliedJobId);
                            deleteInvitations.ExecuteNonQuery();
                        }

                        int interviewId = 0;
                        using (SqlCommand interviewIdCmd = new SqlCommand("SELECT TOP 1 InterviewId FROM Interviews WHERE AppliedJobId = @AppliedJobId ORDER BY InterviewId DESC", con, tran))
                        {
                            interviewIdCmd.Parameters.AddWithValue("@AppliedJobId", appliedJobId);
                            object interviewResult = interviewIdCmd.ExecuteScalar();
                            if (interviewResult != null && interviewResult != DBNull.Value)
                            {
                                interviewId = Convert.ToInt32(interviewResult);
                            }
                        }

                        if (interviewId > 0)
                        {
                            using (SqlCommand deleteTranscript = new SqlCommand("DELETE FROM InterviewTranscripts WHERE InterviewId = @InterviewId", con, tran))
                            {
                                deleteTranscript.Parameters.AddWithValue("@InterviewId", interviewId);
                                deleteTranscript.ExecuteNonQuery();
                            }

                            using (SqlCommand deleteFeedback = new SqlCommand("DELETE FROM InterviewFeedback WHERE InterviewId = @InterviewId", con, tran))
                            {
                                deleteFeedback.Parameters.AddWithValue("@InterviewId", interviewId);
                                deleteFeedback.ExecuteNonQuery();
                            }

                            using (SqlCommand deleteQuestions = new SqlCommand("DELETE FROM InterviewQuestions WHERE InterviewId = @InterviewId", con, tran))
                            {
                                deleteQuestions.Parameters.AddWithValue("@InterviewId", interviewId);
                                deleteQuestions.ExecuteNonQuery();
                            }

                            using (SqlCommand deleteInterview = new SqlCommand("DELETE FROM Interviews WHERE InterviewId = @InterviewId", con, tran))
                            {
                                deleteInterview.Parameters.AddWithValue("@InterviewId", interviewId);
                                deleteInterview.ExecuteNonQuery();
                            }
                        }

                        using (SqlCommand deleteApplication = new SqlCommand("DELETE FROM AppliedJobs WHERE AppliedJobId = @AppliedJobId", con, tran))
                        {
                            deleteApplication.Parameters.AddWithValue("@AppliedJobId", appliedJobId);
                            deleteApplication.ExecuteNonQuery();
                        }
                    }

                    using (SqlCommand deleteJob = new SqlCommand("DELETE FROM Jobs WHERE JobId = @id", con, tran))
                    {
                        deleteJob.Parameters.AddWithValue("@id", jobId);
                        int r = deleteJob.ExecuteNonQuery();
                        if (r > 0)
                        {
                            tran.Commit();
                            lblMsg.Text = "Job delete successfully!";
                            lblMsg.CssClass = "alert alter-success";
                        }
                        else
                        {
                            tran.Rollback();
                            lblMsg.Text = "Cannot delete this record!";
                            lblMsg.CssClass = "alert alter-success";
                        }
                    }
                    GridView1.EditIndex = -1;
                    ShowJob();
                }
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "');</script>");
            }
            finally
            {
                if (con != null && con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
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