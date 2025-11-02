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
            // TODO: Implement file reading.
            return Array.Empty<byte>();
        }

        public void WriteOutput(string path, byte[] data)
        {
            // TODO: Implement file writing.
        }
    }
}
