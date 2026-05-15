using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI.WebControls;

namespace IntelliJob.User
{
    public partial class JobListing : System.Web.UI.Page
    {
        SqlConnection con;
        SqlCommand cmd;
        SqlDataAdapter sda;
        DataTable dt;

        string str = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;
        public int jobCount = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                showjobList();
                RBSelectedColorChange();
                lbljobCount.Text = JobCount(dt.Rows.Count);
            }
        }

        private void showjobList()
        {
            if (dt == null)
            {
                con = new SqlConnection(str);
                string query = @"SELECT JobId,Title,Salary,JobType,CompanyName,
                                        COALESCE(NULLIF(LTRIM(RTRIM(CompanyImage)), ''), 'Images/No_image.png') AS DisplayImage,
                                        Country,State,CreateDate
                                 FROM Jobs";
                cmd = new SqlCommand(query, con);
                sda = new SqlDataAdapter(cmd);
                dt = new DataTable();
                sda.Fill(dt);
            }

            if (dt != null && dt.Rows.Count > 0)
            {
                DataList1.DataSource = dt;
                DataList1.DataBind();
                DataList1.Visible = true;
            }
            else
            {
                DataList1.Visible = false;
            }
        }

        string JobCount(int count)
        {
            if (count > 1)
            {
                return "Total <b>" + count + "</b> jobs found";
            }
            else if (count == 1)
            {
                return "Total <b>" + count + "</b> job found";
            }
            else
            {
                return "No job found";
            }
        }

        protected void ddlCountry_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlCountry.SelectedValue != "0")
            {
                string selectedCountryLower = ddlCountry.SelectedValue.ToLower().Trim();
                con = new SqlConnection(str);

                string query = @"SELECT JobId,Title,Salary,JobType,CompanyName,
                                        COALESCE(NULLIF(LTRIM(RTRIM(CompanyImage)), ''), 'Images/No_image.png') AS DisplayImage,
                                        Country,State,CreateDate FROM Jobs
            where LOWER(Country) LIKE '%" + selectedCountryLower + "%' ";

                cmd = new SqlCommand(query, con);
                sda = new SqlDataAdapter(cmd);
                dt = new DataTable();
                sda.Fill(dt);
                showjobList();
                RBSelectedColorChange();
            }
            else
            {
                showjobList();
                RBSelectedColorChange();
            }
        }

        protected string GetImageUrl(Object url)
        {
            if (url == null || url == DBNull.Value)
            {
                return ResolveUrl("~/Images/No_image.png");
            }

            string logoPath = url.ToString().Trim();
            if (string.IsNullOrWhiteSpace(logoPath))
            {
                return ResolveUrl("~/Images/No_image.png");
            }

            if (logoPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || logoPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return logoPath;
            }

            string normalizedPath = logoPath.StartsWith("~/", StringComparison.OrdinalIgnoreCase)
                ? logoPath
                : "~/" + logoPath.TrimStart('~', '/');

            string[] candidates = new[]
            {
                normalizedPath,
                "~/Images/" + logoPath.TrimStart('~', '/'),
                "~/photos/" + logoPath.TrimStart('~', '/')
            };

            foreach (string candidate in candidates)
            {
                try
                {
                    if (File.Exists(Server.MapPath(candidate)))
                    {
                        return ResolveUrl(candidate);
                    }
                }
                catch
                {
                }
            }

            return ResolveUrl("~/Images/No_image.png");
        }

        public static string RelativeDate(DateTime theDate)
        {
            Dictionary<long, string> thresholds = new Dictionary<long, string>();
            int minute = 60;
            int hour = 60 * minute;
            int day = 24 * hour;
            thresholds.Add(60, "{0} seconds ago");
            thresholds.Add(minute * 2, "a minute ago");
            thresholds.Add(45 * minute, "{0} minutes ago");
            thresholds.Add(120 * minute, "an hour ago");
            thresholds.Add(day, "{0} hours ago");
            thresholds.Add(day * 2, "yesterday");
            thresholds.Add(day * 30, "{0} days ago");
            thresholds.Add(day * 365, "{0} months ago");
            thresholds.Add(long.MaxValue, "{0} years ago");
            long since = (DateTime.Now.Ticks - theDate.Ticks) / 10000000;
            foreach (long threshold in thresholds.Keys)
            {
                if (since < threshold)
                {
                    TimeSpan t = new TimeSpan((DateTime.Now.Ticks - theDate.Ticks));
                    return string.Format(thresholds[threshold], (t.Days > 365 ? t.Days / 365 : (t.Days > 0 ? t.Days : (t.Hours > 0 ? t.Hours : (t.Minutes > 0 ? t.Minutes : (t.Seconds > 0 ? t.Seconds : 0))))).ToString());
                }
            }
            return "";
        }

        protected void CheckBoxList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string jobType = string.Empty;
            jobType = SelectedCheckBox();
            if (jobType != "")
            {
                con = new SqlConnection(str);
                string query = @"SELECT Jobid,Title,Salary,JobType,CompanyName,
                                        COALESCE(NULLIF(LTRIM(RTRIM(CompanyImage)), ''), 'Images/No_image.png') AS DisplayImage,
                                        Country,State,CreateDate FROM Jobs
                    where JobType IN (" + jobType + ")";
                cmd = new SqlCommand(query, con);
                sda = new SqlDataAdapter(cmd);
                dt = new DataTable();
                sda.Fill(dt);
                showjobList();
                RBSelectedColorChange();
            }
            else
            {
                showjobList();
            }
        }

        string SelectedCheckBox()
        {
            string selectedJobType = RadioButtonListJobType.SelectedValue;

            if (string.IsNullOrEmpty(selectedJobType) || selectedJobType.Equals("Any", StringComparison.OrdinalIgnoreCase))
            {
                return "'Full Time','Part Time','Remote','Freelance'";
            }
            else
            {
                return "'" + selectedJobType + "'";
            }
        }

        string selectedRadioButton()
        {
            string postedDate = string.Empty;
            DateTime date = DateTime.Today;
            if (RadioButtonList1.SelectedValue == "1")
            {
                postedDate = "=Convert(DATE,'" + date.ToString("yyyy/MM/dd") + "') ";
            }
            else if (RadioButtonList1.SelectedValue == "2")
            {
                postedDate = " between Convert(DATE,'" + DateTime.Now.AddDays(-2).ToString("yyyy/MM/dd") + "') and Convert(DATE,'" + date.ToString("yyyy/MM/dd") + "') ";
            }
            else if (RadioButtonList1.SelectedValue == "3")
            {
                postedDate = " between Convert(DATE,'" + DateTime.Now.AddDays(-3).ToString("yyyy/MM/dd") + "') and Convert(DATE,'" + date.ToString("yyyy/MM/dd") + "') ";
            }
            else if (RadioButtonList1.SelectedValue == "4")
            {
                postedDate = " between Convert(DATE,'" + DateTime.Now.AddDays(-5).ToString("yyyy/MM/dd") + "') and Convert(DATE,'" + date.ToString("yyyy/MM/dd") + "') ";
            }
            else
            {
                postedDate = " between Convert(DATE,'" + DateTime.Now.AddDays(-10).ToString("yyyy/MM/dd") + "') and Convert(DATE,'" + date.ToString("yyyy/MM/dd") + "') ";
            }
            return postedDate;
        }

        protected void lbFilter_Click(object sender, EventArgs e)
        {
            try
            {
                bool isCondition = false;
                string subquery = string.Empty;
                string jobType = string.Empty;
                string postedDate = string.Empty;
                string query = string.Empty;
                List<string> queryList = new List<string>();
                con = new SqlConnection(str);

                if (ddlCountry.SelectedValue != "0")
                {
                    string selectedCountryLower = ddlCountry.SelectedValue.ToLower().Trim();
                    queryList.Add("LOWER(Country) LIKE '%" + selectedCountryLower + "%'");
                    isCondition = true;
                }

                jobType = SelectedCheckBox();

                if (jobType != "")
                {
                    queryList.Add("JobType IN (" + jobType + ")");
                    isCondition = true;
                }

                if (RadioButtonList1.SelectedValue != "0")
                {
                    postedDate = selectedRadioButton();
                    queryList.Add(" Convert(DATE,CreateDate) " + postedDate);
                    isCondition = true;
                }

                string dbKeyword = txtKeyword.Text?.Trim();
                if (!string.IsNullOrWhiteSpace(dbKeyword))
                {
                    string keywordLower = dbKeyword.ToLower().Trim();
                    string keywordQuery = @"
    LOWER(
        LTRIM(RTRIM(
            REPLACE(REPLACE(REPLACE(Title, CHAR(9), ''), CHAR(10), ''), CHAR(13), '')
        ))
    ) LIKE '%" + keywordLower + @"%'";

                    queryList.Add(keywordQuery);
                    isCondition = true;
                }

                if (isCondition)
                {
                    foreach (string a in queryList)
                    {
                        subquery += a + " and ";
                    }
                    subquery = subquery.Remove(subquery.LastIndexOf("and"), 3);
                    query = @"SELECT JobId,Title,Salary,JobType,CompanyName,
                                     COALESCE(NULLIF(LTRIM(RTRIM(CompanyImage)), ''), 'Images/No_image.png') AS DisplayImage,
                                     Country,State,CreateDate FROM Jobs where " + subquery + " ";
                }
                else
                {
                    query = @"SELECT JobId,Title,Salary,JobType,CompanyName,
                                     COALESCE(NULLIF(LTRIM(RTRIM(CompanyImage)), ''), 'Images/No_image.png') AS DisplayImage,
                                     Country,State,CreateDate FROM Jobs ";
                }

                SqlDataAdapter sda = new SqlDataAdapter(query, con);
                dt = new DataTable();
                sda.Fill(dt);
                showjobList();

                lbljobCount.Text = JobCount(dt.Rows.Count);
                RBSelectedColorChange();
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "');</script>");
            }
            finally
            {
                con.Close();
            }
        }

        protected void lbReset_Click(object sender, EventArgs e)
        {
            ddlCountry.ClearSelection();
            RadioButtonListJobType.SelectedValue = "Any";
            RadioButtonList1.SelectedValue = "Any";
            txtKeyword.Text = string.Empty;

            RBSelectedColorChange();
            showjobList();
            lbljobCount.Text = JobCount(dt.Rows.Count);
        }

        void RBSelectedColorChange()
        {
            foreach (ListItem item in RadioButtonList1.Items)
            {
                if (item.Selected)
                {
                    item.Attributes.Add("class", "selectedradio");
                }
                else
                {
                    item.Attributes.Remove("class");
                }
            }
        }
    }
}