using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace OfficeAutomationClient.Model
{
    public class Person
    {
        [DisplayName("姓名")]
        public string LastName { get; set; }
        [DisplayName("编号")]
        public string WorkCode { get; set; }
        [DisplayName("性别")]
        public string Sex { get; set; }
        [DisplayName("状态")]
        public string Status { get; set; }
        [DisplayName("直接上级")]
        public string Manager { get; set; }
        [DisplayName("岗位")]
        public string JobTitle { get; set; }
        [DisplayName("显示顺序")]
        public int ID { get; set; }

        public string OrganizationID { get; set; }
    }

    public class People : Collection<Person>
    {

    }
}
