using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MakeCert
{
    class CommandLineHelper
    {
        #region 使用openssl生成rsa密钥对
        // openssl req -newkey rsa:2048 -nodes -keyout private.key -x509 -days 365 -out OfficeAutomationClient.crt
        // openssl pkcs12 -export -inkey private.key -in OfficeAutomationClient.crt -out OfficeAutomationClient.pfx -nodes
        #endregion

        private static readonly string CmdName = Assembly.GetExecutingAssembly().GetName().Name;

        private static readonly List<string> GenerateOptions = new List<string> { "-newkey", "-config", "-days", "-password", "-basedir", "-outprivkey", "-outpubkey", "-outcrt", "-outpfx" };
        private static readonly List<string> ExportOptions = new List<string> { "-password", "-basedir", "-outprivkey", "-outpubkey", "-outcrt" };
        private static readonly List<string> VerbKeyWords = new List<string> { "generate", "export" };

        public static CommandLineOption Parse(string[] args)
        {
            var option = new CommandLineOption();

            string verb = null;
            if (args.Length == 0 || (args.Length > 0 && !VerbKeyWords.Contains((verb = args[0].Trim().ToLower()))))
            {
                option.Verb = CmdVerb.ShowHelp;
                return option;
            }

            List<string> options;
            string[] parameters;
            if ("generate".Equals(verb))
            {
                option.Verb = CmdVerb.Generate;
                options = GenerateOptions;
                parameters = args.Skip(1).ToArray();
            }
            else if ("export".Equals(verb))
            {
                string pfxPath = null;
                if (args.Length == 1 || (args.Length > 1 && string.IsNullOrWhiteSpace((pfxPath = args[1].Trim()))))
                {
                    PrintLine("{0} parameter is required", "pfx-file");
                    option.Verb = CmdVerb.ParseError;
                    return option;
                }
                if (!File.Exists(pfxPath))
                {
                    PrintLine("pfx file:{0} not found", pfxPath);
                    option.Verb = CmdVerb.ParseError;
                    return option;
                }

                option.InPfxPath = pfxPath;
                option.Verb = CmdVerb.Export;
                options = ExportOptions;
                parameters = args.Skip(2).ToArray();
            }
            else
            {
                option.Verb = CmdVerb.ShowHelp;
                return option;
            }

            var tmpOpt = string.Empty;

            foreach (var parameter in parameters)
            {
                var optVal = parameter.Trim().ToLower();
                if (string.IsNullOrWhiteSpace(optVal)) continue;

                if (optVal.StartsWith("-"))
                {
                    if (options.Contains(optVal))
                    {
                        if (!string.IsNullOrEmpty(tmpOpt))
                            InternalParse(option, tmpOpt, null);

                        tmpOpt = optVal;
                    }
                    else
                        PrintLine("has ignored an unrecognized instruction:", optVal);
                }
                else
                {
                    if (!string.IsNullOrEmpty(tmpOpt))
                    {
                        InternalParse(option, tmpOpt, optVal);

                        tmpOpt = string.Empty;
                    }
                    else
                        PrintLine("has ignored a parameter:", optVal);
                }
            }

            if (!string.IsNullOrEmpty(tmpOpt))
                Intern­alParse(option, tmpOpt, null);

            if (option.Verb == CmdVerb.Generate)
            {
                if (option.Algorithm == null)
                    InternalParse(option, "-newkey", null);
                if (string.IsNullOrEmpty(option.DistinguishedNames))
                    InternalParse(option, "-config", null);
                if (new List<string> { option.OutCrtPath, option.OutPfxPath, option.OutPrivateKeyPath, option.OutPublicKeyPath }.All(string.IsNullOrEmpty))
                {
                    InternalParse(option, "-outpfx", null);
                }
            }
            else if (option.Verb == CmdVerb.Export)
            {
                if (new List<string> { option.OutCrtPath, option.OutPrivateKeyPath, option.OutPublicKeyPath }.All(string.IsNullOrEmpty))
                {
                    InternalParse(option, "-outprivkey", null);
                    InternalParse(option, "-outpubkey", null);
                    InternalParse(option, "-outcrt", null);
                }
            }

            return option;
        }

        private static void InternalParse­(CommandLineOption option, string opt, string val­)
        {
            if (string.IsNullOrEmpty(opt)) return;

            switch (opt)
            {
                case "-newkey":
                    option.Algorithm = ParseAlgorithm(val);
                    break;
                case "-config":
                    option.DistinguishedNames = DistinguishedNameHelper.Parse(val);
                    break;
                case "-days":
                    option.Days = (!string.IsNullOrWhiteSpace(val) && int.TryParse(val.Trim(), out int days)) ? days : 30;
                    break;
                case "-password":
                    option.Password = string.IsNullOrEmpty(val) ? null : val.ToCharArray();
                    break;
                case "-basedir":
                    option.BaseDir = string.IsNullOrWhiteSpace(val) ? null : val.Trim();
                    break;
                case "-outprivkey":
                    option.OutPrivateKeyPath = string.IsNullOrWhiteSpace(val) ? "private.key" : val.Trim();
                    break;
                case "-outpubkey":
                    option.OutPublicKeyPath = string.IsNullOrWhiteSpace(val) ? "public.key" : val.Trim();
                    break;
                case "-outcrt":
                    option.OutCrtPath = string.IsNullOrWhiteSpace(val) ? "certificate.crt" : val.Trim();
                    break;
                case "-outpfx":
                    option.OutPfxPath = string.IsNullOrWhiteSpace(val) ? "certificate.pfx" : val.Trim();
                    break;
            }
        }

        private static Algorithm ParseAlgorithm(string val)
        {
            var alg = new Algorithm
            {
                Name = AlgorithmName.RSA,
                KeySize = 2048,
            };

            if (!string.IsNullOrEmpty(val))
            {
                var parameters = val.Split(':');
                // only support rsa
                //var name = parameters[0].Trim().ToLower();
                //if (name.Equals("rsa"))
                //    alg.Name = AlgorithmName.RSA;
                if (parameters.Length > 1)
                {
                    if (int.TryParse(parameters[1].Trim(), out int bits))
                    {
                        if (bits != 1024 || bits != 2048 || bits != 4096)
                            PrintLine("rsa key size:{0} invalid, optional values are 1024, 2048, 4096", bits);
                        else
                            alg.KeySize = bits;
                    }
                }
            }

            return alg;
        }

        private static void PrintLine(string format, object arg0)
        {
            Console.WriteLine(format, arg0);
        }

        public static string HelpInfo()
        {
            return $@"{CmdName} generate [options]
where options are
  -newkey       rsa:[1024|2048|4096]                                            (default:[rsa:2048])
  -config       distinguished names contained in the configration file          (default:[C=CN])
  -days         number of days a certificate generated by x509 is valid for     (default:[30])
  -password     set password                                                    (default:[])
  -basedir      output base dir
  -outprivkey   output private key file - PEM format                            (default:[private.key])
  -outpubkey    output public key file - PEM format                             (default:[public.key])
  -outcrt       certificate file - PEM format                                   (default:[certificate.crt])
  -outpfx       personal information exchange file - DER format                 (default:[certificate.pfx])
if -out* is not specified, -outpfx is default option

{CmdName} export pfx-file [options]
where options are
  pfx-file      personal information exchange file - DER format
  -password     password for pfx
  -basedir      output base dir
  -outprivkey   output private key file - PEM format                            (default:[private.key])
  -outpubkey    output public key file - PEM format                             (default:[public.key])
  -outcrt       certificate file - PEM format                                   (default:[certificate.crt])
if -out* is not specified, all -out* options will take effect";
        }
    }

    internal class CommandLineOption
    {
        #region Output Path
        private string _pfxPath;
        private string _crtPath;
        private string _publicKeyPath;
        private string _privateKeyPath;

        public string OutPrivateKeyPath { get => GetPath(_privateKeyPath); set => _privateKeyPath = value; }
        public string OutPublicKeyPath { get => GetPath(_publicKeyPath); set => _publicKeyPath = value; }
        public string OutCrtPath { get => GetPath(_crtPath); set => _crtPath = value; }
        public string OutPfxPath { get => GetPath(_pfxPath); set => _pfxPath = value; }
        #endregion

        #region Input Path
        public string InPfxPath { get; set; }
        #endregion

        public Algorithm Algorithm { get; set; }
        public string DistinguishedNames { get; set; }
        public int Days { get; set; } = 30;
        public char[] Password { get; set; }

        public CmdVerb Verb { get; set; }
        public string BaseDir { get; set; }

        private string GetPath(string path)
        {
            if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(BaseDir))
                return Path.Combine(BaseDir, path);
            return path;
        }
    }

    internal class Algorithm
    {
        public AlgorithmName Name { get; internal set; }
        public int KeySize { get; internal set; }
    }

    internal enum AlgorithmName
    {
        RSA
    }

    internal enum CmdVerb
    {
        ShowHelp,
        Generate,
        Export,
        ParseError
    }
}
