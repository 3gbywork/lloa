using System.Reflection;
using System.Windows.Controls;

namespace OfficeAutomationClient.Extension
{
    public static class WebBrowserExtensions
    {
        /// <summary>
        ///     设置浏览器静默，不弹错误提示框
        /// </summary>
        /// <param name="webBrowser">要设置的WebBrowser控件浏览器</param>
        /// <param name="silent">是否静默</param>
        public static void SuppressScriptErrors(this WebBrowser webBrowser, bool silent)
        {
            var fiComWebBrowser =
                typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser == null) return;

            var objComWebBrowser = fiComWebBrowser.GetValue(webBrowser);

            objComWebBrowser?.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser,
                new object[] {silent});
        }
    }
}