using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.SessionState;

namespace IntelliJob.User
{
    public class ConsumeInterviewAccess : IHttpHandler, IRequiresSessionState
    {
        private readonly string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            if (context.Session["userId"] == null)
            {
                context.Response.StatusCode = 401;
                context.Response.Write("{\"error\":\"Not authenticated\"}");
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
                string idStr = context.Request.Form["InterviewId"];
                if (!int.TryParse(idStr, out int interviewId))
                {
                    context.Response.StatusCode = 400;
                    context.Response.Write("{\"error\":\"Invalid InterviewId\"}");
                    return;
                }

                object authorizedId = context.Session["AuthorizedInterviewId"];
                if (authorizedId == null || Convert.ToInt32(authorizedId) != interviewId)
                {
                    context.Response.StatusCode = 403;
                    context.Response.Write("{\"error\":\"Access denied\"}");
                    return;
                }

                using (SqlConnection con = new SqlConnection(str))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(@"UPDATE InterviewInvitations
                                                             SET IsPasswordUsed = 1,
                                                                 PasswordUsedAt = @Now
                                                             WHERE InterviewId = @InterviewId", con))
                    {
                        cmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);
                        cmd.Parameters.AddWithValue("@InterviewId", interviewId);
                        cmd.ExecuteNonQuery();
                    }
                }

                context.Session.Remove("AuthorizedInterviewId");
                context.Response.Write("{\"success\":true}");
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write("{\"error\":\"" + ex.Message.Replace("\"", "'") + "\"}");
            }
        }

        public bool IsReusable => false;
    }
}
