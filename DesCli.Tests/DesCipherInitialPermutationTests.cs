using System;
using System.IO;
using Xunit;
using DesCli;

namespace DesCli.Tests
{
    public class DesCipherInitialPermutationTests
    {
        [Fact]
        public void InitialThenFinalPermutation_RoundTripsFirstBlockOfSampleBin()
        {
            // Arrange
            var path = Path.Combine(AppContext.BaseDirectory, "sample.bin");
            Assert.True(File.Exists(path), $"sample.bin not found at runtime path: {path}");

            var bytes = File.ReadAllBytes(path);
            Assert.True(bytes.Length >= 8, "sample.bin must contain at least one 8-byte block for this test.");

            var block = new byte[8];
            Array.Copy(bytes, 0, block, 0, 8);

            var cipher = new DesCipher();

            // Act
            var permuted = cipher.InitialPermutation(block);
            var roundTripped = cipher.FinalPermutation(permuted);

            // Assert: final permutation should return the original block
            Assert.Equal(block, roundTripped);

            // Sanity: initial permutation should not be identical to original for typical data
            Assert.NotEqual(block, permuted);
        }
    }
}
