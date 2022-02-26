using System;

namespace SshConfig
{
    public class Program
    {

        public static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("Usage: dotnet SshConfig.dll [group]");
                return 1;
            }

            var result = Handle(args);
            if (result == 0)
            {
                Console.WriteLine("Finished successfully!");
            }

            return result;
        }

        private static int Handle(string[] args)
        { 
            switch (args[0])
            {
                case "sshd":
                    return SshdHandler.Handle(args);
                case "hosts":
                    return HostsHandler.Handle(args);
                default:
                    Console.Error.WriteLine($"Group '{args[0]}' is not supported.");
                    return 1;
            }
        }
    }
}
