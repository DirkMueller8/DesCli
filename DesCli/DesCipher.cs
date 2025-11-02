using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesCli
{
    public class DesCipher : IDesCipher
    {
        public byte[] Encrypt(byte[] plaintext, byte[] key)
        {
            // TODO: Implement encryption logic.
            return Array.Empty<byte>();
        }
        public byte[] Decrypt(byte[] ciphertext, byte[] key)
        {
            // TODO: Implement decryption logic.
            return Array.Empty<byte>();
        }
        public byte[] InitialPermutation(byte[] block)
        {
            // TODO: Implement Initial Permutation.
            return Array.Empty<byte>();
        }
        public byte[] FinalPermutation(byte[] block)
        {
            // TODO: Implement Final Permutation.
            return Array.Empty<byte>();
        }
        public byte[] Feistel(byte[] right, byte[] subkey)
        {
            // TODO: Implement Feistel function.
            return Array.Empty<byte>();
        }
        public byte[] SBoxSubstitution(byte[] input)
        {
            // TODO: Implement SBox Substitution.
            return Array.Empty<byte>();
        }
    }
}
