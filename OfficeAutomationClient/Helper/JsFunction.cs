using GalaSoft.MvvmLight.Messaging;
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

        public async Task GetAttendance(string date)
        {
            var attendance = await Business.Instance.GetAndCacheAttendance(date);

            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (string.IsNullOrEmpty(attendance))
                {
                    Messenger.Default.Send("获取考勤数据失败", TMessageToken.ShowConfirmation);
                }
                else
                {
                    browser.InvokeScript("showAttendance", new object[] { date, attendance });
                }
            });
        }
    }
}