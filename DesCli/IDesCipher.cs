using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesCli
{
    // Interface used to decouple the CLI and tests from a concrete cipher implementation:
    // it enables dependency injection, easy mocking for unit tests,
    // and swapping algorithms (DES -> AES) without changing callers.
    public interface IDesCipher
    {
        byte[] Encrypt(byte[] plaintext, byte[] key);
        byte[] Decrypt(byte[] ciphertext, byte[] key);
        byte[] InitialPermutation(byte[] block);
        byte[] FinalPermutation(byte[] block);
        byte[] Feistel(byte[] right, byte[] subkey);
        byte[] SBoxSubstitution(byte[] input);
    }
}
