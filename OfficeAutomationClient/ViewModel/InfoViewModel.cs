using System.Collections.Generic;
using System.Windows.Input;
using CommonUtility.Logging;
using GalaSoft.MvvmLight.Command;
using OfficeAutomationClient.Helper;
using OfficeAutomationClient.OA;
using OfficeAutomationClient.View;

namespace OfficeAutomationClient.ViewModel
{
    public class InfoViewModel : ViewModelBase
    {
        private static readonly ILogger logger = LogHelper.GetLogger<InfoViewModel>();

        private object _content;
        private List<object> _statusBarItems;

        public InfoViewModel()
        {
            QueryOrganizationCommand = new RelayCommand(() =>
            {
                logger.Info("查询 组织结构 信息……");
                Content = ViewLocator.QueryControl;
            });
            QueryMonthAttDataCommand = new RelayCommand(() =>
            {
                logger.Info("查询 月考勤 信息……");
                Content = ViewLocator.AttendanceInfo;
            });
            AboutCommand = new RelayCommand(() =>
            {
                logger.Info("关于……");
                ViewLocator.AboutWindow.ShowDialog();
            });
            AccountLoginCommand = new RelayCommand(() =>
            {
                logger.Info("登录");
                ViewLocator.LoginWindow.ShowDialog();
            });
            AccountLogoutCommand = new RelayCommand(() =>
            {
                logger.Info("登出");
                Business.Instance.Logout();
            });
#if INIT_ORGANIZATION_DB
            RebuildDatabaseCommand = new RelayCommand(() =>
            {
                logger.Info("重建数据库……");
                Business.Instance.RebuildDatabase();
            });
#endif
        }

        public string Title => Business.Instance.Title;

        public ICommand QueryOrganizationCommand { get; set; }
        public ICommand QueryMonthAttDataCommand { get; set; }
        public ICommand AboutCommand { get; set; }
        public ICommand RebuildDatabaseCommand { get; set; }
        public ICommand AccountLoginCommand { get; set; }
        public ICommand AccountLogoutCommand { get; set; }

        public object Content
        {
            get => _content;
            set => Set(ref _content, value);
        }

        public List<object> StatusBarItems
        {
            get => _statusBarItems;
            set => Set(ref _statusBarItems, value);
        }
    }
}