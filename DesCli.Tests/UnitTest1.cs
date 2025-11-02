using DesCli;
using System;
using System.IO;
using System.Text;
using Xunit;

namespace DesCli.Tests
{
    public class FileProcessorTests
    {
        [Fact]
        public void ReadInput_NullOrEmpty_ThrowsArgumentException()
        {
            var fp = new FileProcessor();
            Assert.Throws<ArgumentException>(() => fp.ReadInput(null!));
            Assert.Throws<ArgumentException>(() => fp.ReadInput(string.Empty));
        }

        [Fact]
        public void ReadInput_FileNotFound_ThrowsFileNotFoundException()
        {
            var fp = new FileProcessor();
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var ex = Assert.Throws<FileNotFoundException>(() => fp.ReadInput(path));
            // either FileName or Message may contain the path depending on implementation
            Assert.True((ex.FileName != null && ex.FileName.Contains(path)) || ex.Message.Contains(path));
        }

        [Fact]
        public void ReadInput_ExistingFile_ReturnsBytes()
        {
            var fp = new FileProcessor();
            var temp = Path.GetTempFileName();
            try
            {
                var expected = new byte[] { 0x01, 0x02, 0xAB, 0xFF };
                File.WriteAllBytes(temp, expected);

                var actual = fp.ReadInput(temp);

                Assert.Equal(expected, actual);
            }
            finally
            {
                File.Delete(temp);
            }
        }
    }

    public class UseSampleFileTests
    {
        [Fact]
        public void SampleBin_IsPresentInTestOutput_AndHasExpectedPaddedBytes()
        {
            // runtime path where test runner executes
            var path = Path.Combine(AppContext.BaseDirectory, "sample.bin");

            // ensure file exists at runtime location
            Assert.True(File.Exists(path), $"sample.bin not found at runtime path: {path}");

            // read actual bytes
            var actual = File.ReadAllBytes(path);

            // build expected PKCS#7 padded bytes for "Hello World!"
            var plain = Encoding.UTF8.GetBytes("Hello World!");
            var padLen = 8 - (plain.Length % 8); if (padLen == 0) padLen = 8;
            var expected = new byte[plain.Length + padLen];
            Buffer.BlockCopy(plain, 0, expected, 0, plain.Length);
            for (int i = 0; i < padLen; i++) expected[plain.Length + i] = (byte)padLen;

            // assert content matches expected padded bytes
            Assert.Equal(expected, actual);
        }
    }
}