using System;
namespace DesCli
{
    class Program
    {
        static void Main(string[] args)
        {
            var cliHandler = new CliHandler();
            cliHandler.Handle(args);
        }
    }
}
