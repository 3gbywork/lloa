using CommonUtility.Extension;
using GalaSoft.MvvmLight.Messaging;
using OfficeAutomationClient.OA;

namespace OfficeAutomationClient.View
{
    /// <summary>
    /// EmailWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EmailWindow : WindowBase
    {
        public EmailWindow()
        {
            InitializeComponent();

            Messenger.Default.Register<string>(this, TMessengerToken.EmailUserChanged, user =>
            {
                if (IsLoaded)
                {
                    password.Password = Business.Instance.QueryPassword(user).CreateString();
                }
            });
        }
    }
}
