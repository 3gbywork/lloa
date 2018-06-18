using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OfficeAutomationClient.Model
{
    class AttendanceInfo
    {
        public string Attend { get; set; }
        /// <summary>
        /// 是否法定假日
        /// true:法定假期
        /// false:法定工作日
        /// null:非法定日期
        /// </summary>
        public bool? Holiday { get; set; }
    }
}
