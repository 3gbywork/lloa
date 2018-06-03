using CredentialManagement;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;

namespace OfficeAutomationClient.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        public List<string> Users { get; set; }
        public string User { get; set; }
        public string ValidateCode { get; set; }
        public ImageSource ValidateCodeImage { get; set; }
        public bool RememberPwd { get; set; }

        public ICommand RefreshValidateCodeCommand { get; set; }
        public ICommand LoginCommand { get; set; }


        public LoginViewModel()
        {
            

        }
    }
}
