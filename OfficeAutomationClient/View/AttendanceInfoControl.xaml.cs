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

            browser.Navigating += (sender, e) =>
            {
                (sender as WebBrowser).SuppressScriptErrors(true);
            };

            browser.LoadCompleted += (sender, e) =>
            {
                var dom = browser.Document as HTMLDocument;

                dom.getElementById("indexWindow").children[0].innerHTML = "";

                var css = dom.createStyleSheet();
                css.cssText = GetStringFromFile("css/attendance.css");
                //css.addRule(".status", "width:21px;height:21px;line-height:21px;font-weight:700;color:#fff;position:absolute;left:5px;top:5px;z-index:2;overflow:hidden;font-size:14px;display:block;");
                //css.addRule(".attendance", "width:21px;height:21px;line-height:21px;font-weight:700;color:#fff;position:absolute;left:5px;bottom:5px;z-index:2;overflow:hidden;font-size:14px;display:block;");

                var js = dom.createElement("script");
                js.setAttribute("type", "text/javascript");
                js.setAttribute("text", "window.external.GetAttendance(\"2018-06-02\")");
                //js.setAttribute("text", GetStringFromFile("js/attendance.js"));
                var head = dom.getElementsByTagName("head").Cast<HTMLHeadElement>().First();
                head.appendChild((IHTMLDOMNode)js);
                //(dom.body.document as HTMLDocument).appendChild(js.document);
                //dom.appendChild(js.document);

                browser.InvokeScript("showCal", new object[] { DateTime.Now.ToString("yyyy-MM-dd") });

                //var head = dom.getElementsByTagName("head")[0];
                //var year = dom.getElementById("yearValue").innerText.Trim();
                //var month = dom.getElementById("monthValue").innerText.Trim().PadLeft(2, '0');
                //var day = Regex.Match(dom.documentElement.innerHTML, "NumberDay\">(\\d+)<").Value.Split('>', '<')[1];

                //var attInfo = Business.Instance.GetAttendance($"{year}-{month}-{day}");

                //var htmlDom = new HtmlDocument();
                //htmlDom.LoadHtml(dom.documentElement.innerHTML);
                ////htmlDom.GetElementbyId("cont").ChildNodes.

                //var table = dom.getElementById("cont").document as HTMLDocument;
                //var alist = table.getElementsByName("a");

                //var firstDate = $"{month}{day}";
                //var findFirst = false;
                //for (int i = 0, j = 0; i < alist.length; i++)
                //{
                //    var a = alist.item(index: i) as IHTMLElement;
                //    if (firstDate.Equals(a.getAttribute("data")))
                //    {
                //        findFirst = true;
                //    }
                //    if (findFirst)
                //    {
                //        //a.document
                //        //attInfo[j].Attend
                //    }
                //}
            };

            Loaded += delegate
            {
                browser.Navigate(OAUrl.Calendar);
            };
        }

        private string GetStringFromFile(string filename)
        {
            if (File.Exists(filename))
            {
                return File.ReadAllText(filename);
            }

            return string.Empty;
        }
    }
}
