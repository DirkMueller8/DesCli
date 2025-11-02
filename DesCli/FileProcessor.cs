using System;
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

            // Support "-" as stdin for CLI usage
            if (path == "-")
            {
                using var stdin = Console.OpenStandardInput();
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

            // Support "-" as stdout for CLI usage
            if (path == "-")
            {
                using var stdout = Console.OpenStandardOutput();
                stdout.Write(data, 0, data.Length);
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
                File.WriteAllBytes(tempFile, data);
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
