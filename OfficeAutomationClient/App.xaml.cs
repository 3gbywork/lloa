﻿using CommonUtility.Logging;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using OfficeAutomationClient.Helper;
using OfficeAutomationClient.OA;
using OfficeAutomationClient.View;
using System;
using System.Threading.Tasks;
using System.Windows;
using CommonUtility.Rand;

namespace OfficeAutomationClient
{
    /// <summary>
    ///     App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILogger Logger = LogHelper.GetLogger<App>();

        protected override async void OnStartup(StartupEventArgs e)
        {
            var splash = new SplashScreenWindow($"../Resources/splash{RandomEx.Next(maxValue: 10)}.gif");
            splash.Show();

            base.OnStartup(e);

            var initSucceed = await TaskEx.Run(() =>
            {
                try
                {
                    DispatcherHelper.Initialize();
                    Business.Instance.WarmUp();
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, null, "初始化出错");
                    return false;
                }
            });

            splash.Close(2000);

            if (!initSucceed) Current.Shutdown();

            AppDomain.CurrentDomain.UnhandledException += (sender, arg) =>
            {
                Logger.Error(arg.ExceptionObject as Exception, null, "程序异常终止");
            };

            Messenger.Default.Register<string>(this, TMessageToken.ShowConfirmation, msg => { MessageBox.Show(msg, Business.Instance.Title, MessageBoxButton.OK, MessageBoxImage.Warning); });

            var rst = ViewLocator.LoginWindow.ShowDialog();
            if (rst.HasValue && rst.Value)
            {
                Current.MainWindow = null;
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