using CommonUtility.Logging;

namespace OfficeAutomationClient.Helper
{
    public class LogHelper
    {
        private static LogFactory logFactory;

        static LogHelper()
        {
            logFactory = new LogFactory();
            logFactory.AddProvider(NLogProvider.Instance);
        }

        public static ILogger GetLogger<T>()
        {
            var type = typeof(T);
            return logFactory.CreateLogger(type.FullName);
        }
    }
}
