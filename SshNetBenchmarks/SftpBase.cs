using System.IO;
using Renci.SshNet;
using System;

namespace SshNetBenchmarks
{
    public abstract class SftpBase
    {
        protected static void SftpCreateRemoteFile(ConnectionInfo connectionInfo, string remoteFile, int size)
        {
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();

                SftpCreateRemoteFile(client, remoteFile, size);
            }
        }

        protected static void SftpCreateRemoteFile(SftpClient client, string remoteFile, int size)
        {
            var file = CreateTempFile(size);

            try
            {
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    SftpUploadFileFromStream(client, fs, false, remoteFile);
                }
            }
            finally
            {
                File.Delete(file);
            }
        }

        private static void SftpUploadFileFromStream(SftpClient client, Stream input, bool cleanup, string remoteFile)
        {
            if (client.Exists(remoteFile))
                client.Delete(remoteFile);

            client.UploadFile(input, remoteFile);

            if (cleanup)
            {
                if (client.Exists(remoteFile))
                    client.Delete(remoteFile);
            }
        }

        protected static string CreateTempFile(int size)
        {
            var file = Path.GetTempFileName();
            CreateFile(file, size);
            return file;
        }

        protected static void CreateFile(string fileName, int size)
        {
            using (var fs = File.OpenWrite(fileName))
            {
                FillStream(fs, size);
            }
        }

        protected static MemoryStream CreateMemoryStream(int size)
        {
            var stream = new MemoryStream(size);
            FillStream(stream, size);
            return stream;
        }

        private static void FillStream(Stream stream, int size)
        {
            var randomContent = new byte[50];
            var random = new Random();

            var numberOfBytesToWrite = size;

            while (numberOfBytesToWrite > 0)
            {
                random.NextBytes(randomContent);

                var numberOfCharsToWrite = Math.Min(numberOfBytesToWrite, randomContent.Length);
                stream.Write(randomContent, 0, numberOfCharsToWrite);
                numberOfBytesToWrite -= numberOfCharsToWrite;
            }
        }
    }
}
