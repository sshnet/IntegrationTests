using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;
using SshNet.TestTools.OpenSSH;
using SshNetTests.Common;
using System;
using System.Threading;

namespace SshNetTests
{
    [TestClass]
    public class ConnectivityTests : TestBase
    {
        private readonly NetworkConnectivityDisruptor _networkConnectivityDisruptor = NetworkConnectivityDisruptor.Create();
        private AuthenticationMethodFactory _authenticationMethodFactory;
        private IConnectionInfoFactory _connectionInfoFactory;
        private IConnectionInfoFactory _adminConnectionInfoFactory;
        private RemoteSshdConfig _remoteSshdConfig;

        [TestInitialize]
        public void SetUp()
        {
            _authenticationMethodFactory = new AuthenticationMethodFactory();
            _connectionInfoFactory = new LinuxVMConnectionFactory(_authenticationMethodFactory);
            _adminConnectionInfoFactory = new LinuxAdminConnectionFactory();
            _remoteSshdConfig = new RemoteSshd(_adminConnectionInfoFactory).OpenConfig();
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
        public void Common_CreateMoreChannelsThanMaxSessions()
        {
            var connectionInfo = _connectionInfoFactory.Create();
            connectionInfo.MaxSessions = 2;

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();

                // create one more channel than the maximum number of sessions
                // as that would block indefinitely when creating the last channel
                // if the channel would not be properly closed
                for (var i = 0; i < connectionInfo.MaxSessions + 1; i++)
                {
                    using (var stream = client.CreateShellStream("vt220", 20, 20, 20, 20, 0))
                    {
                        stream.WriteLine("echo test");
                        stream.ReadLine();
                    }
                }
            }
        }

        [TestMethod]
        public void Common_DisposeAfterLossOfNetworkConnectivity()
        {
            try
            {
                var hostNetworkConnectionDisabled = false;

                try
                {
                    Exception errorOccurred = null;

                    using (var client = new SftpClient(_connectionInfoFactory.Create()))
                    {
                        client.ErrorOccurred += (sender, args) => errorOccurred = args.Exception;
                        client.Connect();

                        _networkConnectivityDisruptor.Start();
                        hostNetworkConnectionDisabled = true;
                    }

                    Assert.IsNotNull(errorOccurred);
                    Assert.AreEqual(typeof(SshConnectionException), errorOccurred.GetType());

                    var connectionException = (SshConnectionException)errorOccurred;
                    Assert.AreEqual(DisconnectReason.ConnectionLost, connectionException.DisconnectReason);
                    Assert.IsNull(connectionException.InnerException);
                    Assert.AreEqual("An established connection was aborted by the server.", connectionException.Message);
                }
                finally
                {
                    if (hostNetworkConnectionDisabled)
                    {
                        _networkConnectivityDisruptor.End();
                    }
                }
            }
            catch (NetworkConnectivityDisruptorException ex)
            {
                Assert.Inconclusive(ex.Message);
            }
        }

        [TestMethod]
        public void Common_DetectLossOfNetworkConnectivityThroughKeepAlive()
        {
            try
            {
                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    Exception errorOccurred = null;
                    client.ErrorOccurred += (sender, args) => errorOccurred = args.Exception;
                    client.KeepAliveInterval = new TimeSpan(0, 0, 0, 0, 50);
                    client.Connect();

                    _networkConnectivityDisruptor.Start();

                    try
                    {

                        for (var i = 0; i < 500; i++)
                        {
                            if (!client.IsConnected)
                                break;
                            Thread.Sleep(100);
                        }

                        Assert.IsFalse(client.IsConnected);

                        Assert.IsNotNull(errorOccurred);
                        Assert.AreEqual(typeof(SshConnectionException), errorOccurred.GetType());

                        var connectionException = (SshConnectionException)errorOccurred;
                        Assert.AreEqual(DisconnectReason.ConnectionLost, connectionException.DisconnectReason);
                        Assert.IsNull(connectionException.InnerException);
                        Assert.AreEqual("An established connection was aborted by the server.", connectionException.Message);
                    }
                    finally
                    {
                        _networkConnectivityDisruptor.End();
                    }
                }
            }
            catch (NetworkConnectivityDisruptorException ex)
            {
                Assert.Inconclusive(ex.Message);
            }
        }

