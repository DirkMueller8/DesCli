using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesCli
{
    public class CliHandler
    {
        private readonly IDesCipher desCipher;
        private readonly IFileProcessor fileProcessor;

        // Constructor supporting dependency injection. Defaults keep previous behavior.
        public CliHandler(IDesCipher? desCipher = null, IKeyScheduler? keyScheduler = null, IFileProcessor? fileProcessor = null)
        {
            this.desCipher = desCipher ?? new DesCipher();
            this.fileProcessor = fileProcessor ?? new FileProcessor();
        }

        public void Handle(string[] args)
        {
            if (args is null) args = Array.Empty<string>();

            try
            {
                if (args.Length == 0 || args.Contains("-h") || args.Contains("--help"))
                {
                    PrintUsage();
                    return;
                }

                // Mode may be first arg ("encrypt" | "decrypt"), or provided via --mode
                string? mode = null;
                string? input = null;
                string? output = null;
                string? keyArg = null;

                int i = 0;
                // if first token is encrypt/decrypt, consume it
                if (args.Length > 0 && (string.Equals(args[0], "encrypt", StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(args[0], "decrypt", StringComparison.OrdinalIgnoreCase)))
                {
                    mode = args[0].ToLowerInvariant();
                    i = 1;
                }

                for (; i < args.Length; i++)
                {
                    var a = args[i];
                    switch (a)
                    {
                        case "-i":
                        case "--input":
                            if (i + 1 >= args.Length) throw new ArgumentException("Missing value for input.");
                            input = args[++i];
                            break;
                        case "-o":
                        case "--output":
                            if (i + 1 >= args.Length) throw new ArgumentException("Missing value for output.");
                            output = args[++i];
                            break;
                        case "-k":
                        case "--key":
                            if (i + 1 >= args.Length) throw new ArgumentException("Missing value for key.");
                            keyArg = args[++i];
                            break;
                        case "--mode":
                            if (i + 1 >= args.Length) throw new ArgumentException("Missing value for mode.");
                            mode = args[++i].ToLowerInvariant();
                            break;
                        default:
                            // treat unknown single token as positional: input if not set, then output, then key
                            if (input == null) input = a;
                            else if (output == null) output = a;
                            else if (keyArg == null) keyArg = a;
                            else throw new ArgumentException($"Unknown argument or too many positional arguments: {a}");
                            break;
                    }
                }

                if (string.IsNullOrEmpty(mode))
                    throw new ArgumentException("Mode not specified. Use 'encrypt' or 'decrypt' (or --mode).");

                if (string.IsNullOrEmpty(input))
                    throw new ArgumentException("Input path not specified. Use -i <path> or positional.");

                if (string.IsNullOrEmpty(output))
                    throw new ArgumentException("Output path not specified. Use -o <path> or positional.");

                if (string.IsNullOrEmpty(keyArg))
                    throw new ArgumentException("Key not specified. Use -k <hex-or-path> or positional.");

                // Resolve key: allow a hex string (even length) or a file path
                byte[] keyBytes;
                try
                {
                    // If looks like hex (only hex chars and even length), parse as hex
                    var candidate = keyArg!;
                    bool isHex = candidate.Length % 2 == 0 && candidate.All(c => Uri.IsHexDigit(c));
                    if (isHex)
                    {
                        keyBytes = Convert.FromHexString(candidate);
                    }
                    else
                    {
                        // otherwise try to read as file path (supports "-" for stdin)
                        keyBytes = fileProcessor.ReadInput(candidate);
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Failed to parse key from '{keyArg}': {ex.Message}", ex);
                }

                if (keyBytes.Length != 8)
                    throw new ArgumentException($"DES key must be 8 bytes (64 bits). Provided key length: {keyBytes.Length}.");

                // Read input bytes (fileProcessor supports "-" for stdin)
                var inputBytes = fileProcessor.ReadInput(input!);

                byte[] outputBytes;
                if (mode == "encrypt")
                {
                    outputBytes = desCipher.Encrypt(inputBytes, keyBytes);
                }
                else if (mode == "decrypt")
                {
                    outputBytes = desCipher.Decrypt(inputBytes, keyBytes);
                }
                else
                {
                    throw new ArgumentException($"Unknown mode '{mode}'. Use 'encrypt' or 'decrypt'.");
                }

                // Write output (fileProcessor supports "-" for stdout)
                fileProcessor.WriteOutput(output!, outputBytes);

                // success
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Console.Error.WriteLine();
                PrintUsage();
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  DesCli encrypt -i <input> -o <output> -k <key>");
            Console.WriteLine("  DesCli decrypt -i <input> -o <output> -k <key>");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("  encrypt|decrypt        Mode of operation.");
            Console.WriteLine("  -i, --input <path>     Input file path (use '-' for stdin).");
            Console.WriteLine("  -o, --output <path>    Output file path (use '-' for stdout).");
            Console.WriteLine("  -k, --key <hex|path>   16-hex-digit key (e.g. 0123456789ABCDEF) or path to a key file (8 bytes).");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  DesCli encrypt -i sample.bin -o sample.enc -k 0123456789ABCDEF");
            Console.WriteLine("  DesCli decrypt -i sample.enc -o sample.dec -k keyfile.bin");
        }
    }
}