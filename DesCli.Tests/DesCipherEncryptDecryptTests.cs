using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Xunit;
using DesCli;

namespace DesCli.Tests
{
    public class DesCipherEncryptDecryptTests
    {
        [Fact]
        public void EncryptThenDecrypt_RoundTrips_VariousLengths()
        {
            var cipher = new DesCipher();
            var key = Convert.FromHexString("0123456789ABCDEF"); // canonical 8-byte key

            // sample inputs (including the sample.bin content)
            var samplePath = Path.Combine(AppContext.BaseDirectory, "sample.bin");
            Assert.True(File.Exists(samplePath), $"Test data missing: {samplePath}");
            var sample = File.ReadAllBytes(samplePath);

            var cases = new[]
            {
                Encoding.UTF8.GetBytes(""),                // empty -> padded block
                Encoding.UTF8.GetBytes("OpenAI"),          // short
                Encoding.UTF8.GetBytes("Hello World!"),    // >8, non-multiple
                sample                                      // real test file
            };

            foreach (var plaintext in cases)
            {
                var ct = cipher.Encrypt(plaintext, key);
                Assert.NotNull(ct);
                Assert.True(ct.Length % 8 == 0 && ct.Length > 0);

                var pt = cipher.Decrypt(ct, key);
                Assert.Equal(plaintext, pt);
            }
        }

        [Fact]
        public void Encrypt_MatchesDotNetDES_Pkcs7_Ecb()
        {
            var cipher = new DesCipher();
            var key = Convert.FromHexString("0123456789ABCDEF");

            var plaintext = Encoding.UTF8.GetBytes("CompareWithDotNet"); // arbitrary length

            // expected using System DES ECB PKCS7
            using var des = DES.Create();
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;
            des.Key = key;

            var expected = des.CreateEncryptor().TransformFinalBlock(plaintext, 0, plaintext.Length);

            var actual = cipher.Encrypt(plaintext, key);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EncryptDecrypt_InvalidArguments_Throw()
        {
            var cipher = new DesCipher();
            var key = new byte[8];

            Assert.Throws<ArgumentNullException>(() => cipher.Encrypt(null!, key));
            Assert.Throws<ArgumentNullException>(() => cipher.Encrypt(Array.Empty<byte>(), null!));
            Assert.Throws<ArgumentException>(() => cipher.Encrypt(Array.Empty<byte>(), new byte[7])); // wrong key length

            Assert.Throws<ArgumentNullException>(() => cipher.Decrypt(null!, key));
            Assert.Throws<ArgumentNullException>(() => cipher.Decrypt(new byte[8], null!));
            Assert.Throws<ArgumentException>(() => cipher.Decrypt(new byte[3], key)); // not multiple of 8
        }
    }
}