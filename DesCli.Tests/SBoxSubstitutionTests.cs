using System;
using Xunit;
using DesCli;

namespace DesCli.Tests
{
    public class SBoxSubstitutionTests
    {
        [Fact]
        public void SBoxSubstitution_AllZeroInput_ReturnsExpectedBytes()
        {
            // Arrange
            var cipher = new DesCipher();
            var input = new byte[6]; // 48 bits all zero --> each S-box sees row=0,col=0

            // Expected nibbles for S1..S8 at row=0,col=0:
            // S1=14, S2=15, S3=10, S4=7, S5=2, S6=12, S7=4, S8=13
            // Concatenated nibbles: E F A 7 2 C 4 D -> bytes: 0xEF,0xA7,0x2C,0x4D
            var expected = new byte[] { 0xEF, 0xA7, 0x2C, 0x4D };

            // Act
            var actual = cipher.SBoxSubstitution(input);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SBoxSubstitution_InvalidArguments_Throw()
        {
            var cipher = new DesCipher();

            Assert.Throws<ArgumentNullException>(() => cipher.SBoxSubstitution(null!));
            Assert.Throws<ArgumentException>(() => cipher.SBoxSubstitution(new byte[5])); // too short
            Assert.Throws<ArgumentException>(() => cipher.SBoxSubstitution(new byte[7])); // too long
        }
    }
}