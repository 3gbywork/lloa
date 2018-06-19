using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using OfficeAutomationClient.OA;

namespace OfficeAutomationClient.Helper
{
    [ComVisible(true)]
    public class JsFunction
    {
        private JsFunction()
        {

        }

        public static JsFunction Instance = new JsFunction();

        public string GetAttendance(string date)
        {
            var att = Business.Instance.GetAttendance(date);
            var rst = JsonConvert.SerializeObject(att);
            return rst;
        }
    }
}
