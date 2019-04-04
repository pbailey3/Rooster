using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebUI.DTOs
{
    public class LogFileDTO
    {
        public int LinesRead { get; set; }
        public int ValidLines { get; set; }
        public int ErrorLines { get; set; }
        public int LoadedSuccesfully { get; set; }
        public List<string> ErrorList { get; set; }
        public string Status { get; set; }

        public LogFileDTO()
        {
            ErrorList = new List<string>();
            Status = "OK";
        }
    }
}
