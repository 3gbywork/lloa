using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Caching;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CommonUtility.Extension;
using CommonUtility.Http;
using CommonUtility.Logging;
using CommonUtility.Rand;
using CredentialManagement;
using HtmlAgilityPack;
using ICSharpCode.SharpZipLib.Zip;
using MailKit.Net.Smtp;
using MailKit.Security;
using Newtonsoft.Json;
using OfficeAutomationClient.Database;
using OfficeAutomationClient.Helper;
using OfficeAutomationClient.Model;
using OfficeAutomationClient.ViewModel;
using Polly;
using ValidateCodeProcessor;

namespace OfficeAutomationClient.OA
{
    internal class Business : IDisposable
    {
        private const string DefaultTitle = "OA";
        private const string DefaultCompany = "办公自动化系统";
        private const string CredentialSetTarget = "OfficeAutomationClient";
        private const string BugReporter = "Bug Reporter";
        private const string DbFileName = "organization.db";
        private const string RsaPublicKey = "MIIBIDANBgkqhkiG9w0BAQEFAAOCAQ0AMIIBCAKCAQEAk74T5uR8KEz/pyx4N8xa2l+cmL0pftBkkgOotcprvgOzZf3joBOv+0Tk7wa+/g0b6SJTGUm5Z79XHM1smYeeqO4Q6jgPg39JEYJ8TLZvGD0hPxHAfhjjqQg4eHXmN4l+DIxt5Et/5N537fS8CgvgxHT1YbfJgXu5L1sRmPW2bjagPciRsd9vxQNOdQZGtZzFYA/Dv9t4RXy6rY9PlQ1ImYC3biv61BEbaUXshvACXrenuMX/MMdv+IjxiPvB7M7LVBhxrfaVeI0+P7EOGeGhSBzMKIm8G/BTElgZ9wgbvZ8Jl8S/4Tf5nafTcGcJFUT+8H+FivusM3ZjReC554laTwIBAw==";
        private static readonly byte[] OptionalEntropy = Encoding.UTF8.GetBytes("3.141592653589793238462643383279");
        private static readonly ILogger Logger = LogHelper.GetLogger<Business>();

        public static AssemblyName AssemblyName = Assembly.GetExecutingAssembly().GetName();
        private static HttpClient _httpClient;

        private static readonly CookieContainer CookieContainer = new CookieContainer();
        private readonly CredentialSet _credentials;
        private string _loginCookieCheck;

        private string _userId;

        private Business()
        {
            _credentials = new CredentialSet();
            _credentials.Load();

            var version = RegistryHelper.GetIEVersion();
            if (version != IEVersion.None)
            {
                RegistryHelper.SetWebBrowserEmulation(AssemblyName.Name + ".exe", version);
            }
        }

        public static Business Instance { get; } = new Business();

        public string CompanyName { get; set; }
        public string Title { get; set; }
        public string ValidateCode { get; set; }

        public void Dispose()
        {
            Logout();
            OcrProcessor.Instance.Dispose();
            LogHelper.Shutdown();
        }

        #region 账号读取和保存

        private void SaveOrUpdateCredential(string user, SecureString password)
        {
            SaveOrUpdateCredential(user, Encoding.UTF8.GetBytes(password.CreateString()));
        }

        private void SaveOrUpdateCredential(string user, byte[] userData)
        {
            var credential = _credentials.Find(c => string.Equals(c.Username, user)) ?? new Credential(user)
            {
                PersistanceType = PersistanceType.LocalComputer,
                Type = CredentialType.Generic,
                Target = $"{CredentialSetTarget}:{Guid.NewGuid()}"
            };
            credential.Password = ProtectedData
                .Protect(userData, OptionalEntropy, DataProtectionScope.CurrentUser)
                .ToBase64String();
            credential.Save();
            _credentials.Load();
        }

        internal List<string> GetLoginUsers()
        {
            return _credentials.Where(c => c.Target.StartsWith(CredentialSetTarget) && !c.Username.Equals(BugReporter)).Select(c => c.Username).ToList();
        }

