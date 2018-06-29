using CommonUtility.Logging;

namespace OfficeAutomationClient.Helper
{
    public class LogHelper
    {
        private static readonly LogFactory LogFactory;

        static LogHelper()
        {
            LogFactory = new LogFactory();
            LogFactory.AddProvider(NLogProvider.Instance);
        }

        public static ILogger GetLogger<T>()
        {
            var type = typeof(T);
            return LogFactory.CreateLogger(type.FullName);
        }

        public static void Shutdown()
        {
            LogFactory?.Dispose();
        }
    }
}