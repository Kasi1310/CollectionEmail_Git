using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Data;

namespace FileImportedServices
{
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public Worker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    string fileName = System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location) + "\\appsettings.json";
                    string jsonString = File.ReadAllText(fileName);
                    AppSettingsModel appSettings = JsonSerializer.Deserialize<AppSettingsModel>(jsonString)!;
                    string from_path = appSettings.AppSettings.from_path;
                    string to_path = appSettings.AppSettings.to_path;
                    DB db = new DB();
                    string query = "SELECT [path] FROM [iweb_assembly].[dbo].[popath]";
                    DataTable user = db.GetDataTable(query);
                    var directory = @""+ user.Rows[0]["path"].ToString();
                    DirectoryInfo d = new DirectoryInfo(directory); //Assuming Test is your Folder

                    FileInfo[] Files = d.GetFiles("*.csv"); //Getting Text files
                    foreach (FileInfo file in Files)
                    {
                        string filePath = directory + "\\" + file.Name;

                        string[] csvData = await File.ReadAllLinesAsync(filePath);

                        if (csvData.Length == 0)
                        {
                            Console.WriteLine("CSV file is empty.");
                            continue;
                        }

                        string[] headers = csvData[0].Split(';');
                        string[] heater = headers[0].Split(';');

                        var data = csvData.Skip(1).Take(csvData.Length - 1).Select(line =>
                        {
                            string[] bodydata = line.Split(';');
                            return new
                            {
                                sfg_desc = bodydata[0].Trim(),
                                prdn_order = bodydata[1].Trim(),
                                sfg = bodydata[2].Trim(),
                                sub = bodydata[3].Trim(),
                                sub_desc = bodydata[4].Trim(),
                                req_qty = (int)float.Parse(bodydata[5].Trim()),
                                issue_qty = (int)float.Parse(bodydata[6].Trim())
                            };
                        }).ToList();
                        
                        string qry = "";
                        int COUNT = data.Count;
                        for (int k = 0; k < COUNT; k++)
                        {
                            int i = k + 1;
                            qry += "INSERT INTO [dbo].[po] ([sfg_desc],[prdn_order],[sfg],[sub],[sub_desc],[req_qty],[issue_qty],[line_no])  VALUES  ('" + data[k].sfg_desc.ToString() + "','" + data[k].prdn_order.ToString() + "','" + data[k].sfg.ToString() + "','" + data[k].sub.ToString() + "','" + data[k].sub_desc.ToString() + "','" + data[k].req_qty + "','" + data[k].issue_qty + "','"+ i + "');";
                            // var id = data[k].sfg_desc.ToString();
                        }

                        int res = db.ExecQry(qry);
                        if (res > 0)
                        {
                            // Move the file to a new path
                            var newPath = @"" + to_path + "\\PO";
                            File.Move(filePath, Path.Combine(newPath, Path.GetFileName(filePath)));

                            // Remove the imported file
                            File.Delete(filePath);

                            Console.WriteLine("Data inserted successfully.");
                        }
                    }

                    string query1 = "SELECT [path] FROM [iweb_assembly].[dbo].[attendancepath]";
                    DataTable user1 = db.GetDataTable(query1);
                    var directory1 = @"" + user1.Rows[0]["path"].ToString();
                    DirectoryInfo d1 = new DirectoryInfo(directory1); //Assuming Test is your Folder

                    FileInfo[] Files1 = d1.GetFiles("*.csv"); //Getting Text files
                    foreach (FileInfo file in Files1)
                    {
                        string filePath1 = directory1 + "\\" + file.Name;

                        string[] csvData1 = await File.ReadAllLinesAsync(filePath1);

                        if (csvData1.Length == 0)
                        {
                            Console.WriteLine("CSV file is empty.");
                            continue;
                        }

                        string[] headers1 = csvData1[0].Split(';');
                        string[] heater1 = headers1[0].Split(';');

                        var data1 = csvData1.Skip(1).Take(csvData1.Length - 1).Select(line1 =>
                        {
                            string[] bodydata1 = line1.Split(',');
                            return new
                            {
                                date = bodydata1[0].Trim(),
                                emp_id = bodydata1[1].Trim(),
                                first_name = bodydata1[2].Trim(),
                                first_punch = bodydata1[3].Trim(),
                                last_punch = bodydata1[4].Trim(),
                                total_time = bodydata1[5].Trim()
                            };
                        }).ToList();
                        
                        string qry1 = "";
                        int COUNT1 = data1.Count;
                        for (int k = 0; k < COUNT1; k++)
                        {
                            qry1 += "INSERT INTO [dbo].[emp_attendance] ([date],[emp_id],[first_name],[first_punch],[last_punch],[total_time])  VALUES  ('" + data1[k].date.ToString() + "','" + data1[k].emp_id.ToString() + "','" + data1[k].first_name.ToString() + "','" + data1[k].first_punch.ToString() + "','" + data1[k].last_punch.ToString() + "','" + data1[k].total_time.ToString() + "')";
                            // var id = data[k].sfg_desc.ToString();
                        }

                        int res = db.ExecQry(qry1);
                        if (res > 0)
                        {
                            // Move the file to a new path
                            var newPath = @"" + to_path + "\\Attendance";
                            File.Move(filePath1, Path.Combine(newPath, Path.GetFileName(filePath1)));

                            // Remove the imported file
                            File.Delete(filePath1);

                            Console.WriteLine("Data inserted successfully.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait for 1 minute before processing the next file
            }
        }
    }
}
