using CommonUtility.Logging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using OfficeAutomationClient.Helper;
using OfficeAutomationClient.OA;
using System;
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

        public List<string> Users { get => _users; set => Set(nameof(Users), ref _users, value); }
        public string User
        {
            get => _user;
            set
            {
                Set(nameof(User), ref _user, value);
                Messenger.Default.Send(value, TMessage.UserChanged);
            }
        }
        public string ValidateCode { get => _validateCode; set => Set(nameof(ValidateCode), ref _validateCode, value); }
        public ImageSource ValidateCodeImage
        {
            get => _validateCodeImage;
            set
            {
                Set(nameof(ValidateCodeImage), ref _validateCodeImage, value);
                ValidateCode = Business.Instance.GetValidateCode();
            }
        }
        public bool RememberPwd { get => _rememberPwd; set => Set(nameof(RememberPwd), ref _rememberPwd, value); }
        public bool AutoLogin { get => _autoLogin; set => Set(nameof(AutoLogin), ref _autoLogin, value); }

        public ICommand RefreshValidateCodeCommand { get; set; }
        public ICommand LoginCommand { get; set; }


        public LoginViewModel()
        {
            logger.Info("初始化账号");
            Users = Business.Instance.GetUsers();
            ValidateCodeImage = Business.Instance.GetValidateCodeImage();

            User = ConfigHelper.GetString(ConfigKey.User);
            RememberPwd = ConfigHelper.GetBoolean(ConfigKey.RememberPwd);
            AutoLogin = ConfigHelper.GetBoolean(ConfigKey.AutoLogin);

            RefreshValidateCodeCommand = new RelayCommand(() =>
            {
                ValidateCodeImage = Business.Instance.GetValidateCodeImage();
            });
            LoginCommand = new RelayCommand<PasswordBox>((p) =>
            {
                Business.Instance.Login(this, p.SecurePassword);

                //Business.Instance.GetAttendance(DateTime.Now.ToString("yyyy-MM-dd"));
            });
        }
    }
}
