using System;
using System.IO;
using Xunit;
using DesCli;

namespace DesCli.Tests
{
    public class CliHandlerTests
    {
        [Fact]
        public void Handle_EncryptThenDecrypt_ProducesOriginalFile()
        {
            // Arrange
            var samplePath = Path.Combine(AppContext.BaseDirectory, "sample.bin");
            Assert.True(File.Exists(samplePath), $"Test data missing: {samplePath}");
            var original = File.ReadAllBytes(samplePath);

            var encryptedPath = Path.Combine(Path.GetTempPath(), "DesCli_CliHandler_" + Guid.NewGuid().ToString("N") + ".enc");
            var decryptedPath = Path.Combine(Path.GetTempPath(), "DesCli_CliHandler_" + Guid.NewGuid().ToString("N") + ".dec");

            var keyHex = "0123456789ABCDEF"; // 8-byte key in hex

            var handler = new CliHandler();

            try
            {
                // Act - encrypt via CLI handler
                var encryptArgs = new[] { "encrypt", "-i", samplePath, "-o", encryptedPath, "-k", keyHex };
                handler.Handle(encryptArgs);

                // Assert encrypted file created
                Assert.True(File.Exists(encryptedPath), "Encrypted output file was not created.");
                var encryptedBytes = File.ReadAllBytes(encryptedPath);
                Assert.True(encryptedBytes.Length % 8 == 0 && encryptedBytes.Length > 0);

                // Act - decrypt via CLI handler
                var decryptArgs = new[] { "decrypt", "-i", encryptedPath, "-o", decryptedPath, "-k", keyHex };
                handler.Handle(decryptArgs);

                // Assert decrypted file created and matches original
                Assert.True(File.Exists(decryptedPath), "Decrypted output file was not created.");
                var decrypted = File.ReadAllBytes(decryptedPath);
                Assert.Equal(original, decrypted);
            }
            finally
            {
                try { if (File.Exists(encryptedPath)) File.Delete(encryptedPath); } catch { }
                try { if (File.Exists(decryptedPath)) File.Delete(decryptedPath); } catch { }
            }
        }
    }
}