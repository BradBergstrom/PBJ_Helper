using ExcelDataReader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PBJ_Helper
{
    public class Program
    {
        static List<FullEmpoyeeOutput> employees;
        public static void ToJson(string excelFileName, string outputFileName)
        {
            employees = new List<FullEmpoyeeOutput>();

            using (var stream = File.Open(excelFileName, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        UseColumnDataType = true,
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true
                        }
                    });

                    EmployeeWork currentEmployee = null;
                    bool nextRowIsDates = false;
                    DataRow punchesRow = null;
                    DataRow lunchDeductionRow = null;
                    DataRow regRow = null;
                    DataRow otRow = null;

                    foreach (DataTable table in dataSet.Tables)
                    {
                        for(int i = 0; i < table.Rows.Count; i++)
                        {
                            DataRow row = table.Rows[i];
                            if (row.ItemArray[1].ToString() == "Name:")
                            {
                                currentEmployee = new EmployeeWork();
                                currentEmployee.name = row.ItemArray[2].ToString();
                            }
                            else if (row.ItemArray[1].ToString() == "Employee Id:")
                            {
                                currentEmployee.employeeId = row.ItemArray[2].ToString();
                            }
                            else if (row.ItemArray[1].ToString() == "Department:")
                            {
                                currentEmployee.department = row.ItemArray[2].ToString();
                            }
                            //if row sunday contains value 'Sun', then the next row will be the dates
                            else if (row.ItemArray[3].ToString() == "Sun")
                            {
                                nextRowIsDates = true;
                                int manualRowIndex = 1; //if the row we are expecting is next, move the counter. otherwise assume this person does not have that row
                                DataRow daysOfMonthRow = table.Rows[i + manualRowIndex];
                                if(table.Rows[i + manualRowIndex + 1].ItemArray[1].ToString() == "Punches")
                                {
                                    manualRowIndex++; //2
                                    punchesRow = table.Rows[i + manualRowIndex];
                                }
                                if (table.Rows[i + manualRowIndex + 1].ItemArray[1].ToString() == "Lunch Deductions")
                                {
                                    manualRowIndex++; //3
                                    lunchDeductionRow = table.Rows[i + manualRowIndex];
                                }
                                if (table.Rows[i + manualRowIndex + 1].ItemArray[1].ToString() == "Reg")
                                {
                                    manualRowIndex++; //4
                                    regRow = table.Rows[i + manualRowIndex];
                                }
                                if (table.Rows[i + manualRowIndex + 1].ItemArray[1].ToString() == "OT")
                                {
                                    manualRowIndex++; //5
                                    otRow = table.Rows[i + manualRowIndex];
                                }

                                currentEmployee.loadWorkWeek(daysOfMonthRow, punchesRow, lunchDeductionRow, regRow, otRow);
                                daysOfMonthRow = null; punchesRow = null; lunchDeductionRow = null; regRow = null; otRow = null;
                                i += manualRowIndex;
                                employees.Add(new FullEmpoyeeOutput(currentEmployee));
                            }
                        }
                        int derp = 3;
                    }

                    //string csv = String.Join(",", employees.Select(x => x.ToString()).ToArray());

                    PropertyInfo[] properties = typeof(FullEmpoyeeOutput).GetProperties();
                    CreateCSV(employees, outputFileName);

                    //File.WriteAllText(
                    //    jsonFileName,
                    //    JsonConvert.SerializeObject(dataSet.Tables[0],
                    //        Formatting.Indented));
                }
            }            
        }
        public static void CreateCSV<T>(List<T> list, string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                CreateHeader(list, sw);
                CreateRows(list, sw);
            }
        }
        private static void CreateHeader<T>(List<T> list, StreamWriter sw)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            for (int i = 0; i < properties.Length - 1; i++)
            {
                sw.Write(properties[i].Name + ",");
            }
            var lastProp = properties[properties.Length - 1].Name;
            sw.Write(lastProp + sw.NewLine);
        }
        private static void CreateRows<T>(List<T> list, StreamWriter sw)
        {
            foreach (var item in list)
            {
                PropertyInfo[] properties = typeof(T).GetProperties();
                for (int i = 0; i < properties.Length - 1; i++)
                {
                    var prop = properties[i];
                    sw.Write(prop.GetValue(item) + ",");
                }
                var lastProp = properties[properties.Length - 1];
                sw.Write(lastProp.GetValue(item) + sw.NewLine);
            }
        }
        static void Main(string[] args)
        {
            string wd = System.IO.Directory.GetCurrentDirectory();

            ToJson(@"C:\Users\bbergstr\Documents\WorkBench\c#\LizTimeCard\timeCard2.xlsx", @"C:\Users\bbergstr\Documents\WorkBench\c#\LizTimeCard\pog.csv");

            //var csv = new List<string[]>(); // or, List<YourClass>
            //var lines = System.IO.File.ReadAllLines(@"C:\Users\bbergstr\Documents\WorkBench\c#\LizTimeCard\timeCard.csv");
            //foreach (string line in lines)
            //    csv.Add(line.Split(',')); // or, populate YourClass          
            //string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(csv);

            int derp = 5;
        }
    }
}
