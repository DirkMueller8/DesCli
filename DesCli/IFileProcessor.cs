using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesCli
{
    public interface IFileProcessor
    {
        byte[] ReadInput(string path);
        void WriteOutput(string path, byte[] data);
    }
}
