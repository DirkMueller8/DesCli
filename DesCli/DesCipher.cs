using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DesCli
{
    public class DesCipher : IDesCipher
    {
        // Standard DES Initial Permutation table (1-based indices)
        private static readonly int[] IP = new int[]
        {
            // Table 3.1. 64 bits read from top to bottom, left to right:
            // 58 -> position 1, 50 -> position 2, ..., 7 -> position 64
            58,50,42,34,26,18,10,2,
            60,52,44,36,28,20,12,4,
            62,54,46,38,30,22,14,6,
            64,56,48,40,32,24,16,8,
            57,49,41,33,25,17,9,1,
            59,51,43,35,27,19,11,3,
            61,53,45,37,29,21,13,5,
            63,55,47,39,31,23,15,7
        };

        // Table 3.2. Final Permutation (inverse of IP)
        private static readonly int[] FP = new int[]
        {
            40,8,48,16,56,24,64,32,
            39,7,47,15,55,23,63,31,
            38,6,46,14,54,22,62,30,
            37,5,45,13,53,21,61,29,
            36,4,44,12,52,20,60,28,
            35,3,43,11,51,19,59,27,
            34,2,42,10,50,18,58,26,
            33,1,41,9,49,17,57,25
        };

        // Expansion table 3.3 (32 -> 48)
        private static readonly int[] E = new int[]
        {
            32,1,2,3,4,5,
            4,5,6,7,8,9,
            8,9,10,11,12,13,
            12,13,14,15,16,17,
            16,17,18,19,20,21,
            20,21,22,23,24,25,
            24,25,26,27,28,29,
            28,29,30,31,32,1
        };

        // Table 3.12.: Permutation P (32 bits) of the S-box output
        private static readonly int[] P = new int[]
        {
            16,7,20,21,29,12,28,17,
            1,15,23,26,5,18,31,10,
            2,8,24,14,32,27,3,9,
            19,13,30,6,22,11,4,25
        };

        // Core of DES providing confusion
        // Table 3.4.: Standard DES S-boxes (S1..S8), each 4x16 flattened row-major
        private static readonly int[,] S1 = {
            {14,4,13,1,2,15,11,8,3,10,6,12,5,9,0,7},
            {0,15,7,4,14,2,13,1,10,6,12,11,9,5,3,8},
            {4,1,14,8,13,6,2,11,15,12,9,7,3,10,5,0},
            {15,12,8,2,4,9,1,7,5,11,3,14,10,0,6,13}
        };

        // Table 3.5.: S2   
        private static readonly int[,] S2 = {
            {15,1,8,14,6,11,3,4,9,7,2,13,12,0,5,10},
            {3,13,4,7,15,2,8,14,12,0,1,10,6,9,11,5},
            {0,14,7,11,10,4,13,1,5,8,12,6,9,3,2,15},
            {13,8,10,1,3,15,4,2,11,6,7,12,0,5,14,9}
        };
        private static readonly int[,] S3 = {
            {10,0,9,14,6,3,15,5,1,13,12,7,11,4,2,8},
            {13,7,0,9,3,4,6,10,2,8,5,14,12,11,15,1},
            {13,6,4,9,8,15,3,0,11,1,2,12,5,10,14,7},
            {1,10,13,0,6,9,8,7,4,15,14,3,11,5,2,12}
        };
        private static readonly int[,] S4 = {
            {7,13,14,3,0,6,9,10,1,2,8,5,11,12,4,15},
            {13,8,11,5,6,15,0,3,4,7,2,12,1,10,14,9},
            {10,6,9,0,12,11,7,13,15,1,3,14,5,2,8,4},
            {3,15,0,6,10,1,13,8,9,4,5,11,12,7,2,14}
        };
        private static readonly int[,] S5 = {
            {2,12,4,1,7,10,11,6,8,5,3,15,13,0,14,9},
            {14,11,2,12,4,7,13,1,5,0,15,10,3,9,8,6},
            {4,2,1,11,10,13,7,8,15,9,12,5,6,3,0,14},
            {11,8,12,7,1,14,2,13,6,15,0,9,10,4,5,3}
        };
        private static readonly int[,] S6 = {
            {12,1,10,15,9,2,6,8,0,13,3,4,14,7,5,11},
            {10,15,4,2,7,12,9,5,6,1,13,14,0,11,3,8},
            {9,14,15,5,2,8,12,3,7,0,4,10,1,13,11,6},
            {4,3,2,12,9,5,15,10,11,14,1,7,6,0,8,13}
        };
        private static readonly int[,] S7 = {
            {4,11,2,14,15,0,8,13,3,12,9,7,5,10,6,1},
            {13,0,11,7,4,9,1,10,14,3,5,12,2,15,8,6},
            {1,4,11,13,12,3,7,14,10,15,6,8,0,5,9,2},
            {6,11,13,8,1,4,10,7,9,5,0,15,14,2,3,12}
        };
        private static readonly int[,] S8 = {
            {13,2,8,4,6,15,11,1,10,9,3,14,5,0,12,7},
            {1,15,13,8,10,3,7,4,12,5,6,11,0,14,9,2},
            {7,11,4,1,9,12,14,2,0,6,10,13,15,3,5,8},
            {2,1,14,7,4,10,8,13,15,12,9,0,3,5,6,11}
        };
        public byte[] Encrypt(byte[] plaintext, byte[] key)
        {
            if (plaintext is null) throw new ArgumentNullException(nameof(plaintext));
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (key.Length != 8) throw new ArgumentException("Key must be 8 bytes (64 bits).", nameof(key));

            // PKCS#7 padding to full 8-byte blocks
            int padLen = 8 - (plaintext.Length % 8);
            if (padLen == 0) padLen = 8;
            var padded = new byte[plaintext.Length + padLen];
            Buffer.BlockCopy(plaintext, 0, padded, 0, plaintext.Length);
            for (int i = 0; i < padLen; i++) padded[plaintext.Length + i] = (byte)padLen;

            var keyScheduler = new DesKeyScheduler();
            var roundKeys = keyScheduler.GenerateRoundKeys(key); // 16 subkeys of 6 bytes each

            var output = new byte[padded.Length];

            for (int offset = 0; offset < padded.Length; offset += 8)
            {
                // Take one 8-byte block
                var block = new byte[8];
                Buffer.BlockCopy(padded, offset, block, 0, 8);

                // Initial permutation
                var permuted = InitialPermutation(block);

                // Split into L and R (4 bytes each)
                var L = new byte[4];
                var R = new byte[4];
                Array.Copy(permuted, 0, L, 0, 4);
                Array.Copy(permuted, 4, R, 0, 4);

                // 16 rounds
                for (int round = 0; round < 16; round++)
                {
                    var newL = new byte[4];
                    var newR = new byte[4];

                    // L_{i+1} = R_i
                    Array.Copy(R, 0, newL, 0, 4);

                    // R_{i+1} = L_i XOR f(R_i, K_i)
                    var f = Feistel(R, roundKeys[round]);
                    for (int b = 0; b < 4; b++)
                    {
                        newR[b] = (byte)(L[b] ^ f[b]);
                    }

                    L = newL;
                    R = newR;
                }

                // Preoutput is R||L (note the swap)
                var preoutput = new byte[8];
                Array.Copy(R, 0, preoutput, 0, 4);
                Array.Copy(L, 0, preoutput, 4, 4);

                // Final permutation
                var cipherBlock = FinalPermutation(preoutput);

                // Write to output
                Buffer.BlockCopy(cipherBlock, 0, output, offset, 8);
            }

            return output;
        }
        public byte[] Decrypt(byte[] ciphertext, byte[] key)
        {
            if (ciphertext is null) throw new ArgumentNullException(nameof(ciphertext));
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (key.Length != 8) throw new ArgumentException("Key must be 8 bytes (64 bits).", nameof(key));
            if (ciphertext.Length == 0 || (ciphertext.Length % 8) != 0)
                throw new ArgumentException("Ciphertext length must be a non-zero multiple of 8 bytes.", nameof(ciphertext));

            var keyScheduler = new DesKeyScheduler();
            var roundKeys = keyScheduler.GenerateRoundKeys(key); // 16 subkeys (6 bytes each)

            var output = new byte[ciphertext.Length];

            for (int offset = 0; offset < ciphertext.Length; offset += 8)
            {
                var block = new byte[8];
                Buffer.BlockCopy(ciphertext, offset, block, 0, 8);

                // Initial permutation
                var permuted = InitialPermutation(block);

                // Split into L and R
                var L = new byte[4];
                var R = new byte[4];
                Array.Copy(permuted, 0, L, 0, 4);
                Array.Copy(permuted, 4, R, 0, 4);

                // 16 rounds with keys in reverse order
                for (int round = 0; round < 16; round++)
                {
                    var newL = new byte[4];
                    var newR = new byte[4];

                    // L_{i+1} = R_i
                    Array.Copy(R, 0, newL, 0, 4);

                    // R_{i+1} = L_i XOR f(R_i, K_{15-round})
                    var f = Feistel(R, roundKeys[15 - round]);
                    for (int b = 0; b < 4; b++)
                    {
                        newR[b] = (byte)(L[b] ^ f[b]);
                    }

                    L = newL;
                    R = newR;
                }

                // Preoutput is R||L (swap)
                var preoutput = new byte[8];
                Array.Copy(R, 0, preoutput, 0, 4);
                Array.Copy(L, 0, preoutput, 4, 4);

                // Final permutation -> plaintext block
                var plainBlock = FinalPermutation(preoutput);

                Buffer.BlockCopy(plainBlock, 0, output, offset, 8);
            }

            // Remove PKCS#7 padding
            if (output.Length == 0)
                return Array.Empty<byte>();

            int padLen = output[^1];
            if (padLen < 1 || padLen > 8)
                throw new CryptographicException("Invalid PKCS#7 padding.");

            for (int i = output.Length - padLen; i < output.Length; i++)
            {
                if (output[i] != (byte)padLen)
                    throw new CryptographicException("Invalid PKCS#7 padding.");
            }

            var result = new byte[output.Length - padLen];
            if (result.Length > 0)
                Buffer.BlockCopy(output, 0, result, 0, result.Length);

            return result;
        }
        public byte[] InitialPermutation(byte[] block)
        {
            if (block is null)
                throw new ArgumentNullException(nameof(block));
            if (block.Length != 8)
                throw new ArgumentException("InitialPermutation requires a 8-byte block.", nameof(block));

            var output = new byte[8];

            // For each output bit position i (0..63), set it from input bit at IP[i]-1
            for (int i = 0; i < 64; i++)
            {
                int srcBitPos = IP[i] - 1; // 0-based bit index in input (0..63)
                int srcByte = srcBitPos / 8;
                int srcBitInByte = srcBitPos % 8; // 0 = MSB, 7 = LSB
                // Extract bit: MSB-first within a byte
                int bit = (block[srcByte] >> (7 - srcBitInByte)) & 0x01;
                int destByte = i / 8;
                int destBitInByte = i % 8;
                output[destByte] |= (byte)(bit << (7 - destBitInByte));
            }

            return output;
        }
        public byte[] FinalPermutation(byte[] block)
        {
            {
                if (block is null)
                    throw new ArgumentNullException(nameof(block));
                if (block.Length != 8)
                    throw new ArgumentException("FinalPermutation requires a 8-byte block.", nameof(block));

                var output = new byte[8];

                for (int i = 0; i < 64; i++)
                {
                    int srcBitPos = FP[i] - 1;
                    int srcByte = srcBitPos / 8;
                    int srcBitInByte = srcBitPos % 8;
                    int bit = (block[srcByte] >> (7 - srcBitInByte)) & 0x01;
                    int destByte = i / 8;
                    int destBitInByte = i % 8;
                    output[destByte] |= (byte)(bit << (7 - destBitInByte));
                }

                return output;
            }
        }
        public byte[] Feistel(byte[] right, byte[] subkey)
        {
            if (right is null) throw new ArgumentNullException(nameof(right));
            if (subkey is null) throw new ArgumentNullException(nameof(subkey));
            if (right.Length != 4) throw new ArgumentException("Right half must be 4 bytes (32 bits).", nameof(right));
            if (subkey.Length != 6) throw new ArgumentException("Subkey must be 6 bytes (48 bits).", nameof(subkey));

            // Expand right (32 bits) to 48 bits using E table
            var expanded = new byte[6]; // 48 bits
            for (int i = 0; i < 48; i++)
            {
                int srcBitPos = E[i] - 1; // 0-based within 32-bit right
                int srcByte = srcBitPos / 8;
                int srcBitInByte = srcBitPos % 8;
                int bit = (right[srcByte] >> (7 - srcBitInByte)) & 0x01;
                int destByte = i / 8;
                int destBitInByte = i % 8;
                expanded[destByte] |= (byte)(bit << (7 - destBitInByte));
            }

            // XOR with subkey
            for (int i = 0; i < 6; i++) expanded[i] ^= subkey[i];

            // S-box substitution: 48 -> 32 bits
            var sboxed = SBoxSubstitution(expanded); // 4 bytes

            // Apply P permutation to the 32-bit result
            var output = new byte[4];
            for (int i = 0; i < 32; i++)
            {
                int srcBitPos = P[i] - 1; // 0-based in sboxed (32 bits)
                int srcByte = srcBitPos / 8;
                int srcBitInByte = srcBitPos % 8;
                int bit = (sboxed[srcByte] >> (7 - srcBitInByte)) & 0x01;
                int destByte = i / 8;
                int destBitInByte = i % 8;
                output[destByte] |= (byte)(bit << (7 - destBitInByte));
            }

            return output;
        }
        public byte[] SBoxSubstitution(byte[] input)
        {
            if (input is null) throw new ArgumentNullException(nameof(input));
            if (input.Length != 6) throw new ArgumentException("SBoxSubstitution requires 6 bytes (48 bits) input.", nameof(input));

            // Convert input to bit array (MSB-first)
            int[] bits = new int[48];
            for (int i = 0; i < 48; i++)
            {
                int bIndex = i / 8;
                int bitInByte = i % 8;
                bits[i] = (input[bIndex] >> (7 - bitInByte)) & 0x01;
            }

            // For each of the 8 S-boxes, take 6 bits and map to 4 bits
            int[] sOutputBits = new int[32];
            for (int s = 0; s < 8; s++)
            {
                int offset = s * 6;
                int b0 = bits[offset];
                int b1 = bits[offset + 1];
                int b2 = bits[offset + 2];
                int b3 = bits[offset + 3];
                int b4 = bits[offset + 4];
                int b5 = bits[offset + 5];

                int row = (b0 << 1) | b5; // outer bits
                int col = (b1 << 3) | (b2 << 2) | (b3 << 1) | b4;

                int val;
                switch (s)
                {
                    case 0: val = S1[row, col]; break;
                    case 1: val = S2[row, col]; break;
                    case 2: val = S3[row, col]; break;
                    case 3: val = S4[row, col]; break;
                    case 4: val = S5[row, col]; break;
                    case 5: val = S6[row, col]; break;
                    case 6: val = S7[row, col]; break;
                    default: val = S8[row, col]; break;
                }

                // place 4 bits of val into sOutputBits at position s*4 .. s*4+3 (MSB-first)
                int outOffset = s * 4;
                sOutputBits[outOffset + 0] = (val >> 3) & 1;
                sOutputBits[outOffset + 1] = (val >> 2) & 1;
                sOutputBits[outOffset + 2] = (val >> 1) & 1;
                sOutputBits[outOffset + 3] = (val >> 0) & 1;
            }

            // Convert sOutputBits to 4 bytes
            var result = new byte[4];
            for (int i = 0; i < 32; i++)
            {
                int byteIndex = i / 8;
                int bitInByte = i % 8;
                result[byteIndex] |= (byte)(sOutputBits[i] << (7 - bitInByte));
            }

            return result;
        }
    }
}
