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
        private static readonly ILogger Logger = LogHelper.GetLogger<LoginViewModel>();

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
                Messenger.Default.Send(value, TMessageToken.UserChanged);
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
            Logger.Info("初始化登陆信息……");
            Users = Business.Instance.GetUsers();
            ValidateCodeImage = Business.Instance.GetValidateCodeImage();

            User = ConfigHelper.Get<string>(ConfigKey.User);
            RememberPwd = ConfigHelper.Get<bool>(ConfigKey.RememberPwd);
            AutoLogin = ConfigHelper.Get<bool>(ConfigKey.AutoLogin);

            RefreshValidateCodeCommand = new RelayCommand(() =>
            {
                Logger.Info("获取验证码……");
                ValidateCodeImage = Business.Instance.GetValidateCodeImage();
            });
            LoginCommand = new RelayCommand<PasswordBox>((p) =>
            {
                if (string.IsNullOrEmpty(User) || p.SecurePassword.Length == 0)
                    Messenger.Default.Send("用户名或密码不能为空", TMessageToken.ShowConfirmation);

                var rst = Business.Instance.Login(this, p.SecurePassword);
                Logger.Info($"登陆 {(rst ? "成功" : "失败")}");
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
