using System;
using System.Collections.Generic;
using CommonUtility.Logging;
using Microsoft.Win32;

namespace OfficeAutomationClient.Helper
{
    internal class RegistryHelper
    {
        public const string IE = @"SOFTWARE\Microsoft\Internet Explorer";

        public const string FEATURE_BROWSER_EMULATION =
            @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";

        private static readonly ILogger Logger = LogHelper.GetLogger<RegistryHelper>();

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

        private static string GetVersionFromRegistry()
        {
            // Opens the registry key for the .NET Framework entry.
            try
            {
                using (RegistryKey ndpKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "").OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
                {
                    var installedVersions = new List<string>();
                    // As an alternative, if you know the computers you will query are running .NET Framework 4.5 
                    // or later, you can use:
                    // using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, 
                    // RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
                    foreach (string versionKeyName in ndpKey.GetSubKeyNames())
                    {
                        if (versionKeyName.StartsWith("v"))
                        {
                            RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                            string name = (string)versionKey.GetValue("Version", "");
                            string sp = versionKey.GetValue("SP", "").ToString();
                            string install = versionKey.GetValue("Install", "").ToString();

                            if (sp != "" && install == "1" && name != "")
                            {
                                installedVersions.Add($"{name} SP{sp}");
                            }
                            if (name != "")
                            {
                                continue;
                            }
                            foreach (string subKeyName in versionKey.GetSubKeyNames())
                            {
                                RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                                name = (string)subKey.GetValue("Version", "");
                                if (name != "")
                                    sp = subKey.GetValue("SP", "").ToString();
                                install = subKey.GetValue("Install", "").ToString();

                                if (install == "1")
                                {
                                    installedVersions.Add($"{name}{(sp != "" ? $" SP{sp}" : "")}");
                                }
                            }
                        }
                    }
                    if (installedVersions.Count > 0)
                    {
                        installedVersions.Sort((a, b) => b.CompareTo(a));
                        return installedVersions[0];
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "获取 dotNetFramework 1-4 版本信息出错");
            }
            return Environment.Version.ToString();
        }

        internal static string GetDotNetFrameworkVersion()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

            try
            {
                using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
                {
                    if (ndpKey != null && ndpKey.GetValue("Release") != null)
                    {
                        return CheckFor45PlusVersion((int)ndpKey.GetValue("Release"));
                    }
                    else
                    {
                        return GetVersionFromRegistry();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "获取 dotNetFramework 4.5以上 版本信息出错");
            }

            return Environment.Version.ToString();
        }

        // Checking the version using >= will enable forward compatibility.
        private static string CheckFor45PlusVersion(int releaseKey)
        {
            if (releaseKey >= 461808)
                return "4.7.2 or later";
            if (releaseKey >= 461308)
                return "4.7.1";
            if (releaseKey >= 460798)
                return "4.7";
            if (releaseKey >= 394802)
                return "4.6.2";
            if (releaseKey >= 394254)
                return "4.6.1";
            if (releaseKey >= 393295)
                return "4.6";
            if (releaseKey >= 379893)
                return "4.5.2";
            if (releaseKey >= 378675)
                return "4.5.1";
            if (releaseKey >= 378389)
                return "4.5";
            // This code should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            return "No 4.5 or later version detected";
        }
    }

    internal enum IEVersion
    {
        None,
        V11 = 11001,
        V10 = 10001,
        V9 = 9999,
        V8 = 8888
    }
}