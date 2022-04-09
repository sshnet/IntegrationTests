using System;
using System.Diagnostics;
using System.Threading;

namespace SshNet.TestTools.OpenSSH
{
    public sealed class DockerNetworkConnectivityDisruptor : NetworkConnectivityDisruptor
    {
        public DockerNetworkConnectivityDisruptor(string containerName)
        {
            ContainerName = containerName;
        }

        private string ContainerName { get; }

        public override void Start()
        {
            var process = DockerCommand($"pause {ContainerName}");
            if (!process.WaitForExit(30000))
            {
                process.Kill();
                throw new ApplicationException("Failed to pause the 'sshnet' container: timed out.");
            }
            if (process.ExitCode != 0)
            {
                throw new ApplicationException($"Failed to pause the 'sshnet' container: status {process.ExitCode}.");
            }
        }

        public override void End()
        {
            var process = DockerCommand($"unpause {ContainerName}");
            if (!process.WaitForExit(30000))
            {
                process.Kill();
                throw new ApplicationException("Failed to resume the 'sshnet' container: timed out.");
            }
            if (process.ExitCode != 0)
            {
                throw new ApplicationException($"Failed to resume the 'sshnet' container: status {process.ExitCode}.");
            }
        }

        private static Process DockerCommand(string command)
        {
            return Process.Start(
                new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                });
        }
    }
}
