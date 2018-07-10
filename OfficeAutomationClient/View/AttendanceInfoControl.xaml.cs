using CommonUtility.Http;
using mshtml;
using OfficeAutomationClient.Extension;
using OfficeAutomationClient.Helper;
using OfficeAutomationClient.OA;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommonUtility.Logging;

namespace OfficeAutomationClient.View
{
    /// <summary>
    ///     AttendanceInfoControl.xaml 的交互逻辑
    /// </summary>
    public partial class AttendanceInfoControl : UserControl
    {
        private static readonly ILogger Logger = LogHelper.GetLogger<AttendanceInfoControl>();
        private JsFunction _function;

        public AttendanceInfoControl()
        {
            InitializeComponent();

            _function = new JsFunction(browser);
            browser.ObjectForScripting = _function;
            browser.SuppressScriptErrors(true);

            browser.LoadCompleted += (sender, e) =>
            {
                // 屏蔽右键菜单
                (browser.Document as HTMLDocumentEvents_Event).oncontextmenu += () => false;
                (browser.Document as HTMLDocumentEvents2_Event).onerrorupdate += (evt) =>
                {
                    Logger.Error(evt);
                    return true;
                };
                var dom = browser.Document as HTMLDocument;

                // 清空头部文字及下载链接
                dom.getElementById("indexWindow").children[0].innerHTML = "";

                // 注入css
                var css = dom.createStyleSheet();
                css.cssText = GetStringFromFile("css/attendance.css");

                // 注入js
                var js = dom.createElement("script");
                js.setAttribute("type", "text/javascript");
                js.setAttribute("text", GetStringFromFile("js/attendance.js"));

                var head = dom.getElementsByTagName("head").Cast<HTMLHeadElement>().First();
                head.appendChild((IHTMLDOMNode)js);

                // 切换当前城市
                var city = GetCityName();
                // 调用注入后的js
                browser.InvokeScript("relocate", new object[] { city });
                browser.InvokeScript("showCalendar", new object[] { DateTime.Now.ToString("yyyy-MM-dd") });
            };

            Loaded += delegate { browser.Navigate(OAUrl.Calendar); };
        }

        private string GetCityName()
        {
            try
            {
                var resp = HttpWebRequestClient.Create("http://ip.chinaz.com/getip.aspx").GetResponseString();
                return resp.Split('省', '市')[1];
            }
            catch (Exception)
            {
                return "沈阳";
            }
        }

        private string GetStringFromFile(string filename)
        {
            if (File.Exists(filename))
            {
                return File.ReadAllText(filename);
            }

            return string.Empty;
        }

        private void browser_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 屏蔽 F5 刷新
            if (e.Key == Key.F5) e.Handled = true;
        }

        private async void RefreshAttendanceInfoClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (null == btn) return;

            btn.IsEnabled = false;

            try
            {
                var date = browser.InvokeScript("getCurrentDate") as string;

                Business.Instance.RemoveAttendance(date);
                await _function.GetAttendance(date);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "刷新考勤数据出错");
            }
            finally
            {
                btn.IsEnabled = true;
            }
        }
    }
}