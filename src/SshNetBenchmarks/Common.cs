using BenchmarkDotNet.Attributes;
using Renci.SshNet;
using SshNetTests;
using System.Threading;

namespace SshNetBenchmarks
{
    public class Common
    {
        private SshClient _client;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var connectionFactory = new LinuxVMConnectionFactory();

            _client = new SshClient(connectionFactory.Create());
            _client.Connect();
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
        public bool IsConnected_ConnectionIdle()
        {
            return _client.IsConnected;
        }

        [Benchmark(OperationsPerInvoke = 100_000)]
        public void IsConnected_ConnectionBusy()
        {
            var isConnectedThread = new Thread(() =>
                {
                    var isConnectedCount = 0;

                    while (isConnectedCount < 100_000)
                    {
                        if (_client.IsConnected)
                        {
                            isConnectedCount++;
                        }
                        else
                        {
                            isConnectedCount++;
                        }

                        if (isConnectedCount > 100_000)
                        {
                            break;
                        }
                    }
                });

            var executeCommandThread = new Thread(token =>
                {
                    var cancellationToken = (CancellationToken)token;

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var command = _client.CreateCommand("ls");
                        command.Execute();
                    }
                });

            var cancellationTokenSource = new CancellationTokenSource();

            executeCommandThread.Start(cancellationTokenSource.Token);
            isConnectedThread.Start();
            isConnectedThread.Join();

            cancellationTokenSource.Cancel();
            executeCommandThread.Join();
        }
    }
}
