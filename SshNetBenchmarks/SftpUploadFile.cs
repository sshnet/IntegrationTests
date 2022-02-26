using BenchmarkDotNet.Attributes;
using Renci.SshNet;
using SshNetTests;
using System.IO;

namespace SshNetBenchmarks
{
    //[MemoryDiagnoser]
    public class SftpUploadFile : SftpBase
    {
        private SftpClient _client;
        private string _remoteFileBig;
        private int _fileSizeBig;
        private MemoryStream _memoryStreamBig;
        private string _remoteFileSmall;
        private int _fileSizeSmall;
        private MemoryStream _memoryStreamSmall;
        private uint _defaultBufferSize;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var authenticationMethodFactory = new AuthenticationMethodFactory();
            var linuxVmConnectionFactory = new LinuxVMConnectionFactory(authenticationMethodFactory);

            _client = new SftpClient(linuxVmConnectionFactory.Create(authenticationMethodFactory.CreateRegulatUserPasswordAuthenticationMethod()));
            _client.Connect();

            _defaultBufferSize = _client.BufferSize;

            _remoteFileSmall = "/home/sshnet/small";
            _fileSizeSmall = 10_000;
            _memoryStreamSmall = CreateMemoryStream(_fileSizeSmall);

            _remoteFileBig = "/home/sshnet/big";
            _fileSizeBig = 50_000_000;
            _memoryStreamBig = CreateMemoryStream(_fileSizeBig);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            if (_client != null)
            {
                _client.Dispose();
            }
        }

        //[Benchmark]
        public void SmallFile_MemoryStream_SftpClient_BufferSize4KB()
        {
            _memoryStreamSmall.Position = 0;
            _client.BufferSize = 4*1024;
            _client.UploadFile(_memoryStreamSmall, _remoteFileSmall);
        }

        //[Benchmark]
        public void SmallFile_MemoryStream_SftpClientDefaultBufferSize()
        {
            _memoryStreamSmall.Position = 0;
            _client.BufferSize = _defaultBufferSize;
            _client.UploadFile(_memoryStreamSmall, _remoteFileSmall);
        }

        //[Benchmark]
        public void BigFile_MemoryStream_SftpClientDefaultBufferSize()
        {
            //262144

            _memoryStreamBig.Position = 0;
            //_client.BufferSize = (128*1024) + 135000;
            _client.BufferSize = 262119; // MAX = 262119 + 25u + 4 
            _client.UploadFile(_memoryStreamBig, _remoteFileBig);
        }

        //[Benchmark]
        public void BigFile_MemoryStream_SftpClient_BufferSize4KB()
        {
            _memoryStreamBig.Position = 0;
            _client.BufferSize = 4 * 1024;
            _client.UploadFile(_memoryStreamBig, _remoteFileBig);
        }

        [Benchmark]
        public void BigFile_MemoryStream_SftpClientBufferSize32739()
        {
            _memoryStreamBig.Position = 0;
            _client.BufferSize = 32739;
            _client.UploadFile(_memoryStreamBig, _remoteFileBig);
        }
    }
}
