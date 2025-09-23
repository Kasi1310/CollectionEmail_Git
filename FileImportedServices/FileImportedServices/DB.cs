using System.Data;
using System.Data.SqlClient;
using System.Text.Json;

namespace FileImportedServices
{
    public class DB
    {
        public string DBErr = "";
        private static string server;
        private static string db;
        private static string user;
        private static string pass;
        public SqlConnection link;
        public DB()
        {
            string fileName = System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location) + "\\appsettings.json";
            string jsonString = File.ReadAllText(fileName);
            AppSettingsModel appSettings = JsonSerializer.Deserialize<AppSettingsModel>(jsonString)!;
            server = appSettings.AppSettings.server;
            db = appSettings.AppSettings.db;
            user = appSettings.AppSettings.user;
            pass = appSettings.AppSettings.password;
            link = new SqlConnection("Data Source = " + server + "; Initial Catalog =" + db + "; User ID = " + user + "; Password = " + pass + ";MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False;Connection Timeout=30;");

        }

        public DataTable GetDataTable(string sQry)
        {
            DataTable dataTable = new DataTable();
            this.DBErr = "";
            try
            {
                this.link.Open();
                new SqlDataAdapter(new SqlCommand(sQry, this.link)).Fill(dataTable);
                return dataTable;
            }
            catch (Exception ex)
            {
                this.DBErr = "GetDataTable : " + ex.Message.ToString();
                return (DataTable)null;
            }
            finally
            {
                this.link.Close();
            }
        }
        public DataSet GetDataSet(string sQry)
        {
            DataSet dataSet = new DataSet();
            this.DBErr = "";
            try
            {
                this.link.Open();
                new SqlDataAdapter(new SqlCommand(sQry, this.link)).Fill(dataSet);
                return dataSet;
            }
            catch (Exception ex)
            {
                this.DBErr = "GetDataSet : " + ex.Message.ToString();
                return (DataSet)null;
            }
            finally
            {
                this.link.Close();
            }
        }

        public int ExecQry(string Qry)
        {
            this.DBErr = "";
            try
            {
                link.Open();
                return new SqlCommand(Qry, link).ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                this.DBErr = "ExecQry : " + ex.Message.ToString();
                return -1;
            }
            finally
            {
                link.Close();
            }
        }
    }
}
