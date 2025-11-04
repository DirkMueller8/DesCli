using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using DesCli;

namespace DesCli.Tests
{
    public class CliHandlerKeyParsingTests
    {
        [Fact]
        public void KeyFormat_Hex_ParsesHexAndPassesBytesToCipher()
        {
            // Arrange
            var inputBytes = new byte[] { 0x10, 0x11 };
            var expectedEncrypt = new byte[] { 0xAA };
            var fakeFiles = new Dictionary<string, byte[]>
            {
                ["input.bin"] = inputBytes
            };
            var fp = new FakeFileProcessor(fakeFiles);
            var cipher = new FakeDesCipher(expectedEncrypt);
            var handler = new CliHandler(desCipher: cipher, fileProcessor: fp);

            var hexKey = "0123456789ABCDEF";

            // Act
            handler.Handle(new[] { "encrypt", "-i", "input.bin", "-o", "out.bin", "-k", hexKey, "--key-format", "hex" });

            // Assert
            Assert.True(fp.Written.TryGetValue("out.bin", out var written));
            Assert.Equal(expectedEncrypt, written);
            Assert.Equal(Convert.FromHexString(hexKey), cipher.LastKey);
        }

        [Fact]
        public void KeyFormat_Ascii_UsesFirst8AsciiBytesAsKey()
        {
            // Arrange
            var inputBytes = new byte[] { 0x01, 0x02 };
            var expectedEncrypt = new byte[] { 0xBB };
            var fp = new FakeFileProcessor(new Dictionary<string, byte[]> { ["in"] = inputBytes });
            var cipher = new FakeDesCipher(expectedEncrypt);
            var handler = new CliHandler(desCipher: cipher, fileProcessor: fp);

            var asciiKey = "MySecretKExtra"; // will take first 8 ASCII bytes "MySecretK"

            // Act
            handler.Handle(new[] { "encrypt", "-i", "in", "-o", "out", "-k", asciiKey, "--key-format", "ascii" });

            // Assert
            Assert.True(fp.Written.TryGetValue("out", out var written));
            Assert.Equal(expectedEncrypt, written);

            var expectedKeyBytes = Encoding.ASCII.GetBytes(asciiKey).Take(8).ToArray();
            Assert.Equal(expectedKeyBytes, cipher.LastKey);
        }

        [Fact]
        public void KeyFile_TakesPrecedence_Over_KArgument()
        {
            // Arrange
            var inputBytes = new byte[] { 0xDE, 0xAD };
            var keyFileBytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
            var fakeFiles = new Dictionary<string, byte[]>
            {
                ["data.bin"] = inputBytes,
                ["keyfile.bin"] = keyFileBytes
            };
            var fp = new FakeFileProcessor(fakeFiles);
            var cipher = new FakeDesCipher(new byte[] { 0xCC });
            var handler = new CliHandler(desCipher: cipher, fileProcessor: fp);

            // Provide both -k and --key-file; key-file should be used
            handler.Handle(new[] {
                "encrypt",
                "-i", "data.bin",
                "-o", "out.bin",
                "-k", "0123456789ABCDEF",
                "--key-file", "keyfile.bin" });

            // Assert key bytes used are those read from keyfile.bin
            Assert.Equal(keyFileBytes, cipher.LastKey);

            // Ensure keyfile was actually read by file processor
            Assert.Contains("keyfile.bin", fp.ReadCalls);
        }

        [Fact]
        public void KeyFormat_File_With_K_TreatedAsPath_ReadsKeyFromPath()
        {
            // Arrange
            var inputBytes = new byte[] { 0x0A };
            var keyFileBytes = new byte[] { 0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70, 0x80 };
            var fakeFiles = new Dictionary<string, byte[]>
            {
                ["in.bin"] = inputBytes,
                ["theKey.bin"] = keyFileBytes
            };
            var fp = new FakeFileProcessor(fakeFiles);
            var cipher = new FakeDesCipher(new byte[] { 0xDD });
            var handler = new CliHandler(desCipher: cipher, fileProcessor: fp);

            // Use -k as a path but tell CLI it's a file via --key-format file
            handler.Handle(new[] {
                "encrypt",
                "-i", "in.bin",
                "-o", "out.bin",
                "-k", "theKey.bin",
                "--key-format", "file" });

            Assert.Equal(keyFileBytes, cipher.LastKey);
            Assert.Contains("theKey.bin", fp.ReadCalls);
        }

        #region Fakes

        private class FakeFileProcessor : IFileProcessor
        {
            private readonly Dictionary<string, byte[]> _files;

            public Dictionary<string, byte[]> Written { get; } = new();
            public List<string> ReadCalls { get; } = new();

            public FakeFileProcessor(Dictionary<string, byte[]> files)
            {
                _files = files ?? new Dictionary<string, byte[]>();
            }

            public byte[] ReadInput(string path)
            {
                ReadCalls.Add(path);
                if (_files.TryGetValue(path, out var data))
                    return data;
                throw new FileNotFoundException(path);
            }

            public void WriteOutput(string path, byte[] data)
            {
                Written[path] = data;
            }

            public Stream ReadInputStream(string path)
            {
                ReadCalls.Add(path);
                if (_files.TryGetValue(path, out var data))
                    return new MemoryStream(data, writable: false);
                throw new FileNotFoundException(path);
            }

            public void WriteOutputStream(string path, Stream data)
            {
                using var ms = new MemoryStream();
                data.CopyTo(ms);
                Written[path] = ms.ToArray();
            }
        }

        private class FakeDesCipher : IDesCipher
        {
            private readonly byte[] _result;
            public byte[]? LastKey { get; private set; }

            public FakeDesCipher(byte[] result)
            {
                _result = result;
            }

            public byte[] Encrypt(byte[] plaintext, byte[] key)
            {
                // record key and return canned result
                LastKey = (byte[])key.Clone();
                return (byte[])_result.Clone();
            }

            public byte[] Decrypt(byte[] ciphertext, byte[] key)
            {
                LastKey = (byte[])key.Clone();
                return (byte[])_result.Clone();
            }

            // Unused by CLI tests; return simple defaults
            public byte[] InitialPermutation(byte[] block) => block;
            public byte[] FinalPermutation(byte[] block) => block;
            public byte[] Feistel(byte[] right, byte[] subkey) => new byte[4];
            public byte[] SBoxSubstitution(byte[] input) => new byte[4];
        }

        #endregion
    }
}