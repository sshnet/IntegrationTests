using BenchmarkDotNet.Running;

namespace SshNetBenchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            //RunSftpUploadFileTest();
            BenchmarkRunner.Run(typeof(Program).Assembly);
#else
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
#endif
        }

        static void RunSftpReadAllTest()
        {
            var x = new SftpReadAllText();
            x.GlobalSetup();
            x.SmallFile();
            x.GlobalCleanup();
        }

        static void RunSftpUploadFileTest()
        {
            var x = new SftpUploadFile();
            x.GlobalSetup();
            x.BigFile_MemoryStream_SftpClientBufferSize32739();
            //x.BigFile_MemoryStream_SftpClientDefaultBufferSize();
            //x.SmallFile_MemoryStream_SftpClientDefaultBufferSize();
            x.GlobalCleanup();
        }

    }
}