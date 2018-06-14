using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OfficeAutomationClient.OA
{
    class OAUrl
    {
        public const string Home = "http://oa.win-stock.com.cn";
        public const string Login = Home + "/login/login.jsp";
        public const string VerifyLogin = Home + "/login/verifylogin.jsp";
        public const string SysRemind = Home + "/system/SysRemind.jsp";

        public const string Logout = Home + "/login/Logout.jsp";

        public const string ValidateCode = Home + "/weaver/weaver.file.MakeValidateCode";

        public const string Organization = Home + "/js/hrm/getdata.jsp";
        //public const string Department = Home + "/hrm/HrmTab.jsp";
        //public const string DepartmentInfo = Home + "/hrm/company/HrmDepartmentDsp.jsp";
        public const string HrmResourceList = Home + "/hrm/company/HrmResourceList.jsp?id=";
        public const string SplitPage = Home + "/weaver/weaver.common.util.taglib.SplitPageXmlServlet";

        public const string MonthAttDetail = Home + "/hrm/report/schedulediff/HrmScheduleDiffMonthAttDetail.jsp?fromHrmTab=1";
        public const string MonthAttData = Home + "/hrm/report/schedulediff/HrmScheduleDiffMonthAttData.jsp";
    }
}
