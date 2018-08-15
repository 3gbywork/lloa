using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using System;
using System.IO;
using System.Linq;

namespace MakeCert
{
    class BouncyCastleHelper
    {
        private const string PfxAliasName = "BadHex Root Certificate Authority";

        public static SecureRandom SecureRandom = SecureRandom.GetInstance("SHA256PRNG");

        public static AsymmetricCipherKeyPair GenerateRsaKeyPair(int bits)
        {
            if (bits != 1024 && bits != 2048 && bits != 4096)
                throw new InvalidParameterException($"{nameof(bits)} must be 1024/2048/4096");

            var generator = new RsaKeyPairGenerator();
            generator.Init(new RsaKeyGenerationParameters(
                BigInteger.Three, //  BigInteger.ValueOf(65537),
                SecureRandom,
                bits,
                128
            ));

            return generator.GenerateKeyPair();
        }

        public static X509Certificate GenerateX509Certificate(AsymmetricKeyParameter privateKey, AsymmetricKeyParameter publicKey, string dirName, DateTime startDate, int days)
        {
            var generator = new X509V3CertificateGenerator();
            generator.SetSerialNumber(BigIntegers.CreateRandomInRange(BigInteger.Zero, BigInteger.ValueOf(long.MaxValue), SecureRandom));

            var dn = new X509Name(dirName);
            generator.SetIssuerDN(dn);
            generator.SetSubjectDN(dn);

            generator.SetNotBefore(startDate);
            generator.SetNotAfter(startDate.AddDays(days));

            generator.SetPublicKey(publicKey);

            return generator.Generate(new Asn1SignatureFactory("SHA256WITHRSA", privateKey, SecureRandom));
        }

        public static void SavePfx(string path, X509Certificate certificate, AsymmetricKeyParameter privateKey, char[] password)
        {
            var pkcs12Store = new Pkcs12StoreBuilder().Build();

            var certificateEntry = new X509CertificateEntry(certificate);
            pkcs12Store.SetCertificateEntry(PfxAliasName, certificateEntry);
            pkcs12Store.SetKeyEntry(PfxAliasName, new AsymmetricKeyEntry(privateKey), new X509CertificateEntry[] { certificateEntry });

            CreateDirIfNotExists(path);

            using (var fs = File.Create(path))
                pkcs12Store.Save(fs, password, SecureRandom);
        }

        public static void ExportPfx(string path, char[] password, string privKeyPath, string pubKeyPath, string certificatePath)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

            var pkcs12Store = new Pkcs12StoreBuilder().Build();

            using (var fs = File.OpenRead(path))
                pkcs12Store.Load(fs, password);

            var alias = pkcs12Store.Aliases.Cast<string>().FirstOrDefault();

            var certificateEntry = pkcs12Store.GetCertificate(alias);
            var keyEntry = pkcs12Store.GetKey(alias);

            if (null != keyEntry)
                WriteObject(privKeyPath, keyEntry.Key);
            if (null != certificateEntry && null != certificateEntry.Certificate)
            {
                WriteObject(certificatePath, certificateEntry.Certificate);

                var pubKey = certificateEntry.Certificate.GetPublicKey();
                WriteObject(pubKeyPath, pubKey);
            }
        }

        public static void ExportCrt(string path, string pubKeyPath)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

            using (var fs = File.OpenRead(path))
            {
                var parser = new X509CertificateParser();
                var certificate = parser.ReadCertificate(fs);

                var pubKey = certificate.GetPublicKey();
                WriteObject(pubKeyPath, pubKey);
            }
        }

        public static void WriteObject(string path, object obj)
        {
            if (string.IsNullOrEmpty(path) || null == obj) return;

            CreateDirIfNotExists(path);

            using (var sw = new StreamWriter(path))
            {
                var pemWriter = new PemWriter(sw);
                pemWriter.WriteObject(obj);
            }
        }

        private static void CreateDirIfNotExists(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }
}
