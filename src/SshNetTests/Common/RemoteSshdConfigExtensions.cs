﻿using SshNet.TestTools.OpenSSH;

namespace SshNetTests.Common
{
    internal static class RemoteSshdConfigExtensions
    {
        private const string DefaultAuthenticationMethods = "password publickey";

        public static void Reset(this RemoteSshdConfig remoteSshdConfig)
        {
            remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, DefaultAuthenticationMethods)
                            .WithChallengeResponseAuthentication(false)
                            .WithKeyboardInteractiveAuthentication(false)
                            .WithLogLevel(LogLevel.Debug3)
                            .ClearHostKeyFiles()
                            .AddHostKeyFile(HostKeyFile.Rsa.FilePath)
                            .ClearSubsystems()
                            .AddSubsystem(new Subsystem("sftp", "/usr/lib/ssh/sftp-server"))
                            .ClearCiphers()
                            .ClearKeyExchangeAlgorithms()
                            .ClearHostKeyAlgorithms()
                            .AddHostKeyAlgorithm(HostKeyAlgorithm.SshRsa)
                            .ClearPublicKeyAcceptedAlgorithms()
                            .AddPublicKeyAcceptedAlgorithms(PublicKeyAlgorithm.SshRsa)
                            .WithUsePAM(true)
                            .Update()
                            .Restart();
        }
    }
}
