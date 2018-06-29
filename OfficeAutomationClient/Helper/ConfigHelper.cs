using CommonUtility.Config;

namespace OfficeAutomationClient.Helper
{
    internal class ConfigHelper
    {
        internal static T Get<T>(string key)
        {
            return Configurator.GetConfiguration(key, default(T));
        }

        internal static void Save(string key, string value)
        {
            Configurator.SetConfiguration(key, value);
        }
    }

    public struct ConfigKey
    {
        public const string User = "user";
        public const string RememberPwd = "rememberpass";
        public const string AutoLogin = "autologin";
    }
}