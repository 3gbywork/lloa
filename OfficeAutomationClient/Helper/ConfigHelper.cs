using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace OfficeAutomationClient.Helper
{
    class ConfigHelper
    {
        private static ILogger logger = LogHelper.GetLogger<ConfigHelper>();

        private static Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        public static string GetString(string key)
        {
            try
            {
                return configuration.AppSettings.Settings[key].Value;
            }
            catch (Exception ex)
            {
                logger.Error($"获取配置异常 key:{key}, error:{ex}");
                return string.Empty;
            }
        }

        internal static bool GetBoolean(string key)
        {
            var value = GetString(key);
            if (string.IsNullOrEmpty(value)) return false;

            bool rst = false;
            bool.TryParse(value, out rst);
            return rst;
        }

        internal static void Save(string key, string value)
        {
            try
            {
                configuration.AppSettings.Settings.Remove(key);
                configuration.AppSettings.Settings.Add(key, value);
                configuration.Save();
            }
            catch (Exception ex)
            {
                logger.Error($"保存配置异常 key:{key}, value:{value}, error:{ex}");
            }
        }
    }

    public struct ConfigKey
    {
        public const string User = "user";
        public const string RememberPwd = "rememberpass";
        public const string AutoLogin = "autologin";
    }
}
