using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OfficeAutomationClient.OA
{
    class OAUrl
    {
        public const string Home = "http://oa.win-stock.com.cn";
        public const string Login = Home + "/login/login.jsp?logintype=1";
        public const string VerifyLogin = Home + "/login/verifylogin.jsp";
        public const string RemindLogin = Home + "/login/remindlogin.jsp?redirectfile=/wui/main.jsp";

        public const string ValidateCode = Home + "/weaver/weaver.file.MakeValidateCode";

        public const string MonthAttData = Home + "/hrm/report/schedulediff/HrmScheduleDiffMonthAttData.jsp";
    }
}
