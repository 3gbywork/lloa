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
using Polly;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using ValidateCodeProcessor;

namespace OfficeAutomationClient.OA
{
    internal class Business : IDisposable
    {
        private const string DefaultTitle = "OA";
        private const string DefaultCompany = "办公自动化系统";
        private const string CredentialSetTarget = "OfficeAutomationClient";
        private const string DbFileName = "organization.db";
        private static readonly ILogger Logger = LogHelper.GetLogger<Business>();

        public static string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        private readonly CookieContainer _cookieContainer = new CookieContainer();
        private readonly CredentialSet _credentials;

        private readonly string _title;
        private string _companyName;
        private string _userId;
        private string _validateCode;

        private Business()
        {
            TryGetResponseString(OAUrl.Home);
            var resp = TryGetResponseString(OAUrl.Login);

            _title = TryGetTitle(resp);

            _credentials = new CredentialSet();
            _credentials.Load();
        }

        public static Business Instance { get; } = new Business();

        public void Dispose()
        {
            Logout();
            OcrProcessor.Instance.Dispose();
            LogHelper.Shutdown();
        }

        private string TryGetResponseString(string url, Dictionary<string, string> parameters = null)
        {
            try
            {
                return HttpWebRequestClient.Create(url).WithCookies(_cookieContainer).WithParamters(parameters)
                    .GetResponseString();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "访问 {0} 出错", url);
                return default(string);
            }
        }

