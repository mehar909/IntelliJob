using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IntelliJob.Admin
{
    public partial class UserList : System.Web.UI.Page
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
                ShowUsers();
            }
        }
        private void ShowUsers()
        {
            string query = string.Empty;
            con = new SqlConnection(str);
            query = @"Select Row_Number() over(Order by (Select 1)) as[Sr.No], u.UserId, u.Username, u.Email, u.Address, u.Country 
                      from Users u";
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
                con.Open();
                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    // First, delete related records from AppliedJobs
                    cmd = new SqlCommand("DELETE FROM AppliedJobs WHERE UserId=@id", con, tran);
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.ExecuteNonQuery(); // Don't check result - user might not have any applications
                    
                    // Delete from AdminReply and AdminContact
                    // First get all ContactIds for this user
                    cmd = new SqlCommand("SELECT ContactId FROM AdminContact WHERE UserId=@id", con, tran);
                    cmd.Parameters.AddWithValue("@id", userId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    System.Collections.Generic.List<int> contactIds = new System.Collections.Generic.List<int>();
                    while (reader.Read())
                    {
                        contactIds.Add(reader.GetInt32(0));
                    }
                    reader.Close();
                    
                    // Delete replies for these contacts
                    foreach (int contactId in contactIds)
                    {
                        cmd = new SqlCommand("DELETE FROM AdminReply WHERE ContactId=@contactId", con, tran);
                        cmd.Parameters.AddWithValue("@contactId", contactId);
                        cmd.ExecuteNonQuery();
                    }
                    
                    // Delete from AdminContact
                    cmd = new SqlCommand("DELETE FROM AdminContact WHERE UserId=@id", con, tran);
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.ExecuteNonQuery();
                    
                    // Delete from JobSeekers (if exists)
                    cmd = new SqlCommand("DELETE FROM JobSeekers WHERE ProfileId=@id", con, tran);
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.ExecuteNonQuery();
                    
                    // Delete from Companies (if exists)
                    cmd = new SqlCommand("DELETE FROM Companies WHERE CompanyId=@id", con, tran);
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.ExecuteNonQuery();
                    
                    // Finally delete from Users
                    cmd = new SqlCommand("DELETE FROM Users WHERE UserId=@id", con, tran);
                    cmd.Parameters.AddWithValue("@id", userId);
                    int r = cmd.ExecuteNonQuery();
                    
                    tran.Commit();
                    
                    if (r > 0)
                    {
                        lblMsg.Text = "User deleted successfully!";
                        lblMsg.CssClass = "alert alert-success";
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
                    tran.Rollback();
                    throw;
                }
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