        internal SecureString QueryPassword(string username)
        {
            return QueryUserData(username).ToString(Encoding.UTF8).CreateSecureString();
        }

        internal byte[] QueryUserData(string username)
        {
            var result = new byte[0];
            if (string.IsNullOrEmpty(username)) return result;

            var user = _credentials.Find(c => c.Target.StartsWith(CredentialSetTarget) && c.Username.Equals(username));
            if (null != user)
            {
                try
                {
                    return ProtectedData.Unprotect(user.SecurePassword.CreateString().FromBase64String(), OptionalEntropy, DataProtectionScope.CurrentUser);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, null, "解析 {0} 保存的数据失败", username);
                }
            }

            return result;
        }

        #endregion

        #region 数据库查询

        private string GetCompanyID(string deptID)
        {
            try
            {
                using (var context = new OrganizationContext(DbFileName))
                {
                    // 避免死循环，查询10个层次
                    for (var i = 10; i > 0; i--)
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
            catch (Exception ex)
            {
                Logger.Error(ex, null, "获取部门 {0} 所属公司出错", deptID);
            }

            return string.Empty;
        }

        private string GetDepartmentID(string userID)
        {
            try
            {
                using (var context = new OrganizationContext(DbFileName))
                {
                    return context.People.Single(p => p.RequestID.Equals(userID)).OrganizationID;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "获取员工 {0} 所属部门出错", userID);
            }

            return string.Empty;
        }

        #endregion

        #region HTML解析

        private static string TryMatch(string input, string pattern, string defaultValue)
        {
            try
            {
                var match = Regex.Match(input, pattern);
                if (!string.IsNullOrEmpty(match.Value))
                {
                    return match.Value.Split('=')[1].Trim('\'', '\"', ' ');
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "解析 {0} 出错", new { input = input.JavaScriptStringEncode(), pattern });
            }
            return defaultValue;
        }

        private static string GetTitle(string resp)
        {
            return TryMatch(resp, "document.title='(.+)'", DefaultTitle);
        }

        private static string GetCompanyName(string resp)
        {
            return TryMatch(resp, "companyname = \"(.+)\"", DefaultCompany);
        }

        private static string GetLoginCookie(string resp)
        {
            return TryMatch(resp, "logincookiecheck=(.+)'", string.Empty);
        }

        private static string GetPaidLeaveDays(string resp)
        {
            if (string.IsNullOrEmpty(resp)) return string.Empty;
            try
            {
                return resp.HtmlDecode().Split(':')[1].Trim();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "解析可用调休天数出错，resp：{0}", resp.JavaScriptStringEncode());
            }
            return string.Empty;
        }

        #endregion

        #region 业务API（模拟请求）

        private async Task<string> GetString(string url, Dictionary<string, string> parameters = null)
        {
            try
            {
                if (null == parameters)
                {
                    return await _httpClient.GetStringAsync(url);
                }

                var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(parameters));
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "访问 {0} 出错", url);
                return default(string);
            }
        }

        #region 获取验证码

        internal async Task<ImageSource> GetValidateCodeAsync()
        {
            var task = _httpClient.GetStreamAsync(OAUrl.ValidateCode);

            return await task.ContinueWith(t =>
            {
                try
                {
                    var bitmap = t.Result.ToBitmap();
                    t.Result.Dispose();

                    var imageSource = bitmap.ToImageSource();
                    imageSource.Freeze();
                    ValidateCode = bitmap.Gray().DeNoise().Binarize().Text().Replace(" ", "").Trim();

                    return imageSource;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, null, "获取并解析验证码失败");
                }

                return null;
            });
        }

        #endregion

        #region 登录登出

        internal async Task<bool> Login(LoginViewModel login, SecureString password)
        {
            await GetString(OAUrl.Home);
            var rsp = await GetString(OAUrl.Login2);

            Title = GetTitle(rsp);

            var parameters = new Dictionary<string, string>
            {
                {"loginfile", "/wui/theme/ecology8/page/login.jsp?templateId=3&logintype=1&gopage="},
                {"logintype", "1"},
                {"fontName", "微软雅黑"},
                {"message", ""},
                {"gopage", ""},
                {"formmethod", "post"},
                {"rnd", ""},
                {"serial", ""},
                {"username", ""},
                {"isie", "false"},
                {"islanguid", "7"},
                {"loginid", login.User},
                {"userpassword", password.CreateString()},
                {"validatecode", login.ValidateCode},
                {"submit", "登录"}
            };
            var resp = await GetString(OAUrl.VerifyLogin, parameters);
            _loginCookieCheck = GetLoginCookie(resp);
            if (string.IsNullOrEmpty(_loginCookieCheck)) return false;

            CookieContainer.Add(new Uri(OAUrl.Login), new Cookie("logincookiecheck", _loginCookieCheck, "/login/"));
            await GetString(OAUrl.RemindLogin);
            //_cookieContainer.GetCookies(uri)["logincookiecheck"].Expires = DateTime.Now.AddDays(-1);

            _userId = CookieContainer.GetCookies(new Uri(OAUrl.Home))["loginidweaver"].Value;
            CompanyName = GetCompanyName(await GetString(OAUrl.SysRemind));

            if (login.RememberPwd)
            {
                SaveOrUpdateCredential(login.User, password);

                ConfigHelper.Save(ConfigKey.User, login.User);
                ConfigHelper.Save(ConfigKey.RememberPwd, login.RememberPwd.ToString());
                ConfigHelper.Save(ConfigKey.AutoLogin, login.AutoLogin.ToString());
            }

            return true;
        }

        internal async void Logout()
        {
            if (!string.IsNullOrEmpty(_loginCookieCheck))
                CookieContainer.Add(new Uri(OAUrl.Home), new Cookie("logincookiecheck", _loginCookieCheck));
            await GetString(OAUrl.Logout);

            foreach (Cookie cookie in CookieContainer.GetCookies(new Uri(OAUrl.Home)))
            {
                cookie.Expired = true;
            }
        }

        #endregion

        #region 获取/缓存/清除缓存考勤信息

        internal void RemoveAttendance(string date)
        {
            try
            {
                var cache = MemoryCache.Default;
                cache.Remove(DateTime.Parse(date).ToString("yyyy-MM"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "清除考勤缓存数据失败，date：{0}", date);
            }
        }

        internal async Task<string> GetAndCacheAttendance(string date)
        {
            try
            {
                var cache = MemoryCache.Default;
                var key = DateTime.Parse(date).ToString("yyyy-MM");
                if (cache.Contains(key))
                {
                    return cache[key] as string;
                }

                var rst = await GetAttendanceJson(date);
                if (!string.IsNullOrEmpty(rst))
                    cache.Set(key, rst, DateTimeOffset.MaxValue);
                return rst;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "获取考勤数据失败，date：{0}", date);
            }

            return string.Empty;
        }

        private async Task<string> GetAttendanceJson(string date)
        {
            var attInfo = await GetAttendance(date);
            if (null == attInfo || attInfo.Count == 0) return string.Empty;

            return JsonConvert.SerializeObject(attInfo);
        }

        internal async Task<List<AttendanceInfo>> GetAttendance(string date)
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
            var attdata = await GetString(OAUrl.MonthAttData, parameters);

            try
            {
                var html = new HtmlDocument();
                html.LoadHtml(attdata);

                var rstNode = html.DocumentNode.SelectSingleNode("//table[@id=\"monthAttData\"]").ChildNodes
                    .Where(n => n.Name.Equals("tr")).Last();
                var attrst = rstNode.ChildNodes.Where(n => n.Name.Equals("td")).Skip(2).Select(n =>
                {
                    var info = new AttendanceInfo();
                    info.Attend = n.InnerText.Trim();
                    var style = n.Attributes["style"].Value.ToLower();
                    if (style.Contains("red"))
                    {
                        info.Holiday = true;
                    }
                    else if (style.Contains("green"))
                    {
                        info.Holiday = false;
                    }

                    return info;
                }).ToList();

                return attrst;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "解析考勤数据出错，date：{0}，response：{1}", date, attdata.JavaScriptStringEncode());
            }

            return Enumerable.Empty<AttendanceInfo>().ToList();
        }

        #endregion

        #region 获取组织结构信息

