using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SshConfig
{
    public static class SshdHandler
    {
        public static int Handle(string[] args)
        {
            if (args.Length != 4)
            {
                Console.Error.WriteLine(
                    "Usage: dotnet SshConfig.dll sshd [location of sshd_config] [authentication methods] [challengeResponseAuthentication]");
                return 1;
            }

            var sshdConfigFile = args[1];
            var authenticationMethods = args[2];
            var challengeResponseAuthentication = bool.Parse(args[3]);

            if (!TryReadConfig(sshdConfigFile, out var sshdConfig))
                return 1;

            #region Update configuration


            sshdConfig.ChallengeResponseAuthentication = challengeResponseAuthentication;

            var sshNetMatch = sshdConfig.Matches.FirstOrDefault(m => m.Users.Contains("sshnet"));
            if (sshNetMatch == null)
            {
                sshNetMatch = new MatchConfiguration(new[] {"sshnet"}, Array.Empty<string>());
                sshdConfig.Matches.Add(sshNetMatch);
            }

            sshNetMatch.AuthenticationMethods = authenticationMethods;

            #endregion Update configuration

            sshdConfig.Save(sshdConfigFile);

            return 0;
        }

        private static bool TryReadConfig(string file, out SshdConfig sshdConfig)
        {
            if (!File.Exists(file))
            {
                Console.Error.WriteLine($"Ssh config file '{file}' does not exist.");
                sshdConfig = null;
                return false;
            }

            var matchRegex = new Regex($@"\s*Match\s+(User\s+(?<users>[\S]+))?\s*(Address\s+(?<addresses>[\S]+))?\s*");
            sshdConfig = new SshdConfig();

            using (var fs = File.OpenRead(file))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            {
                MatchConfiguration currentMatchConfiguration = null;

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var match = matchRegex.Match(line);
                    if (match.Success)
                    {
                        Console.WriteLine("Start match:" + line);
                        if (currentMatchConfiguration != null)
                        {
                            sshdConfig.Matches.Add(currentMatchConfiguration);
                        }

                        var usersGroup = match.Groups["users"];
                        var addressesGroup = match.Groups["addresses"];
                        var users = usersGroup.Success ? usersGroup.Value.Split(',') : Array.Empty<string>();
                        var addresses = addressesGroup.Success ? addressesGroup.Value.Split(',') : Array.Empty<string>();
                        currentMatchConfiguration = new MatchConfiguration(users, addresses);
                        continue;
                    }

                    Console.WriteLine("No match:" + line);

                    if (currentMatchConfiguration != null)
                    {
                        ProcessMatchOption(currentMatchConfiguration, line);
                    }
                    else
                    {
                        ProcessGlobalOption(sshdConfig, line);
                    }
                }
            }

            return true;
        }

        private static void ProcessGlobalOption(SshdConfig sshdConfig, string line)
        {
            var matchOptionRegex = new Regex(@"^\s*(?<name>[\S]+)\s+(?<value>.+?){1}\s*$");

            var optionsMatch = matchOptionRegex.Match(line);
            if (!optionsMatch.Success)
                return;

            var nameGroup = optionsMatch.Groups["name"];
            var valueGroup = optionsMatch.Groups["value"];

            var name = nameGroup.Value;
            var value = valueGroup.Value;

            switch (name)
            {
                case "Port":
                    sshdConfig.Port = ToInt(value);
                    break;
                case "HostKey":
                    sshdConfig.HostKey = value;
                    break;
                case "ServerKeyBits":
                    sshdConfig.ServerKeyBits = ToInt(value);
                    break;
                case "ChallengeResponseAuthentication":
                    sshdConfig.ChallengeResponseAuthentication = ToBool(value);
                    break;
                case "Subsystem":
                    sshdConfig.Subsystems.Add(SubsystemConfiguration.FromConfig(value));
                    break;
                default:
                    throw new Exception($"Global option '{name}' is not implemented.");
            }
        }

        private static void ProcessMatchOption(MatchConfiguration matchConfiguration, string line)
        {
            var matchOptionRegex = new Regex(@"^\s+(?<name>[\S]+)\s+(?<value>.+?){1}\s*$");

            var optionsMatch = matchOptionRegex.Match(line);
            if (!optionsMatch.Success)
                return;

            var nameGroup = optionsMatch.Groups["name"];
            var valueGroup = optionsMatch.Groups["value"];

            var name = nameGroup.Value;
            var value = valueGroup.Value;

            switch (name)
            {
                case "AuthenticationMethods":
                    matchConfiguration.AuthenticationMethods = value;
                    break;
                default:
                    throw new Exception($"Match option '{name}' is not implemented.");
            }
        }

        private static bool ToBool(string value)
        {
            switch (value)
            {
                case "yes":
                    return true;
                case "no":
                    return false;
                default:
                    throw new Exception($"Value '{value}' cannot be mapped to a boolean.");
            }
        }

        private static int ToInt(string value)
        {
            return int.Parse(value, NumberFormatInfo.InvariantInfo);
        }

        private static string ToConfigValue(bool value)
        {
            return value ? "yes" : "no";
        }

        public class SshdConfig
        {
            public SshdConfig()
            {
                Subsystems = new List<SubsystemConfiguration>();
                Matches = new List<MatchConfiguration>();
            }

            public int Port { get; set; }
            public string HostKey { get; set; }
            public int ServerKeyBits { get; set; }
            public bool? ChallengeResponseAuthentication { get; set; }
            public List<SubsystemConfiguration> Subsystems { get; }
            public List<MatchConfiguration> Matches { get; }

            public void Save(string file)
            {
                using (var fs = File.Open(file, FileMode.Truncate, FileAccess.Write))
                using (var sw = new StreamWriter(fs, new UTF8Encoding(false)))
                {
                    sw.WriteLine("Port " + Port.ToString(NumberFormatInfo.InvariantInfo));
                    if (HostKey != null)
                        sw.WriteLine("HostKey " + HostKey);
                    sw.WriteLine("ServerKeyBits " + ServerKeyBits.ToString(NumberFormatInfo.InvariantInfo));
                    if (ChallengeResponseAuthentication.HasValue)
                        sw.WriteLine("ChallengeResponseAuthentication " + ToConfigValue(ChallengeResponseAuthentication.Value));
                    foreach (var subsystem in Subsystems)
                        sw.WriteLine(subsystem.Name + " " + subsystem.Command);
                    foreach (var match in Matches)
                        match.WriteTo(sw);
                }
            }
        }

        public class SubsystemConfiguration
        {
            public SubsystemConfiguration(string name, string command)
            {
                Name = name;
                Command = command;
            }

            public string Name { get; }

            public string Command { get; set; }

            public void WriteTo(TextWriter writer)
            {
                writer.WriteLine(Name + "=" + Command);
            }

            public static SubsystemConfiguration FromConfig(string value)
            {
                var subSystemValueRegex = new Regex(@"^\s*(?<name>[\S]+)\s+(?<command>.+?){1}\s*$");

                var match = subSystemValueRegex.Match(value);
                if (match.Success)
                {
                    var nameGroup = match.Groups["name"];
                    var commandGroup = match.Groups["command"];

                    var name = nameGroup.Value;
                    var command = commandGroup.Value;

                    return new SubsystemConfiguration(name, command);
                }

                throw new Exception($"'{value}' not recognized as value for Subsystem.");
            }
        }

        public class MatchConfiguration
        {
            public MatchConfiguration(string[] users, string[] addresses)
            {
                Users = users;
                Addresses = addresses;
            }

            public string[] Users { get; }

            public string[] Addresses { get; }

            public string AuthenticationMethods { get; set; }

            public void WriteTo(TextWriter writer)
            {
                writer.Write("Match ");

                if (Users.Length > 0)
                {
                    writer.Write("User ");
                    for (var i = 0; i < Users.Length; i++)
                    {
                        if (i > 0)
                            writer.Write(',');
                        writer.Write(Users[i]);
                    }
                }

                if (Addresses.Length > 0)
                {
                    writer.Write("Address ");
                    for (var i = 0; i < Addresses.Length; i++)
                    {
                        if (i > 0)
                            writer.Write(',');
                        writer.Write(Addresses[i]);
                    }
                }

                writer.WriteLine();

                if (AuthenticationMethods != null)
                    writer.WriteLine("    AuthenticationMethods " + AuthenticationMethods);
            }
        }
    }
}
