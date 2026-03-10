using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace IntelliJob.Company
{
    public partial class CompanyDashboard : System.Web.UI.Page
    {
        SqlConnection con;
        SqlDataAdapter sda;
        DataTable dt;
        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Only allow logged-in companies
            if (Session["role"] == null || Session["role"].ToString() != "Company")
            {
                Response.Redirect("~/User/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                Jobs();
                Applications();
                Applicants();
                Shortlisted();

            }
        }

        private void ContactCount()
        {
            con = new SqlConnection(str);
            sda = new SqlDataAdapter("SELECT COUNT(*) FROM Contact", con);
            dt = new DataTable();
            sda.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                Session["Contact"] = dt.Rows[0][0];
            }
            else
            {
                Session["Contact"] = 0;
            }
        }

        private void Applications()
        {
            string companyName = GetCompanyName();

            using (SqlConnection con = new SqlConnection(str))
            using (SqlDataAdapter sda = new SqlDataAdapter(@"
        SELECT COUNT(*)
        FROM AppliedJobs aj
        INNER JOIN Jobs j ON aj.JobId = j.JobId
        WHERE j.CompanyName = @company", con))
            {
                sda.SelectCommand.Parameters.AddWithValue("@company", companyName);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                Session["Applications"] = dt.Rows[0][0];
            }
        }


        private void Shortlisted()
        {
            string companyName = GetCompanyName();

            using (SqlConnection con = new SqlConnection(str))
            using (SqlDataAdapter sda = new SqlDataAdapter(@"
        SELECT COUNT(*)
        FROM AppliedJobs aj
        INNER JOIN Jobs j ON aj.JobId = j.JobId
        WHERE aj.Shortlisted = 'Yes'
          AND j.CompanyName = @company", con))
            {
                sda.SelectCommand.Parameters.AddWithValue("@company", companyName);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                Session["Shortlisted"] = dt.Rows[0][0];
            }
        }


        private void Jobs()
        {
            string companyName = GetCompanyName();

            using (SqlConnection con = new SqlConnection(str))
            using (SqlDataAdapter sda = new SqlDataAdapter(
                "SELECT COUNT(*) FROM Jobs WHERE CompanyName = @company", con))
            {
                sda.SelectCommand.Parameters.AddWithValue("@company", companyName);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                Session["Jobs"] = dt.Rows[0][0];
            }
        }


        private void Applicants()
        {
            string companyName = GetCompanyName();

            using (SqlConnection con = new SqlConnection(str))
            using (SqlDataAdapter sda = new SqlDataAdapter(@"
        SELECT COUNT(DISTINCT aj.UserId)
        FROM AppliedJobs aj
        INNER JOIN Jobs j ON aj.JobId = j.JobId
        WHERE j.CompanyName = @company", con))
            {
                sda.SelectCommand.Parameters.AddWithValue("@company", companyName);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                Session["Applicants"] = dt.Rows[0][0];
            }
        }


        private string GetCompanyName()
        {
            string name = "";
            using (SqlConnection con = new SqlConnection(str))
            using (SqlCommand cmd = new SqlCommand("SELECT CompanyName FROM Companies WHERE CompanyId = @id", con))
            {
                cmd.Parameters.AddWithValue("@id", Session["userId"]);
                con.Open();
                name = Convert.ToString(cmd.ExecuteScalar());
            }
            return name;
        }

    }
}
