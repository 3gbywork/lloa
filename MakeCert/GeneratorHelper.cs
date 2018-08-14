using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using System;
using System.IO;
using System.Linq;

namespace MakeCert
{
    class GeneratorHelper
    {
        private const string PfxAliasName = "Pfx's Primary Certificate";

        public static SecureRandom SecureRandom = SecureRandom.GetInstance("SHA256PRNG");

        public static AsymmetricCipherKeyPair GenerateRsaKeyPair(int bits)
        {
            if (bits != 1024 && bits != 2048 && bits != 4096)
                throw new InvalidParameterException($"{nameof(bits)} must be 1024/2048/4096");

            var generator = new RsaKeyPairGenerator();
            generator.Init(new RsaKeyGenerationParameters(
                BigInteger.Three, //  BigInteger.ValueOf(65537),
                SecureRandom.GetInstance("SHA256PRNG"),
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

            return generator.Generate(new Asn1SignatureFactory("SHA512WITHRSA", privateKey, SecureRandom));
        }

        public static void SavePfx(string path, X509Certificate certificate, AsymmetricKeyParameter privateKey, char[] password)
        {
            var pkcs12Store = new Pkcs12StoreBuilder().Build();

            var certificateEntry = new X509CertificateEntry(certificate);
            pkcs12Store.SetCertificateEntry(PfxAliasName, certificateEntry);
            pkcs12Store.SetKeyEntry(PfxAliasName, new AsymmetricKeyEntry(privateKey), new X509CertificateEntry[] { certificateEntry });

            using (var fs = File.Create(path))
                pkcs12Store.Save(fs, password, SecureRandom);
        }

        public static void ExportPfx(string path, char[] password, string privKeyPath, string pubKeyPath, string certificatePath)
        {
            var pkcs12Store = new Pkcs12StoreBuilder().Build();

            using (var fs = File.OpenRead(path))
                pkcs12Store.Load(fs, password);

            var alias = pkcs12Store.Aliases.Cast<string>().FirstOrDefault();

            var certificateEntry = pkcs12Store.GetCertificate(alias);
            var keyEntry = pkcs12Store.GetKey(alias);

            if (null != keyEntry && null != keyEntry.Key && !string.IsNullOrEmpty(privKeyPath))
                PemWriterHelper.WriteObject(privKeyPath, keyEntry.Key);
            if (null != certificateEntry && null != certificateEntry.Certificate)
            {
                if (!string.IsNullOrEmpty(certificatePath))
                    PemWriterHelper.WriteObject(certificatePath, certificateEntry.Certificate);

                var pubKey = certificateEntry.Certificate.GetPublicKey();
                if (null != pubKey && !string.IsNullOrEmpty(pubKeyPath))
                    PemWriterHelper.WriteObject(pubKeyPath, pubKey);
            }
        }
    }
}
