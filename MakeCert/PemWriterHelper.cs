using Org.BouncyCastle.OpenSsl;
using System.IO;

namespace MakeCert
{
    class PemWriterHelper
    {
        public static void WriteObject(string path, object obj)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using (var sw = new StreamWriter(path))
            {
                var pemWriter = new PemWriter(sw);
                pemWriter.WriteObject(obj);
            }
        }
    }
}
