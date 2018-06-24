﻿using CommonUtility.Extension;
using CommonUtility.Logging;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using OfficeAutomationClient.Helper;
using OfficeAutomationClient.OA;
using OfficeAutomationClient.ViewModel;
using System;
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
                if (users.Count == 0) return;

                var user = users.Last();
                GetPassword(user);

                var vm = GetViewModel<LoginViewModel>();
                if (vm.AutoLogin && !string.IsNullOrEmpty(user) && password.SecurePassword.Length > 0)
                {
                    logger.Info("开始自动登陆……");
                    vm.LoginCommand.Execute(password);
                }
            };

            Messenger.Default.Register<string>(this, TMessageToken.UserChanged, (user) =>
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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Messenger.Default.Unregister<string>(this, TMessageToken.UserChanged);
        }
    }
}
