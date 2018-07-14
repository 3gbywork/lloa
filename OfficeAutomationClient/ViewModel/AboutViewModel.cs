using OfficeAutomationClient.Helper;
using OfficeAutomationClient.OA;
using System;
using System.Globalization;
using System.Text;

namespace OfficeAutomationClient.ViewModel
{
    public class AboutViewModel : ViewModelBase
    {
        public string Title => $"关于 {Business.AssemblyName.Name}";
        public string Info => BuildAboutInfo();

        private static string BuildAboutInfo()
        {
            var builder = new StringBuilder().AppendLine().AppendLine();

            builder.AppendLine(Business.AssemblyName.Name);
            builder.AppendLine($"版本：{Business.AssemblyName.Version}");
            builder.AppendLine($"Microsoft .NET Framework 版本：{RegistryHelper.GetDotNetFrameworkVersion()}");
            builder.AppendLine("© 2018").AppendLine();

            builder.AppendLine($"用户：{Environment.UserName}");
            builder.AppendLine($"计算机名：{Environment.MachineName}");
            builder.AppendLine(
                $"操作系统：{new ConsoleTool { FileName = "cmd.exe", Arguments = "/c ver" }.RunAndGetOutput().Trim()}");
            builder.AppendLine($"时区：{TimeZoneInfo.Local.DisplayName}");
            builder.AppendLine($"区域：{CultureInfo.CurrentCulture.DisplayName}");

            return builder.ToString();
        }
    }
}