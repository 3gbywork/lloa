using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MakeCert
{
    class DistinguishedNameHelper
    {
        private static readonly List<string> DistinguishedNames = new List<string>() { "C", "O", "OU", "T", "CN", "SN", "STREET", "SERIALNUMBER", "L", "ST" };
        public static string DefaultDistinguishedNameString = "C=CN";

        public static string Parse(string configFile)
        {
            var dic = InternalParse(configFile);

            var builder = new StringBuilder();
            foreach (var item in dic)
            {
                builder.AppendFormat("{0}={1},", item.Key, item.Value);
            }
            builder.Remove(builder.Length - 1, 1);

            return builder.ToString();
        }

        private static Dictionary<string, string> InternalParse(string configFile)
        {
            var dic = new Dictionary<string, string>()
            {
                {"C", "CN" }
            };

            if (!string.IsNullOrEmpty(configFile) && File.Exists(configFile))
            {
                using (var reader = new StreamReader(configFile))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();

                        if (string.IsNullOrWhiteSpace(line))
                        {
                            // 空白行忽略
                        }
                        else if (line.StartsWith("//"))
                        {
                            // 注释忽略
                        }
                        else
                        {
                            // 配置项
                            var keyValue = line.Split('=');
                            if (keyValue.Length == 2)
                            {
                                var key = keyValue[0].Trim().ToUpper();
                                var value = keyValue[1].Trim();
                                if (DistinguishedNames.Contains(key) && !string.IsNullOrEmpty(value))
                                    dic[key] = value;
                            }
                        }
                    }
                }
            }

            return dic;
        }
    }
}
