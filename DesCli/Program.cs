using System;
using System.Text;
using System.Security.Cryptography;

namespace DesCli
{
    class Program
    {
        static void Main(string[] args)
        {
            // If no args, run an interactive demo; otherwise keep CLI behavior
            if (args is null || args.Length == 0)
            {
                RunInteractiveDemo();
                return;
            }

            var cliHandler = new CliHandler();
            cliHandler.Handle(args);
        }

        private static void RunInteractiveDemo()
        {
            // Illustrative header box
            Console.WriteLine("***************************************************************");
            Console.WriteLine("*     Data Encryption Standard (DES) Interactive Demo         *");
            Console.WriteLine("*                                                             *");
            Console.WriteLine("*     This application is a manually coded solution of        *");
            Console.WriteLine("*     the DES using FIPS PUB 46 and the 2nd Edition of the    *");
            Console.WriteLine("*     book by Paar, Plezl and Güneysu, published in 2024:     *");
            Console.WriteLine("*     'Understanding Cryptography'                            *");
            Console.WriteLine("*                                                             *");
            Console.WriteLine("*     This demo                                               *");
            Console.WriteLine("*     - asks the plaintext from the user encoded in UTF-8     *");
            Console.WriteLine("*     - converts it to hexadecimal representation             *");
            Console.WriteLine("*     - applies padding if necessary                          *");
            Console.WriteLine("*     - encrypts the plaintext to ciphertext, and             *");
            Console.WriteLine("*     - decrypts the ciphertext to plaintext again.           *");
            Console.WriteLine("***************************************************************");
            Console.WriteLine();

            Console.WriteLine("Enter a line of text (UTF-8). Press Enter to submit:");
            var inputText = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrEmpty(inputText))
            {
                Console.WriteLine("No input provided. Exiting.");
                return;
            }

            Console.WriteLine();
            Console.Write("Enter 16-hex key (or press Enter to use default 0123456789ABCDEF): ");
            var keyHex = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(keyHex))
            {
                keyHex = "0123456789ABCDEF";
                Console.WriteLine($"Using default key: {keyHex}");
            }

            // Convert hex key to byte array
            byte[] key;
            try
            {
                key = Convert.FromHexString(keyHex.Trim());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Invalid hex key: {ex.Message}");
                return;
            }

            if (key.Length != 8)
            {
                Console.Error.WriteLine($"Key must be 8 bytes (16 hex characters). Provided length: {key.Length} bytes.");
                return;
            }

            try
            {
                // Initialize DES cipher
                var cipher = new DesCipher();

                // Convert input text to bytes
                var plainBytes = Encoding.UTF8.GetBytes(inputText);
                Console.WriteLine();
                Console.WriteLine("Electronic Code Block (ECB) + PKCS7 for padding");
                Console.WriteLine();

                // Show plaintext hex for educational purposes (grouped)
                Console.WriteLine();
                Console.WriteLine("Plaintext (hex, grouped):");
                Console.WriteLine(ToGroupedHex(plainBytes));

                // Compute and show PKCS#7 padded plaintext (what DES will actually encrypt)
                var padLen = 8 - (plainBytes.Length % 8);
                // If already multiple of 8, add a full block of padding
                if (padLen == 0) padLen = 8;
                var paddedPlain = new byte[plainBytes.Length + padLen];

                // Copy original plaintext bytes
                Buffer.BlockCopy(plainBytes, 0, paddedPlain, 0, plainBytes.Length);

                // Add PKCS#7 padding bytes
                for (int i = 0; i < padLen; i++) paddedPlain[plainBytes.Length + i] = (byte)padLen;

                Console.WriteLine();
                Console.WriteLine("Padded plaintext (hex, grouped):");
                Console.WriteLine(ToGroupedHex(paddedPlain));

                // Encrypt (manual implementation)
                var cipherBytes = cipher.Encrypt(plainBytes, key);

                Console.WriteLine();
                Console.WriteLine("Encrypted (hex, grouped):");
                Console.WriteLine(ToGroupedHex(cipherBytes));

                // Comparison: manual implementation vs .NET DES (same mode/padding)
                try
                {
                    // Use .NET DES to encrypt with same key, mode and padding
                    using var sysDes = DES.Create();

                    // Set to ECB mode with PKCS#7 padding
                    sysDes.Mode = CipherMode.ECB;
                    sysDes.Padding = PaddingMode.PKCS7;

                    sysDes.Key = key;

                    var systemCipherBytes = sysDes.CreateEncryptor().TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                    var manualB64 = Convert.ToBase64String(cipherBytes);
                    var systemB64 = Convert.ToBase64String(systemCipherBytes);

                    Console.WriteLine();
                    Console.WriteLine("Comparison (Base64):");

                    var label1 = "Encrypted, manually coded (this application):";
                    var label2 = "Encrypted by .NET System DES library:";
                    var label3 = "Match between the two approaches:";
                    var width = Math.Max(label1.Length, Math.Max(label2.Length, label3.Length));

                    Console.WriteLine($"{label1.PadRight(width)} {manualB64}");
                    Console.WriteLine($"{label2.PadRight(width)} {systemB64}");

                    var isMatch = string.Equals(manualB64, systemB64, StringComparison.Ordinal);
                    Console.WriteLine($"{label3.PadRight(width)} {isMatch}");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"System DES comparison failed: {ex.Message}");
                }
                // --- end comparison ---

                // Decrypt
                var decrypted = cipher.Decrypt(cipherBytes, key);
                var decryptedText = Encoding.UTF8.GetString(decrypted);

                // Show decrypted bytes hex as well (grouped)
                Console.WriteLine();
                Console.WriteLine("Decrypted (hex, grouped):");
                Console.WriteLine(ToGroupedHex(decrypted));

                Console.WriteLine();
                Console.WriteLine("Decrypted back to text:");
                Console.WriteLine(decryptedText);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error during encryption/decryption: {ex.Message}");
            }
        }

        // Helper method to convert byte array to grouped hex string
        private static string ToGroupedHex(byte[] bytes, int bytesPerGroup = 4, int groupsPerLine = 4)
        {
            if (bytes is null || bytes.Length == 0) return string.Empty;
            var sb = new StringBuilder();

            // Iterate through bytes and format
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.AppendFormat("{0:X2}", bytes[i]);

                if (i == bytes.Length - 1)
                    break;

                // Single space between bytes
                sb.Append(' ');

                // Extra space between groups
                if ((i + 1) % bytesPerGroup == 0)
                    sb.Append(' ');

                // newline after groupsPerLine groups
                if ((i + 1) % (bytesPerGroup * groupsPerLine) == 0)
                    sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}