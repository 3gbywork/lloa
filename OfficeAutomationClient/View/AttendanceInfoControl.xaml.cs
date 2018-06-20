using OfficeAutomationClient.Helper;
using OfficeAutomationClient.OA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using mshtml;
using System.Text.RegularExpressions;
using CommonUtility.Http;
using HtmlAgilityPack;
using Newtonsoft.Json;
using OfficeAutomationClient.Extension;
using OfficeAutomationClient.Model;

namespace OfficeAutomationClient.View
{
    /// <summary>
    /// AttendanceInfoControl.xaml 的交互逻辑
    /// </summary>
    public partial class AttendanceInfoControl : UserControl
    {
        public AttendanceInfoControl()
        {
            InitializeComponent();

            browser.ObjectForScripting = JsFunction.Instance;
            browser.SuppressScriptErrors(true);

            browser.LoadCompleted += (sender, e) =>
            {
                // 屏蔽右键菜单
                (browser.Document as HTMLDocumentEvents_Event).oncontextmenu += delegate { return false; };

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

            Loaded += delegate
            {
                browser.Navigate(OAUrl.Calendar);
            };
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
    }
}