        private Bitmap TryGetResponseImage(string url)
        {
            try
            {
                return HttpWebRequestClient.Create(url).WithCookies(_cookieContainer).GetResponseStream().ToBitmap();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "获取 {0} 图片出错", url);
                return default(Bitmap);
            }
        }

        private static string TryGetTitle(string resp)
        {
            if (string.IsNullOrEmpty(resp)) return DefaultTitle;
            try
            {
                return Regex.Match(resp, "document.title='(.+)'").Value.Split('\'')[1];
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "解析 title 出错，resp：{0}", resp);
                return DefaultTitle;
            }
        }

        private static string TryGetCompanyName(string resp)
        {
            if (string.IsNullOrEmpty(resp)) return DefaultCompany;
            try
            {
                return Regex.Match(resp, "companyname = \"(.+)\"").Value.Split('\"')[1];
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "解析 companyname 出错");
                return DefaultCompany;
            }
        }

        internal ImageSource GetValidateCodeImage()
        {
            var bitmap = TryGetResponseImage(OAUrl.ValidateCode);

            var imageSource = bitmap.ToImageSource();
            _validateCode = bitmap.Gray().DeNoise().Binarize().Text().Trim();

            return imageSource;
        }

        internal string GetValidateCode()
        {
            return _validateCode;
        }

        internal bool Login(LoginViewModel login, SecureString password)
        {
            var parameters = new Dictionary<string, string>
            {
                {"loginfile", "/wui/theme/ecology8/page/login.jsp?templateId=3&logintype=1&gopage="},
                {"logintype", "1"},
                {"fontName", "微软雅黑"},
                {"formmethod", "post"},
                {"isie", "false"},
                {"islanguid", "7"},
                {"loginid", login.User},
                {"userpassword", password.CreateString()},
                {"submit", "登录"},
                {"validatecode", login.ValidateCode}
            };
            var resp = TryGetResponseString(OAUrl.VerifyLogin, parameters);
            if (!resp.Contains("logincookiecheck")) return false;

            _userId = _cookieContainer.GetCookies(new Uri(OAUrl.Home))["loginidweaver"].Value;
            _companyName = TryGetCompanyName(TryGetResponseString(OAUrl.SysRemind));

            if (login.RememberPwd)
            {
                SaveOrUpdateCredential(login.User, password);

                ConfigHelper.Save(ConfigKey.User, login.User);
                ConfigHelper.Save(ConfigKey.RememberPwd, login.RememberPwd.ToString());
                ConfigHelper.Save(ConfigKey.AutoLogin, login.AutoLogin.ToString());
            }

            return true;
        }

        internal void Logout()
        {
            TryGetResponseString(OAUrl.Logout);
        }

        private void SaveOrUpdateCredential(string user, SecureString password)
        {
            var credential = _credentials.Find(c => string.Equals(c.Username, user)) ?? new Credential(user)
            {
                PersistanceType = PersistanceType.LocalComputer,
                Type = CredentialType.Generic,
                Target = $"{CredentialSetTarget}:{Guid.NewGuid()}"
            };
            credential.Password = ProtectedData
                .Protect(Encoding.UTF8.GetBytes(password.CreateString()), null, DataProtectionScope.CurrentUser)
                .ToBase64String();
            credential.Save();
            _credentials.Load();
        }

        internal List<AttendanceInfo> GetAttendance(string date)
        {
            var deptID = GetDepartmentID(_userId);
            var companyID = GetCompanyID(deptID);

            if (string.IsNullOrEmpty(deptID) || string.IsNullOrEmpty(companyID)) return null;

            var parameters = new Dictionary<string, string>
            {
                {"currentdate", date},
                {"resourceId", _userId},
                {"departmentId", deptID.Substring(1)},
                {"rstr", RandomEx.NextString(10)},
                {"subCompanyId", companyID}
            };
            var attdata = TryGetResponseString(OAUrl.MonthAttData, parameters);

            var html = new HtmlDocument();
            html.LoadHtml(attdata);

            var rstNode = html.DocumentNode.SelectSingleNode("//table[@id=\"monthAttData\"]").ChildNodes.Where(n => n.Name.Equals("tr")).Last();
            var attrst = rstNode.ChildNodes.Where(n => n.Name.Equals("td")).Skip(2).Select(n =>
            {
                var info = new AttendanceInfo();
                info.Attend = string.Equals("√", n.InnerText.Trim());
                var style = n.Attributes["style"].Value.ToLower();
                if (style.Contains("red"))
                {
                    info.Holiday = true;
                }
                else if (style.Contains("greed"))
                {
                    info.Holiday = false;
                }
                return info;
            }).ToList();

            return attrst;
        }

        private string GetCompanyID(string deptID)
        {
            using (var context = new OrganizationContext(DbFileName))
            {
                while (true)
                {
                    var org = context.Organizations.Single(o => o.ID.Equals(deptID));
                    if (org.Type == OrganizationType.Company || org.Type == OrganizationType.SubCompany)
                    {
                        return org.ID;
                    }

                    deptID = org.PID;
                }
            }
        }

        private string GetDepartmentID(string userID)
        {
            using (var context = new OrganizationContext(DbFileName))
            {
                return context.People.Single(p => p.RequestID.Equals(userID)).OrganizationID;
            }
        }

        internal List<string> GetUsers()
        {
            return _credentials.Where(c => c.Target.StartsWith(CredentialSetTarget)).Select(c => c.Username).ToList();
        }

        internal SecureString QueryPassword(string username)
        {
            var user = _credentials.Find(c => c.Target.StartsWith(CredentialSetTarget) && c.Username.Equals(username));
            if (null != user)
            {
                try
                {
                    return ProtectedData
                        .Unprotect(user.SecurePassword.CreateString().FromBase64String(), null,
                            DataProtectionScope.CurrentUser).ToString(Encoding.UTF8).CreateSecureString();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, null, "解析 {0} 的密码失败", username);
                }
            }

            return new SecureString();
        }

        internal string GetTitle()
        {
            return _title;
        }

