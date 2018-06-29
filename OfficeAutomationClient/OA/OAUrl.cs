namespace OfficeAutomationClient.OA
{
    internal class OAUrl
    {
        public const string Home = "http://oa.win-stock.com.cn";
        public const string Login = Home + "/login/login.jsp?logintype=1";

        public const string Login2 =
            Home + "/wui/theme/ecology8/page/login.jsp?templateId=3&logintype=1&gopage=&languageid=7&message=";

        public const string VerifyLogin = Home + "/login/VerifyLogin.jsp";
        public const string RemindLogin = Home + "/login/RemindLogin.jsp?RedirectFile=/wui/main.jsp";
        public const string ServerStatusLogin = Home + "/social/im/ServerStatus.jsp?p=webLogin";
        public const string ServerStatusLogout = Home + "/social/im/ServerStatus.jsp?p=logout";
        public const string SysRemind = Home + "/system/SysRemind.jsp";
        public const string Main = Home + "/wui/main.jsp";

        public const string Logout = Home + "/login/Logout.jsp";

        public const string Refresh =
            Home + "/Refresh.jsp?loginfile=/wui/theme/ecology8/page/login.jsp?templateId=3&logintype=1&gopage=";

        public const string ValidateCode = Home + "/weaver/weaver.file.MakeValidateCode";

        public const string Organization = Home + "/js/hrm/getdata.jsp";
        public const string HrmResourceList = Home + "/hrm/company/HrmResourceList.jsp?id=";
        public const string SplitPage = Home + "/weaver/weaver.common.util.taglib.SplitPageXmlServlet";

        // 获取用户头像
        // 解析：navLogo:"(.+)"
        //public const string HrmResource = Home + "/hrm/HrmTab.jsp?_fromURL=HrmResource&id=";
        public const string HrmResourceBase = Home + "/hrm/resource/HrmResourceBase.jsp";

        public const string MonthAttData = Home + "/hrm/report/schedulediff/HrmScheduleDiffMonthAttData.jsp";
        public const string Calendar = "http://yun.rili.cn/wnl/index.html";
    }
}