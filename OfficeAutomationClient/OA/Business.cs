using CommonUtility.Extension;
using CommonUtility.Http;
using CommonUtility.Logging;
using CommonUtility.Rand;
using CredentialManagement;
using HtmlAgilityPack;
using Newtonsoft.Json;
using OfficeAutomationClient.Database;
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
using System.Threading;
using System.Windows.Media;
using ValidateCodeProcessor;

namespace OfficeAutomationClient.OA
{
    class Business : IDisposable
    {
        private static readonly ILogger Logger = LogHelper.GetLogger<Business>();

        public static string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        private const string CredentialSetTarget = "OfficeAutomationClient";
        private const string DbFileName = "organization.db";

        private Business()
        {
            HttpWebRequestClient.Create(OAUrl.Home).WithCookies(_cookieContainer).GetResponseString();
            var resp = HttpWebRequestClient.Create(OAUrl.Login).WithCookies(_cookieContainer).GetResponseString();

            _title = TryGetTitle(resp);

            _credentials = new CredentialSet(CredentialSetTarget);
            _credentials.Load();
        }

        public static Business Instance { get; } = new Business();

        private readonly CookieContainer _cookieContainer = new CookieContainer();
        private string _validateCode;
        private readonly CredentialSet _credentials;

        private readonly string _title;
        private string _userId;
        private string _companyName;

        private string TryGetTitle(string resp)
        {
            try
            {
                return Regex.Match(resp, "document.title='(.+)'").Value.Split('\'')[1];
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "解析 title 出错，resp：{0}", resp);
                return "OA";
            }
        }

        private string GetCompanyName()
        {
            try
            {
                var resp = HttpWebRequestClient.Create(OAUrl.SysRemind).WithCookies(_cookieContainer).GetResponseString();
                return Regex.Match(resp, "companyname = \"(.+)\"").Value.Split('\"')[1];
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "解析 companyname 出错");
                return "Company";
            }
        }

        internal ImageSource GetValidateCodeImage()
        {
            var bitmap = HttpWebRequestClient.Create(OAUrl.ValidateCode).WithCookies(_cookieContainer).GetResponseStream().ToBitmap();

            var imageSource = bitmap.ToImageSource();
            _validateCode = bitmap.Gray().DeNoise().Binarize().Text().Trim();

            return imageSource;
        }

        internal string GetValidateCode()
        {
            return _validateCode;
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
            var resp = HttpWebRequestClient.Create(OAUrl.VerifyLogin).WithCookies(_cookieContainer).WithParamters(parameters).GetResponseString();
            if (!resp.Contains("logincookiecheck")) return false;

            _userId = _cookieContainer.GetCookies(new Uri(OAUrl.Home))["loginidweaver"].Value;
            _companyName = GetCompanyName();

            if (login.RememberPwd)
            {
                var credential = new Credential(login.User, password.CreateString(), CredentialSetTarget, CredentialType.Generic) { PersistanceType = PersistanceType.LocalComputer };
                credential.Save();
                _credentials.Load();

                ConfigHelper.Save(ConfigKey.User, login.User);
                ConfigHelper.Save(ConfigKey.RememberPwd, login.RememberPwd.ToString());
                ConfigHelper.Save(ConfigKey.AutoLogin, login.AutoLogin.ToString());
            }

#if DEBUG
            using (var context = new OrganizationContext(DbFileName))
                context.Database.Initialize(true);
#endif

            return true;
        }

