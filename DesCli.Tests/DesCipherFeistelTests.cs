using System;
using System.IO;
using Xunit;
using DesCli;

namespace DesCli.Tests
{
    public class DesCipherFeistelTests
    {
        [Fact]
        public void Feistel_IsDeterministic_And_DependsOnSubkey()
        {
            // Arrange - runtime sample.bin expected to be copied to output
            var path = Path.Combine(AppContext.BaseDirectory, "sample.bin");
            Assert.True(File.Exists(path), $"sample.bin not found at runtime path: {path}");

            var bytes = File.ReadAllBytes(path);
            Assert.True(bytes.Length >= 8, "sample.bin must contain at least one 8-byte block for this test.");

            // take first 8-byte block and use right half (bytes 4..7)
            var right = new byte[4];
            Array.Copy(bytes, 4, right, 0, 4);

            var cipher = new DesCipher();

            var subkeyAllZero = new byte[6]; // 48-bit zero key
            var subkeyAllOnes = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

            // Act
            var out1 = cipher.Feistel(right, subkeyAllZero);
            var out1Again = cipher.Feistel(right, subkeyAllZero);
            var out2 = cipher.Feistel(right, subkeyAllOnes);

            // Assert
            Assert.Equal(4, out1.Length);
            Assert.Equal(out1, out1Again); // deterministic
            Assert.Equal(4, out2.Length);
            Assert.NotEqual(out1, out2); // different subkeys should change output (sanity check)
        }

        [Fact]
        public void Feistel_InvalidLengths_Throw()
        {
            var cipher = new DesCipher();
            Assert.Throws<ArgumentException>(() => cipher.Feistel(new byte[3], new byte[6])); // right wrong length
            Assert.Throws<ArgumentException>(() => cipher.Feistel(new byte[4], new byte[5])); // subkey wrong length
            Assert.Throws<ArgumentNullException>(() => cipher.Feistel(null!, new byte[6]));
            Assert.Throws<ArgumentNullException>(() => cipher.Feistel(new byte[4], null!));
        }
    }
}
