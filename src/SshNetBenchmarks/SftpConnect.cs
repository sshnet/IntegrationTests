using BenchmarkDotNet.Attributes;
using Renci.SshNet;
using SshNetTests;

namespace SshNetBenchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(invocationCount:1)]
    public class SftpConnect
    {
        private SftpClient _client;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var authenticationMethodFactory = new AuthenticationMethodFactory();
            var linuxVmConnectionFactory = new LinuxVMConnectionFactory(authenticationMethodFactory);

            _client = new SftpClient(linuxVmConnectionFactory.Create(authenticationMethodFactory.CreateRegulatUserPasswordAuthenticationMethod()));
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            if (_client != null)
            {
                _client.Dispose();
            }
        }

        [Benchmark]
        public void ConnectAndDisconnect()
        {
            _client.Connect();
            _client.Disconnect();
        }
    }
}
