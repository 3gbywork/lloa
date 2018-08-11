using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommonUtility.Logging;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using OfficeAutomationClient.Helper;
using OfficeAutomationClient.OA;
using Polly;

namespace OfficeAutomationClient.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        private static readonly ILogger Logger = LogHelper.GetLogger<LoginViewModel>();
        private bool _autoLogin;
        private bool _rememberPwd;
        private string _user;

        private List<string> _users;
        private string _validateCode;
        private ImageSource _validateCodeImage;


        public LoginViewModel()
        {
            Logger.Info("初始化登陆信息……");
            Users = Business.Instance.GetLoginUsers();
            User = ConfigHelper.Get<string>(ConfigKey.User);
            RememberPwd = ConfigHelper.Get<bool>(ConfigKey.RememberPwd);
            AutoLogin = ConfigHelper.Get<bool>(ConfigKey.AutoLogin);

            RefreshValidateCodeCommand = new RelayCommand(async () =>
            {
                Logger.Info("获取验证码……");
                ValidateCodeImage = await Business.Instance.GetValidateCodeAsync();
            });
            LoginCommand = new RelayCommand<PasswordBox>(async p =>
            {
                if (string.IsNullOrEmpty(User) || p.SecurePassword.Length == 0)
                {
                    Messenger.Default.Send("用户名或密码不能为空", TMessengerToken.ShowConfirmation);
                    return;
                }

                // 等待验证码加载并解析成功
                await Policy.HandleResult<string>(string.IsNullOrEmpty)
                       .WaitAndRetryAsync(10, i => TimeSpan.FromMilliseconds(500)).ExecuteAsync(() => TaskEx.FromResult(ValidateCode));
                if (null == ValidateCode || ValidateCode.Length != 4)
                {
                    Messenger.Default.Send("验证码不正确", TMessengerToken.ShowConfirmation);
                    return;
                }

                var rst = await Business.Instance.Login(this, p.SecurePassword);
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

        public string Title => "登录";

        public List<string> Users
        {
            get => _users;
            set => Set(ref _users, value);
        }

        public string User
        {
            get => _user;
            set
            {
                Set(ref _user, value);
                Messenger.Default.Send(value, TMessengerToken.LoginUserChanged);
            }
        }

        public string ValidateCode
        {
            get => _validateCode;
            set => Set(ref _validateCode, value);
        }

        public ImageSource ValidateCodeImage
        {
            get => _validateCodeImage;
            set
            {
                Set(ref _validateCodeImage, value);
                ValidateCode = Business.Instance.ValidateCode;
            }
        }

        public bool RememberPwd
        {
            get => _rememberPwd;
            set => Set(ref _rememberPwd, value);
        }

        public bool AutoLogin
        {
            get => _autoLogin;
            set => Set(ref _autoLogin, value);
        }

        public ICommand RefreshValidateCodeCommand { get; set; }
        public ICommand LoginCommand { get; set; }
    }
}