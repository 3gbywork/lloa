using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using ValidateCodeProcessor;

namespace OfficeAutomationClient
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            NLog.LogManager.Shutdown();
            OcrProcessor.Instance.Dispose();

            base.OnExit(e);
        }
    }
}
