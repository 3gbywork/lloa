using CommonUtility.Logging;
using GalaSoft.MvvmLight.Command;
using OfficeAutomationClient.Helper;
using OfficeAutomationClient.OA;
using OfficeAutomationClient.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace OfficeAutomationClient.ViewModel
{
    public class InfoViewModel : ViewModelBase
    {
        private static ILogger logger = LogHelper.GetLogger<InfoViewModel>();

        private object _content;
        private List<object> _statusBarItems;

        public string Title => Business.Instance.GetTitle();

        public ICommand QueryOrganizationCommand { get; set; }
        public ICommand QueryMonthAttDataCommand { get; set; }
        public ICommand AboutCommand { get; set; }

        public object Content { get => _content; set { Set(ref _content, value); } }
        public List<object> StatusBarItems
        {
            get => _statusBarItems; set { Set(ref _statusBarItems, value); }
        }

        public InfoViewModel()
        {
            QueryOrganizationCommand = new RelayCommand(() =>
            {
                Business.Instance.GetOrganization();
            });
            QueryMonthAttDataCommand = new RelayCommand(() =>
            {
                Business.Instance.GetAttendance(DateTime.Now.ToString("yyyy-MM-dd"));
            });
            AboutCommand = new RelayCommand(() =>
            {
                ViewLocator.AboutWindow.ShowDialog();
            });
        }
    }
}
