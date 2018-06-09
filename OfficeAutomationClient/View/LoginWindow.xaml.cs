using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using OfficeAutomationClient.Helper;
using OfficeAutomationClient.OA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OfficeAutomationClient.View
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        private List<string> users = new List<string>();

        public LoginWindow()
        {
            this.Loaded += delegate
            {
                var user = users.Last();
                GetPassword(user);
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

            Messenger.Default.Register<WindowType>(this, TMessage.CloseWindow, (type) =>
            {
                if (type == WindowType.Login)
                {
                    Close();
                }
            });

            InitializeComponent();
        }

        private void GetPassword(string user)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (!string.IsNullOrEmpty(user))
                    password.Password = Business.Instance.QueryPassword(user).Text();
                else password.Password = string.Empty;
            });
        }
    }
}