#if DEBUG
        internal Organizations GetOrganizations()
        {
            var parameters = new Dictionary<string, string>
            {
                {"cmd", "initChart"},
                {"arg0", ""},
                {"arg1", "10000"},
                {"arg2", _companyName},
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
            try
            {
                var resp = HttpWebRequestClient.Create(OAUrl.Organization).WithCookies(_cookieContainer).WithParamters(parameters).GetResponseString();
                return JsonConvert.DeserializeObject<Organizations>(resp);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "获取 {0} 组织结构出错", _companyName);
                return default(Organizations);
            }
        }

        internal People GetPeople(Organization dept)
        {
            var people = new People();
            if (dept.Type != OrganizationType.Dept) return people;

            try
            {
                var resp = HttpWebRequestClient.Create($"{OAUrl.HrmResourceList}{dept.ID.Substring(1)}").WithCookies(_cookieContainer).GetResponseString();
                var tableString = Regex.Match(resp, "__tableStringKey__='(.+)'").Value.Split('\'')[1];

                var dom = new HtmlDocument();

                var pageIndex = 1;
                const int delayMilliseconds = 100;
                while (true)
                {
                    var parameters = new Dictionary<string, string>
                    {
                        {"tableInstanceId", ""},
                        {"tableString", tableString},
                        {"pageIndex", pageIndex.ToString()},
                        {"orderBy", "dsporder"},
                        {"otype", "ASC"},
                        {"mode", "run"},
                        {"customParams", ""},
                        {"selectedstrs", ""},
                        {"pageId", "Hrm:ResourceList"},
                    };
                    var delayTimes = 1;
                    while (true)
                    {
                        resp = HttpWebRequestClient.Create(OAUrl.SplitPage).WithCookies(_cookieContainer).WithParamters(parameters).GetResponseString();
                        if (resp.StartsWith("<?xml")) break;

                        Thread.Sleep(delayMilliseconds * (1 << delayTimes));
                        if (delayTimes < 5)
                            delayTimes++;
                    }

                    dom.LoadHtml(resp);

                    foreach (HtmlNode row in dom.DocumentNode.SelectNodes("/table/row"))
                    {
                        var index = "<![CDATA[".Length;
                        var cdataLength = "<![CDATA[]]>".Length;
                        try
                        {
                            var columns = row.ChildNodes.Where(n => n.Name.Equals("col")).ToList();
                            var comments = row.ChildNodes.Where(n => n.Name.Equals("#comment")).ToList();
                            var person = new Person
                            {
                                LastName = columns[1].Attributes["value"].Value,
                                WorkCode = columns[2].Attributes["value"].Value,
                                Sex = comments[2].InnerHtml.Substring(index, comments[2].InnerLength - cdataLength),
                                Status = comments[3].InnerHtml.Substring(index, comments[3].InnerLength - cdataLength),
                                Manager = comments[4].InnerHtml.Substring(index, comments[4].InnerLength - cdataLength),
                                DspOrder = int.Parse(columns[7].Attributes["value"].Value.Split('.')[0]),
                                OrganizationID = dept.ID,
                            };

                            if (comments.Count > 5)
                                person.JobTitle = comments[5].InnerHtml.Substring(index, comments[5].InnerLength - cdataLength);

                            people.Add(person);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, null, "解析成员失败");
                            Logger.Error(row);
                        }
                    }

                    var root = dom.DocumentNode.SelectSingleNode("/table");
                    var nowpage = int.Parse(root.Attributes["nowpage"].Value);
                    var pagenum = int.Parse(root.Attributes["pagenum"].Value);

                    pageIndex++;

                    if (nowpage == pagenum || pageIndex > pagenum)
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "查询组织 {0}_{1} 下成员失败", dept.ID, dept.Title);
            }

            return people;
        }
#endif

        internal void GetAttendance(string date)
        {
            //var resp = HttpWebRequestClient.Create(OAUrl.MonthAttDetail).WithCookies(cookieContainer).GetResponseString();

            var parameters = new Dictionary<string, string>
            {
                {"currentdate", date },
                {"resourceId", _userId },
                {"departmentId", "86" },
                {"rstr",RandomEx.NextString(10) },
                {"subCompanyId", "1" },
            };
            var attdata = HttpWebRequestClient.Create(OAUrl.MonthAttData).WithCookies(_cookieContainer).WithParamters(parameters).GetResponseString();
        }

        internal List<string> GetUsers()
        {
            return _credentials.Select(c => c.Username).ToList();
        }

        internal SecureString QueryPassword(string username)
        {
            var user = _credentials.Find(c => c.Username.Equals(username));
            if (null != user) return user.SecurePassword;
            return new SecureString();
        }

        internal string GetTitle()
        {
            return _title;
        }
    }
}
