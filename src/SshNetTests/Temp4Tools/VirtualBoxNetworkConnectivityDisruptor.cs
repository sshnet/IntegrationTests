using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace SshNet.TestTools.OpenSSH
{
    public sealed class VirtualBoxNetworkConnectivityDisruptor : NetworkConnectivityDisruptor
    {
        private const string vmName = "sshnet";

        public override void Start()
        {
            SetLinkState(vmName, on: false);
        }

        public override void End()
        {
            SetLinkState(vmName, on: true);
        }

        private static string VirtualBoxFolder
        {
            get
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    if (!Environment.Is64BitProcess)
                    {
                        // dotnet test runs tests in a 32-bit process (no watter what I f***in' try), so let's hard-code the
                        // path to VirtualBox
                        return Path.Combine("c:\\Program Files", "Oracle", "VirtualBox");
                    }
                }

                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Oracle", "VirtualBox");
            }
        }

        private static List<string> GetRunningVMs()
        {
            var runningVmRegex = new Regex("\"(?<name>.+?)\"\\s?(?<uuid>{.+?})");

            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(VirtualBoxFolder, "VBoxManage.exe"),
                Arguments = "list runningvms",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var process = Process.Start(startInfo);
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new ApplicationException($"Failed to get list of running VMs. Exit code is {process.ExitCode}.");
            }

            var runningVms = new List<string>();

            string line;

            while ((line = process.StandardOutput.ReadLine()) != null)
            {
                var match = runningVmRegex.Match(line);
                if (match != null)
                {
                    runningVms.Add(match.Groups["name"].Value);
                }
            }

            return runningVms;
        }

        private static void SetLinkState(string vmName, bool on)
        {
            var linkStateValue = (on ? "on" : "off");

            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(VirtualBoxFolder, "VBoxManage.exe"),
                Arguments = $"controlvm \"{vmName}\" setlinkstate1 {linkStateValue}",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var process = Process.Start(startInfo);
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new ApplicationException($"Failed to set linkstate for VM '{vmName}' to '{linkStateValue}'. Exit code is {process.ExitCode}.");
            }
            else
            {
                Console.WriteLine($"Changed linkstate for VM '{vmName}' to '{linkStateValue}.");
            }
        }

        private static void SetPromiscuousMode(string vmName, string value)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(VirtualBoxFolder, "VBoxManage.exe"),
                Arguments = $"controlvm \"{vmName}\" nicpromisc1 {value}",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var process = Process.Start(startInfo);
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new ApplicationException($"Failed to set promiscuous for VM '{vmName}' to '{value}'. Exit code is {process.ExitCode}.");
            }
            else
            {
                Console.WriteLine($"Changed promiscuous for VM '{vmName}' to '{value}'.");
            }
        }

        private static void DisableVirtualMachineNetworkConnection()
        {
            var runningVMs = GetRunningVMs();
//            Assert.AreEqual(1, runningVMs.Count);

            SetLinkState(runningVMs[0], false);
            Thread.Sleep(1000);
        }

        private static void EnableVirtualMachineNetworkConnection()
        {
            var runningVMs = GetRunningVMs();
//            Assert.AreEqual(1, runningVMs.Count);

            SetLinkState(runningVMs[0], true);
            Thread.Sleep(1000);
        }

        private static void ResetVirtualMachineNetworkConnection()
        {
            var runningVMs = GetRunningVMs();
//            Assert.AreEqual(1, runningVMs.Count);

            SetPromiscuousMode(runningVMs[0], "allow-all");
            Thread.Sleep(1000);
            SetPromiscuousMode(runningVMs[0], "deny");
            Thread.Sleep(1000);
        }

    }
}
