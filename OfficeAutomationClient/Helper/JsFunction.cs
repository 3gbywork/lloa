using GalaSoft.MvvmLight.Threading;
using OfficeAutomationClient.OA;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OfficeAutomationClient.Helper
{
    [ComVisible(true)]
    public class JsFunction
    {
        private readonly WebBrowser browser;

        public JsFunction(WebBrowser browser)
        {
            this.browser = browser;
        }

        public void GetAttendance(string date)
        {
            TaskEx.Run(() =>
            {
                var attendance = Business.Instance.GetAndCacheAttendance(date);

                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    browser.InvokeScript("showAttendance", new object[] { date, attendance });
                });
            });
        }
    }
}
