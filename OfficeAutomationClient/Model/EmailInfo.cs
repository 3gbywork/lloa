using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OfficeAutomationClient.Model
{
    class EmailInfo
    {
        public string User { get; set; }
        public byte[] Password { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public bool EnableSsl { get; set; }
    }
}
