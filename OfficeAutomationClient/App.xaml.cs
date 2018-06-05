using GalaSoft.MvvmLight.Threading;
using OfficeAutomationClient.OA;
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
        }

        protected override void OnExit(ExitEventArgs e)
        {
            NLog.LogManager.Shutdown();
            Business.Instance.Dispose();

            base.OnExit(e);
        }
    }
}
