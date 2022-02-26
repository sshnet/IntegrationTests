using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet;
using Renci.SshNet.Common;
using SshNet.TestTools.OpenSSH;
using SshNetTests.Common;
using System.Linq;

namespace SshNetTests
{
    [TestClass]
    public class HostKeyAlgorithmTests
    {
        private IConnectionInfoFactory _connectionInfoFactory;
        private RemoteSshdConfig _remoteSshdConfig;

        [TestInitialize]
        public void SetUp()
        {
            _connectionInfoFactory = new LinuxVMConnectionFactory();
            _remoteSshdConfig = new RemoteSshd(new LinuxAdminConnectionFactory()).OpenConfig();
        }

        [TestCleanup]
        public void TearDown()
        {
            if (_remoteSshdConfig != null)
            {
                _remoteSshdConfig.Reset();
            }
        }

        /// <summary>
        
        [TestMethod]
        [Ignore] // No longer supported in recent versions of OpenSSH
        public void SshDsa()
        {
            _remoteSshdConfig.ClearHostKeyAlgorithms()
                             .AddHostKeyAlgorithm(HostKeyAlgorithm.SshDsa)
                             .ClearHostKeyFiles()
                             .AddHostKeyFile(HostKeyFile.Dsa.FilePath)
                             .Update()
                             .Restart();

            HostKeyEventArgs hostKeyEventsArgs = null;

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.HostKeyReceived += (sender, e) => hostKeyEventsArgs = e;
                client.Connect();
                client.Disconnect();
            }

            Assert.IsNotNull(hostKeyEventsArgs);
            Assert.AreEqual(HostKeyFile.Dsa.KeyName, hostKeyEventsArgs.HostKeyName);
            Assert.AreEqual(1024, hostKeyEventsArgs.KeyLength);
            Assert.IsTrue(hostKeyEventsArgs.FingerPrint.SequenceEqual(HostKeyFile.Dsa.FingerPrint));
        }

        [TestMethod]
        public void SshRsa()
        {
            _remoteSshdConfig.ClearHostKeyAlgorithms()
                             .AddHostKeyAlgorithm(HostKeyAlgorithm.SshRsa)
                             .Update()
                             .Restart();

            HostKeyEventArgs hostKeyEventsArgs = null;

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.HostKeyReceived += (sender, e) => hostKeyEventsArgs = e;
                client.Connect();
                client.Disconnect();
            }

            Assert.IsNotNull(hostKeyEventsArgs);
            Assert.AreEqual(HostKeyFile.Rsa.KeyName, hostKeyEventsArgs.HostKeyName);
            Assert.AreEqual(3072, hostKeyEventsArgs.KeyLength);
            Assert.IsTrue(hostKeyEventsArgs.FingerPrint.SequenceEqual(HostKeyFile.Rsa.FingerPrint));
        }

        [TestMethod]
        public void SshEd25519()
        {
            _remoteSshdConfig.ClearHostKeyAlgorithms()
                             .AddHostKeyAlgorithm(HostKeyAlgorithm.SshEd25519)
                             .ClearHostKeyFiles()
                             .AddHostKeyFile(HostKeyFile.Ed25519.FilePath)
                             .Update()
                             .Restart();

            HostKeyEventArgs hostKeyEventsArgs = null;

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.HostKeyReceived += (sender, e) => hostKeyEventsArgs = e;
                client.Connect();
                client.Disconnect();
            }

            Assert.IsNotNull(hostKeyEventsArgs);
            Assert.AreEqual(HostKeyFile.Ed25519.KeyName, hostKeyEventsArgs.HostKeyName);
            Assert.AreEqual(256, hostKeyEventsArgs.KeyLength);
            Assert.IsTrue(hostKeyEventsArgs.FingerPrint.SequenceEqual(HostKeyFile.Ed25519.FingerPrint));
        }

        private void Client_HostKeyReceived(object sender, Renci.SshNet.Common.HostKeyEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
