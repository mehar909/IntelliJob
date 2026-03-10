using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IntelliJob.Company
{
    public partial class UserList : System.Web.UI.Page
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
                ShowUsers();
            }
        }
        private void ShowUsers()
        {
            string query = string.Empty;
            con = new SqlConnection(str);
            query = @"Select Row_Number() over(Order by (Select 1)) as [Sr.No], u.UserId, u.Username, u.Email, js.Mobile, u.Country from Users u inner join JobSeekers js on u.UserId = js.ProfileId";
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
            ShowUsers();
        }

        protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                GridViewRow row = GridView1.Rows[e.RowIndex];
                int userId = Convert.ToInt32(GridView1.DataKeys[e.RowIndex].Values[0]);
                con = new SqlConnection(str);
                
                // First, delete related records from AppliedJobs (even if user has no applications)
                cmd = new SqlCommand("Delete from AppliedJobs where UserId=@id", con);
                cmd.Parameters.AddWithValue("@id", userId);
                con.Open();
                cmd.ExecuteNonQuery(); // Don't check result - user might not have any applications
                con.Close();
                
                // Also delete from AdminContact and AdminReply if user has sent messages
                // First get all ContactIds for this user
                cmd = new SqlCommand("SELECT ContactId FROM AdminContact WHERE UserId=@id", con);
                cmd.Parameters.AddWithValue("@id", userId);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                System.Collections.Generic.List<int> contactIds = new System.Collections.Generic.List<int>();
                while (reader.Read())
                {
                    contactIds.Add(reader.GetInt32(0));
                }
                reader.Close();
                con.Close();
                
                // Delete replies for these contacts
                foreach (int contactId in contactIds)
                {
                    cmd = new SqlCommand("DELETE FROM AdminReply WHERE ContactId=@contactId", con);
                    cmd.Parameters.AddWithValue("@contactId", contactId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
                
                // Now delete from AdminContact
                cmd = new SqlCommand("Delete from AdminContact where UserId=@id", con);
                cmd.Parameters.AddWithValue("@id", userId);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                
                // Now delete the user
                cmd = new SqlCommand("Delete from Users where UserId = @id", con);
                cmd.Parameters.AddWithValue("@id", userId);
                con.Open();
                int r = cmd.ExecuteNonQuery();
                con.Close();
                
                if (r > 0)
                {
                    lblMsg.Text = "User deleted successfully!";
                    lblMsg.CssClass = "alert alert-success";
                    // Auto-hide message after 7 seconds
                    ClientScript.RegisterStartupScript(this.GetType(), "hideMessage",
                        "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 7000);", true);

                }
                else
                {
                    lblMsg.Text = "Cannot delete this record!";
                    lblMsg.CssClass = "alert alert-danger";
                }
                GridView1.EditIndex = -1;
                ShowUsers();
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Error deleting user: " + ex.Message;
                lblMsg.CssClass = "alert alert-danger";
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
    }
}