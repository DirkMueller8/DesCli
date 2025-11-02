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
            // TODO: Implement file writing.
        }
    }
}
