using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SshConfig
{
    public static class HostsHandler
    {
        private static readonly Regex HostsEntryRegEx = new Regex(@"^(?<IPAddress>[\S]+)\s+(?<HostName>[a-zA-Z]+[a-zA-Z\-\.]*[a-zA-Z]+)\s*(?<Aliases>.+)*$", RegexOptions.Singleline);

        public static int Handle(string[] args)
        {
            if (args.Length < 3)
            {
                Console.Error.WriteLine("Usage: dotnet SshConfig.dll hosts [hosts file] [action]");
                return 1;
            }

            switch (args[2])
            {
                case "add":
                    return HandleAdd(args);
                case "remove":
                    return HandleRemove(args);
                default:
                    Console.Error.WriteLine($"hosts action '{args[1]}' is not supported.");
                    return 1;
            }
        }

        private static int HandleAdd(string[] args)
        {
            if (args.Length < 5)
            {
                Console.Error.WriteLine(
                    $"Usage: dotnet SshConfig.dll {args[0]} [hosts file] {args[2]} [IP address] [hostname]");
                return 1;
            }

            var hostsFile = args[1];
            var ipAddress = args[3];
            var hostName = args[4];

            if (!File.Exists(hostsFile))
            {
                Console.Error.WriteLine($"Hosts file '{hostsFile}' does not exist.");
                return 1;
            }

            var entryFound = false;
            var lines = new List<string>();

            using (var fs = File.OpenRead(hostsFile))
            {
                using (var sr = new StreamReader(fs, Encoding.ASCII))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        // skip comments
                        if (line.StartsWith("#"))
                        {
                            lines.Add(line);
                            continue;
                        }

                        var hostEntryMatch = HostsEntryRegEx.Match(line);
                        if (!hostEntryMatch.Success)
                        {
                            lines.Add(line);
                            continue;
                        }

                        var entryIPAddress = hostEntryMatch.Groups["IPAddress"].Value;
                        var entryAliasesGroup = hostEntryMatch.Groups["Aliases"];

                        if (entryIPAddress != ipAddress)
                        {
                            lines.Add(line);
                        }
                        else
                        {
                            // update the entry with  the specified host name

                            var entryBuilder = new StringBuilder();
                            entryBuilder.Append(ipAddress);
                            entryBuilder.Append(' ');
                            entryBuilder.Append(hostName);

                            if (entryAliasesGroup.Success)
                            {
                                entryBuilder.Append(entryAliasesGroup.Value);
                            }

                            lines.Add(entryBuilder.ToString());
                        }
                    }
                }
            }

            if (!entryFound)
            {
                var entryBuilder = new StringBuilder();
                entryBuilder.Append(ipAddress);
                entryBuilder.Append(' ');
                entryBuilder.Append(hostName);
                lines.Add(entryBuilder.ToString());
            }

            RewriteHostsFile(hostsFile, lines);

            return 0;
        }

        private static int HandleRemove(string[] args)
        {
            if (args.Length < 4)
            {
                Console.Error.WriteLine($"Usage: dotnet SshConfig.dll {args[0]} [hosts file] {args[2]} [IP address]");
                return 1;
            }

            var hostsFile = args[1];
            var ipAddress = args[3];

            if (!File.Exists(hostsFile))
            {
                Console.Error.WriteLine($"Hosts file '{hostsFile}' does not exist.");
                return 1;
            }

            var lines = new List<string>();
            var entryFound = false;

            using (var fs = File.OpenRead(hostsFile))
            {
                using (var sr = new StreamReader(fs, Encoding.ASCII))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        // skip comments
                        if (line.StartsWith("#"))
                        {
                            lines.Add(line);
                            continue;
                        }

                        var hostEntryMatch = HostsEntryRegEx.Match(line);
                        if (!hostEntryMatch.Success)
                        {
                            lines.Add(line);
                            continue;
                        }

                        var entryIPAddress = hostEntryMatch.Groups["IPAddress"].Value;

                        if (entryIPAddress == ipAddress)
                        {
                            entryFound = true;
                            continue;
                        }

                        lines.Add(line);
                    }
                }
            }

            if (entryFound)
            {
                RewriteHostsFile(hostsFile, lines);
            }

            return 0;
        }

        private static void RewriteHostsFile(string hostsFile, IEnumerable<string> lines)
        {
            using (var fs = File.Open(hostsFile, FileMode.Truncate, FileAccess.Write))
            using (var sw = new StreamWriter(fs, Encoding.ASCII))
            {
                foreach (var line in lines)
                    sw.WriteLine(line);
            }
        }
    }
}
