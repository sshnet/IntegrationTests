using BenchmarkDotNet.Attributes;
using Renci.SshNet;
using SshNetTests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SshNetBenchmarks
{
    public class SftpDownloadFile : SftpBase
    {
        private SftpClient _client;
        private List<RemoteFile> _remoteFiles;

        public SftpDownloadFile()
        {
            var authenticationMethodFactory = new AuthenticationMethodFactory();
            var linuxVmConnectionFactory = new LinuxVMConnectionFactory(authenticationMethodFactory);

            _client = new SftpClient(linuxVmConnectionFactory.Create(authenticationMethodFactory.CreateRegulatUserPasswordAuthenticationMethod()));
            _client.Connect();

            _remoteFiles = new List<RemoteFile>
                {
                    new RemoteFile("/home/sshnet/file_0_KB", 0),
                    new RemoteFile("/home/sshnet/file_32_KB", 32 * 1024),
                    new RemoteFile("/home/sshnet/file_50_KB", 50 * 1024),
                    new RemoteFile("/home/sshnet/file_64_KB", 64 * 1024),
                    new RemoteFile("/home/sshnet/file_50_MB", 50 * 1024 * 1024),
                };

            foreach (var remoteFile in _remoteFiles)
            {
                SftpCreateRemoteFile(_client, remoteFile.Path, remoteFile.Size);
            }
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            if (_client != null)
            {
                if (_remoteFiles != null && _client.IsConnected)
                {
                    foreach (var remoteFile in _remoteFiles)
                    {
                        if (_client.Exists(remoteFile.Path))
                        {
                            _client.Delete(remoteFile.Path);
                        }

                        remoteFile.Dispose();
                    }
                }

                _client.Dispose();
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(DownloadStreamArguments))]
        public void DownloadFile_Stream_Serial(RemoteFile remoteFile, int size)
        {
            remoteFile.Stream.Position = 0;
            _client.DownloadFile(remoteFile.Path, remoteFile.Stream);
        }

        [Benchmark(OperationsPerInvoke = 10 * 50)]
        [ArgumentsSource(nameof(DownloadStreamArguments))]
        public void DownloadFile_Steam_Parallel(RemoteFile remoteFile, int size)
        {
            var downloadTasks = Enumerable.Range(1, 10).Select(i =>
                {
                    return new Task(() =>
                    {
                        using (var memoryStream = new MemoryStream(remoteFile.Size))
                        {
                            for (var j = 0; j < 50; i++)
                            {
                                _client.DownloadFile(remoteFile.Path, memoryStream);
                                memoryStream.Position = 0;
                            }
                        }
                    });
                }).ToArray();

            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.Start();
            }

            Task.WaitAll(downloadTasks);
        }

        public IEnumerable<object[]> DownloadStreamArguments()
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