#if INIT_ORGANIZATION_DB
        private readonly Func<int, TimeSpan> _delayDurationProvider = attempt => TimeSpan.FromSeconds(Math.Min(5, Math.Pow(2, attempt - 1)));

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
                {"arg11", ";;P"}
            };

            return Policy<Organizations>.HandleResult(r => null == r || r.Count == 0)
                   .Or<Exception>()
                   .WaitAndRetryForever(_delayDurationProvider)
                   .Execute(() => JsonConvert.DeserializeObject<Organizations>(TryGetResponseString(OAUrl.Organization, parameters)));
        }

        internal People GetPeople(Organization dept)
        {
            var people = new People();
            if (dept.Type != OrganizationType.Dept) return people;

            try
            {
                var resp = TryGetResponseString($"{OAUrl.HrmResourceList}{dept.ID.Substring(1)}");
                var tableString = Regex.Match(resp, "__tableStringKey__='(.+)'").Value.Split('\'')[1];

                var pageIndex = 1;
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
                        {"pageId", "Hrm:ResourceList"}
                    };

                    resp = Policy<string>.HandleResult(r => !(!string.IsNullOrEmpty(r) && r.StartsWith("<?xml")))
                           .WaitAndRetryForever(_delayDurationProvider)
                           .Execute(() => TryGetResponseString(OAUrl.SplitPage, parameters));

                    using (var reader = new StringReader(resp))
                    {
                        var xElement = XElement.Load(reader);
                        foreach (var person in ParsePerson(xElement))
                        {
                            person.OrganizationID = dept.ID;
                            people.Add(person);
                        }

                        var nowpage = int.Parse(xElement.Attribute("nowpage").Value);
                        var pagenum = int.Parse(xElement.Attribute("pagenum").Value);

                        pageIndex++;

                        if (nowpage == pagenum || pageIndex > pagenum)
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "查询组织 {0}:{1} 下成员失败", dept.ID, dept.Title);
            }

            return people;
        }

        internal PersonalDetail GetPersonalDetail(Person person)
        {
            var detail = new PersonalDetail();
            if (string.IsNullOrEmpty(person.RequestID)) return default(PersonalDetail);

            try
            {
                var parameter = new Dictionary<string, string>
                {
                    {"isfromtab", "true"},
                    {"id", person.RequestID.ToString()},
                    {"fromHrmTab", "1"},
                };

                var html = new HtmlDocument();
                var detailsDic = Policy<Dictionary<string, string>>
                    .HandleResult(r => null == r || r.Count == 0)
                    .WaitAndRetryForever(_delayDurationProvider)
                    .Execute(() =>
                    {
                        var resp = TryGetResponseString(OAUrl.HrmResourceBase, parameter);
                        html.LoadHtml(resp);

                        var names = (from name in html.DocumentNode.SelectNodes("//td[@class=\"fieldName\"]") select name.InnerHtml.HtmlDecode().Trim()).ToList();
                        var values = (from value in html.DocumentNode.SelectNodes("//td[@class=\"field\"]") select value.InnerText.HtmlDecode().Trim()).ToList();

                        var rstdic = new Dictionary<string, string>();
                        if (names.Count == 0 || values.Count == 0 || names.Count != values.Count) return rstdic;

                        for (int i = 0; i < names.Count; i++)
                        {
                            rstdic.Add(names[i], values[i]);
                        }

                        return rstdic;
                    });

                detail.JobName = detailsDic["职务"];
                detail.MobilePhone = detailsDic["移动电话"];
                detail.OfficeLocation = detailsDic["办公地点"];
                detail.OfficePhone = detailsDic["办公室电话"];
                detail.Responsibility = detailsDic["职责描述"];
                detail.StartDate = detailsDic["入职日期"];
                detail.TechnicalTitle = detailsDic["职称"];
                detail.RequestID = person.RequestID;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "获取 {0} 详细信息出错", person.RequestID);
            }

            return detail;
        }

        private IEnumerable<Person> ParsePerson(XElement xElement)
        {
            var rows = xElement.XPathSelectElements("/row");
            foreach (var row in rows)
            {
                var person = new Person();
                foreach (var element in row.Nodes().Where(n => n.NodeType == XmlNodeType.Element).Cast<XElement>())
                {
                    if (string.Equals(element.Name.LocalName, "col"))
                    {
                        var attributeDictionary = element.Attributes()
                            .ToDictionary(a => a.Name.LocalName, a => a.Value);
                        if (attributeDictionary.TryGetValue("column", out var colValue))
                        {
                            attributeDictionary.TryGetValue("value", out var value);
                            try
                            {
                                switch (colValue)
                                {
                                    case "lastname":
                                        person.LastName = value;
                                        person.RequestID = Regex.Match(element.Value, "\\((.+)\\)").Value.Trim('(', ')');
                                        break;
                                    case "workcode":
                                        person.WorkCode = value;
                                        break;
                                    case "sex":
                                        person.Sex = element.Value;
                                        break;
                                    case "status":
                                        person.Status = element.Value;
                                        break;
                                    case "managerid":
                                        person.Manager = element.Value;
                                        break;
                                    case "jobtitle":
                                        person.JobTitle = element.Value;
                                        break;
                                    case "dsporder":
                                        person.DspOrder = int.Parse(value.Split('.')[0]);
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex, null, "解析 col:{0} 出错, value:{1}", colValue, value);
                            }
                        }
                    }
                }

                yield return person;
            }
        }

        internal void RebuildDatabase()
        {
            using (var context = new OrganizationContext(DbFileName))
            {
                context.Database.Initialize(true);
            }
        }
#endif
    }
}