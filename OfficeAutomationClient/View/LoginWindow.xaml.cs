using CommonUtility.Extension;
using CommonUtility.Logging;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using OfficeAutomationClient.Helper;
using OfficeAutomationClient.OA;
using OfficeAutomationClient.ViewModel;
using System.Collections.Generic;
using System.Linq;

namespace OfficeAutomationClient.View
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : WindowBase
    {
        private static ILogger logger = LogHelper.GetLogger<LoginWindow>();

        private List<string> users = new List<string>();

        public LoginWindow()
        {
            this.Loaded += delegate
            {
                var user = users.Last();
                GetPassword(user);

                var vm = GetViewModel<LoginViewModel>();
                if (vm.AutoLogin)
                {
                    logger.Info("开始自动登陆……");
                    vm.LoginCommand.Execute(password);
                }
            };

            Messenger.Default.Register<string>(this, TMessage.UserChanged, (user) =>
             {
                 if (IsLoaded)
                 {
                     GetPassword(user);
                 }
                 else
                 {
                     users.Add(user);
                 }
             });

            InitializeComponent();
        }

        private void GetPassword(string user)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (!string.IsNullOrEmpty(user))
                    password.Password = Business.Instance.QueryPassword(user).CreateString();
                else password.Password = string.Empty;
            });
        }
    }
}
