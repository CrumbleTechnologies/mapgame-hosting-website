using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapgameHostingWebsite.Models
{
    public class NationApplication
    {
        public Dictionary<string, string> Fields { get; set; }
        public string MapClaimCode { get; set; }
        public string Status { get; set; }
    }
}
