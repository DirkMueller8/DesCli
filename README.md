## Manual Coding Data Encryption Standard (DES)
***********************************************
Software:	&emsp;	C# 12 / .NET8

Version: &emsp;   	1.0

Date: 	&emsp;		Nov 4, 2025

Author:	&emsp;		Dirk Mueller
************************************************

### Introduction ###
This is a console C# application for taking a text from file of via the command
line as an input for encryption by the Data Encryption Standard (DES) algorithm.
The encrypted data is shwon, and lastly the decryption is performed.

It uses PKCS#7 padding padding to 16 bytes (DES needs 8-byte blocks). No salt or initialization vector is used.

### Architecture of Application ###  
The modular architecture is implemented from the following hierarchy:  

├── CliHandler (parse args, route to logic)  
│   ├── IDesCipher  
│   │   └── DesCipher  
│   │        ├── Encrypt(byte[] plaintext, byte[] key): byte[]  
│   │        ├── Decrypt(byte[] ciphertext, byte[] key): byte[]  
│   │        ├── InitialPermutation(byte[] block): byte[]  
│   │        ├── FinalPermutation(byte[] block): byte[]  
│   │        ├── Feistel(byte[] right, byte[] subkey): byte[]  
│   │        └── SBoxSubstitution(byte[] input): byte[]  
│   │     
│   ├── IKeyScheduler  
│   │   └── DesKeyScheduler  
│   │        ├── GenerateRoundKeys(byte[] key): byte[][]  
│   │  
│   └── IFileProcessor  
│        ├── ReadInput(string path): byte[]  
│        └── WriteOutput(string path, byte[] data)  

### Why this design is flexible and maintainable ###  
- Dependency Injection (Dependency Inversion)
- CliHandler accepts IDesCipher and IFileProcessor via constructor injection. Code that depends on abstractions (not concrete classes) is easy to replace; e.g., swap DesCipher for an AesCipher implementation without changing the CLI.

### SOLID adherence ###  
- Single Responsibility: each class has one reason to change (CLI parsing, file handling, cipher math).
- Open/Closed: IDesCipher allows adding new cipher implementations without modifying consumers.
- Liskov & Interface Segregation: interfaces are minimal and focused so implementations don't carry unrelated methods.
- Dependency Inversion: high-level modules depend on abstractions not on low-level concrete implementations.

### Testability###  
- Small interfaces + DI make unit tests trivial. Fake IFileProcessor and IDesCipher are used in tests to validate CLI behavior without touching disk or running full crypto.
- Canonical test vectors and block-level APIs (EncryptBlock) enable bit-exact verification against reference implementations (System.Security.Cryptography.DES).
- Tests run via Test Explorer or dotnet test in CI.
- Observability & developer ergonomics
- Clear usage/help messages, an interactive demo, and hex/Base64 outputs make debugging and demonstration easier.
- Grouped hex and Base64 outputs make comparing implementations straightforward.

### calability and production considerations ###  
- Streaming API for I/O avoids loading large files into memory. The ReadInputStream / WriteOutputStream methods let you process arbitrarily large files block by block.
- Parallelism: the block algorithm is amenable to parallelizing independent blocks (ECB) or pipelining (CBC/Ctr) - the architecture isolates the block primitive so higher-level orchestration can add parallel processing later.
- Replace DES for security: because the system is wired to IDesCipher, switching to AesCipher or TripleDesCipher is a drop-in change - no CLI or I/O code must be rewritten.

### Maintainability and next steps ###  
- Add CI (run tests on push/PR), static analyzers, and code coverage.
- Document key format, CLI conventions (- for stdin/stdout, --key-format), and security caveats (DES is deprecated).
- For production usage, prefer platform crypto (AES-GCM) and authenticated modes. Keep the educational DES code for learning and verification tests.

### One-line summary ###  
- DesCli is intentionally modular: small, focused classes and interfaces give you the freedom to swap cryptography, scale I/O, and write reliable unit tests - all aligned with SOLID design and modern .NET practices.