#if INIT_ORGANIZATION_DB
        private readonly Func<int, TimeSpan> _delayDurationProvider =
            attempt => TimeSpan.FromSeconds(Math.Min(5, Math.Pow(2, attempt - 1)));

        internal Task<Organizations> GetOrganizations()
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
                .ExecuteAsync(async () =>
                    JsonConvert.DeserializeObject<Organizations>(await GetString(OAUrl.Organization, parameters)));
        }

        internal async Task<People> GetPeople(Organization dept)
        {
            var people = new People();
            if (dept.Type != OrganizationType.Dept) return people;

            try
            {
                var resp = await GetString($"{OAUrl.HrmResourceList}{dept.ID.Substring(1)}");
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

                    resp = await Policy<string>.HandleResult(r => !(!string.IsNullOrEmpty(r) && r.StartsWith("<?xml")))
                        .WaitAndRetryForever(_delayDurationProvider)
                        .ExecuteAsync(async () => await GetString(OAUrl.SplitPage, parameters));

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

        internal async Task<PersonalDetail> GetPersonalDetail(Person person)
        {
            var detail = new PersonalDetail();
            if (string.IsNullOrEmpty(person.RequestID)) return default(PersonalDetail);

            try
            {
                var parameter = new Dictionary<string, string>
                {
                    {"isfromtab", "true"},
                    {"id", person.RequestID},
                    {"fromHrmTab", "1"}
                };

                var html = new HtmlDocument();
                var detailsDic = await Policy<Dictionary<string, string>>
                    .HandleResult(r => null == r || r.Count == 0)
                    .WaitAndRetryForever(_delayDurationProvider)
                    .ExecuteAsync(async () =>
                    {
                        var resp = await GetString(OAUrl.HrmResourceBase, parameter);
                        html.LoadHtml(resp);

                        var names = (from name in html.DocumentNode.SelectNodes("//td[@class=\"fieldName\"]")
                            select name.InnerHtml.HtmlDecode().Trim()).ToList();
                        var values = (from value in html.DocumentNode.SelectNodes("//td[@class=\"field\"]")
                            select value.InnerText.HtmlDecode().Trim()).ToList();

                        var rstdic = new Dictionary<string, string>();
                        if (names.Count == 0 || values.Count == 0 || names.Count != values.Count) return rstdic;

                        for (var i = 0; i < names.Count; i++)
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
                                        person.RequestID = Regex.Match(element.Value, "\\((.+)\\)").Value
                                            .Trim('(', ')');
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

        #endregion

        #region 获取调休天数

        internal async Task<string> GetPaidLeaveDays()
        {
            var parameters = new Dictionary<string, string>
            {
                {"operation", "getTXInfo"},
                {"resourceId", _userId},
                {"currentDate", ""},
            };

            var resp = await GetString(OAUrl.PaidLeaveDays, parameters);
            return GetPaidLeaveDays(resp);
        }

        #endregion

        #endregion

        #region 验证CrashReporter账号

        internal async Task<EmailValidationResult> CheckEmailValid(EmailViewModel email, SecureString password)
        {
            var result = EmailValidationResult.Failed;
            var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(email.IP, int.Parse(email.Port), email.EnableSsl);
                await client.AuthenticateAsync(email.User, password.CreateString());
                await client.DisconnectAsync(true);

                result = EmailValidationResult.OK;

                var emailInfo = new EmailInfo
                {
                    User = email.User,
                    Password = Encoding.UTF8.GetBytes(password.CreateString()),
                    Host = email.IP,
                    Port = email.Port,
                    EnableSsl = email.EnableSsl
                };
                var serializeString = JsonConvert.SerializeObject(emailInfo);
                SaveOrUpdateCredential(BugReporter, Encoding.UTF8.GetBytes(serializeString));
            }
            catch (Exception ex)
            {
                if (ex is SocketException)
                    result = EmailValidationResult.ConnectionError;
                else if (ex is SmtpProtocolException)
                    result = EmailValidationResult.ProtocolError;
                else if (ex is AuthenticationException)
                    result = EmailValidationResult.AuthenticationFailed;
                Logger.Error(ex, null, "验证Crash Reporter账号异常, {0}:{1} ssl:{2} user:{3}", email.IP, email.Port, email.EnableSsl, email.User);
            }
            finally
            {
                client.Dispose();
            }

            return result;
        }

        internal string GetSmtpServerHost(string email)
        {
            return CrashReporter.GetSmtpServerHost(email);
        }

        internal int GetSmtpServerPort(bool enableSsl)
        {
            return CrashReporter.GetSmtpServerPort(enableSsl);
        }

        internal EmailInfo GetEmailInfo()
        {
            var infoString = QueryUserData(BugReporter).ToString(Encoding.UTF8);
            if (!string.IsNullOrEmpty(infoString))
                return JsonConvert.DeserializeObject<EmailInfo>(infoString);
            return null;
        }

        internal void SendCrashReport(string body, string attachmentPath)
        {
            try
            {
                var email = GetEmailInfo();
                if (null != email)
                {
                    var zipPwd = RandomEx.NextString(100).CreateSecureString();
                    var crashReporter = CrashReporter.CreateSimpleCrashReporter(email.User, email.EnableSsl);
                    crashReporter.Password = Encoding.UTF8.GetString(email.Password).CreateSecureString();
                    crashReporter.Body = $@"{body}

=====================================================================================================================

{CryptoHelper.RsaEncrypt(Encoding.UTF8.GetBytes(zipPwd.CreateString()), CryptoHelper.GetAsymmetricKeyParameter(RsaPublicKey, false)).ToBase64String()}";
                    crashReporter.IsBodyHtml = false;

                    var zipFile = ZipFile.Create(Path.Combine(Path.GetTempPath(), $"report-{Guid.NewGuid().ToString("N")}.zip"));
                    try
                    {
                        zipFile.Password = zipPwd.CreateString();
                        zipFile.BeginUpdate();

                        var systemInfo = Path.GetTempFileName();
                        File.WriteAllText(systemInfo, ConsoleTool.Create("cmd.exe", "/c systeminfo").RunAndGetOutput());
                        zipFile.Add(systemInfo, "systeminfo.txt");

                        var dir = new DirectoryInfo(attachmentPath);
                        if (dir.Exists)
                        {
                            foreach (var logFile in dir.GetFiles())
                            {
                                zipFile.Add(logFile.FullName, logFile.Name);
                            }
                        }
                        zipFile.CommitUpdate();
                    }
                    finally
                    {
                        zipFile.Close();
                    }

                    crashReporter.Attachments.Add(new Attachment()
                    {
                        FileName = zipFile.Name,
                        MediaType = MediaTypeNames.Text.Plain,
                    });

                    crashReporter.Send();
                }
            }
            catch (Exception)
            {

            }
        }

        #endregion

        public void WarmUp()
        {
            // 预热HttpClient
            // 参考：https://www.cnblogs.com/dudu/p/csharp-httpclient-attention.html
            // DON'T USE WebRequestHandler!!!!!! that will cause deadlock
            _httpClient = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = CookieContainer
            });
            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
            _httpClient.DefaultRequestHeaders.ConnectionClose = false;
            _httpClient.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) OfficeAutomationClient/20180628");

            // 热身
            _httpClient.SendAsync(new HttpRequestMessage
            {
                Method = new HttpMethod("HEAD"),
                RequestUri = new Uri(OAUrl.Home),
            }).Result.EnsureSuccessStatusCode();


            // 预热entity framework
            // 参考：https://www.cnblogs.com/dudu/p/entity-framework-warm-up.html
            using (var context = new OrganizationContext(DbFileName))
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                var mappingCollection = (StorageMappingItemCollection)objectContext
                    .MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);
                mappingCollection.GenerateViews(new List<EdmSchemaError>());
            }
        }
    }
}