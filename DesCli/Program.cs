using System;
using System.Text;

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
            Console.WriteLine("*************************************************************");
            Console.WriteLine("*        Data Encryption Standard (DES) Interactive Demo    *");
            Console.WriteLine("*                                                           *"  );
            Console.WriteLine("*        This demo                                          *");
            Console.WriteLine("*        - asks the plaintext from the user,                *");
            Console.WriteLine("*        - converts it to hexadecimal representation,       *");
            Console.WriteLine("*        - applies padding if necessary,                    *");
            Console.WriteLine("*        - encrypts the plaintext to ciphertext, and        *");
            Console.WriteLine("*        - decrypts the ciphertext to plaintext again.      *");
            Console.WriteLine("*************************************************************");
            Console.WriteLine();

            Console.WriteLine("Enter a line of text (UTF-8). Press Enter to submit:");
            var inputText = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrEmpty(inputText))
            {
                Console.WriteLine("No input provided. Exiting.");
                return;
            }

            Console.Write("Enter 16-hex key (or press Enter to use default 0123456789ABCDEF): ");
            var keyHex = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(keyHex))
            {
                keyHex = "0123456789ABCDEF";
                Console.WriteLine($"Using default key: {keyHex}");
            }

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
                Console.Error.WriteLine($"Key must be 8 bytes (16 hex chars). Provided length: {key.Length} bytes.");
                return;
            }

            try
            {
                var cipher = new DesCipher();
                var plainBytes = Encoding.UTF8.GetBytes(inputText);

                // Show plaintext hex for educational purposes (grouped)
                Console.WriteLine();
                Console.WriteLine("Plaintext (hex, grouped):");
                Console.WriteLine(ToGroupedHex(plainBytes));

                // Compute and show PKCS#7 padded plaintext (what DES will actually encrypt)
                var padLen = 8 - (plainBytes.Length % 8);
                if (padLen == 0) padLen = 8;
                var paddedPlain = new byte[plainBytes.Length + padLen];
                Buffer.BlockCopy(plainBytes, 0, paddedPlain, 0, plainBytes.Length);
                for (int i = 0; i < padLen; i++) paddedPlain[plainBytes.Length + i] = (byte)padLen;

                Console.WriteLine();
                Console.WriteLine("Padded plaintext (hex, grouped):");
                Console.WriteLine(ToGroupedHex(paddedPlain));

                var cipherBytes = cipher.Encrypt(plainBytes, key);

                Console.WriteLine();
                Console.WriteLine("Encrypted (hex, grouped):");
                Console.WriteLine(ToGroupedHex(cipherBytes));

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

        private static string ToGroupedHex(byte[] bytes, int bytesPerGroup = 4, int groupsPerLine = 4)
        {
            if (bytes is null || bytes.Length == 0) return string.Empty;
            var sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.AppendFormat("{0:X2}", bytes[i]);

                if (i == bytes.Length - 1)
                    break;

                // single space between bytes
                sb.Append(' ');

                // extra space between groups
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