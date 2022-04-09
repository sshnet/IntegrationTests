using System;
using System.Diagnostics;

namespace SshNet.TestTools.OpenSSH
{
    public sealed class DockerNetworkConnectivityDisruptor : NetworkConnectivityDisruptor
    {
        [Obsolete("This method does not work, but is left here to test some other possibilities...")]
        public DockerNetworkConnectivityDisruptor(string containerName)
        {
            ContainerName = containerName;
        }

        private string ContainerName { get; }

        public override void Start()
        {
            try
            {
                var process = DockerCommand($"pause {ContainerName}");
                if (!process.WaitForExit(30000))
                {
                    process.Kill();
                    throw new ApplicationException("Timed out.");
                }
                if (process.ExitCode != 0)
                {
                    throw new ApplicationException($"Status {process.ExitCode}.");
                }
            }
            catch (Exception ex)
            {
                throw new NetworkConnectivityDisruptorException($"Failed to pause container '{ContainerName}': {ex.Message}", ex);
            }
        }

        public override void End()
        {
            try
            {
                var process = DockerCommand($"unpause {ContainerName}");
                if (!process.WaitForExit(30000))
                {
                    process.Kill();
                    throw new ApplicationException("Timed out.");
                }
                if (process.ExitCode != 0)
                {
                    throw new ApplicationException($"Status {process.ExitCode}.");
                }
            }
            catch (Exception ex)
            {
                throw new NetworkConnectivityDisruptorException($"Failed to resume container '{ContainerName}': {ex.Message}", ex);
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
