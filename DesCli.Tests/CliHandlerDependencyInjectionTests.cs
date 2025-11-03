using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using DesCli;

namespace DesCli.Tests
{
    public class CliHandlerDependencyInjectionTests
    {
        [Fact]
        public void Handle_UsesInjectedDependencies_ForEncrypt()
        {
            // Arrange
            var plaintext = new byte[] { 0x01, 0x02 };
            var expectedCipher = new byte[] { 0xAA, 0xBB };
            var fakeFile = new FakeFileProcessor(readBytes: plaintext);
            var fakeCipher = new FakeDesCipher(encryptResult: expectedCipher);
            var handler = new CliHandler(desCipher: fakeCipher, fileProcessor: fakeFile);

            var outPath = "out.bin";
            var keyHex = "0123456789ABCDEF";

            // Act
            handler.Handle(new[] { "encrypt", "-i", "input", "-o", outPath, "-k", keyHex });

            // Assert - fakeFile was asked to write expected bytes to outPath
            Assert.True(fakeFile.Written.TryGetValue(outPath, out var written));
            Assert.Equal(expectedCipher, written);
        }

        private class FakeFileProcessor : IFileProcessor
        {
            private readonly byte[] _read;
            public Dictionary<string, byte[]> Written { get; } = new();

            public FakeFileProcessor(byte[] readBytes) => _read = readBytes;

            public byte[] ReadInput(string path) => _read;

            public void WriteOutput(string path, byte[] data) => Written[path] = data;

            public Stream ReadInputStream(string path)
            {
                // Return a readable MemoryStream positioned at start; caller is responsible for disposing.
                return new MemoryStream(_read, writable: false);
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
            private readonly byte[] _encryptResult;
            public FakeDesCipher(byte[] encryptResult) => _encryptResult = encryptResult;

            public byte[] Encrypt(byte[] plaintext, byte[] key) => (byte[])_encryptResult.Clone();

            public byte[] Decrypt(byte[] ciphertext, byte[] key) => ciphertext;

            // Not used by CLI in the test, but implement trivial behavior to satisfy interface
            public byte[] InitialPermutation(byte[] block) => block;
            public byte[] FinalPermutation(byte[] block) => block;
            public byte[] Feistel(byte[] right, byte[] subkey) => new byte[4];
            public byte[] SBoxSubstitution(byte[] input) => new byte[4];
        }
    }
}