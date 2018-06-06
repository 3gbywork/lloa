using CredentialManagement;
using OfficeAutomationClient.Helper;
using OfficeAutomationClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Windows.Media;
using ValidateCodeProcessor;

namespace OfficeAutomationClient.OA
{
    class Business : IDisposable
    {
        const string CredentialSetTarget = "OfficeAutomationClient";

        private Business()
        {
            WebRequestHelper.Create(OAUrl.Home).WithCookies(cookieContainer).GetResponseString();
            WebRequestHelper.Create(OAUrl.Login).WithCookies(cookieContainer).GetResponseString();

            credentials = new CredentialSet(CredentialSetTarget);
            credentials.Load();
        }

        private static Business business = new Business();
        public static Business Instance => business;

        private CookieContainer cookieContainer = new CookieContainer();
        private string validateCode;
        private CredentialSet credentials;

        internal ImageSource GetValidateCodeImage()
        {
            var bitmap = WebRequestHelper.Create(OAUrl.ValidateCode).WithCookies(cookieContainer).GetResponseStream().ToBitmap();

            var imageSource = bitmap.GetImageSource();
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
        }

        internal void Login(LoginViewModel login, SecureString password)
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
                {"userpassword", password.Text() },
                {"submit", "登录" },
                {"validatecode", login.ValidateCode },
            };
            var resp = WebRequestHelper.Create(OAUrl.VerifyLogin).WithCookies(cookieContainer).WithParamters(loginparameters).GetResponseString();

            if (login.RememberPwd)
            {
                var credential = new Credential(login.User, password.Text(), CredentialSetTarget, CredentialType.Generic);
                credential.Save();
                if (!credentials.Exists(c => c.Username.Equals(login.User)))
                {
                    credentials.Add(credential);
                }

                ConfigHelper.Save(ConfigKey.User, login.User);
                ConfigHelper.Save(ConfigKey.RememberPwd, login.RememberPwd.ToString());
                ConfigHelper.Save(ConfigKey.AutoLogin, login.AutoLogin.ToString());
            }
        }

        internal void GetAttendance(string date)
        {
            var resp = WebRequestHelper.Create(OAUrl.MonthAttDetail).WithCookies(cookieContainer).GetResponseString();

            var attparameters = new Dictionary<string, string>
            {
                {"currentdate", date },
                {"resourceId", "1209" },
                {"departmentId", "86" },
                {"rstr",RandomHelper.RandomString(10) },
                {"subCompanyId", "1" },
            };
            var attdata = WebRequestHelper.Create(OAUrl.MonthAttData).WithCookies(cookieContainer).WithParamters(attparameters).GetResponseString();
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
    }
}
