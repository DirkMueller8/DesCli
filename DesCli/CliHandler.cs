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
        private readonly IKeyScheduler keyScheduler;
        private readonly IFileProcessor fileProcessor;

        public CliHandler()
        {
            // Add dependency injections or instantiate classes directly.
            desCipher = new DesCipher();
            keyScheduler = new DesKeyScheduler();
            fileProcessor = new FileProcessor();
        }

        public void Handle(string[] args)
        {
            // TODO: Parse args and route to encrypt/decrypt.
        }
    }
}
