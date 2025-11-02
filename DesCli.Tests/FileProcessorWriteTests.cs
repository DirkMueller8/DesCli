using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesCli.Tests
{
    public class FileProcessorWriteTests
    {
        [Fact]
        public void WriteOutput_NullOrEmptyPath_ThrowsArgumentException()
        {
            var fp = new FileProcessor();
            var data = new byte[] { 0x01 };

            Assert.Throws<ArgumentException>(() => fp.WriteOutput(null!, data));
            Assert.Throws<ArgumentException>(() => fp.WriteOutput(string.Empty, data));
        }

        [Fact]
        public void WriteOutput_NullData_ThrowsArgumentNullException()
        {
            var fp = new FileProcessor();
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".bin");

            Assert.Throws<ArgumentNullException>(() => fp.WriteOutput(path, null!));
        }

        [Fact]
        public void WriteOutput_CreatesDirectoryAndWritesFile()
        {
            var fp = new FileProcessor();
            var baseDir = Path.Combine(Path.GetTempPath(), "DesCliTests_" + Guid.NewGuid().ToString("N"));
            var nestedDir = Path.Combine(baseDir, "sub", "dir");
            var path = Path.Combine(nestedDir, "out.bin");

            try
            {
                var expected = new byte[] { 0x48, 0x65, 0x6C }; // sample bytes

                // Ensure directory does not exist initially
                if (Directory.Exists(baseDir)) Directory.Delete(baseDir, recursive: true);

                fp.WriteOutput(path, expected);

                Assert.True(File.Exists(path), "Output file was not created.");
                var actual = File.ReadAllBytes(path);
                Assert.Equal(expected, actual);
            }
            finally
            {
                if (Directory.Exists(baseDir))
                {
                    try { Directory.Delete(baseDir, recursive: true); } catch { }
                }
            }
        }

        [Fact]
        public void WriteOutput_OverwritesExistingFile()
        {
            var fp = new FileProcessor();
            var dir = Path.Combine(Path.GetTempPath(), "DesCliTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, "out.bin");

            try
            {
                var first = new byte[] { 0x00, 0x01, 0x02 };
                var second = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD };

                // create initial file
                File.WriteAllBytes(path, first);
                Assert.Equal(first, File.ReadAllBytes(path));

                // overwrite
                fp.WriteOutput(path, second);

                Assert.True(File.Exists(path), "Output file missing after overwrite.");
                var actual = File.ReadAllBytes(path);
                Assert.Equal(second, actual);
            }
            finally
            {
                if (Directory.Exists(dir))
                {
                    try { Directory.Delete(dir, recursive: true); } catch { }
                }
            }
        }
    }
}
