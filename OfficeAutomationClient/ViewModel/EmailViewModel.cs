using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using OfficeAutomationClient.OA;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace OfficeAutomationClient.ViewModel
{
    public class EmailViewModel : ViewModelBase
    {
        private bool _showAdvSettings;
        private double _height = 190;
        private string _user;
        private List<string> _users;
        private bool _enableSsl;
        private string _iP;
        private string _port;

        public string Title => "程序已崩溃!请设置用于发送崩溃报告的邮箱!";

        public bool ShowAdvSettings
        {
            get => _showAdvSettings;
            set
            {
                Set(ref _showAdvSettings, value);
                if (value)
                    Height = 250;
                else
                    Height = 190;
            }
        }

        public double Height { get => _height; set => Set(ref _height, value); }

        public string User
        {
            get => _user;
            set
            {
                Set(ref _user, value);
                Messenger.Default.Send(value, TMessengerToken.EmailUserChanged);

                IP = Business.Instance.GetSmtpServerHost(value);
            }
        }

        public bool EnableSsl
        {
            get => _enableSsl; set
            {
                Set(ref _enableSsl, value);

                Port = Business.Instance.GetSmtpServerPort(value).ToString();
            }
        }

        public string IP { get => _iP; set => Set(ref _iP, value); }

        public string Port { get => _port; set => Set(ref _port, value); }

        public ICommand CheckLoginCommand { get; set; }

        public EmailViewModel()
        {
            EnableSsl = true;

            CheckLoginCommand = new RelayCommand<PasswordBox>(async p =>
            {
                if (string.IsNullOrEmpty(User) || p.SecurePassword.Length == 0)
                {
                    Messenger.Default.Send("用户名或密码不能为空", TMessengerToken.ShowConfirmation);
                    return;
                }

                var message = string.Empty;
                var result = await Business.Instance.CheckEmailValid(this, p.SecurePassword);
                if (result == EmailValidationResult.OK)
                    //message = "验证成功";
                    Close(true);
                else if (result == EmailValidationResult.AuthenticationFailed)
                    message = "用户名或密码不正确";
                else if (result == EmailValidationResult.ConnectionError)
                    message = "无法连接,请检查设置或网络";
                else if (result == EmailValidationResult.ProtocolError)
                    message = "请检查SSL和Port设置是否正确";
                else
                    message = "验证失败,查看日志获取详细信息";

                if (!string.IsNullOrEmpty(message))
                    Messenger.Default.Send(message, TMessengerToken.ShowConfirmation);
            });
        }
    }
}
