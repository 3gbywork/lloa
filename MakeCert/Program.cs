using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MakeCert
{
    class Program
    {
        static void Main(string[] args)
        {
            var option = CommandLineHelper.Parse(args);

            if (option.Verb == CmdVerb.ShowHelp)
            {
                Console.WriteLine(CommandLineHelper.HelpInfo());
                return;
            }
            if (option.Verb == CmdVerb.Generate)
            {
                if (option.Algorithm.Name == AlgorithmName.RSA)
                {
                    var pair = BouncyCastleHelper.GenerateRsaKeyPair(option.Algorithm.KeySize);
                    var privKey = pair.Private;
                    var pubKey = pair.Public;

                    X509Certificate certificate = null;
                    if (new List<string> { option.OutCrtPath, option.OutPfxPath }.Any(p => !string.IsNullOrEmpty(p)))
                        certificate = BouncyCastleHelper.GenerateX509Certificate(privKey, pubKey, option.DistinguishedNames, DateTime.UtcNow, option.Days);

                    if (!string.IsNullOrEmpty(option.OutPrivateKeyPath))
                        BouncyCastleHelper.WriteObject(option.OutPrivateKeyPath, privKey);
                    if (!string.IsNullOrEmpty(option.OutPublicKeyPath))
                        BouncyCastleHelper.WriteObject(option.OutPublicKeyPath, pubKey);
                    if (!string.IsNullOrEmpty(option.OutCrtPath))
                        BouncyCastleHelper.WriteObject(option.OutCrtPath, certificate);
                    if (!string.IsNullOrEmpty(option.OutPfxPath))
                        BouncyCastleHelper.SavePfx(option.OutPfxPath, certificate, privKey, option.Password);
                }
            }
            else if (option.Verb == CmdVerb.Export)
            {
                if (!string.IsNullOrEmpty(option.InPfxPath))
                    BouncyCastleHelper.ExportPfx(option.InPfxPath, option.Password, option.OutPrivateKeyPath, option.OutPublicKeyPath, option.OutCrtPath);
                else if (!string.IsNullOrEmpty(option.InCrtPath))
                    BouncyCastleHelper.ExportCrt(option.InCrtPath, option.OutPublicKeyPath);
            }
        }
    }
}
