using System;
using System.IO;
using Xunit;
using DesCli;

namespace DesCli.Tests
{
    public class DesKeySchedulerTests
    {
        [Fact]
        public void GenerateRoundKeys_NullOrWrongLength_Throws()
        {
            var ks = new DesKeyScheduler();
            Assert.Throws<ArgumentNullException>(() => ks.GenerateRoundKeys(null!));
            Assert.Throws<ArgumentException>(() => ks.GenerateRoundKeys(new byte[7]));
            Assert.Throws<ArgumentException>(() => ks.GenerateRoundKeys(new byte[9]));
        }

        [Fact]
        public void GenerateRoundKeys_AllZeroKey_ProducesAllZeroRoundKeys()
        {
            var ks = new DesKeyScheduler();
            var key = new byte[8]; // all zeros
            var roundKeys = ks.GenerateRoundKeys(key);

            Assert.Equal(16, roundKeys.Length);
            foreach (var rk in roundKeys)
            {
                Assert.Equal(6, rk.Length);
                // all zero input -> after PC-1 and rotations -> still zeros -> PC-2 -> zeros
                Assert.True(Array.TrueForAll(rk, b => b == 0));
            }
        }

        [Fact]
        public void GenerateRoundKeys_Deterministic_And_VariesAcrossRounds()
        {
            var ks = new DesKeyScheduler();
            var key = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

            var first = ks.GenerateRoundKeys(key);
            var second = ks.GenerateRoundKeys(key);

            // Deterministic
            Assert.Equal(first.Length, second.Length);
            for (int i = 0; i < first.Length; i++)
                Assert.Equal(first[i], second[i]);

            // 16 round keys, each 6 bytes
            Assert.Equal(16, first.Length);
            foreach (var rk in first) Assert.Equal(6, rk.Length);

            // sanity: at least one round key differs from the others
            bool foundDifferent = false;
            for (int i = 1; i < first.Length && !foundDifferent; i++)
            {
                if (!AreEqual(first[0], first[i])) foundDifferent = true;
            }
            Assert.True(foundDifferent, "Expected at least one round key to differ across rounds");
        }

        private static bool AreEqual(byte[] a, byte[] b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
            return true;
        }
    }
}
