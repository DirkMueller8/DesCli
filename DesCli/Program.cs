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
            Console.WriteLine("DES Interactive demo (educational only).");
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

                var cipherBytes = cipher.Encrypt(plainBytes, key);
                Console.WriteLine();
                Console.WriteLine("Encrypted (hex):");
                Console.WriteLine(Convert.ToHexString(cipherBytes));

                var decrypted = cipher.Decrypt(cipherBytes, key);
                var decryptedText = Encoding.UTF8.GetString(decrypted);

                Console.WriteLine();
                Console.WriteLine("Decrypted back to text:");
                Console.WriteLine(decryptedText);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error during encryption/decryption: {ex.Message}");
            }
        }
    }
}
