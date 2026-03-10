    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    namespace IntelliJob.Company
    {
        public partial class PostJob : System.Web.UI.Page
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

                Session["title"] = "Post Job";

                if (!IsPostBack)
                {
                    if (Request.QueryString["id"] != null)
                    {
                        // Editing existing job (same as admin)
                        fillData();
                    }
                    else
                    {
                        // New job → auto-fill company info
                        FillCompanyInfo();
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
            private void fillData()
            {
                if (Request.QueryString["id"] != null)
                {
                    con = new SqlConnection(str);
                    query = "Select * from Jobs where JobId = '" + Request.QueryString["id"] + "' ";
                    cmd = new SqlCommand(query, con);
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
                            txtLastDate.Text = Convert.ToDateTime(sdr["LastDateToApply"]).ToString("yyyy-MM-dd");
                            txtSalary.Text = sdr["Salary"].ToString();
                            ddlJobType.SelectedValue = sdr["JobType"].ToString();
                            txtCompany.Text = sdr["CompanyName"].ToString();
                            txtWebsite.Text = sdr["Website"].ToString();
                            txtEmail.Text = sdr["Email"].ToString();
                            txtAddress.Text = sdr["Address"].ToString();
                            ddlCountry.SelectedValue = sdr["Country"].ToString();
                            txtState.Text = sdr["State"].ToString();
                            btnAdd.Text = "Update";
                            linkBack.Visible = true;
                            Session["title"] = "Edit Job";
                        }
                    }
                    else
                    {
                        lblMsg.Text = "Job Not Found.";
                        lblMsg.CssClass = "alert alert-danger";
                    }
                    sdr.Close();
                    con.Close();
                }
            }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            // Use using statements for proper resource disposal
            using (SqlConnection con = new SqlConnection(str))
            {
                SqlCommand cmd = null;
                string type, imagePath = string.Empty;
                string concatQuery = string.Empty;
                string query = string.Empty;
                bool isValidToExecute = false;

                try
                {
                    // 1. Determine action (Update or Insert)
                    bool isUpdate = Request.QueryString["id"] != null;

                    // 2. Handle File Upload Logic (Perform check/upload once)
                    string companyImageParamValue = string.Empty;

                    if (fuCompanyLogo.HasFile)
                    {
                        if (IsValidExtension(fuCompanyLogo.FileName))
                        {
                            // Valid file uploaded
                            Guid obj = Guid.NewGuid();
                            imagePath = "Images/" + obj.ToString() + fuCompanyLogo.FileName;
                            fuCompanyLogo.PostedFile.SaveAs(Server.MapPath("~/Images/") + obj.ToString() + fuCompanyLogo.FileName);
                            companyImageParamValue = imagePath;
                            isValidToExecute = true; // Set to true here only if everything succeeds

                            // For update query, we need to include the CompanyImage column
                            if (isUpdate)
                            {
                                concatQuery = "CompanyImage= @CompanyImage,";
                            }
                        }
                        else
                        {
                            // Invalid file extension
                            lblMsg.Text = "Please select .jpg, .jpeg, .png for logo";
                            lblMsg.CssClass = "alert alert-danger";
                            return; // CRITICAL FIX: Exit the method immediately on error
                        }
                    }
                    else // No file uploaded
                    {
                        // Since the INSERT query and the refactored UPDATE query always expect @CompanyImage,
                        // we must supply the current/default logo path.
                        companyImageParamValue = GetCompanyLogo(); // Use existing logo path

                        // For both insert and update, we proceed with the current image
                        isValidToExecute = true;

                        if (isUpdate)
                        {
                            concatQuery = "CompanyImage= @CompanyImage,";
                        }
                    }

                    // Note: The original INSERT query always expected @CompanyImage, so no changes needed for INSERT's query string.

                    // 3. Construct the SQL Query
                    if (isUpdate)
                    {
                        query = $@"Update Jobs set Title=@Title,NoOfPost=@NoOfPost,Description=@Description,Qualification=@Qualification,
                                         Experience=@Experience,Specialization=@Specialization,LastDateToApply=@LastDateToApply,
                                         Salary=@Salary,JobType=@JobType,CompanyName=@CompanyName,{concatQuery}Website=@Website,
                                         Email=@Email,Address=@Address,Country=@Country,State=@State where JobId=@id";
                        type = "Updated";
                    }
                    else // New job (Insert)
                    {
                        query = @"INSERT INTO Jobs (Title, NoOfPost, Description, Qualification, Experience, Specialization,
                                         LastDateToApply, Salary, JobType, CompanyName, CompanyImage, Website, Email,
                                         Address, Country, State, CreateDate)
                               VALUES (@Title, @NoOfPost, @Description, @Qualification, @Experience, @Specialization,
                                       @LastDateToApply, @Salary, @JobType, @CompanyName, @CompanyImage, @Website,
                                       @Email, @Address, @Country, @State, @CreateDate);
                               SELECT SCOPE_IDENTITY();";
                        type = "Posted";
                    }

                    // 4. Set Parameters and Execute
                    if (isValidToExecute) // Only execute if a valid image action was determined or no image action was needed (e.g., if update didn't include image)
                    {
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

                        // Conditional parameters
                        if (isUpdate)
                        {
                            cmd.Parameters.AddWithValue("@id", Request.QueryString["id"].ToString());
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@CreateDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        }

                        // Image parameter must be set since the queries were constructed to always require it
                        cmd.Parameters.AddWithValue("@CompanyImage", companyImageParamValue);


                        con.Open();

                        int newJobId = 0;
                        if (isUpdate)
                        {
                            // Update returns the number of rows affected, not SCOPE_IDENTITY
                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                // For updates, we use the ID from the query string
                                int.TryParse(Request.QueryString["id"], out newJobId);
                            }
                        }
                        else // Insert
                        {
                            // Insert returns SCOPE_IDENTITY()
                            newJobId = Convert.ToInt32(cmd.ExecuteScalar());

                            // 2. Insert initial featured status into FeaturedMarks table
                            if (newJobId > 0)
                            {
                                string featuredQuery = "INSERT INTO FeaturedMarks (JobId, isFeatured) VALUES (@JobId, 0)";
                                using (SqlCommand featuredCmd = new SqlCommand(featuredQuery, con))
                                {
                                    featuredCmd.Parameters.AddWithValue("@JobId", newJobId);
                                    featuredCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        if (newJobId > 0)
                        {
                            lblMsg.Text = "Job " + type + " Successfully.";
                            lblMsg.CssClass = "alert alert-success";
                            // Auto-hide message after 5 seconds
                            ClientScript.RegisterStartupScript(this.GetType(), "hideMessage",
                                "setTimeout(function() { var msg = document.getElementById('" + lblMsg.ClientID + "'); if(msg) msg.style.display='none'; }, 5000);", true);

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
                // The 'using' statement handles resource disposal, so the finally block is less necessary
                // but can be kept if there are other resources to manage.
                // The original code's finally block is now redundant due to the 'using (SqlConnection con = new SqlConnection(str))' block.
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
        }
    }