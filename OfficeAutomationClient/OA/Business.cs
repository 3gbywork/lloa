using CommonUtility.Extension;
using CommonUtility.Http;
using CommonUtility.Logging;
using CommonUtility.Rand;
using CredentialManagement;
using OfficeAutomationClient.Helper;
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

        internal void GetOrganization()
        {
            throw new NotImplementedException();
        }

        private CookieContainer cookieContainer = new CookieContainer();
        private string validateCode;
        private CredentialSet credentials;

        private string title;
        private string userId;

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
            var loginparameters = new Dictionary<string, string>
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
            var resp = HttpWebRequestClient.Create(OAUrl.VerifyLogin).WithCookies(cookieContainer).WithParamters(loginparameters).GetResponseString();

            if (!resp.Contains("logincookiecheck")) return false;

            userId = cookieContainer.GetCookies(new Uri(OAUrl.Home))["loginidweaver"].Value;

            if (login.RememberPwd)
            {
                var credential = new Credential(login.User, password.CreateString(), CredentialSetTarget, CredentialType.Generic);
                credential.Save();
                if (!credentials.Exists(c => c.Username.Equals(login.User)))
                {
                    credentials.Add(credential);
                }

                ConfigHelper.Save(ConfigKey.User, login.User);
                ConfigHelper.Save(ConfigKey.RememberPwd, login.RememberPwd.ToString());
                ConfigHelper.Save(ConfigKey.AutoLogin, login.AutoLogin.ToString());
            }

            return true;
        }

        internal void GetAttendance(string date)
        {
            var resp = HttpWebRequestClient.Create(OAUrl.MonthAttDetail).WithCookies(cookieContainer).GetResponseString();

            var attparameters = new Dictionary<string, string>
            {
                {"currentdate", date },
                {"resourceId", userId },
                {"departmentId", "86" },
                {"rstr",RandomEx.NextString(10) },
                {"subCompanyId", "1" },
            };
            var attdata = HttpWebRequestClient.Create(OAUrl.MonthAttData).WithCookies(cookieContainer).WithParamters(attparameters).GetResponseString();
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
