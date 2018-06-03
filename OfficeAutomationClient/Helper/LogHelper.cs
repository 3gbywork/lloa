using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OfficeAutomationClient.Helper
{
    public class LogHelper
    {
        public static void Config()
        {

        }

        public static ILogger GetLogger<T>()
        {
            var type = typeof(T);
            return LogManager.GetLogger(type.FullName);
        }
    }
}
