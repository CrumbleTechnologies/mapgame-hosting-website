using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapgameHostingWebsite.Models
{
    public class NationApplication
    {
        public Dictionary<string, string> fields { get; set; }
        public string mapClaimCode { get; set; }
        public string status { get; set; }
    }
}
