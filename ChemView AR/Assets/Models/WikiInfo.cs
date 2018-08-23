using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Models
{
    class WikiInfo
    {
        public string type { get; set; }
        public string title { get; set; }
        public string displaytitle { get; set; }
        public int pageid { get; set; }
        public string lang { get; set; }
        public string dir { get; set; }
        public string revision { get; set; }
        public string tid { get; set; }
        public DateTime timestamp { get; set; }
        public string description { get; set; }
        public string extract { get; set; }
        public string extract_html { get; set; }
    }
}
