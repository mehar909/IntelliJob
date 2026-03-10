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


namespace IntelliJob.Company
{
    public partial class ShortlistedCandidates : System.Web.UI.Page
    {

        SqlConnection con;
        SqlCommand cmd;
        DataTable dt;
        String str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["userId"] == null || Session["role"] == null || Session["role"].ToString() != "Company")
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
            // Get the logged-in company's name
            string companyQuery = "SELECT CompanyName FROM Companies WHERE CompanyId = @id";
            SqlCommand cmdCompany = new SqlCommand(companyQuery, con);
            cmdCompany.Parameters.AddWithValue("@id", Session["userId"]);

            con.Open();
            string companyName = Convert.ToString(cmdCompany.ExecuteScalar());
            con.Close();

            if (string.IsNullOrEmpty(companyName))
            {
                lblMsg.Visible = true;
                lblMsg.Text = "Error: Company not found.";
                lblMsg.CssClass = "alert alert-danger";
                return;
            }
            query = @"
                        SELECT 
                        ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS [Sr.No],
                        aj.AppliedJobId,
                        aj.JobId,  
                        j.Title,
                        js.Name AS UserName,
                        u.Email,
                        js.Mobile,
                        js.Resume,
                        j.CompanyName
                    FROM AppliedJobs aj
                    INNER JOIN JobSeekers js ON aj.UserId = js.ProfileId
                    INNER JOIN Users u ON aj.UserId = u.UserId
                    INNER JOIN Jobs j ON aj.JobId = j.JobId
                    WHERE aj.Shortlisted = 'Yes'
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
                cmd = new SqlCommand("UPDATE AppliedJobs SET Shortlisted = 'no' WHERE AppliedJobId = @id", con);
                cmd.Parameters.AddWithValue("@id", AppliedjobId);
                con.Open();
                int r = cmd.ExecuteNonQuery();
                if (r > 0)
                {
                    lblMsg.Text = "Candidate removed from shortlisted list successfully!";
                    lblMsg.CssClass = "alert alter-success";
                    lblMsg.Visible = true;
                    // Auto-hide message after 7 seconds
                    ClientScript.RegisterStartupScript(this.GetType(), "hideMessage",
                        "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 7000);", true);

                }
                else
                {
                    lblMsg.Text = "Cannot remove this candidate!";
                    lblMsg.CssClass = "alert alter-success";
                    lblMsg.Visible = true;
                    // Auto-hide message after 7 seconds
                    ClientScript.RegisterStartupScript(this.GetType(), "hideMessage",
                        "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 7000);", true);

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

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Company/ViewApplications.aspx");
        }

