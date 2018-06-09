using GalaSoft.MvvmLight.Threading;
using OfficeAutomationClient.OA;
using OfficeAutomationClient.View;
using System.Windows;

namespace OfficeAutomationClient
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherHelper.Initialize();

            var login = new LoginWindow();
            var rst = login.ShowDialog();
            if (rst.HasValue && rst.Value)
            {
                new InfoWindow().Show();
            }

        }

        protected override void OnExit(ExitEventArgs e)
        {
            Business.Instance.Dispose();

            base.OnExit(e);
        }
    }
}
