using GalaSoft.MvvmLight.Messaging;
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
            Messenger.Default.Register<string>(this, TMessageToken.ShowConfirmation, (msg) =>
            {
                MessageBox.Show(msg);
            });

            var rst = ViewLocator.LoginWindow.ShowDialog();

            if (rst.HasValue && rst.Value)
            {
                Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                ViewLocator.InfoWindow.Show();
            }
            else
            {
                Current.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            Business.Instance.Dispose();
        }
    }
}
