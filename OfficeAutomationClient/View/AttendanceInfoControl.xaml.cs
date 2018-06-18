using OfficeAutomationClient.Helper;
using OfficeAutomationClient.OA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            browser.Navigating += (sender, e) =>
            {
                SetWebBrowserSilent(sender as WebBrowser, true);
            };

            browser.LoadCompleted += (sender, e) =>
            {
                var dom = browser.Document as HTMLDocument;

                dom.getElementById("indexWindow").children[0].innerHTML = "";

                var css = dom.createStyleSheet();
                css.addRule(".status", "width:21px;height:21px;line-height:21px;font-weight:700;color:#fff;position:absolute;left:5px;top:5px;z-index:2;overflow:hidden;font-size:14px;display:block;");
                css.addRule(".attendance", "width:21px;height:21px;line-height:21px;font-weight:700;color:#fff;position:absolute;left:5px;bottom:5px;z-index:2;overflow:hidden;font-size:14px;display:block;");

                var year = dom.getElementById("yearValue").innerText.Trim();
                var month = dom.getElementById("monthValue").innerText.Trim().PadLeft(2, '0');
                var day = Regex.Match(dom.documentElement.innerHTML, "NumberDay\">(\\d+)<").Value.Split('>', '<')[1];

                var attInfo = Business.Instance.GetAttendance($"{year}-{month}-{day}");

                var table = dom.getElementById("cont").document as HTMLDocument;
                var alist = table.getElementsByName("a");

                var firstDate = $"{month}{day}";
                var findFirst = false;
                for (int i = 0, j = 0; i < alist.length; i++)
                {
                    var a = alist.item(index: i) as IHTMLElement;
                    if (firstDate.Equals(a.getAttribute("data")))
                    {
                        findFirst = true;
                    }
                    if (findFirst)
                    {
                        //a.document
                        //attInfo[j].Attend
                    }
                }
            };

            Loaded += delegate
            {
                browser.Navigate(OAUrl.Calendar);
            };
        }

        /// <summary>  
        /// 设置浏览器静默，不弹错误提示框  
        /// </summary>  
        /// <param name="webBrowser">要设置的WebBrowser控件浏览器</param>  
        /// <param name="silent">是否静默</param>  
        private void SetWebBrowserSilent(WebBrowser webBrowser, bool silent)
        {
            FieldInfo fi = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi != null)
            {
                object browser = fi.GetValue(webBrowser);
                if (browser != null)
                    browser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, browser, new object[] { silent });
            }
        }
    }
}
