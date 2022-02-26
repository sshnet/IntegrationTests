using Renci.SshNet;
using SshNetTests;
using BenchmarkDotNet.Attributes;

namespace SshNetBenchmarks
{
    [MemoryDiagnoser]
    public class SftpReadAllText : SftpBase
    {
        private SftpClient _client;
        private string _remoteFileBig;
        private int _fileSizeBig;
        private string _remoteFileSmall;
        private int _fileSizeSmall;
        private int _bufferSize;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var authenticationMethodFactory = new AuthenticationMethodFactory();
            var linuxVmConnectionFactory = new LinuxVMConnectionFactory(authenticationMethodFactory);

            _bufferSize = ((32 * 1024) - 13) * 2;
            //_bufferSize = 512 * 1024;
            //_bufferSize = 32 * 1024;

            _client = new SftpClient(linuxVmConnectionFactory.Create(authenticationMethodFactory.CreateRegulatUserPasswordAuthenticationMethod()));
            _client.BufferSize = (uint) _bufferSize;
            _client.Connect();

            // file size should be smaller than buffer size of SftpFileStream and buffer that is used to read
            _remoteFileSmall = "/home/sshnet/small";
            _fileSizeSmall = 10_000;
            SftpCreateRemoteFile(_client, _remoteFileSmall, _fileSizeSmall);

            _remoteFileBig = "/home/sshnet/big";
            _fileSizeBig = 20_000_000;
            SftpCreateRemoteFile(_client, _remoteFileBig, _fileSizeBig);
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
        public void SmallFile()
        {
            _client.ReadAllText(_remoteFileSmall);
        }

        [Benchmark]
        public void BigFile()
        {
            _client.ReadAllText(_remoteFileBig);
        }
    }
}
