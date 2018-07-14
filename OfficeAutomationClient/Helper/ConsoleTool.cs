using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace OfficeAutomationClient.Helper
{
    internal class ConsoleTool
    {
        public int Run()
        {
            return Run(DefaultAction);
        }

        public string RunAndGetOutput()
        {
            var builder = new StringBuilder();
            Run(msg => builder.AppendLine(msg));
            return builder.ToString();
        }

        public int Run(Action<string> onDataReceived)
        {
            try
            {
                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = FileName,
                        Arguments = Arguments,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        StandardErrorEncoding = this.Encoding ?? DefaultEncoding,
                        StandardOutputEncoding = this.Encoding ?? DefaultEncoding
                    }
                };

                process.Start();

                if (null != onDataReceived)
                {
                    string line;
                    while (null != (line = process.StandardOutput.ReadLine()))
                    {
                        onDataReceived(line);
                    }

                    var error = process.StandardError.ReadLine();
                    if (!string.IsNullOrEmpty(error))
                        onDataReceived(error);
                }

                process.WaitForExit();
                return process.ExitCode;
            }
            catch (Exception e)
            {
                onDataReceived?.Invoke(e.Message);
                return Marshal.GetHRForException(e);
            }
        }

        public Action<string> DefaultAction = Console.WriteLine;

        public Encoding Encoding { get; set; }

        public Encoding DefaultEncoding = Encoding.Default;

        public string Arguments { get; set; }

        public string FileName { get; set; }
    }
}
