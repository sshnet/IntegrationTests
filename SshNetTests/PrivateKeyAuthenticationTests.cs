using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet;
using SshNetTests.Common;

namespace SshNetTests
{
    [TestClass]
    public class PrivateKeyAuthenticationTests : TestBase
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

        [TestMethod]
        public void Ecdsa256()
        {
            var connectionInfo = _connectionInfoFactory.Create(CreatePrivateKeyAuthenticationMethod("key_ecdsa_256_openssh"));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        public void Ecdsa384()
        {

        }

        public void EcdsaA521()
        {
        }

        private PrivateKeyAuthenticationMethod CreatePrivateKeyAuthenticationMethod(string keyResource)
        {
            var privateKey = CreatePrivateKeyFromManifestResource("SshNetTests.resources.client." + keyResource);
            return new PrivateKeyAuthenticationMethod(Users.Regular.UserName, privateKey);
        }

        private PrivateKeyFile CreatePrivateKeyFromManifestResource(string resourceName)
        {
            using (var stream = GetManifestResourceStream(resourceName))
            {
                return new PrivateKeyFile(stream);
            }
        }
    }
}
