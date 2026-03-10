using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace IntelliJob.Company
{
    public partial class CompanyMaster : MasterPage
    {
        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Only companies allowed
            if (Session["role"] == null || Session["role"].ToString() != "Company")
            {
                Response.Redirect("~/User/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadCompanyName();
            }
        }

        private void LoadCompanyName()
        {
            if (Session["user"] == null) return;

            using (var con = new SqlConnection(str))
            using (var cmd = new SqlCommand(@"
                SELECT c.CompanyName
                FROM Users u
                INNER JOIN Companies c ON u.UserId = c.CompanyId
                WHERE u.Username = @username", con))
            {
                cmd.Parameters.AddWithValue("@username", Session["user"].ToString());
                con.Open();
                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    lblCompanyName.Text = result.ToString();
                }
                else
                {
                    lblCompanyName.Text = Session["user"].ToString();
                }
            }
        }

        protected void lbLogout_OnClick(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/User/Login.aspx");
        }
    }
}
