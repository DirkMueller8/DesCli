using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesCli
{
    public class DesKeyScheduler : IKeyScheduler
    {
        public byte[][] GenerateRoundKeys(byte[] key)
        {
            // TODO: Implement round key generation.
            return Array.Empty<byte[]>();
        }
    }
}
