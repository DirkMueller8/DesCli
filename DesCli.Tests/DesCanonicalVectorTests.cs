using System;
using System.IO;
using System.Security.Cryptography;
using Xunit;
using DesCli;

namespace DesCli.Tests
{
    public class DesCanonicalVectorTests
    {
        [Fact]
        public void Encrypt_FirstBlock_MatchesStandardDesForSingleBlock()
        {
            var cipher = new DesCipher();

            // canonical example:
            // plaintext: 0x0123456789ABCDEF
            // key:       0x133457799BBCDFF1
            // expected ciphertext (single-block) per standard DES: 0x85E813540F0AB405
            var plaintext = Convert.FromHexString("0123456789ABCDEF");
            var key = Convert.FromHexString("133457799BBCDFF1");
            var expectedBlock = Convert.FromHexString("85E813540F0AB405");

            // Use .NET DES to confirm expectedBlock (optional sanity)
            using (var des = DES.Create())
            {
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.None;
                des.Key = key;
                var expectedFromDotNet = des.CreateEncryptor().TransformFinalBlock(plaintext, 0, plaintext.Length);
                Assert.Equal(expectedBlock, expectedFromDotNet);
            }

            // Your Encrypt uses PKCS#7 and will produce 16 bytes for an 8-byte input.
            var actual = cipher.Encrypt(plaintext, key);
            Assert.True(actual.Length >= 8, "ciphertext must contain at least one block");
            var actualFirstBlock = new byte[8];
            Array.Copy(actual, 0, actualFirstBlock, 0, 8);

            Assert.Equal(expectedBlock, actualFirstBlock);
        }
    }
}