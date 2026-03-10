using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IntelliJob.Company
{
    public partial class EditJobDetails : System.Web.UI.Page
    {
        SqlConnection con;
        SqlCommand cmd;
        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;
        string query;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["userId"] == null || Session["role"] == null || Session["role"].ToString() != "Company")
            {
                Response.Redirect("../User/Login.aspx");
            }

            if (!IsPostBack)
            {
                if (Request.QueryString["id"] != null)
                {
                    LoadJobDetails();
                    // Set the View All Applicants link URL with returnUrl if present
                    string viewApplicantsUrl = "ViewJobApplicants.aspx?jobId=" + Request.QueryString["id"];
                    string returnUrl = Request.QueryString["returnUrl"];
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        viewApplicantsUrl += "&returnUrl=" + Server.UrlEncode(returnUrl);
                    }
                    linkViewApplicants.NavigateUrl = viewApplicantsUrl;
                    // Set the back button URL
                    SetBackButtonUrl();
                }
                else
                {
                    lblMsg.Text = "Job ID not provided.";
                    lblMsg.CssClass = "alert alert-danger";
                }
            }
        }

        private void FillCompanyInfo()
        {
            try
            {
                con = new SqlConnection(str);
                query = @"
                        SELECT 
                            c.CompanyName,
                            c.Website,
                            c.CompanyLogo,
                            u.Email,
                            u.Address,
                            u.Country
                        FROM Companies c
                        INNER JOIN Users u ON c.CompanyId = u.UserId
                        WHERE c.CompanyId = @CompanyId";

                cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CompanyId", Session["userId"]);

                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                if (sdr.Read())
                {
                    // These are your existing textboxes – they stay editable
                    txtCompany.Text = sdr["CompanyName"].ToString();
                    txtWebsite.Text = sdr["Website"].ToString();
                    txtEmail.Text = sdr["Email"].ToString();
                    txtAddress.Text = sdr["Address"].ToString();

                    string country = sdr["Country"].ToString();
                    if (!string.IsNullOrEmpty(country) && ddlCountry.Items.FindByValue(country) != null)
                    {
                        ddlCountry.SelectedValue = country;
                    }

                    // You can also optionally show logo somewhere in UI if needed
                    // but we don't change the .aspx structure here.
                }
                sdr.Close();
            }
            catch (Exception ex)
            {
                // Optional: show error
                lblMsg.Visible = true;
                lblMsg.Text = ex.Message;
                lblMsg.CssClass = "alert alert-danger";
            }
            finally
            {
                if (con != null && con.State == System.Data.ConnectionState.Open)
                    con.Close();
            }
        }
        private void LoadJobDetails()
        {
            try
            {
                con = new SqlConnection(str);
                query = "Select * from Jobs where JobId = @id";
                cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", Request.QueryString["id"]);
                con.Open();
                SqlDataReader sdr = cmd.ExecuteReader();
                if (sdr.HasRows)
                {
                    while (sdr.Read())
                    {
                        txtJobTitle.Text = sdr["Title"].ToString();
                        txtNoOfPost.Text = sdr["NoOfPost"].ToString();
                        txtDescription.Text = sdr["Description"].ToString();
                        txtQualification.Text = sdr["Qualification"].ToString();
                        txtExperience.Text = sdr["Experience"].ToString();
                        txtSpecialization.Text = sdr["Specialization"].ToString();
                        if (sdr["LastDateToApply"] != DBNull.Value)
                        {
                            txtLastDate.Text = Convert.ToDateTime(sdr["LastDateToApply"]).ToString("yyyy-MM-dd");
                        }
                        txtSalary.Text = sdr["Salary"].ToString();
                        ddlJobType.SelectedValue = sdr["JobType"].ToString();
                        txtCompany.Text = sdr["CompanyName"].ToString();
                        txtWebsite.Text = sdr["Website"].ToString();
                        txtEmail.Text = sdr["Email"].ToString();
                        txtAddress.Text = sdr["Address"].ToString();
                        ddlCountry.SelectedValue = sdr["Country"].ToString();
                        txtState.Text = sdr["State"].ToString();
                        //if (sdr["CreateDate"] != DBNull.Value)
                        //{
                        //    lblPostedDate.Text = Convert.ToDateTime(sdr["CreateDate"]).ToString("yyyy-MM-dd");
                        //}

                        // Display company logo if available
                        if (sdr["CompanyImage"] != DBNull.Value && !string.IsNullOrEmpty(sdr["CompanyImage"].ToString()))
                        {
                            imgCompanyLogo.ImageUrl = "~/" + sdr["CompanyImage"].ToString();
                            imgCompanyLogo.Visible = true;
                        }
                        else
                        {
                            imgCompanyLogo.Visible = false;
                        }
                    }
                }
                else
                {
                    lblMsg.Text = "Job Not Found.";
                    lblMsg.CssClass = "alert alert-danger";
                }
                sdr.Close();
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Error loading job details: " + ex.Message;
                lblMsg.CssClass = "alert alert-danger";
            }
            finally
            {
                con.Close();
            }
        }


        protected void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                string type, concatQuery, imagePath = string.Empty;
                bool isValidToExecute = false;
                con = new SqlConnection(str);
                if (Request.QueryString["id"] != null)
                {
                    if (fuCompanyLogo.HasFile)
                    {
                        if (IsValidExtension(fuCompanyLogo.FileName))
                        {
                            concatQuery = "CompanyImage= @CompanyImage,";
                        }
                        else
                        {
                            concatQuery = string.Empty;
                        }
                    }
                    else
                    {
                        concatQuery = string.Empty;
                    }
                    query = @"Update Jobs set Title=@Title,NoOfPost=@NoOfPost,Description=@Description,Qualification=@Qualification,
                                Experience=@Experience,Specialization=@Specialization,LastDateToApply=@LastDateToApply,
                                Salary=@Salary,JobType=@JobType,CompanyName=@CompanyName," + concatQuery + @"Website=@Website,
                                Email=@Email,Address=@Address,Country=@Country,State=@State where JobId=@id";
                    type = "updated";

                    cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Title", txtJobTitle.Text.Trim());
                    cmd.Parameters.AddWithValue("@NoOfPost", txtNoOfPost.Text.Trim());
                    cmd.Parameters.AddWithValue("@Description", txtDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@Qualification", txtQualification.Text.Trim());
                    cmd.Parameters.AddWithValue("@Experience", txtExperience.Text.Trim());
                    cmd.Parameters.AddWithValue("@Specialization", txtSpecialization.Text.Trim());
                    cmd.Parameters.AddWithValue("@LastDateToApply", txtLastDate.Text.Trim());
                    cmd.Parameters.AddWithValue("@Salary", txtSalary.Text.Trim());
                    cmd.Parameters.AddWithValue("@JobType", ddlJobType.Text.Trim());
                    cmd.Parameters.AddWithValue("@CompanyName", txtCompany.Text.Trim());
                    cmd.Parameters.AddWithValue("@Website", txtWebsite.Text.Trim());
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());
                    cmd.Parameters.AddWithValue("@Country", ddlCountry.Text.Trim());
                    cmd.Parameters.AddWithValue("@State", txtState.Text.Trim());
                    cmd.Parameters.AddWithValue("@id", Request.QueryString["id"].ToString());
                    if (fuCompanyLogo.HasFile)
                    {
                        if (IsValidExtension(fuCompanyLogo.FileName))
                        {
                            Guid obj = Guid.NewGuid();
                            imagePath = "Images/" + obj.ToString() + fuCompanyLogo.FileName;
                            fuCompanyLogo.PostedFile.SaveAs(Server.MapPath("~/Images/") + obj.ToString() + fuCompanyLogo.FileName);
                            cmd.Parameters.AddWithValue("@CompanyImage", imagePath);
                            isValidToExecute = true;
                        }
                        else
                        {
                            lblMsg.Text = "Please select .jpg, .jpeg, .png for logo";
                            lblMsg.CssClass = "alert alert-danger";
                        }
                    }
                    else
                    {
                        isValidToExecute = true;
                    }
                }
                else
                {
                    query = @"
                        INSERT INTO Jobs 
                        (Title, NoOfPost, Description, Qualification, Experience, Specialization,
                         LastDateToApply, Salary, JobType, CompanyName, CompanyImage, Website, Email,
                         Address, Country, State, CreateDate, isFeatured)
                        VALUES
                        (@Title, @NoOfPost, @Description, @Qualification, @Experience, @Specialization,
                         @LastDateToApply, @Salary, @JobType, @CompanyName, @CompanyImage, @Website,
                         @Email, @Address, @Country, @State, @CreateDate, 0)";

                    type = "saved";
                    DateTime time = DateTime.Now;
                    cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Title", txtJobTitle.Text.Trim());
                    cmd.Parameters.AddWithValue("@NoOfPost", txtNoOfPost.Text.Trim());
                    cmd.Parameters.AddWithValue("@Description", txtDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@Qualification", txtQualification.Text.Trim());
                    cmd.Parameters.AddWithValue("@Experience", txtExperience.Text.Trim());
                    cmd.Parameters.AddWithValue("@Specialization", txtSpecialization.Text.Trim());
                    cmd.Parameters.AddWithValue("@LastDateToApply", txtLastDate.Text.Trim());
                    cmd.Parameters.AddWithValue("@Salary", txtSalary.Text.Trim());
                    cmd.Parameters.AddWithValue("@JobType", ddlJobType.Text.Trim());
                    cmd.Parameters.AddWithValue("@CompanyName", txtCompany.Text.Trim());
                    cmd.Parameters.AddWithValue("@Website", txtWebsite.Text.Trim());
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());
                    cmd.Parameters.AddWithValue("@Country", ddlCountry.Text.Trim());
                    cmd.Parameters.AddWithValue("@State", txtState.Text.Trim());
                    cmd.Parameters.AddWithValue("@CreateDate", time.ToString("yyyy-MM-dd HH:mm:ss"));
                    if (fuCompanyLogo.HasFile)
                    {
                        if (IsValidExtension(fuCompanyLogo.FileName))
                        {
                            Guid obj = Guid.NewGuid();
                            imagePath = "Images/" + obj.ToString() + fuCompanyLogo.FileName;
                            fuCompanyLogo.PostedFile.SaveAs(Server.MapPath("~/Images/") + obj.ToString() + fuCompanyLogo.FileName);
                            cmd.Parameters.AddWithValue("@CompanyImage", imagePath);
                            isValidToExecute = true;
                        }
                        else
                        {
                            lblMsg.Text = "Please select .jpg, .jpeg, .png for logo";
                            lblMsg.CssClass = "alert alert-danger";
                        }
                    }
                    else
                    {
                        string defaultLogo = GetCompanyLogo();
                        cmd.Parameters.AddWithValue("@CompanyImage", defaultLogo);
                        isValidToExecute = true;
                    }

                }
                if (isValidToExecute)
                {
                    con.Open();
                    int res = cmd.ExecuteNonQuery();
                    if (res > 0)
                    {
                        lblMsg.Text = "Job " + type + " Successfully";
                        lblMsg.CssClass = "alert alert-success";
                        clear();

                        FillCompanyInfo();
                    }
                    else
                    {
                        lblMsg.Text = "Cannot " + type + " The Records, Please Try After Sometime...";
                        lblMsg.CssClass = "alert alert-danger";

                    }
                }
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "')</script>");

            }
            finally
            {
                con.Close();
            }
        }

        private string GetCompanyLogo()
        {
            using (SqlConnection con = new SqlConnection(str))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT CompanyLogo FROM Companies WHERE CompanyId = @id", con))
            {
                cmd.Parameters.AddWithValue("@id", Session["userId"]);
                con.Open();
                var logo = cmd.ExecuteScalar()?.ToString();
                return string.IsNullOrEmpty(logo) ? "company_logo.png" : logo;
            }
        }

        private void clear()
        {
            txtJobTitle.Text = string.Empty;
            txtNoOfPost.Text = string.Empty;
            txtDescription.Text = string.Empty;
            txtQualification.Text = string.Empty;
            txtExperience.Text = string.Empty;
            txtSpecialization.Text = string.Empty;
            txtLastDate.Text = string.Empty;
            txtSalary.Text = string.Empty;

            txtState.Text = string.Empty;
            ddlJobType.ClearSelection();

        }

        private bool IsValidExtension(string fileName)
        {
            bool isValid = false;
            string[] fileExtension = { ".jpg", ".png", ".jpeg" };
            for (int i = 0; i < fileExtension.Length; i++)
            {
                if (fileName.Contains(fileExtension[i]))
                {
                    isValid = true;
                    break;
                }
            }
            return isValid;
        }
        private void SetBackButtonUrl()
        {
            // Check if returnUrl parameter exists
            string returnUrl = Request.QueryString["returnUrl"];
            
            if (!string.IsNullOrEmpty(returnUrl))
            {
                // Decode the URL-encoded returnUrl
                returnUrl = Server.UrlDecode(returnUrl);
                
                // Build the JobList URL with id and returnUrl parameters
                string jobId = Request.QueryString["id"];
                if (!string.IsNullOrEmpty(jobId))
                {
                    linkBack.NavigateUrl = "JobList.aspx?id=" + jobId + "&returnUrl=" + Server.UrlEncode(returnUrl);
                }
                else
                {
                    // If no jobId, just go to the returnUrl
                    if (!returnUrl.StartsWith("Company/") && !returnUrl.StartsWith("~/Company/"))
                    {
                        linkBack.NavigateUrl = "~/Company/" + returnUrl;
                    }
                    else if (returnUrl.StartsWith("Company/"))
                    {
                        linkBack.NavigateUrl = "~/" + returnUrl;
                    }
                    else
                    {
                        linkBack.NavigateUrl = returnUrl;
                    }
                }
            }
            else
            {
                // Default to JobList.aspx
                linkBack.NavigateUrl = "~/Company/JobList.aspx";
            }
        }
    }
}

