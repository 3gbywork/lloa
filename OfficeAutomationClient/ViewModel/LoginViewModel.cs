using CommonUtility.Logging;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using OfficeAutomationClient.Helper;
using OfficeAutomationClient.OA;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OfficeAutomationClient.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        private static ILogger logger = LogHelper.GetLogger<LoginViewModel>();

        private List<string> _users;
        private string _user;
        private string _validateCode;
        private ImageSource _validateCodeImage;
        private bool _rememberPwd;
        private bool _autoLogin;

        public string Title => "登录";

        public List<string> Users { get => _users; set => Set(ref _users, value); }
        public string User
        {
            get => _user;
            set
            {
                Set(ref _user, value);
                Messenger.Default.Send(value, TMessage.UserChanged);
            }
        }
        public string ValidateCode { get => _validateCode; set => Set(ref _validateCode, value); }
        public ImageSource ValidateCodeImage
        {
            get => _validateCodeImage;
            set
            {
                Set(ref _validateCodeImage, value);
                ValidateCode = Business.Instance.GetValidateCode();
            }
        }
        public bool RememberPwd { get => _rememberPwd; set => Set(ref _rememberPwd, value); }
        public bool AutoLogin { get => _autoLogin; set => Set(ref _autoLogin, value); }

        public ICommand RefreshValidateCodeCommand { get; set; }
        public ICommand LoginCommand { get; set; }


        public LoginViewModel()
        {
            logger.Info("初始化登陆信息……");
            Users = Business.Instance.GetUsers();
            ValidateCodeImage = Business.Instance.GetValidateCodeImage();

            User = ConfigHelper.GetString(ConfigKey.User);
            RememberPwd = ConfigHelper.GetBoolean(ConfigKey.RememberPwd);
            AutoLogin = ConfigHelper.GetBoolean(ConfigKey.AutoLogin);

            RefreshValidateCodeCommand = new RelayCommand(() =>
            {
                logger.Info("获取验证码……");
                ValidateCodeImage = Business.Instance.GetValidateCodeImage();
            });
            LoginCommand = new RelayCommand<PasswordBox>((p) =>
            {
                var rst = Business.Instance.Login(this, p.SecurePassword);
                logger.Info($"登陆 {(rst ? "成功" : "失败")}");
                if (rst)
                {
                    Close(true);
                }
                else
                {
                    RefreshValidateCodeCommand.Execute(null);
                }
            });
        }
    }
}
