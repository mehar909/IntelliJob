<%@ WebHandler Language="C#" Class="SendInterviewInvite" %>

using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.SessionState;

/// <summary>
/// Single-file HTTP handler (no CodeBehind) for Mono/Linux compatibility.
/// Called via AJAX from ViewJobApplicants when the company clicks "Send Invite".
/// POST param: appliedJobId (int)
/// </summary>
public class SendInterviewInvite : IHttpHandler, IRequiresSessionState
{
    private string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";

        if (context.Session["userId"] == null ||
            context.Session["role"] == null ||
            context.Session["role"].ToString() != "Company")
        {
            context.Response.StatusCode = 401;
            context.Response.Write("{\"error\":\"Not authorised\"}");
            return;
        }

        if (context.Request.HttpMethod != "POST")
        {
            context.Response.StatusCode = 405;
            context.Response.Write("{\"error\":\"POST only\"}");
            return;
        }

        try
        {
            string idStr = context.Request.Form["appliedJobId"];
            if (!int.TryParse(idStr, out int appliedJobId) || appliedJobId <= 0)
            {
                context.Response.StatusCode = 400;
                context.Response.Write("{\"error\":\"Invalid appliedJobId\"}");
                return;
            }

            int companyId = Convert.ToInt32(context.Session["userId"]);

            string candidateName  = "";
            string candidateEmail = "";
            string jobTitle       = "";
            int    jobId          = 0;
            bool   alreadySent    = false;

            using (SqlConnection con = new SqlConnection(str))
            {
                con.Open();
                string sql = @"
                    SELECT  u.Email, u.Username, j.Title, j.JobId, aj.InterviewPassword
                    FROM    AppliedJobs aj
                    INNER JOIN Users u  ON aj.UserId = u.UserId
                    INNER JOIN Jobs  j  ON aj.JobId  = j.JobId
                    WHERE   aj.AppliedJobId = @AppliedJobId
                      AND   j.JobId IN (
                                SELECT JobId FROM Jobs
                                WHERE  CompanyName = (
                                    SELECT CompanyName FROM Companies
                                    WHERE  CompanyId = @CompanyId))";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@AppliedJobId", appliedJobId);
                    cmd.Parameters.AddWithValue("@CompanyId",    companyId);
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (!rdr.Read())
                        {
                            context.Response.StatusCode = 403;
                            context.Response.Write("{\"error\":\"Application not found or access denied\"}");
                            return;
                        }
                        candidateEmail = rdr["Email"].ToString();
                        candidateName  = rdr["Username"].ToString();
                        jobTitle       = rdr["Title"].ToString();
                        jobId          = Convert.ToInt32(rdr["JobId"]);
                        alreadySent    = (rdr["InterviewPassword"] != DBNull.Value &&
                                         !string.IsNullOrEmpty(rdr["InterviewPassword"].ToString()));
                    }
                }
            }

            string password;

            if (alreadySent)
            {
                using (SqlConnection con = new SqlConnection(str))
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT InterviewPassword FROM AppliedJobs WHERE AppliedJobId = @Id", con))
                {
                    cmd.Parameters.AddWithValue("@Id", appliedJobId);
                    con.Open();
                    password = cmd.ExecuteScalar()?.ToString() ?? GeneratePassword();
                }
            }
            else
            {
                password = GeneratePassword();
                using (SqlConnection con = new SqlConnection(str))
                using (SqlCommand cmd = new SqlCommand(
                    @"UPDATE AppliedJobs
                      SET InterviewPassword = @Pwd, PasswordUsed = 0, InterviewSentAt = GETDATE()
                      WHERE AppliedJobId = @Id", con))
                {
                    cmd.Parameters.AddWithValue("@Pwd", password);
                    cmd.Parameters.AddWithValue("@Id",  appliedJobId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            string baseUrl = context.Request.Url.GetLeftPart(UriPartial.Authority) +
                             context.Request.ApplicationPath.TrimEnd('/');
            string interviewUrl = baseUrl + "/User/InterviewAccess.aspx?jobId=" +
                                  jobId + "&pwd=" + Uri.EscapeDataString(password);

            SendEmail(candidateEmail, candidateName, jobTitle, password, interviewUrl);

            string msg = alreadySent ? "Invitation re-sent successfully." : "Interview invitation sent successfully.";
            context.Response.Write("{\"success\":true,\"message\":\"" + msg + "\"}");
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.Write("{\"error\":\"" + ex.Message.Replace("\"", "'") + "\"}");
        }
    }

    private void SendEmail(string toEmail, string name, string jobTitle, string password, string url)
    {
        string host = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
        int    port = int.Parse(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
        string user = ConfigurationManager.AppSettings["SmtpUser"] ?? "";
        string pass = ConfigurationManager.AppSettings["SmtpPass"] ?? "";
        string from = ConfigurationManager.AppSettings["SmtpFrom"] ?? user;

        string body = "<html><body style='font-family:Arial,sans-serif;color:#2d3436;'>" +
            "<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:12px;padding:40px;'>" +
            "<h2 style='color:#6c5ce7;'>Interview Invitation</h2>" +
            "<p>Dear <strong>" + name + "</strong>,</p>" +
            "<p>You have been invited to take an AI-powered interview for <strong>" + jobTitle + "</strong>.</p>" +
            "<div style='background:#f8f9fa;border-radius:10px;padding:20px;margin:24px 0;text-align:center;'>" +
            "<p style='margin:0 0 8px;color:#636e72;font-size:14px;'>Your one-time interview password</p>" +
            "<span style='font-size:28px;font-weight:700;letter-spacing:4px;color:#6c5ce7;'>" + password + "</span>" +
            "</div>" +
            "<a href='" + url + "' style='display:inline-block;background:#6c5ce7;color:#fff;" +
            "text-decoration:none;padding:14px 32px;border-radius:8px;font-weight:600;font-size:16px;'>" +
            "Open Interview Room</a>" +
            "<p style='margin-top:16px;font-size:13px;color:#b2bec3;'>Or copy: " + url + "</p>" +
            "<p style='font-size:12px;color:#b2bec3;margin-top:16px;'>This password can only be used once.</p>" +
            "</div></body></html>";

        using (MailMessage mail = new MailMessage())
        {
            mail.From       = new MailAddress(from, "IntelliJob");
            mail.To.Add(toEmail);
            mail.Subject    = "Your Interview Invitation - " + jobTitle;
            mail.Body       = body;
            mail.IsBodyHtml = true;

            using (SmtpClient smtp = new SmtpClient(host, port))
            {
                smtp.EnableSsl   = true;
                smtp.Credentials = new NetworkCredential(user, pass);
                smtp.Send(mail);
            }
        }
    }

    private string GeneratePassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var rng   = new System.Security.Cryptography.RNGCryptoServiceProvider();
        var bytes = new byte[10];
        rng.GetBytes(bytes);
        var sb = new System.Text.StringBuilder();
        foreach (byte b in bytes)
            sb.Append(chars[b % chars.Length]);
        return sb.ToString();
    }

    public bool IsReusable { get { return false; } }
}
