using Renci.SshNet;
using System;
using System.IO;

namespace SshNetBenchmarks
{
    public abstract class ScpBase
    {
        protected static bool ScpCreateRemoteFile(ScpClient client, string remoteFile, int size)
        {
            var file = CreateTempFile(size);

            try
            {
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return ScpUploadFileFromStream(client, fs, remoteFile);
                }
            }
            finally
            {
                File.Delete(file);
            }
        }

        private static bool SftpCreateRemoteFile(ConnectionInfo connectionInfo, string remoteFile, int size)
        {
            var file = CreateTempFile(size);

            try
            {
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return SftpUploadFileFromStream(connectionInfo, fs, false, remoteFile);
                }
            }
            finally
            {
                File.Delete(file);
            }
        }

        private static bool SftpUploadFileFromStream(ConnectionInfo connectionInfo, Stream input, bool cleanup,
                                                     string remoteFile)
        {
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();

                var exists = client.Exists(remoteFile);
                if (exists)
                    client.Delete(remoteFile);

                client.UploadFile(input, remoteFile);
            }

            if (cleanup)
            {
                using (var client = new SftpClient(connectionInfo))
                {
                    client.Connect();

                    var exists = client.Exists(remoteFile);
                    if (exists)
                        client.Delete(remoteFile);
                }
            }

            return true;
        }

        private static bool ScpUploadFileFromStream(ScpClient client, Stream input, string remoteFile)
        {
            client.Upload(input, remoteFile);

            return true;
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
