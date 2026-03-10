using System;
using System.Configuration;
using System.Data.SqlClient;

namespace IntelliJob.Company
{
    public partial class CompanyProfile : System.Web.UI.Page
    {
        string cs = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["userId"] == null || Session["role"].ToString() != "Company")
            {
                Response.Redirect("../User/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadProfile();
            }
        }

        private void LoadProfile()
        {
            int companyId = Convert.ToInt32(Session["userId"]);


            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"SELECT 
                                    u.Address, u.Country,
                                    c.CompanyName, c.Website, c.Description, 
                                    c.CompanySize, c.CompanyLogo
                                 FROM Users u
                                 INNER JOIN Companies c ON u.UserId = c.CompanyId
                                 WHERE u.UserId = @CompanyId";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CompanyId", companyId);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    txtCompanyName.Text = dr["CompanyName"].ToString();
                    txtCompanyNameHeader.Text = dr["CompanyName"].ToString();
                    txtWebsite.Text = dr["Website"].ToString();
                    txtDescription.Text = dr["Description"].ToString();
                    txtCompanySize.Text = dr["CompanySize"].ToString();
                    txtAddress.Text = dr["Address"].ToString();
                    ddlCountry.SelectedValue = dr["Country"].ToString();

                    string logo = dr["CompanyLogo"].ToString();
                    imgLogo.ImageUrl = "~/photos/" + logo;
                    Image1.ImageUrl = "~/photos/" + logo;
                }
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            int companyId = Convert.ToInt32(Session["UserId"]);
            string logoName = "";

            // Handle logo upload
            if (fuLogo.HasFile)
            {
                logoName = fuLogo.FileName;
                fuLogo.SaveAs(Server.MapPath("~/photos/") + logoName);
            }
            else
            {
                logoName = imgLogo.ImageUrl.Replace("~/photos/", "");
            }

            using (SqlConnection con = new SqlConnection(cs))
            {
                string updateUser = @"UPDATE Users 
                                      SET Address=@Address, Country=@Country 
                                      WHERE UserId=@UserId";

                string updateCompany = @"UPDATE Companies 
                                         SET CompanyName=@CompanyName,
                                             Website=@Website,
                                             Description=@Description,
                                             CompanySize=@CompanySize,
                                             CompanyLogo=@Logo
                                         WHERE CompanyId=@CompanyId";

                SqlCommand cmdUser = new SqlCommand(updateUser, con);
                SqlCommand cmdCompany = new SqlCommand(updateCompany, con);

                cmdUser.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());
                cmdUser.Parameters.AddWithValue("@Country", ddlCountry.SelectedValue);
                cmdUser.Parameters.AddWithValue("@UserId", companyId);

                cmdCompany.Parameters.AddWithValue("@CompanyName", txtCompanyName.Text.Trim());
                cmdCompany.Parameters.AddWithValue("@Website", txtWebsite.Text.Trim());
                cmdCompany.Parameters.AddWithValue("@Description", txtDescription.Text.Trim());
                cmdCompany.Parameters.AddWithValue("@CompanySize", txtCompanySize.Text.Trim());
                cmdCompany.Parameters.AddWithValue("@Logo", logoName);
                cmdCompany.Parameters.AddWithValue("@CompanyId", companyId);

                con.Open();
                cmdUser.ExecuteNonQuery();
                cmdCompany.ExecuteNonQuery();

                lblMsg.Visible = true;
                lblMsg.Text = "Profile Updated Successfully!";
                lblMsg.CssClass = "alert alert-success";
                // Auto-hide message after 7 seconds
                ClientScript.RegisterStartupScript(this.GetType(), "hideMessage",
                    "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 7000);", true);

            }
        }
    }
}
