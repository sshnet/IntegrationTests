using System.IO;

namespace SshNetTests.Issue67
{
    public interface ISshStream
    {
        void Connect(string host, string userName, string password);
        void Close();
        void Write(string data);
        StreamReader GetStreamReader();
        StreamWriter GetStreamWriter();
    }
}