        private void SendInterviewEmail(GridViewRow row)
        {
            // Retrieve data from row controls/cells
            HiddenField hdnEmail = row.FindControl("hdnEmail") as HiddenField;
            HiddenField hdnJobId = row.FindControl("hdnJobId") as HiddenField;

            // Based on your original Button1_Click logic:
            string email = hdnEmail?.Value?.Trim();
            string jobId = hdnJobId?.Value;
            string company = row.Cells[1].Text.Trim();   // Assuming cell 1 is Company
            string jobTitle = row.Cells[2].Text.Trim();  // Assuming cell 2 is Job Title
            string candidateName = row.Cells[3].Text.Trim(); // Assuming cell 3 is Candidate Name

            // Validation checks
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                lblMsg.Text = $"Invalid or missing email address for candidate {candidateName}.";
                lblMsg.CssClass = "alert alert-danger";
                lblMsg.Visible = true;
                return;
            }

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
                // Auto-hide message after 7 seconds
                ClientScript.RegisterStartupScript(this.GetType(), "hideMessage",
                    "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 7000);", true);

            }
            catch (Exception ex)
            {
                lblMsg.Text = $"Error sending email to {candidateName} ({email}): {ex.Message}";
                lblMsg.CssClass = "alert alert-danger";
            }
            lblMsg.Visible = true;
        }

        //protected void Button1_Click(object sender, EventArgs e)
        //{
        //    foreach (GridViewRow row in GridView1.Rows)
        //    {
        //        CheckBox chkSelect = row.FindControl("chkSelect") as CheckBox;
        //        if (chkSelect != null && chkSelect.Checked)
        //        {
        //            // Get hidden field values
        //            HiddenField hdnEmail = row.FindControl("hdnEmail") as HiddenField;
        //            HiddenField hdnJobId = row.FindControl("hdnJobId") as HiddenField;

        //            string email = hdnEmail?.Value?.Trim();
        //            string jobId = hdnJobId?.Value;
        //            string candidateName = row.Cells[3].Text.Trim();   // Or make this also a hidden field
        //            string jobTitle = row.Cells[2].Text.Trim();
        //            string company = row.Cells[1].Text.Trim();
        //            // Or make this also a hidden field

        //            // 🧪 DEBUG: Check what email was retrieved
        //            if (string.IsNullOrEmpty(email))
        //            {
        //                lblMsg.Text = $"Email is null or empty for candidate {candidateName}.";
        //                lblMsg.CssClass = "alert alert-danger";
        //                continue;
        //            }

        //            if (!email.Contains("@"))
        //            {
        //                lblMsg.Text = $"Invalid email address: '{email}'";
        //                lblMsg.CssClass = "alert alert-danger";
        //                continue;
        //            }

        //            try
        //            {
        //                MailMessage mail = new MailMessage();
        //                mail.From = new MailAddress("online.jobportal.nuces@gmail.com", $"HR Manager - {company}");
        //                mail.To.Add(email);
        //                mail.Subject = "Onsite Interview Invitation";
        //                mail.Body = $"Dear {candidateName},\n\tWe are pleased to inform you that you have been shortlisted for the {jobTitle} position at {company}. We would like to invite you to attend an onsite interview to discuss your qualifications and skills further.\r\n\r\nHere are the details of the interview:\r\n\r\nDate: 20, May, 2025\r\nTime: 10:00 am\r\n\r\nPlease bring the following documents with you to the interview:\n\n*Your Resume\n*Your original CNIC\n\nBest Regards,\nHiring Manager @ {company}\n";
        //                mail.IsBodyHtml = false;

        //                SmtpClient smtp = new SmtpClient();
        //                smtp.Host = "smtp.gmail.com";
        //                smtp.Port = 587;
        //                smtp.EnableSsl = true;
        //                smtp.Credentials = new NetworkCredential("online.jobportal.nuces@gmail.com", "gvozavnwfiierlug");
        //                smtp.Send(mail);

        //                lblMsg.Text = $"Interview email sent to {candidateName} ({email})!";
        //                lblMsg.CssClass = "alert alert-success";
        //            }
        //            catch (Exception ex)
        //            {
        //                lblMsg.Text = $"Error sending email to {candidateName} ({email}): {ex.Message}";
        //                lblMsg.CssClass = "alert alert-danger";
        //            }
        //        }
        //    }
        //}

        // Updated Button1_Click to use the new helper method
        protected void Button1_Click(object sender, EventArgs e)
        {
            lblMsg.Text = string.Empty; // Clear message at start
            bool anySelected = false;

            foreach (GridViewRow row in GridView1.Rows)
            {
                CheckBox chkSelect = row.FindControl("chkSelect") as CheckBox;
                if (chkSelect != null && chkSelect.Checked)
                {
                    // Call the shared email function
                    SendInterviewEmail(row);
                    anySelected = true;
                    // Note: If multiple emails are sent, the lblMsg will only show the status 
                    // of the last processed row.
                }
            }

            if (!anySelected && string.IsNullOrEmpty(lblMsg.Text))
            {
                lblMsg.Text = "Please select at least one candidate to send an interview mail.";
                lblMsg.CssClass = "alert alert-info";
                lblMsg.Visible = true;
            }
        }


        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string jobId = e.CommandArgument.ToString();
            if (e.CommandName == "ViewJob")
            {
                string url = "JobList.aspx?id=" + jobId;
                url += "&returnUrl=" + Server.UrlEncode("~/Company/ShorlistedCandidates.aspx");
                Response.Redirect(url);
                return;
            }

            if (e.CommandName == "SendSingleMail")
            {
                // 1. Find the row that contains the button that was clicked
                LinkButton btn = (LinkButton)e.CommandSource;
                GridViewRow row = (GridViewRow)btn.NamingContainer;

                // 2. Clear message and call the shared email function for this single row
                lblMsg.Text = string.Empty;
                lblMsg.Visible = false;

                SendInterviewEmail(row);

                return;
            }
        }

    }
}