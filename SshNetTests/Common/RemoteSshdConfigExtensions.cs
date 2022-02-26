using SshNet.TestTools.OpenSSH;

namespace SshNetTests.Common
{
    internal static class RemoteSshdConfigExtensions
    {
        private const string DefaultAuthenticationMethods = "password publickey";

        public static void Reset(this RemoteSshdConfig remoteSshdConfig)
        {
            remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, DefaultAuthenticationMethods)
                            .WithChallengeResponseAuthentication(false)
                            .WithLogLevel(LogLevel.Info)
                            .ClearHostKeyFiles()
                            .AddHostKeyFile(HostKeyFile.Rsa.FilePath)
                            .ClearSubsystems()
                            .AddSubsystem(new Subsystem("sftp", "/usr/lib/openssh/sftp-server"))
                            .ClearCiphers()
                            .ClearKeyExchangeAlgorithms()
                            .ClearHostKeyAlgorithms()
                            .WithUsePAM(true)
                            .Update()
                            .Restart();
        }
    }
}
