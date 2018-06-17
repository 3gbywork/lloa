using CommonUtility.Logging;
using GalaSoft.MvvmLight.Command;
using OfficeAutomationClient.Helper;
using OfficeAutomationClient.Model;
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
        public ICommand RebuildDatabaseCommand { get; set; }

        public object Content { get => _content; set { Set(ref _content, value); } }
        public List<object> StatusBarItems
        {
            get => _statusBarItems; set { Set(ref _statusBarItems, value); }
        }

        public InfoViewModel()
        {
            QueryOrganizationCommand = new RelayCommand(() =>
            {
                logger.Info("查询 组织结构 信息……");
                //var organizations = Business.Instance.GetOrganizations();
                //var org = organizations.Where(o => o.Type == OrganizationType.Dept).First();

                //var people = Business.Instance.GetPeople(org);
            });
            QueryMonthAttDataCommand = new RelayCommand(() =>
            {
                var date = DateTime.Now.ToString("yyyy-MM-dd");
                logger.Info($"查询 {date} 月考勤 记录……");
                Business.Instance.GetAttendance(date);
            });
            AboutCommand = new RelayCommand(() =>
            {
                logger.Info("关于……");
                ViewLocator.AboutWindow.ShowDialog();
            });
#if INIT_ORGANIZATION_DB
            RebuildDatabaseCommand = new RelayCommand(() =>
            {
                logger.Info("重建数据库……");
                Business.Instance.RebuildDatabase();
            });
#endif
        }
    }
}
