using CommonUtility.Extension;
using CommonUtility.Logging;
using GalaSoft.MvvmLight.Messaging;
using OfficeAutomationClient.Helper;
using OfficeAutomationClient.OA;
using OfficeAutomationClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OfficeAutomationClient.View
{
    /// <summary>
    ///     LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : WindowBase
    {
        private static readonly ILogger logger = LogHelper.GetLogger<LoginWindow>();

        private readonly List<string> users = new List<string>();

        public LoginWindow()
        {
            Loaded += delegate
            {
                var vm = GetViewModel<LoginViewModel>();
                vm?.RefreshValidateCodeCommand.Execute(null);

                if (users.Count == 0) return;

                var user = users.Last();
                GetPassword(user);

                if (null == vm) return;
                if (vm.AutoLogin && !string.IsNullOrEmpty(user) && password.SecurePassword.Length > 0)
                {
                    logger.Info("开始自动登陆……");
                    vm.LoginCommand.Execute(password);
                }
            };

            Messenger.Default.Register<string>(this, TMessageToken.UserChanged, user =>
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
            if (!string.IsNullOrEmpty(user))
                password.Password = Business.Instance.QueryPassword(user).CreateString();
            else password.Password = string.Empty;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Messenger.Default.Unregister<string>(this, TMessageToken.UserChanged);
        }
    }
}