        [TestMethod]
        public void Common_DetectConnectionResetThroughSftpInvocation()
        {
            try
            {
                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.KeepAliveInterval = TimeSpan.FromSeconds(1);
                    client.OperationTimeout = TimeSpan.FromSeconds(60);
                    ManualResetEvent errorOccurredSignaled = new ManualResetEvent(false);
                    Exception errorOccurred = null;
                    client.ErrorOccurred += (sender, args) =>
                        {
                            errorOccurred = args.Exception;
                            errorOccurredSignaled.Set();
                        };
                    client.Connect();

                    _networkConnectivityDisruptor.Start();

                    try
                    {
                        client.ListDirectory("/");
                        Assert.Fail();
                    }
                    catch (SshConnectionException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual("Client not connected.", ex.Message);

                        Assert.IsNotNull(errorOccurred);
                        Assert.AreEqual(typeof(SshConnectionException), errorOccurred.GetType());

                        var connectionException = (SshConnectionException)errorOccurred;
                        Assert.AreEqual(DisconnectReason.ConnectionLost, connectionException.DisconnectReason);
                        Assert.IsNull(connectionException.InnerException);
                        Assert.AreEqual("An established connection was aborted by the server.", connectionException.Message);
                    }
                    finally
                    {
                        _networkConnectivityDisruptor.End();
                    }
                }
            }
            catch (NetworkConnectivityDisruptorException ex)
            {
                Assert.Inconclusive(ex.Message);
            }
        }

        [TestMethod]
        public void Common_LossOfNetworkConnectivityDisconnectAndConnect()
        {
            try
            {
                bool vmNetworkConnectionDisabled = false;

                try
                {
                    using (var client = new SftpClient(_connectionInfoFactory.Create()))
                    {
                        Exception errorOccurred = null;
                        client.ErrorOccurred += (sender, args) => errorOccurred = args.Exception;

                        client.Connect();

                        _networkConnectivityDisruptor.Start();
                        vmNetworkConnectionDisabled = true;

                        // disconnect while network connectivity is lost
                        client.Disconnect();

                        Assert.IsFalse(client.IsConnected);

                        _networkConnectivityDisruptor.End();
                        vmNetworkConnectionDisabled = false;

                        // connect when network connectivity is restored
                        client.Connect();
                        client.ChangeDirectory(client.WorkingDirectory);
                        client.Dispose();

                        Assert.IsNull(errorOccurred);
                    }
                }
                finally
                {
                    if (vmNetworkConnectionDisabled)
                    {
                        _networkConnectivityDisruptor.End();
                    }
                }
            }
            catch (NetworkConnectivityDisruptorException ex)
            {
                Assert.Inconclusive(ex.Message);
            }
        }

        [TestMethod]
        public void Common_DetectLossOfNetworkConnectivityThroughSftpInvocation()
        {
            try
            {
                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    ManualResetEvent errorOccurredSignaled = new ManualResetEvent(false);
                    Exception errorOccurred = null;
                    client.ErrorOccurred += (sender, args) =>
                        {
                            errorOccurred = args.Exception;
                            errorOccurredSignaled.Set();
                        };
                    client.Connect();

                    _networkConnectivityDisruptor.Start();

                    try
                    {
                        client.ListDirectory("/");
                        Assert.Fail();
                    }
                    catch (SshConnectionException ex)
                    {
                        Assert.AreEqual(DisconnectReason.ConnectionLost, ex.DisconnectReason);
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual("An established connection was aborted by the server.", ex.Message);
                    }
                    finally
                    {
                        _networkConnectivityDisruptor.End();
                    }
                }
            }
            catch (NetworkConnectivityDisruptorException ex)
            {
                Assert.Inconclusive(ex.Message);
            }
        }

        [TestMethod]
        public void Common_DetectSessionKilledOnServer()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                ManualResetEvent errorOccurredSignaled = new ManualResetEvent(false);
                Exception errorOccurred = null;
                client.ErrorOccurred += (sender, args) =>
                    {
                        errorOccurred = args.Exception;
                        errorOccurredSignaled.Set();
                    };
                client.Connect();

                // Kill the server session
                using (var adminClient = new SshClient(_adminConnectionInfoFactory.Create()))
                {
                    adminClient.Connect();

                    var command = $"sudo ps --no-headers -u {client.ConnectionInfo.Username} -f | grep \"{client.ConnectionInfo.Username}@notty\" | awk '{{print $2}}' | xargs sudo kill -9";
                    var sshCommand = adminClient.CreateCommand(command);
                    var result = sshCommand.Execute();
                    Assert.AreEqual(0, sshCommand.ExitStatus, sshCommand.Error);
                }

                Assert.IsTrue(errorOccurredSignaled.WaitOne(200));
                Assert.IsNotNull(errorOccurred);
                Assert.AreEqual(typeof(SshConnectionException), errorOccurred.GetType());
                Assert.IsNull(errorOccurred.InnerException);
                Assert.AreEqual("An established connection was aborted by the server.", errorOccurred.Message);
                Assert.IsFalse(client.IsConnected);
            }
        }

        [TestMethod]
        public void Common_HostKeyValidation_Failure()
        {
            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.HostKeyReceived += (sender, e) => { e.CanTrust = false; };

                try
                {
                    client.Connect();
                    Assert.Fail();
                }
                catch (SshConnectionException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Key exchange negotiation failed.", ex.Message);
                }
            }
        }

        [TestMethod]
        public void Common_HostKeyValidation_Success()
        {
            byte[] host_rsa_key_openssh_fingerprint =
                {
                    0x3d, 0x90, 0xd8, 0x0d, 0xd5, 0xe0, 0xb6, 0x13,
                    0x42, 0x7c, 0x78, 0x1e, 0x19, 0xa3, 0x99, 0x2b
                };

            var hostValidationSuccessful = false;

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.HostKeyReceived += (sender, e) =>
                    {
                        if (host_rsa_key_openssh_fingerprint.Length == e.FingerPrint.Length)
                        {
                            for (var i = 0; i < host_rsa_key_openssh_fingerprint.Length; i++)
                            {
                                if (host_rsa_key_openssh_fingerprint[i] != e.FingerPrint[i])
                                {
                                    e.CanTrust = false;
                                    break;
                                }
                            }

                            hostValidationSuccessful = e.CanTrust;
                        }
                        else
                        {
                            e.CanTrust = false;
                        }
                    };
                client.Connect();
            }

            Assert.IsTrue(hostValidationSuccessful);
        }

        /// <summary>
        /// Verifies whether we handle a disconnect initiated by the SSH server (through a SSH_MSG_DISCONNECT message).
        /// </summary>
        /// <remarks>
        /// We force this by only configuring <c>keyboard-interactive</c> as authentication method, while <c>ChallengeResponseAuthentication</c>
        /// is not enabled.  This causes OpenSSH to terminate the connection because there are no authentication methods left.
        /// </remarks>
        [TestMethod]
        public void Common_ServerRejectsConnection()
        {
            _remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, "keyboard-interactive")
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(_authenticationMethodFactory.CreateRegularUserKeyboardInteractiveAuthenticationMethod());
            using (var client = new SftpClient(connectionInfo))
            {
                try
                {
                    client.Connect();
                    Assert.Fail();
                }
                catch (SshConnectionException ex)
                {
                    Assert.AreEqual(DisconnectReason.ProtocolError, ex.DisconnectReason);
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("The connection was closed by the server: no authentication methods enabled (ProtocolError).", ex.Message);
                }
            }
        }



    }
}
