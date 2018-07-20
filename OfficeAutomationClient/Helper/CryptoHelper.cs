using System;
using CommonUtility.Extension;
using CommonUtility.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace OfficeAutomationClient.Helper
{
    internal class CryptoHelper
    {
        private static readonly ILogger Logger = LogHelper.GetLogger<CryptoHelper>();

        #region AES加密/解密

        private static byte[] AesCbc(bool forEncryption, byte[] input, byte[] key, byte[] iv)
        {
            var cipher = CipherUtilities.GetCipher("AES/CBC/ZEROBYTEPADDING");
            //var cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(new AesEngine()), new ZeroBytePadding());

            cipher.Init(forEncryption, new ParametersWithIV(new KeyParameter(key), iv));

            return cipher.DoFinal(input);
        }

        /// <summary>
        ///     AES加密（CbcBlockCipher/ZeroBytePadding）
        /// </summary>
        /// <param name="input">需要加密的内容</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">初始化向量</param>
        /// <returns>加密后的内容</returns>
        /// <example>
        ///     <code>
        /// var text = "hello world!";
        /// 
        /// var input = Encoding.UTF8.GetBytes(text);
        /// var key = Encoding.UTF8.GetBytes("1234567898765432");
        /// var iv = Encoding.UTF8.GetBytes("9876543212345678");
        /// 
        /// var output = AesEncrypt(input, key, iv);
        /// var encryptText = Convert.ToBase64String(output);
        /// </code>
        ///     The value of encryptText is; "+RX1IL56rR4afHXhbtP9dA=="
        /// </example>
        public static byte[] AesEncrypt(byte[] input, byte[] key, byte[] iv)
        {
            try
            {
                return AesCbc(true, input, key, iv);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "Aes 加密失败");
                return null;
            }
        }

        /// <summary>
        ///     AES解密（CbcBlockCipher/ZeroBytePadding）
        /// </summary>
        /// <param name="input">需要解密的内容</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">初始化向量</param>
        /// <returns>解密后的内容</returns>
        /// <example>
        ///     <code>
        /// string encryptText = "+RX1IL56rR4afHXhbtP9dA==";
        /// 
        /// var key = Encoding.UTF8.GetBytes("1234567898765432");
        /// var iv = Encoding.UTF8.GetBytes("9876543212345678");
        /// 
        /// var input = Convert.FromBase64String(encryptText);
        /// var output = AesDecrypt(input, key, iv);
        /// 
        /// var text = Encoding.UTF8.GetString(output);
        /// </code>
        ///     The value of text is: "hello world!";
        /// </example>
        public static byte[] AesDecrypt(byte[] input, byte[] key, byte[] iv)
        {
            try
            {
                return AesCbc(false, input, key, iv);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "Aes 解密失败");
                return null;
            }
        }

        #endregion

        #region RSA密钥生成/转换

        /// <summary>
        ///     生成RSA密钥对
        /// </summary>
        /// <param name="bits">密钥位数，可选值：1024/2048/4096</param>
        /// <returns></returns>
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

        /// <summary>
        ///     生成RSA密钥的Base64字符串
        /// </summary>
        /// <param name="key">RSA密钥</param>
        /// <returns>DerEncodedBase64String</returns>
        public static string GetRsaDerEncodedBase64String(AsymmetricKeyParameter key)
        {
            try
            {
                if (key.IsPrivate)
                    return PrivateKeyInfoFactory.CreatePrivateKeyInfo(key).GetDerEncoded().ToBase64String();
                return SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(key).GetDerEncoded().ToBase64String();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "生成 RsaDerEncodedBase64String 密钥失败，是否私钥：{0}", key.IsPrivate);
            }

            return null;
        }

        /// <summary>
        ///     从Base64字符串生成RSA密钥
        /// </summary>
        /// <param name="key">Base64字符串</param>
        /// <param name="isPrivate">是否私钥</param>
        /// <returns>RSA密钥</returns>
        public static AsymmetricKeyParameter GetAsymmetricKeyParameter(string key, bool isPrivate)
        {
            try
            {
                if (isPrivate)
                    return PrivateKeyFactory.CreateKey(key.FromBase64String());
                return PublicKeyFactory.CreateKey(key.FromBase64String());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "生成 AsymmetricKeyParameter 密钥失败，是否私钥：{0}", isPrivate);
            }

            return null;
        }

        private static byte[] RsaPkcs1(bool forEncryption, byte[] input, AsymmetricKeyParameter key)
        {
            var cipher = CipherUtilities.GetCipher("RSA/ECB/PKCS1");
            //var cipher = new BufferedAsymmetricBlockCipher(new Pkcs1Encoding(new RsaEngine()));
            cipher.Init(forEncryption, key);

            return cipher.DoFinal(input);
        }

        /// <summary>
        ///     RSA加密
        /// </summary>
        /// <param name="input">需要加密的内容</param>
        /// <param name="key">RSA密钥</param>
        /// <returns>加密后的内容</returns>
        /// <example>
        ///     <code>
        /// var text = "hello world!";
        /// 
        /// var input = Encoding.UTF8.GetBytes(text);
        /// var publickey = "MIGdMA0GCSqGSIb3DQEBAQUAA4GLADCBhwKBgQCEHmH9z6wKb2FvqsYLYi1y4nlR8fX3NH1fYFcsDN1G8Ck0MQzo+sx86D80yRLnw+U2lz5LudB6VDYStcvEpRIFFyeF03y8yoptLX8UyvhM7fXmXZRxVFe0B68QL4OyTCrQVkAc7OPfeeZ/boBERXfxfKUxU6OPQKujtIcSF8FftQIBAw==";
        /// var pubkey = GetAsymmetricKeyParameter(publickey, false);
        /// 
        /// var output = RsaEncrypt(input, pubkey);
        /// var encryptText = Convert.ToBase64String(output);
        /// </code>
        ///     The value of encryptText is;
        ///     "M522T50olKQ6ZBZArgqZGBgDslxw77ju9fcAUMhX+Nqc0eNieUPv0KuUqAvp5m6LYk4l2dS1WGpqbjYbmiOPLCNZpCIHmfCGzheKwWA08vOstdBXMATazcDPlvQjQoa4Yfhi9WwOmhziBx/LAan6SsMdrxRgF7/X+MmdSoYyyQ0="
        /// </example>
        public static byte[] RsaEncrypt(byte[] input, AsymmetricKeyParameter key)
        {
            try
            {
                return RsaPkcs1(true, input, key);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "Rsa 加密失败");
            }

            return null;
        }

        /// <summary>
        ///     RSA解密
        /// </summary>
        /// <param name="input">需要解密的内容</param>
        /// <param name="key">RSA密钥</param>
        /// <returns>解密后的内容</returns>
        /// <example>
        ///     <code>
        /// var base64Text = "M522T50olKQ6ZBZArgqZGBgDslxw77ju9fcAUMhX+Nqc0eNieUPv0KuUqAvp5m6LYk4l2dS1WGpqbjYbmiOPLCNZpCIHmfCGzheKwWA08vOstdBXMATazcDPlvQjQoa4Yfhi9WwOmhziBx/LAan6SsMdrxRgF7/X+MmdSoYyyQ0="
        /// 
        /// var input = Encoding.UTF8.GetBytes(base64Text);
        /// var privatekey = "MIICdAIBADANBgkqhkiG9w0BAQEFAASCAl4wggJaAgEAAoGBAIQeYf3PrApvYW+qxgtiLXLieVHx9fc0fV9gVywM3UbwKTQxDOj6zHzoPzTJEufD5TaXPku50HpUNhK1y8SlEgUXJ4XTfLzKim0tfxTK+Ezt9eZdlHFUV7QHrxAvg7JMKtBWQBzs49955n9ugERFd/F8pTFTo49Aq6O0hxIXwV+1AgEDAoGAFgUQVPfyAb065/HLrJBc6HsUOFL+U94U5Tq5MgIk4SgG3ggs0X8iFNFf3iGDJqCmM8Pft0mivw4JAx5MoMYtq0ZiZdYSOXttgKio0V7r576wsLLWgLTxcdMaOMQCeXbHIdZFP+zzz3fYHazBoMaXDHD50j0k3+xUBA+B5scdQ+0CQQDGNARnzvOVtwRjNeE8oCcGQZ2lZYsjDyZzWsuduiOfe9cEpuC/2qfBXD+WbgEG4BsuLJGAtGDs7+XgYQtfbFaXAkEAqqUeZ0BwUEJk1lRLVNC3b5Akhyfh85/iobbv+bq15AQuS/m8n1Ja6XmNy4h+lw2LqJ2yZBHuycOldxqeDaVxkwJBAIQirZqJ97kkrZd5QNMVb1mBE8OZB2y0xEznMmkmwmpSj1hvQH/nGoDoKmRJVgSVZ3QdtlXNlfNKmUBAsj+djw8CQHHDaZoq9YrW7eQ4MjiLJPUKwwTFQU0VQcEkn/vRzpgCyYf70xThkfD7s90FqboJB8W+du1hSdvXw6S8aV5uS7cCQAWZ8f5WREo0GsjWQ5cfquxjLMfctSoixuPfuEhdxSbb3pENtnRgNtgHSZHwZro5ADqoPHlYgTNnNH4S2L9QU3c=";
        /// var prikey = GetAsymmetricKeyParameter(privatekey, true);
        /// 
        /// var output = RsaDecrypt(input, prikey);
        /// var decryptText = Convert.ToBase64String(output);
        /// </code>
        ///     The value of decryptText is; "hello world!"
        /// </example>
        public static byte[] RsaDecrypt(byte[] input, AsymmetricKeyParameter key)
        {
            try
            {
                return RsaPkcs1(false, input, key);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, null, "Rsa 加密失败");
            }

            return null;
        }

        #endregion
    }
}