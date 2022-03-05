using System;

namespace SshNetTests.Issue67
{
    internal static class SshStreamFactory
    {
        public static ISshStream CreateSshStream(string sshStreamType)
        {
            switch (sshStreamType)
            {
#if NETFRAMEWORK
                case "sharpssh":
                    return new SharpSshStream();
#endif // NETFRAMEWORK
                case "sshnet":
                    return new SshNetStream();
                default:
                    throw new Exception("Invalid SshStream type:" + sshStreamType);
            }
        }
    }
}