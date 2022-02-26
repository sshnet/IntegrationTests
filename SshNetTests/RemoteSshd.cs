using System;
using System.IO;
using System.Linq;
using System.Text;
using Renci.SshNet;
using SshNet.TestTools.OpenSSH;

namespace SshNetTests
{
    internal class RemoteSshd
    {
        private readonly IConnectionInfoFactory _connectionInfoFactory;

        public RemoteSshd(IConnectionInfoFactory connectionInfoFactory)
        {
            _connectionInfoFactory = connectionInfoFactory;
        }

        public RemoteSshdConfig OpenConfig()
        {
            return new RemoteSshdConfig(this, _connectionInfoFactory);
        }

        public RemoteSshd Restart()
        {
            // Restart SSH daemon
            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var stopCommand = client.CreateCommand("sudo systemctl stop ssh");
                var stopOutput = stopCommand.Execute();
                if (stopCommand.ExitStatus != 0)
                {
                    throw new ApplicationException($"Stopping ssh service failed with exit code {stopCommand.ExitStatus}.\r\n{stopOutput}");
                }

                var resetFailedCommand = client.CreateCommand("sudo systemctl reset-failed ssh");
                var resetFailedOutput = resetFailedCommand.Execute();
                if (resetFailedCommand.ExitStatus != 0)
                {
                    throw new ApplicationException($"Reset failures for ssh service failed with exit code {resetFailedCommand.ExitStatus}.\r\n{resetFailedOutput}");
                }

                var startCommand = client.CreateCommand("sudo systemctl start ssh");
                var startOutput = startCommand.Execute();
                if (startCommand.ExitStatus != 0)
                {
                    throw new ApplicationException($"Starting ssh service failed with exit code {startCommand.ExitStatus}.\r\n{startOutput}");
                }
            }

            return this;
        }
    }

    internal class RemoteSshdConfig
    {
        private const string SshdConfigFilePath = "/etc/ssh/sshd_config";
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false, true);

        private readonly RemoteSshd _remoteSshd;
        private readonly IConnectionInfoFactory _connectionInfoFactory;
        private readonly SshdConfig _config;

        public RemoteSshdConfig(RemoteSshd remoteSshd, IConnectionInfoFactory connectionInfoFactory)
        {
            _remoteSshd = remoteSshd;
            _connectionInfoFactory = connectionInfoFactory;

            using (var client = new ScpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var memoryStream = new MemoryStream())
                {
                    client.Download(SshdConfigFilePath, memoryStream);

                    memoryStream.Position = 0;
                    _config = SshdConfig.LoadFrom(memoryStream, Encoding.UTF8);
                }
            }
        }

        public RemoteSshdConfig WithChallengeResponseAuthentication(bool value)
        {
            _config.ChallengeResponseAuthentication = value;
            return this;
        }

        public RemoteSshdConfig WithAuthenticationMethods(string user, string authenticationMethods)
        {
            var sshNetMatch = _config.Matches.FirstOrDefault(m => m.Users.Contains(user));
            if (sshNetMatch == null)
            {
                sshNetMatch = new Match(new[] { user }, new string[0]);
                _config.Matches.Add(sshNetMatch);
            }

            sshNetMatch.AuthenticationMethods = authenticationMethods;

            return this;
        }

        public RemoteSshdConfig ClearCiphers()
        {
            _config.Ciphers.Clear();
            return this;
        }

        public RemoteSshdConfig AddCipher(Cipher cipher)
        {
            _config.Ciphers.Add(cipher);
            return this;
        }

        public RemoteSshdConfig ClearKeyExchangeAlgorithms()
        {
            _config.KeyExchangeAlgorithms.Clear();
            return this;
        }

        public RemoteSshdConfig AddKeyExchangeAlgorithm(KeyExchangeAlgorithm keyExchangeAlgorithm)
        {
            _config.KeyExchangeAlgorithms.Add(keyExchangeAlgorithm);
            return this;
        }

        public RemoteSshdConfig ClearHostKeyAlgorithms()
        {
            _config.HostKeyAlgorithms.Clear();
            return this;
        }

        public RemoteSshdConfig AddHostKeyAlgorithm(HostKeyAlgorithm hostKeyAlgorithm)
        {
            _config.HostKeyAlgorithms.Add(hostKeyAlgorithm);
            return this;
        }

        public RemoteSshdConfig ClearSubsystems()
        {
            _config.Subsystems.Clear();
            return this;
        }

        public RemoteSshdConfig AddSubsystem(Subsystem subsystem)
        {
            _config.Subsystems.Add(subsystem);
            return this;
        }

        public RemoteSshdConfig WithLogLevel(LogLevel logLevel)
        {
            _config.LogLevel = logLevel;
            return this;
        }

        public RemoteSshdConfig WithUsePAM(bool usePAM)
        {
            _config.UsePAM = true;
            return this;
        }

        public RemoteSshdConfig ClearHostKeyFiles()
        {
            _config.HostKeyFiles.Clear();
            return this;
        }

        public RemoteSshdConfig AddHostKeyFile(string hostKeyFile)
        {
            _config.HostKeyFiles.Add(hostKeyFile);
            return this;
        }

        public RemoteSshd Update()
        {
            using (var client = new ScpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var memoryStream = new MemoryStream())
                using (var sw = new StreamWriter(memoryStream, Utf8NoBom))
                {
                    sw.NewLine = "\n";
                    _config.SaveTo(sw);
                    sw.Flush();

                    memoryStream.Position = 0;

                    client.Upload(memoryStream, SshdConfigFilePath);
                }
            }

            return _remoteSshd;
        }
    }
}
