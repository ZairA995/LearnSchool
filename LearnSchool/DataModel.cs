using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnSchool
{
    public class DataModel
    {
        public string Title { get; set; }
        public string MainImagePath { get; set; }
        public decimal Cost { get; set; }
        public decimal CostNew { get; set; }
        public int DurationInSeconds { get; set; }
        public double Discount { get; set; }
        public int Id { get; set; }
        public string Description { get; set; }
        public bool IsDiscounted { get { return Discount > 0; } }


        public string FIO { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string StartTime { get; set; }
        public int DateDiffHour {get; set;}
        public int DateDiffMinute { get; set; }

    }

}
