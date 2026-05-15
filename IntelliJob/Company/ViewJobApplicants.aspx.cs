using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IntelliJob.Company
{
    public partial class ViewJobApplicants : System.Web.UI.Page
    {
        SqlConnection con;
        SqlCommand    cmd;
        DataTable     dt;
        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["userId"] == null ||
                Session["role"] == null   ||
                Session["role"].ToString() != "Company")
            {
                Response.Redirect("../User/Login.aspx");
            }

            if (!IsPostBack)
            {
                if (Request.QueryString["jobId"] != null)
                {
                    ShowApplicants();
                    SetBackButtonUrl();
                }
                else
                {
                    lblMsg.Text     = "Job ID not provided.";
                    lblMsg.CssClass = "alert alert-danger";
                    linkBack.NavigateUrl = "JobList.aspx";
                }
            }
        }

        private void ShowApplicants()
        {
            try
            {
                con = new SqlConnection(str);

                // ── Full query: applicants + password status + interview report ──
                string query = @"
                    SELECT
                        ROW_NUMBER() OVER (ORDER BY (SELECT 1))   AS [Sr.No],
                        u.UserId,
                        u.Username,
                        u.Email,
                        js.Mobile,
                        u.Country,
                        aj.AppliedJobId,
                        aj.InterviewPassword,
                        aj.PasswordUsed,
                        aj.InterviewSentAt,
                        -- Latest completed interview for this applicant on this job
                        (SELECT TOP 1 i.InterviewId
                         FROM   Interviews i
                         WHERE  i.UserId = aj.UserId
                           AND  i.JobId  = aj.JobId
                           AND  i.Status = 'completed'
                         ORDER BY i.InterviewId DESC)             AS CompletedInterviewId,
                        (SELECT TOP 1 f.TotalScore
                         FROM   Interviews i
                         INNER JOIN InterviewFeedback f ON i.InterviewId = f.InterviewId
                         WHERE  i.UserId = aj.UserId
                           AND  i.JobId  = aj.JobId
                           AND  i.Status = 'completed'
                         ORDER BY i.InterviewId DESC)             AS InterviewScore
                    FROM  AppliedJobs aj
                    INNER JOIN Users      u  ON aj.UserId    = u.UserId
                    INNER JOIN JobSeekers js ON u.UserId     = js.ProfileId
                    WHERE aj.JobId = @JobId
                      AND (aj.Shortlisted IS NULL OR LOWER(aj.Shortlisted) <> 'yes')";

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
                lblMsg.Text     = "Error loading applicants: " + ex.Message;
                lblMsg.CssClass = "alert alert-danger";
            }
        }

        // ── GridView events ──────────────────────────────────────────────────

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;
            ShowApplicants();
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            // ── "View Profile" link ──────────────────────────────────────────
            HyperLink lnkViewUser = e.Row.FindControl("lnkViewUser") as HyperLink;
            if (lnkViewUser != null && Request.QueryString["jobId"] != null)
            {
                string returnUrl = "ViewJobApplicants.aspx?jobId=" + Request.QueryString["jobId"];
                string origReturn = Request.QueryString["returnUrl"];
                if (!string.IsNullOrEmpty(origReturn))
                    returnUrl += "&returnUrl=" + Server.UrlEncode(origReturn);
                lnkViewUser.NavigateUrl += "&returnUrl=" + Server.UrlEncode(returnUrl);
            }

            // ── "Send Invite" button ─────────────────────────────────────────
            System.Web.UI.WebControls.Button btnSend =
                e.Row.FindControl("btnSendInvite") as System.Web.UI.WebControls.Button;
            if (btnSend != null)
            {
                var row        = e.Row.DataItem as DataRowView;
                bool pwdUsed   = row != null && row["PasswordUsed"] != DBNull.Value
                                 && Convert.ToBoolean(row["PasswordUsed"]);
                bool alreadySent = row != null && row["InterviewPassword"] != DBNull.Value
                                   && !string.IsNullOrEmpty(row["InterviewPassword"]?.ToString());

                if (pwdUsed)
                {
                    // Interview already taken — disable button
                    btnSend.Text    = "Interview Done";
                    btnSend.Enabled = false;
                    btnSend.CssClass = "btn btn-sm btn-secondary";
                }
                else if (alreadySent)
                {
                    btnSend.Text = "Resend Invite";
                }
            }

            // ── "View Report" link ───────────────────────────────────────────
            HyperLink lnkReport = e.Row.FindControl("lnkViewReport") as HyperLink;
            if (lnkReport != null)
            {
                var row = e.Row.DataItem as DataRowView;
                if (row != null && row["CompletedInterviewId"] != DBNull.Value)
                {
                    lnkReport.NavigateUrl =
                        "ViewCandidateReport.aspx?interviewId=" + row["CompletedInterviewId"].ToString();
                    lnkReport.Visible = true;

                    // Show score badge
                    Literal litScore = e.Row.FindControl("litScore") as Literal;
                    if (litScore != null && row["InterviewScore"] != DBNull.Value)
                    {
                        int score = Convert.ToInt32(row["InterviewScore"]);
                        string css = score >= 70 ? "#00b894" : score >= 40 ? "#fdcb6e" : "#d63031";
                        litScore.Text = $"<span style='background:{css};color:#fff;padding:3px 10px;" +
                                        $"border-radius:12px;font-size:12px;font-weight:600;'>{score}/100</span>";
                    }
                }
                else
                {
                    lnkReport.Visible = false;
                }
            }
        }

        // ── Shortlist button ─────────────────────────────────────────────────

        protected void btnShortlist_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                ImageButton   btn = (ImageButton)sender;
                GridViewRow   row = (GridViewRow)btn.NamingContainer;
                int appliedJobId  = Convert.ToInt32(
                    GridView1.DataKeys[row.RowIndex].Values["AppliedJobId"]);

                con = new SqlConnection(str);
                string updateQuery =
                    "UPDATE AppliedJobs SET Shortlisted = 'Yes' WHERE AppliedJobId = @AppliedJobId";
                cmd = new SqlCommand(updateQuery, con);
                cmd.Parameters.AddWithValue("@AppliedJobId", appliedJobId);
                con.Open();
                int rows = cmd.ExecuteNonQuery();
                con.Close();

                if (rows > 0)
                {
                    lblMsg.Visible  = true;
                    lblMsg.Text     = "Candidate shortlisted successfully!";
                    lblMsg.CssClass = "alert alert-success";
                    ClientScript.RegisterStartupScript(GetType(), "hideMsg",
                        $"setTimeout(function(){{var m=document.getElementById('{lblMsg.ClientID}');if(m)m.style.display='none';}},5000);", true);
                    ShowApplicants();
                }
                else
                {
                    lblMsg.Visible  = true;
                    lblMsg.Text     = "Failed to shortlist the candidate.";
                    lblMsg.CssClass = "alert alert-danger";
                }
            }
            catch (Exception ex)
            {
                lblMsg.Visible  = true;
                lblMsg.Text     = "Error: " + ex.Message;
                lblMsg.CssClass = "alert alert-danger";
            }
            finally
            {
                if (con != null && con.State == ConnectionState.Open) con.Close();
            }
        }

        private void SetBackButtonUrl()
        {
            string jobId     = Request.QueryString["jobId"];
            string returnUrl = Request.QueryString["returnUrl"];
            string backUrl   = !string.IsNullOrEmpty(jobId) ? "EditJobDetails.aspx?id=" + jobId : "JobList.aspx";
            if (!string.IsNullOrEmpty(returnUrl))
                backUrl += "&returnUrl=" + Server.UrlEncode(returnUrl);
            linkBack.NavigateUrl = backUrl;
        }
        protected string GetInviteStatus(object pwd, object used, object sentAt)
	{
            bool hasPwd = pwd != DBNull.Value && !string.IsNullOrEmpty(pwd?.ToString());
            bool isUsed = used != DBNull.Value && Convert.ToBoolean(used);
            string sentDate = sentAt != DBNull.Value && sentAt != null ? Convert.ToDateTime(sentAt).ToString("MMM d, HH:mm") : "";

            if (isUsed)
                return "<span class='status-used'><i class='fas fa-check-circle'></i> Interview Completed</span>";
            if (hasPwd)
                return $"<span class='status-sent'><i class='fas fa-paper-plane'></i> Invited {sentDate}</span>";
            return "<span class='status-none'>Not Sent</span>";
	}
    }
}
