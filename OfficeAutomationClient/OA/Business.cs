using CommonUtility.Extension;
using CommonUtility.Http;
using CommonUtility.Logging;
using CommonUtility.Rand;
using CredentialManagement;
using HtmlAgilityPack;
using Newtonsoft.Json;
using OfficeAutomationClient.Helper;
using OfficeAutomationClient.Model;
using OfficeAutomationClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Text.RegularExpressions;
using System.Windows.Media;
using ValidateCodeProcessor;

namespace OfficeAutomationClient.OA
{
    class Business : IDisposable
    {
        private static ILogger logger = LogHelper.GetLogger<Business>();

        public static string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        const string CredentialSetTarget = "OfficeAutomationClient";

        private Business()
        {
            HttpWebRequestClient.Create(OAUrl.Home).WithCookies(cookieContainer).GetResponseString();
            var resp = HttpWebRequestClient.Create(OAUrl.Login).WithCookies(cookieContainer).GetResponseString();

            title = TryGetTitle(resp);

            credentials = new CredentialSet(CredentialSetTarget);
            credentials.Load();
        }

        private static Business business = new Business();
        public static Business Instance => business;

        private CookieContainer cookieContainer = new CookieContainer();
        private string validateCode;
        private CredentialSet credentials;

        private readonly string title;
        private string userId;
        private string companyName;

        private string TryGetTitle(string resp)
        {
            try
            {
                return Regex.Match(resp, "document.title='(.+)'").Value.Split('\'')[1];
            }
            catch (Exception ex)
            {
                logger.Error(ex, null, "解析 title 出错，resp：{0}", resp);
                return "OA";
            }
        }

        private string GetCompanyName()
        {
            try
            {
                var resp = HttpWebRequestClient.Create(OAUrl.SysRemind).WithCookies(cookieContainer).GetResponseString();
                return Regex.Match(resp, "companyname = \"(.+)\"").Value.Split('\"')[1];
            }
            catch (Exception ex)
            {
                logger.Error(ex, null, "解析 companyname 出错");
                return "Company";
            }
        }

        internal ImageSource GetValidateCodeImage()
        {
            var bitmap = HttpWebRequestClient.Create(OAUrl.ValidateCode).WithCookies(cookieContainer).GetResponseStream().ToBitmap();

            var imageSource = bitmap.ToImageSource();
            validateCode = bitmap.Gray().DeNoise().Binarize().Text().Trim();

            return imageSource;
        }

        internal string GetValidateCode()
        {
            return validateCode;
        }

        public void Dispose()
        {
            OcrProcessor.Instance.Dispose();
            LogHelper.Shutdown();
        }

        internal bool Login(LoginViewModel login, SecureString password)
        {
            var parameters = new Dictionary<string, string>
            {
                {"loginfile","/wui/theme/ecology8/page/login.jsp?templateId=3&logintype=1&gopage=" },
                {"logintype", "1" },
                {"fontName", "微软雅黑" },
                {"formmethod", "post" },
                {"isie", "false" },
                {"islanguid", "7" },
                {"loginid", login.User},
                {"userpassword", password.CreateString() },
                {"submit", "登录" },
                {"validatecode", login.ValidateCode },
            };
            var resp = HttpWebRequestClient.Create(OAUrl.VerifyLogin).WithCookies(cookieContainer).WithParamters(parameters).GetResponseString();
            if (!resp.Contains("logincookiecheck")) return false;

            userId = cookieContainer.GetCookies(new Uri(OAUrl.Home))["loginidweaver"].Value;
            companyName = GetCompanyName();

            if (login.RememberPwd)
            {
                var credential = new Credential(login.User, password.CreateString(), CredentialSetTarget, CredentialType.Generic) { PersistanceType = PersistanceType.LocalComputer };
                credential.Save();
                credentials.Load();

                ConfigHelper.Save(ConfigKey.User, login.User);
                ConfigHelper.Save(ConfigKey.RememberPwd, login.RememberPwd.ToString());
                ConfigHelper.Save(ConfigKey.AutoLogin, login.AutoLogin.ToString());
            }

            return true;
        }

        internal Organizations GetOrganizations()
        {
            var parameters = new Dictionary<string, string>
            {
                {"cmd", "initChart"},
                {"arg0", ""},
                {"arg1", "10000"},
                {"arg2", companyName},
                {"arg3", "true"},
                {"arg4", "8"},
                {"arg5", ""},
                {"arg6", ""},
                {"arg7", ""},
                {"arg8", ""},
                {"arg9", ""},
                {"arg10", ""},
                {"arg11", ";;P"},
            };
            var resp = HttpWebRequestClient.Create(OAUrl.Organization).WithCookies(cookieContainer).WithParamters(parameters).GetResponseString();
            return JsonConvert.DeserializeObject<Organizations>(resp);
        }

        internal People GetPeople(Organization org)
        {
            //var param = new Dictionary<string, string>
            //{
            //    {"_fromURL", "HrmDepartmentDsp"},
            //    {"id", "0"},
            //    {"hasTree", "false"},
            //};
            //var rsp = HttpWebRequestClient.Create(OAUrl.Department).WithCookies(cookieContainer).WithParamters(param).GetResponseString();

            //param = new Dictionary<string, string>
            //{
            //    {"id", "0"},
            //    {"fromHrmTab", "1"},
            //};
            //rsp = HttpWebRequestClient.Create(OAUrl.DepartmentInfo).WithCookies(cookieContainer).WithParamters(param).GetResponseString();

            var orgID = org.Type == OrganizationType.Dept ? org.ID.Substring(1) : org.ID;
            var resp = HttpWebRequestClient.Create($"{OAUrl.HrmResourceList}{(orgID)}").WithCookies(cookieContainer).GetResponseString();
            var tableString = Regex.Match(resp, "__tableStringKey__='(.+)'").Value.Split('\'')[1];

            var people = new People();

            while (true)
            {
                var pageIndex = 1;
                var parameters = new Dictionary<string, string>
                {
                    {"tableInstanceId", ""},
                    {"tableString", tableString},
                    {"pageIndex", pageIndex.ToString()},
                    {"orderBy", "dsporder"},
                    {"otype", "ASC"},
                    {"mode", "run"},
                    {"customParams", "null"},
                    {"selectedstrs", ""},
                    {"pageId", "Hrm:ResourceList"},
                };
                resp = HttpWebRequestClient.Create(OAUrl.SplitPage).WithCookies(cookieContainer).WithParamters(parameters).GetResponseString();

                var dom = new HtmlDocument();
                dom.LoadHtml(resp);

                break;
            }

            return people;
        }

        internal void GetAttendance(string date)
        {
            //var resp = HttpWebRequestClient.Create(OAUrl.MonthAttDetail).WithCookies(cookieContainer).GetResponseString();

            var parameters = new Dictionary<string, string>
            {
                {"currentdate", date },
                {"resourceId", userId },
                {"departmentId", "86" },
                {"rstr",RandomEx.NextString(10) },
                {"subCompanyId", "1" },
            };
            var attdata = HttpWebRequestClient.Create(OAUrl.MonthAttData).WithCookies(cookieContainer).WithParamters(parameters).GetResponseString();
        }

        internal List<string> GetUsers()
        {
            return credentials.Select(c => c.Username).ToList();
        }

        internal SecureString QueryPassword(string username)
        {
            var user = credentials.Find(c => c.Username.Equals(username));
            if (null != user) return user.SecurePassword;
            return new SecureString();
        }

        internal string GetTitle()
        {
            return title;
        }
    }
}
