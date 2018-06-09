using CommonUtility.Extension;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using OfficeAutomationClient.OA;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

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
