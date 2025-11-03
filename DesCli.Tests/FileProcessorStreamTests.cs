using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using DesCli;

namespace DesCli.Tests
{
    public class FileProcessorStreamTests
    {
        [Fact]
        public void ReadInputStream_File_ReadsAllBytes()
        {
            var fp = new FileProcessor();
            var tempDir = Path.Combine(Path.GetTempPath(), "DesCli_FileProcessorTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            var path = Path.Combine(tempDir, "input.bin");
            var expected = new byte[] { 0x00, 0x11, 0x22, 0x33, 0xFF };

            try
            {
                File.WriteAllBytes(path, expected);

                using var stream = fp.ReadInputStream(path);
                Assert.True(stream.CanRead);

                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                var actual = ms.ToArray();

                Assert.Equal(expected, actual);
            }
            finally
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }

        [Fact]
        public void WriteOutputStream_CreatesFileAtomically_AndNoTempRemains()
        {
            var fp = new FileProcessor();
            var tempDir = Path.Combine(Path.GetTempPath(), "DesCli_FileProcessorTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            var target = Path.Combine(tempDir, "out.bin");
            var expected = Encoding.UTF8.GetBytes("atomic test payload");

            try
            {
                // Ensure directory empty
                Assert.Empty(Directory.GetFiles(tempDir));

                using var ms = new MemoryStream(expected, writable: false);
                fp.WriteOutputStream(target, ms);

                // After WriteOutputStream the target file must exist and contain expected bytes
                Assert.True(File.Exists(target), "target file must exist after write");
                var actual = File.ReadAllBytes(target);
                Assert.Equal(expected, actual);

                // Directory should contain exactly the target file (no leftover temp file)
                var files = Directory.GetFiles(tempDir);
                Assert.Single(files);
                Assert.Equal(Path.GetFileName(target), Path.GetFileName(files[0]));
            }
            finally
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }

        [Fact]
        public void WriteOutputStream_OverwritesExistingFile()
        {
            var fp = new FileProcessor();
            var tempDir = Path.Combine(Path.GetTempPath(), "DesCli_FileProcessorTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            var target = Path.Combine(tempDir, "out.bin");
            var first = Encoding.UTF8.GetBytes("first");
            var second = Encoding.UTF8.GetBytes("second longer payload");

            try
            {
                File.WriteAllBytes(target, first);
                Assert.Equal(first, File.ReadAllBytes(target));

                using var ms = new MemoryStream(second, writable: false);
                fp.WriteOutputStream(target, ms);

                Assert.True(File.Exists(target));
                var actual = File.ReadAllBytes(target);
                Assert.Equal(second, actual);
            }
            finally
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }

        [Fact]
        public void ReadInputStream_StdIn_ReturnsReadableStream()
        {
            var fp = new FileProcessor();

            // For "-" we expect a stream that is readable (Console.OpenStandardInput).
            // Do not attempt to read from it here (may block); just verify properties.
            using var stream = fp.ReadInputStream("-");
            Assert.NotNull(stream);
            Assert.True(stream.CanRead);
        }

        [Fact]
        public void WriteOutputStream_StdOut_DoesNotThrow()
        {
            var fp = new FileProcessor();
            var data = Encoding.UTF8.GetBytes("write-to-stdout-test");

            // For "-" write to stdout. This should not throw.
            using var ms = new MemoryStream(data, writable: false);
            fp.WriteOutputStream("-", ms);

            // If we reach here no exception was thrown.
            Assert.True(true);
        }
    }
}