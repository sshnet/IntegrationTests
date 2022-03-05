using BenchmarkDotNet.Attributes;
using Renci.SshNet;
using SshNetTests;
using System;
using System.Collections.Generic;
using System.IO;

namespace SshNetBenchmarks
{
    public class ScpUpload : ScpBase
    {
        private IConnectionInfoFactory _connectionInfoFactory;
        private ScpClient _client;
        private List<RemoteFile> _remoteFiles;

        public ScpUpload()
        {
            _connectionInfoFactory = new LinuxVMConnectionFactory(new AuthenticationMethodFactory());

            _client = new ScpClient(_connectionInfoFactory.Create());
            _client.Connect();

            _remoteFiles = new List<RemoteFile>
                {
                    new RemoteFile("/home/sshnet/file_0_KB", 0),
                    new RemoteFile("/home/sshnet/file_32_KB", 32 * 1024),
                    new RemoteFile("/home/sshnet/file_50_KB", 50 * 1024),
                    new RemoteFile("/home/sshnet/file_64_KB", 64 * 1024),
                    new RemoteFile("/home/sshnet/file_50_MB", 50 * 1024 * 1024),
                };
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            if (_remoteFiles != null)
            {
                using (SftpClient client = new SftpClient(_connectionInfoFactory.Create()))

                    foreach (var remoteFile in _remoteFiles)
                    {
                        if (client.Exists(remoteFile.Path))
                        {
                            client.DeleteFile(remoteFile.Path);
                        }

                        remoteFile.Dispose();
                    }
            }

            _client?.Dispose();
        }

        [Benchmark]
        [ArgumentsSource(nameof(UploadStreamArguments))]
        public void Upload_Stream(RemoteFile remoteFile, int size)
        {
            remoteFile.Stream.Position = 0;
            _client.Upload(remoteFile.Stream, remoteFile.Path);
        }

        public IEnumerable<object[]> UploadStreamArguments()
        {
            foreach (var remoteFile in _remoteFiles)
            {
                yield return new object[] { remoteFile, remoteFile.Size };
            }
        }

        public class RemoteFile : IDisposable
        {
            public RemoteFile(string path, int size)
            {
                Path = path;
                Size = size;
                Stream = new MemoryStream(size);
            }

            public string Path { get; }
            public int Size { get; }

            public Stream Stream { get; }

            public void Dispose()
            {
                Stream.Dispose();
            }

            public override string ToString()
            {
                return Path;
            }
        }
    }
}
