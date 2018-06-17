using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace OfficeAutomationClient.Model
{
    public class PersonalDetail
    {
        public int ID { get; set; }
        public string RequestID { get; set; }

        [DisplayName("职务")]
        public string JobName { get; set; }
        [DisplayName("职称")]
        public string TechnicalTitle { get; set; }
        [DisplayName("职责描述")]
        public string Responsibility { get; set; }
        [DisplayName("办公地点")]
        public string OfficeLocation { get; set; }
        [DisplayName("入职日期")]
        public string StartDate { get; set; }
        [DisplayName("移动电话")]
        public string MobilePhone { get; set; }
        [DisplayName("办公室电话")]
        public string OfficePhone { get; set; }
    }

    public class PersonalDetails : Collection<PersonalDetail>
    {

    }
}
