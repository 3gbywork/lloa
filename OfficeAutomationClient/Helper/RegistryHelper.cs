using CommonUtility.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OfficeAutomationClient.Helper
{
    class RegistryHelper
    {
        private static ILogger Logger = LogHelper.GetLogger<RegistryHelper>();

        public const string IE = @"SOFTWARE\Microsoft\Internet Explorer";
        public const string FEATURE_BROWSER_EMULATION = @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";

        internal static IEVersion GetIEVersion()
        {
            var version = string.Empty;
            using (var key = Registry.LocalMachine.OpenSubKey(IE))
            {
                try
                {
                    version = (string)key.GetValue("svcVersion");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, null, "读取IE最高版本失败");

                    try
                    {
                        version = (string)key.GetValue("Version");
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, null, "读取IE低版本失败");
                    }
                }
            }

            if (!string.IsNullOrEmpty(version))
            {
                if (version.StartsWith("11")) return IEVersion.V11;
                if (version.StartsWith("10")) return IEVersion.V10;
                if (version.StartsWith("9")) return IEVersion.V9;
                if (version.StartsWith("8")) return IEVersion.V8;
            }
            return IEVersion.None;
        }

        internal static void SetWebBrowserEmulation(string appName, IEVersion version)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(FEATURE_BROWSER_EMULATION, true))
            {
                try
                {
                    key.SetValue(appName, (int)version, RegistryValueKind.DWord);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, null, "设置应用 {0} 使用IE版本 {1} 时出错", appName, version);
                }
            }
        }
    }

    enum IEVersion
    {
        None,
        V11 = 11001,
        V10 = 10001,
        V9 = 9999,
        V8 = 8888,
    }
}
