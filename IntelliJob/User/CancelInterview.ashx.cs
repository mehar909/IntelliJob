using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.SessionState;

namespace IntelliJob.User
{
    public class CancelInterview : IHttpHandler, IRequiresSessionState
    {
        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

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
                if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out int interviewId))
                {
                    context.Response.StatusCode = 400;
                    context.Response.Write("{\"error\":\"Invalid InterviewId\"}");
                    return;
                }

                int userId = Convert.ToInt32(context.Session["userId"]);

                // Verify ownership and that interview is not already completed
                using (SqlConnection con = new SqlConnection(str))
                {
                    con.Open();
                    string checkQuery = "SELECT UserId, Status FROM Interviews WHERE InterviewId = @Id";
                    using (SqlCommand cmd = new SqlCommand(checkQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", interviewId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read() || Convert.ToInt32(reader["UserId"]) != userId)
                            {
                                context.Response.StatusCode = 403;
                                context.Response.Write("{\"error\":\"Access denied\"}");
                                return;
                            }
                            string status = reader["Status"].ToString().ToLower();
                            if (status == "completed")
                            {
                                context.Response.StatusCode = 400;
                                context.Response.Write("{\"error\":\"Cannot cancel a completed interview\"}");
                                return;
                            }
                        }
                    }
                }

                // Update status to cancelled
                using (SqlConnection con = new SqlConnection(str))
                {
                    con.Open();
                    string updateQuery = "UPDATE Interviews SET Status = 'cancelled' WHERE InterviewId = @Id";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", interviewId);
                        cmd.ExecuteNonQuery();
                    }
                }

                context.Response.Write("{\"success\":true}");
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write("{\"error\":\"" + ex.Message.Replace("\"", "'") + "\"}");
            }
        }

        public bool IsReusable { get { return false; } }
    }
}
