using DevExpress.Web.Mvc;
using ReportHelper.Models.TreeView;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ReportHelper.Reports;
using DevExpress.XtraReports.UI;
using System.Drawing;
using ReportHelper.Models;
namespace ReportHelper.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            int i = 1;
            List<TreeNode> nodes = new List<TreeNode>();
            DataTable dt = GetDataTableFromDb("select object_id as Id, name as Name from sys.tables where name <> 'sysdiagrams' and is_ms_shipped <> 1");
            DataTable dt2 = GetDataTableFromDb("SELECT C.name AS Name, C.object_id as SubId FROM sys.objects AS T JOIN sys.columns AS C ON T.object_id = C.object_id WHERE T.type_desc = 'USER_TABLE' and T.is_ms_shipped <> 1 and C.name <> 'rowguid' and T.name <> 'sysdiagrams'");

            dt2.Columns.Add("Id", typeof(System.Int32));
            foreach (DataRow row in dt2.Rows)
            {
                row["Id"] = i++;
            }

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    nodes.Add(
                        new TreeNode
                        {
                            id = row["Id"].ToString(),
                            parent = "#",
                            text = row["Name"].ToString()
                        });
                }
            }

            if (dt2 != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt2.Rows)
                {
                    nodes.Add(
                        new TreeNode
                        {
                            id = row["SubId"].ToString() + "-" + row["Id"].ToString(),
                            parent = row["SubId"].ToString(),
                            text = row["Name"].ToString()
                        });
                }
            }

            ViewBag.Json = (new JavaScriptSerializer()).Serialize(nodes);
            strQuery qry = new strQuery();
            return View(qry);  
        }
        [HttpPost]
        public ActionResult Index(strQuery qr)
        {
            String strQuery = qr.selectQuery;
            String strTitle = qr.title;
            SqlConnection con = ConnectToDb();
            SqlCommand sqlcmd = new SqlCommand(strQuery, con);
            sqlcmd.CommandType = CommandType.Text;

            SqlDataAdapter Adpt = new SqlDataAdapter(strQuery, con);
            try
            {
                sqlcmd.ExecuteNonQuery();
                return RedirectToAction("Report", new { message = strQuery, message2 = strTitle });
            }
            catch (SqlException ex)
            {
                
                ViewBag.Error = ex.Message;
                con.Close();
                return View("Index");
            }
            
        }
        public ActionResult Report (String message, String message2)
        {

            String qr = message;
            String title = "";
            if (message2 != null)
            {
               title = message2.ToUpper();
            }
            SqlConnection cnn = ConnectToDb();
            DataSet dt = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = new SqlCommand(qr, cnn);
            da.Fill(dt);
            XtraReport1 xrp = new XtraReport1();
            xrp.DataSource = dt;
            InitBands(xrp);
            InitDetailsBaseXRTable(xrp, title);
            return View(xrp);
        }
        public SqlConnection ConnectToDb()
        {
            SqlConnection conn = new SqlConnection();
            String conn_str = "Data Source=DESKTOP-8BQ2NC6;Initial Catalog=QLVT;Integrated Security=True";
            conn.ConnectionString = conn_str;
            conn.Open();
            return conn;
        }
        public DataTable GetDataTableFromDb(string query)
        {
            SqlConnection con = ConnectToDb();
            DataTable dt = new DataTable();
            SqlDataAdapter Adpt = new SqlDataAdapter(query, con);
            try
            {
                Adpt.Fill(dt);
            }
            catch (SqlException ex)
            {
                dt = null;
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
            finally
            {
                if (con != null)
                    if (con.State == ConnectionState.Open) con.Close();
                Adpt.Dispose();
            }
            return dt;
        }
        public void InitBands(XtraReport report)
        {
            var detail = new DetailBand() { HeightF = 20 };
            var pageHeader = new PageHeaderBand() { HeightF = 20 };
            var reportFooter = new ReportFooterBand() { HeightF = 380 };
            var reportHeader = new ReportHeaderBand() { HeightF = 20 };
            report.Bands.AddRange(new Band[] { detail, pageHeader, reportFooter, reportHeader });
        }
        public void InitDetailsBaseXRTable(XtraReport report, String titleStr)
        {
            var ds = (report.DataSource as DataSet);
            int colCount = ds.Tables[0].Columns.Count;
            int colWidth = (report.PageWidth - (report.Margins.Left + report.Margins.Right)) / colCount;
            // Create title
            var title = new XRLabel();
            title.Text = titleStr;
            title.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
            title.ForeColor = Color.Black;
            title.Font = new Font("Times New Roman", 18, FontStyle.Bold, GraphicsUnit.Pixel);
            title.Width = 747;
            title.Padding = new DevExpress.XtraPrinting.PaddingInfo(20, 20, 20, 20, 100.0F);
            // Create a table header.
            var tableHeader = new XRTable();
            tableHeader.Height = 20;
            tableHeader.Width = (report.PageWidth - (report.Margins.Left + report.Margins.Right));
            tableHeader.BackColor = Color.LightGray;
            tableHeader.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;


            var headerRow = new XRTableRow();
            headerRow.Width = tableHeader.Width;
            tableHeader.Rows.Add(headerRow);
            headerRow.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            tableHeader.BeginInit();

            // Create a table body.
            var tableBody = new XRTable();
            tableBody.Height = 20;
            tableBody.Width = (report.PageWidth - (report.Margins.Left + report.Margins.Right));
            tableBody.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            var bodyRow = new XRTableRow();
            bodyRow.Width = tableBody.Width;
            tableBody.Rows.Add(bodyRow);
            tableBody.BeginInit();
            bodyRow.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            // Initialize table header and body cells.
            for (int i = 0; i < colCount; i++)
            {
                var headerCell = new XRTableCell();
                headerCell.Width = colWidth;
                headerCell.Text = ds.Tables[0].Columns[i].Caption;
                headerCell.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
                var bodyCell = new XRTableCell();
                bodyCell.Width = colWidth;
                bodyCell.DataBindings.Add("Text", null, ds.Tables[0].Columns[i].Caption);
                bodyCell.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
                if (i == 0)
                {
                    headerCell.Borders = DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top | DevExpress.XtraPrinting.BorderSide.Bottom;
                    bodyCell.Borders = DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top | DevExpress.XtraPrinting.BorderSide.Bottom;
                }
                else
                {
                    headerCell.Borders = DevExpress.XtraPrinting.BorderSide.All;
                    bodyCell.Borders = DevExpress.XtraPrinting.BorderSide.All;
                }

                headerRow.Cells.Add(headerCell);
                bodyRow.Cells.Add(bodyCell);
            }

            tableHeader.EndInit();
            tableBody.EndInit();
            // Add the table header and body to the corresponding report bands.
            report.Bands[BandKind.PageHeader].Controls.Add(tableHeader);
            report.Bands[BandKind.ReportHeader].Controls.Add(title);
            report.Bands[BandKind.Detail].Controls.Add(tableBody);
        }
    }
}