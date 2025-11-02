using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesCli
{
    public class DesKeyScheduler : IKeyScheduler
    {
        // PC-1 (64 -> 56)
        private static readonly int[] PC1 = new int[]
        {
            57,49,41,33,25,17,9,
            1,58,50,42,34,26,18,
            10,2,59,51,43,35,27,
            19,11,3,60,52,44,36,
            63,55,47,39,31,23,15,
            7,62,54,46,38,30,22,
            14,6,61,53,45,37,29,
            21,13,5,28,20,12,4
        };

        // Number of left shifts per round
        private static readonly int[] LeftShifts = new int[]
        {
            1,1,2,2,2,2,2,2,1,2,2,2,2,2,2,1
        };

        // PC-2 (56 -> 48)
        private static readonly int[] PC2 = new int[]
        {
            14,17,11,24,1,5,
            3,28,15,6,21,10,
            23,19,12,4,26,8,
            16,7,27,20,13,2,
            41,52,31,37,47,55,
            30,40,51,45,33,48,
            44,49,39,56,34,53,
            46,42,50,36,29,32
        };
        public byte[][] GenerateRoundKeys(byte[] key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (key.Length != 8) throw new ArgumentException("Key must be 8 bytes (64 bits).", nameof(key));

            // Build 56-bit key after PC-1
            var key56 = new int[56];
            for (int i = 0; i < 56; i++)
            {
                int srcBitPos = PC1[i] - 1; // 0-based
                int srcByte = srcBitPos / 8;
                int srcBitInByte = srcBitPos % 8; // 0 = MSB
                key56[i] = (key[srcByte] >> (7 - srcBitInByte)) & 0x01;
            }

            // Split into C and D (28 bits each)
            var c = new int[28];
            var d = new int[28];
            Array.Copy(key56, 0, c, 0, 28);
            Array.Copy(key56, 28, d, 0, 28);

            var roundKeys = new byte[16][];

            for (int round = 0; round < 16; round++)
            {
                // Left rotate C and D by LeftShifts[round]
                int shift = LeftShifts[round];
                c = LeftRotate28(c, shift);
                d = LeftRotate28(d, shift);

                // Combine C and D into 56 bits
                var cd = new int[56];
                Array.Copy(c, 0, cd, 0, 28);
                Array.Copy(d, 0, cd, 28, 28);

                // Apply PC-2 to get 48-bit subkey
                var subkeyBits = new int[48];
                for (int i = 0; i < 48; i++)
                {
                    int src = PC2[i] - 1; // 0-based index into cd
                    subkeyBits[i] = cd[src];
                }

                // Pack into 6 bytes (MSB-first in each byte)
                var subkey = new byte[6];
                for (int i = 0; i < 48; i++)
                {
                    int byteIndex = i / 8;
                    int bitInByte = i % 8;
                    subkey[byteIndex] |= (byte)(subkeyBits[i] << (7 - bitInByte));
                }

                roundKeys[round] = subkey;
            }

            return roundKeys;
        }

        private static int[] LeftRotate28(int[] arr, int shift)
        {
            var res = new int[28];
            for (int i = 0; i < 28; i++)
            {
                res[i] = arr[(i + shift) % 28];
            }
            return res;
        }
    }
}
