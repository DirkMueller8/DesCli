using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesCli
{
    // Handles command-line interface for DES encryption/decryption

    // Main responsibilities:
    // - Parse CLI switches/positionals
    // - Resolve the encryption key (hex string or key file)
    // - Read input bytes (supports "-" for stdin)
    // - Call desCipher.Encrypt or desCipher.Decrypt
    // - Write output(supports "-" for stdout)
    // - On error print a message and usage.

    public class CliHandler
    {
        private readonly IDesCipher desCipher;
        private readonly IFileProcessor fileProcessor;

        // Constructor supporting dependency injection. Defaults keep previous behavior.
        public CliHandler(IDesCipher? desCipher = null, IFileProcessor? fileProcessor = null)
        {
            this.desCipher = desCipher ?? new DesCipher();
            this.fileProcessor = fileProcessor ?? new FileProcessor();
        }


        // Main method to handle CLI arguments
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

                // New: explicit key options
                // --key-format <hex|file|ascii>
                // --key-file <path>    (alternative to -k when you want to be explicit)
                string? keyFormat = null;
                string? keyFile = null;

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

                    // Recognized switches:
                    // -i / --input  → input path (or - for stdin)
                    // -o / --output → output path (or - for stdout)
                    // -k / --key    → key (either a hex string or a path to a key file)
                    // --key-format  → explicit format: hex | file | ascii
                    // --key-file    → explicit key file path
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
                        case "--key-format":
                            if (i + 1 >= args.Length) throw new ArgumentException("Missing value for --key-format.");
                            keyFormat = args[++i].ToLowerInvariant();
                            break;
                        case "--key-file":
                            if (i + 1 >= args.Length) throw new ArgumentException("Missing value for --key-file.");
                            keyFile = args[++i];
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

                // if key-file was provided, it takes precedence
                if (string.IsNullOrEmpty(keyArg) && string.IsNullOrEmpty(keyFile))
                    throw new ArgumentException("Key not specified. Use -k <hex|path> or --key-file <path> or positional.");

                if (!string.IsNullOrEmpty(keyFormat) && !string.IsNullOrEmpty(keyFile))
                    throw new ArgumentException("Cannot specify both --key-format and --key-file. Choose one.");

                // Resolve keyBytes according to explicit flags or fallback heuristic
                byte[] keyBytes;
                try
                {
                    if (!string.IsNullOrEmpty(keyFile))
                    {
                        // explicit key file path
                        keyBytes = fileProcessor.ReadInput(keyFile);
                    }
                    else if (!string.IsNullOrEmpty(keyFormat))
                    {
                        // explicit format chosen
                        switch (keyFormat)
                        {
                            case "hex":
                                if (string.IsNullOrEmpty(keyArg))
                                    throw new ArgumentException("--key-format hex requires a key string via -k/--key.");
                                keyBytes = Convert.FromHexString(keyArg);
                                break;
                            case "ascii":
                                if (string.IsNullOrEmpty(keyArg))
                                    throw new ArgumentException("--key-format ascii requires a key string via -k/--key.");
                                var ascii = Encoding.ASCII.GetBytes(keyArg);
                                if (ascii.Length < 8)
                                    throw new ArgumentException("ASCII key must be at least 8 bytes. Provide an 8-character key.");
                                // take first 8 bytes (common expectation)
                                keyBytes = ascii.Length == 8 ? ascii : new ReadOnlySpan<byte>(ascii).Slice(0, 8).ToArray();
                                break;
                            case "file":
                                if (string.IsNullOrEmpty(keyArg))
                                    throw new ArgumentException("--key-format file requires a path via -k/--key or use --key-file.");
                                keyBytes = fileProcessor.ReadInput(keyArg);
                                break;
                            default:
                                throw new ArgumentException($"Unknown key format '{keyFormat}'. Supported: hex, ascii, file.");
                        }
                    }
                    else
                    {
                        // Fallback to previous heuristic: if -k value looks like hex parse hex, otherwise treat as file path
                        var candidate = keyArg!;
                        bool isHex = candidate.Length % 2 == 0 && candidate.All(c => Uri.IsHexDigit(c));
                        if (isHex)
                        {
                            keyBytes = Convert.FromHexString(candidate);
                        }
                        else
                        {
                            keyBytes = fileProcessor.ReadInput(candidate);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Failed to obtain key bytes: {ex.Message}", ex);
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
            Console.WriteLine("  encrypt|decrypt            Mode of operation.");
            Console.WriteLine("  -i, --input <path>         Input file path (use '-' for stdin).");
            Console.WriteLine("  -o, --output <path>        Output file path (use '-' for stdout).");
            Console.WriteLine("  -k, --key <hex|path>       Key string (hex or path) or positional key.");
            Console.WriteLine("  --key-format <hex|file|ascii>  Explicitly specify how to interpret -k value.");
            Console.WriteLine("  --key-file <path>          Explicitly provide a key file (alternative to -k for files).");
            Console.WriteLine();
            Console.WriteLine("Notes:");
            Console.WriteLine("  If --key-format is omitted the tool will try to parse -k as hex (even-length hex digits),");
            Console.WriteLine("  otherwise -k is treated as a path to a key file. Use --key-format ascii to supply an 8-char ASCII key.");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  DesCli encrypt -i sample.bin -o sample.enc -k 0123456789ABCDEF --key-format hex");
            Console.WriteLine("  DesCli encrypt -i sample.bin -o sample.enc --key-file keyfile.bin");
            Console.WriteLine("  DesCli encrypt -i sample.bin -o sample.enc -k mysecretK --key-format ascii");
        }
    }
}