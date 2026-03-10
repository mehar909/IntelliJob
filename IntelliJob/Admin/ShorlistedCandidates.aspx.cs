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
using System.Net;
using System.Net.Mail;


namespace IntelliJob.Admin
{
    public partial class ShortlistedCandidates : System.Web.UI.Page
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
                ShowAppliedJob();
            }
        }

        private void ShowAppliedJob()
        {
            string query = string.Empty;
            con = new SqlConnection(str);
            query = @"Select Row_Number() over(Order by (Select 1)) as[Sr.No],aj.AppliedJobId,j.CompanyName,aj.JobId,j.Title,js.Mobile,
                            u.UserName,u.Email,u.Country from AppliedJobs aj 
                            inner join Users u on aj.UserId = u.UserId
                            LEFT JOIN JobSeekers js ON u.UserId = js.ProfileId
                            inner join Jobs j on aj.JobId = j.JobId where aj.Shortlisted = 'Yes' ";
            cmd = new SqlCommand(query, con);
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
                // Update Shortlisted to 'no' instead of deleting, so candidate appears in ViewJobApplicants again
                cmd = new SqlCommand("UPDATE AppliedJobs SET Shortlisted = 'no' WHERE AppliedJobId = @id", con);
                cmd.Parameters.AddWithValue("@id", AppliedjobId);
                con.Open();
                int r = cmd.ExecuteNonQuery();
                con.Close();
                
                if (r > 0)
                {
                    lblMsg.Text = "Candidate removed from shortlisted list successfully!";
                    lblMsg.CssClass = "alert alert-success";
                    lblMsg.Visible = true;
                    // Auto-hide message after 5 seconds
                    ClientScript.RegisterStartupScript(this.GetType(), "hideMessage", 
                        "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 5000);", true);
                }
                else
                {
                    lblMsg.Text = "Cannot remove this candidate!";
                    lblMsg.CssClass = "alert alert-danger";
                    lblMsg.Visible = true;
                }
                GridView1.EditIndex = -1;
                ShowAppliedJob();
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Error removing candidate: " + ex.Message;
                lblMsg.CssClass = "alert alert-danger";
                lblMsg.Visible = true;
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

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            
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

        protected void btnCallForInterview_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                // Get the GridView row where the button was clicked
                ImageButton btn = (ImageButton)sender;
                GridViewRow row = (GridViewRow)btn.NamingContainer;

                // Get hidden field values from the row
                HiddenField hdnEmail = row.FindControl("hdnEmail") as HiddenField;
                HiddenField hdnJobId = row.FindControl("hdnJobId") as HiddenField;
                HiddenField hdnUserName = row.FindControl("hdnUserName") as HiddenField;
                HiddenField hdnJobTitle = row.FindControl("hdnJobTitle") as HiddenField;
                HiddenField hdnCompanyName = row.FindControl("hdnCompanyName") as HiddenField;

                string email = hdnEmail?.Value?.Trim();
                string jobId = hdnJobId?.Value;
                string candidateName = hdnUserName?.Value?.Trim() ?? "";
                string jobTitle = hdnJobTitle?.Value?.Trim() ?? "";
                string company = hdnCompanyName?.Value?.Trim() ?? "";

                // Validate email
                if (string.IsNullOrEmpty(email))
                {
                    lblMsg.Text = $"Email is null or empty for candidate {candidateName}.";
                    lblMsg.CssClass = "alert alert-danger";
                    return;
                }

                if (!email.Contains("@"))
                {
                    lblMsg.Text = $"Invalid email address: '{email}'";
                    lblMsg.CssClass = "alert alert-danger";
                    return;
                }

                // Send email
                try
                {
                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress("online.jobportal.nuces@gmail.com", $"HR Manager - {company}");
                    mail.To.Add(email);
                    mail.Subject = "Onsite Interview Invitation";
                    mail.Body = $"Dear {candidateName},\n\tWe are pleased to inform you that you have been shortlisted for the {jobTitle} position at {company}. We would like to invite you to attend an onsite interview to discuss your qualifications and skills further.\r\n\r\nHere are the details of the interview:\r\n\r\nDate: 20, May, 2025\r\nTime: 10:00 am\r\n\r\nPlease bring the following documents with you to the interview:\n\n*Your Resume\n*Your original CNIC\n\nBest Regards,\nHiring Manager @ {company}\n";
                    mail.IsBodyHtml = false;

                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    smtp.Credentials = new NetworkCredential("online.jobportal.nuces@gmail.com", "gvozavnwfiierlug");
                    smtp.Send(mail);

                    lblMsg.Text = $"Interview email sent to {candidateName} ({email})!";
                    lblMsg.CssClass = "alert alert-success";
                    lblMsg.Visible = true;
                    // Auto-hide message after 5 seconds
                    ClientScript.RegisterStartupScript(this.GetType(), "hideMessage", 
                        "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 5000);", true);
                }
                catch (Exception ex)
                {
                    lblMsg.Text = $"Error sending email to {candidateName} ({email}): {ex.Message}";
                    lblMsg.CssClass = "alert alert-danger";
                    lblMsg.Visible = true;
                }
            }
            catch (Exception ex)
            {
                lblMsg.Text = $"Error: {ex.Message}";
                lblMsg.CssClass = "alert alert-danger";
                lblMsg.Visible = true;
            }
        }


        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewJob")
            {
                string jobId = e.CommandArgument.ToString();
                Response.Redirect("JobList.aspx?id=" + jobId + "&returnUrl=ShorlistedCandidates.aspx");
            }
        }

    }
}