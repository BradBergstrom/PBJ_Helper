using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PBJ_Helper.WorkDay;

namespace PBJ_Helper
{
    public class WorkDay
    {
        public enum DayOfWeek
        {
            sun,
            mon,
            tue,
            wed,
            thu,
            fri,
            sat,
            sun_1,
            mon_1,
            tue_1,
            wed_1,
            thu_1,
            fri_1,
            sat_1
        }
        public WorkDay(int dayOfMonth, DayOfWeek dayOfWeek)
        {
            this.dayOfMonth = dayOfMonth;
            this.dayOfWeek = dayOfWeek;
        }
        public DayOfWeek dayOfWeek;
        public int dayOfMonth { get; set; }
        public string punches;
        public double lunchDeductions;
        public double Reg;
        public double OT;
        public double totalHours;

        public double correctedHours { get; set; }
    }
    public class EmployeeWork
    {
        public string name { get; set; }
        public string employeeId { get; set; }
        public string department { get; set; }
        public List<WorkDay> daysWorked;

        public double calcWeek1Hours()
        {
            double week1Total = 0;
            for (int i = 0; i < 7; i++)
            {
                week1Total += daysWorked[i].correctedHours;
            }
            return week1Total;
        }
        public double calcWeek2Hours()
        {
            double week2Total = 0;
            for (int i = 7; i < 14; i++)
            {
                week2Total += daysWorked[i].correctedHours;
            }
            return week2Total;
        }
        public WorkDay getWorkday(WorkDay.DayOfWeek day)
        {
            return daysWorked[(int)day];
        }
        public void loadWorkWeek(DataRow daysOfMonth, DataRow punches, DataRow lunchDeductions, DataRow reg, DataRow OT)
        {
            daysWorked = new List<WorkDay>();
            WorkDay tempDay;
            WorkDay.DayOfWeek currentDayOfWeek = WorkDay.DayOfWeek.sun;
            for (int i = 3; i < daysOfMonth.ItemArray.Count(); i++)
            {
                if (!(daysOfMonth.ItemArray[i] is System.DBNull))
                {
                    tempDay = new WorkDay(Int32.Parse(daysOfMonth.ItemArray[i].ToString()), currentDayOfWeek);
                    currentDayOfWeek++;
                    if (punches != null && !(punches.ItemArray[i] is System.DBNull))
                    {
                        tempDay.punches = punches.ItemArray[i].ToString();
                    }

                    if (lunchDeductions != null && !(lunchDeductions.ItemArray[i] is System.DBNull))
                    {
                        tempDay.lunchDeductions = float.Parse(lunchDeductions.ItemArray[i].ToString());
                    }

                    if (reg != null && !(reg.ItemArray[i] is System.DBNull))
                    {
                        tempDay.Reg = float.Parse(reg.ItemArray[i].ToString());
                    }

                    if (OT != null && !(OT.ItemArray[i] is System.DBNull))
                    {
                        tempDay.OT = float.Parse(OT.ItemArray[i].ToString());
                    }
                    daysWorked.Add(tempDay);

                }
            }

            //we need all the days to be filled in before correcting the hours. Can't include this in the above loop sadly.
            double totalWeekHours = 0;
            for (int i = 0; i < daysWorked.Count; i++)
            {
                correctHours(daysWorked[i]);
                totalWeekHours += daysWorked[i].correctedHours;
            }

            calcWeek1Hours();
            calcWeek2Hours();

            int derp = 5;
        }
        public void correctHours(WorkDay day)
        {
            if (day.punches != null)
            {
                WorkDay nextDay = daysWorked.FirstOrDefault(d => d.dayOfWeek - 1 == day.dayOfWeek);

                string[] punchTimes = day.punches.Split(new string[] { "\n" }, StringSplitOptions.None);
                string startDateTime = punchTimes[0];
                string stopDateTime = punchTimes[1];

                DateTime startTime = DateTime.Parse(punchTimes[0]); // No error checking
                DateTime endTime = DateTime.Parse(punchTimes[1]); // No error checking
                if (startDateTime.Contains("PM") && stopDateTime.Contains("AM"))
                {
                    DateTime midnight = DateTime.Parse("12:00 AM"); // No error checking
                    midnight = midnight.AddDays(1);
                    TimeSpan timeWorkedToday = midnight - startTime;

                    day.correctedHours += timeWorkedToday.TotalHours; //if a previous day added hours here, Don't want to overwrite that
                    day.correctedHours -= day.lunchDeductions;

                    if (nextDay != null)
                    {
                        endTime = endTime.AddDays(1);
                        TimeSpan timeWorkedTomorrow = endTime - midnight;
                        nextDay.correctedHours += timeWorkedTomorrow.TotalHours;                        
                    }
                }
                else
                {
                    TimeSpan timeWorkedToday = endTime - startTime;
                    day.correctedHours += timeWorkedToday.TotalHours; //if a previous day added hours here, Don't want to overwrite that
                    day.correctedHours -= day.lunchDeductions;
                }
            }
        }
    }
    public class FullEmpoyeeOutput
    {
        public FullEmpoyeeOutput(EmployeeWork employee)
        {
            name = employee.name;
            employeeId = employee.employeeId;
            department = employee.department;

            Sun = employee.daysWorked[0].correctedHours;
            Mon = employee.daysWorked[1].correctedHours;
            Tue = employee.daysWorked[2].correctedHours;
            Wed = employee.daysWorked[3].correctedHours;
            Thu = employee.daysWorked[4].correctedHours;
            Fri = employee.daysWorked[5].correctedHours;
            Sat = employee.daysWorked[6].correctedHours;

            week1Total = employee.calcWeek1Hours();

            Sun_2 = employee.daysWorked[7].correctedHours;
            Mon_2 = employee.daysWorked[8].correctedHours;
            Tue_2 = employee.daysWorked[9].correctedHours;
            Wed_2 = employee.daysWorked[10].correctedHours;
            Thu_2 = employee.daysWorked[11].correctedHours;
            Fri_2 = employee.daysWorked[12].correctedHours;
            Sat_2 = employee.daysWorked[13].correctedHours;
            
            week2Total = employee.calcWeek2Hours();
        }
        public string name { get; set; }
        public string employeeId { get; set; }
        public string department { get; set; }
        public double Sun { get; set; }
        public double Mon { get; set; }
        public double Tue { get; set; }
        public double Wed { get; set; }
        public double Thu { get; set; }
        public double Fri { get; set; }
        public double Sat { get; set; }
        public double week1Total { get; set; }
        public double Sun_2 { get; set; }
        public double Mon_2 { get; set; }
        public double Tue_2 { get; set; }
        public double Wed_2 { get; set; }
        public double Thu_2 { get; set; }
        public double Fri_2 { get; set; }
        public double Sat_2 { get; set; }
        public double week2Total { get; set; }
    }
}
