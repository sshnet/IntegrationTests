using Renci.SshNet;
using SshNetTests;
using BenchmarkDotNet.Attributes;

namespace SshNetBenchmarks
{
    [MemoryDiagnoser]
    public class SftpOpenRead : SftpBase
    {
        private SftpClient _client;
        private string _remoteFileBig;
        private int _fileSizeBig;
        private string _remoteFileSmall;
        private int _fileSizeSmall;
        private byte[] _clientBuffer32755;
        private byte[] _clientBuffer32768;
        private byte[] _clientBuffer65536;
        private uint _defaultBufferSize;
        private byte[] _clientBufferDefault;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var authenticationMethodFactory = new AuthenticationMethodFactory();
            var linuxVmConnectionFactory = new LinuxVMConnectionFactory(authenticationMethodFactory);

            _client = new SftpClient(linuxVmConnectionFactory.Create(authenticationMethodFactory.CreateRegulatUserPasswordAuthenticationMethod()));
            _client.Connect();

            _defaultBufferSize = _client.BufferSize;

            _clientBufferDefault = new byte[_defaultBufferSize];
            _clientBuffer32755 = new byte[32755];
            _clientBuffer32768 = new byte[32 * 1024];
            _clientBuffer65536 = new byte[64 * 1024];

            // file size should be smaller than buffer size of SftpFileStream and buffer that is used to read
            _remoteFileSmall = "/home/sshnet/small";
            _fileSizeSmall = 10_000;
            SftpCreateRemoteFile(_client, _remoteFileSmall, _fileSizeSmall);

            _remoteFileBig = "/home/sshnet/big";
            _fileSizeBig = 50_000_000;
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
        public void SmallFile_OpenRead_SftpClientDefaultBufferSize_ClientDefaultBufferSize()
        {
            _client.BufferSize = _defaultBufferSize;

            OpenRead(_client, _remoteFileSmall, _clientBufferDefault);
        }

        [Benchmark]
        public void SmallFile_OpenRead_SftpClientBuffer32755_ClientBuffer32755()
        {
            _client.BufferSize = 32755;

            OpenRead(_client, _remoteFileSmall, _clientBuffer32755);
        }

        [Benchmark]
        public void SmallFile_OpenRead_SftpClientBuffer32768_ClientBuffer32768()
        {
            _client.BufferSize = 32768;

            OpenRead(_client, _remoteFileSmall, _clientBuffer32768);
        }

        [Benchmark]
        public void SmallFile_OpenRead_SftpClientBuffer65536_ClientBuffer65536()
        {
            _client.BufferSize = 65536;

            OpenRead(_client, _remoteFileSmall, _clientBuffer65536);
        }

        [Benchmark]
        public void  BigFile_OpenRead_SftpClientDefaultBufferSize_ClientDefaultBufferSize()
        {
            _client.BufferSize = _defaultBufferSize;

            OpenRead(_client, _remoteFileBig, _clientBufferDefault);
        }

        //[Benchmark]
        public void BigFile_OpenRead_SftpClientBuffer32755_ClientBuffer32755()
        {
            _client.BufferSize = 32755;

            OpenRead(_client, _remoteFileBig, _clientBuffer32755);
        }

        private void OpenRead(SftpClient sftpClient, string remoteFile, byte[] clientBuffer)
        {
            using (var fs = sftpClient.OpenRead(remoteFile))
            {
                var bytesRead = fs.Read(clientBuffer, 0, clientBuffer.Length);
                while (bytesRead > 0)
                {
                    bytesRead = fs.Read(clientBuffer, 0, clientBuffer.Length);
                }
            }
        }
    }
}
