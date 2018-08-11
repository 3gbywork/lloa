using CommonUtility.Logging;
using CommonUtility.Rand;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using OfficeAutomationClient.Helper;
using OfficeAutomationClient.OA;
using OfficeAutomationClient.View;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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
#if GENERATE_RSA_KEYPAIR
            if (e.Args.Length == 2)
            {
                if (int.TryParse(e.Args[0], out int bits) && !string.IsNullOrWhiteSpace(e.Args[1]))
                {
                    ConsoleAttacher.AllocConsole();

                    try
                    {
                        Console.WriteLine("生成{0}位RSA密钥对到目录{1}......", e.Args[0], e.Args[1]);
                        var pair = CryptoHelper.GenerateRsaKeyPair(bits);

                        if (!Directory.Exists(e.Args[1])) Directory.CreateDirectory(e.Args[1]);

                        var privateKeyFile = Path.Combine(e.Args[1], "private.key");
                        Console.WriteLine("生成私钥文件:{0}", privateKeyFile);
                        File.WriteAllText(privateKeyFile, CryptoHelper.GetRsaDerEncodedBase64String(pair.Private));

                        var publicKeyFile = Path.Combine(e.Args[1], "public.key");
                        Console.WriteLine("生成公钥文件:{0}", publicKeyFile);
                        File.WriteAllText(publicKeyFile, CryptoHelper.GetRsaDerEncodedBase64String(pair.Public));

                        Console.WriteLine("密钥成功生成");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("生成RSA密钥对失败,参数:{0},{1},异常:{2}", e.Args[0], e.Args[1], ex);
                    }

                    Console.Write("按回车键退出");
                    Console.ReadLine();

                    Current.Shutdown();
                }
            }
#endif 
            Messenger.Default.Register<string>(this, TMessengerToken.ShowConfirmation, msg => { MessageBox.Show(msg, Business.Instance.Title, MessageBoxButton.OK, MessageBoxImage.Warning); });

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
                var exception = arg.ExceptionObject as Exception;
                Logger.Error(exception, null, "程序异常终止");

                // 等待nlog将所有日志都写入文件
                // 根据nlog的flushTimeout配置适当修改等待时间
                Thread.Sleep(500);

                if (null != Business.Instance.GetEmailInfo() || true == ViewLocator.EmailWindow.ShowDialog())
                {
                    Business.Instance.SendCrashReport(exception?.Message, $@"logs/{DateTimeOffset.Now:yyyy-MM-dd}");
                }
            };

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