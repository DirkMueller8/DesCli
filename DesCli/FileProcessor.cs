using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesCli
{
    public class FileProcessor : IFileProcessor
    {
        public byte[] ReadInput(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));

            // Special-case "-" (stdin) — read all bytes but do not close the console stream.
            if (path == "-")
            {
                var stdin = Console.OpenStandardInput();
                using var ms = new MemoryStream();
                stdin.CopyTo(ms);
                return ms.ToArray();
            }

            if (!File.Exists(path))
                throw new FileNotFoundException($"Input file not found: {path}", path);

            return File.ReadAllBytes(path);
        }

        public void WriteOutput(string path, byte[] data)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            // Use streaming implementation to avoid duplicating logic.
            using var ms = new MemoryStream(data, writable: false);
            WriteOutputStream(path, ms);
        }

        // Streaming API

        public Stream ReadInputStream(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));

            // Return console input stream for "-" (do NOT dispose the returned stream here)
            if (path == "-")
            {
                return Console.OpenStandardInput();
            }

            if (!File.Exists(path))
                throw new FileNotFoundException($"Input file not found: {path}", path);

            // FileStream returned; caller is responsible for disposing it
            return File.OpenRead(path);
        }

        public void WriteOutputStream(string path, Stream data)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            // If writing to stdout, copy directly (do not close console stream)
            if (path == "-")
            {
                var stdout = Console.OpenStandardOutput();
                // copy from current position to end
                data.CopyTo(stdout);
                stdout.Flush();
                return;
            }

            // Ensure parent directory exists (if any)
            var directory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directory))
            {
                directory = Directory.GetCurrentDirectory();
            }
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Write atomically: write to temp file in same directory then move
            var tempFile = Path.Combine(directory, Path.GetRandomFileName());
            try
            {
                // Ensure data stream position is at a readable point (do not rewind if caller
                // intentionally provided a specific position).
                using (var fs = File.Create(tempFile))
                {
                    data.CopyTo(fs);
                    fs.Flush(true);
                }

                // Overwrite target if it exists
                File.Move(tempFile, path, overwrite: true);
            }
            catch
            {
                // If move fails, attempt to remove temp file then rethrow
                if (File.Exists(tempFile))
                {
                    try { File.Delete(tempFile); } catch { /* swallow */ }
                }
                throw;
            }
        }
    }
}