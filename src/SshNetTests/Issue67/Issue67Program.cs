using System;
using System.Diagnostics;

namespace SshNetTests.Issue67
{
    class Issue67Program
    {
        private const string Host = "192.168.1.122";
        private const int Port = 22;

        public static void Start()
        {
            Stopwatch stopwatch = new Stopwatch();

            Renci.SshNet.SshClient sshNet = new Renci.SshNet.SshClient(Host, Users.Regular.UserName, Users.Regular.Password);
            stopwatch.Restart();
            sshNet.Connect();
            stopwatch.Stop();
            Console.Write("sshNet.Connect()   ");
            Console.WriteLine(stopwatch.ElapsedMilliseconds + " milliseconds");
            stopwatch.Restart();
            Renci.SshNet.SshCommand sshCommand = sshNet.RunCommand("free -m");
            stopwatch.Stop();
            Console.Write("sshNet.RunCommand()   ");
            Console.WriteLine(stopwatch.ElapsedMilliseconds + " milliseconds");

#if NETFRAMEWORK
            Tamir.SharpSsh.SshExec sharpSsh = new Tamir.SharpSsh.SshExec(Host, Users.Regular.UserName, Users.Regular.Password);
            stopwatch.Restart();
            sharpSsh.Connect();
            stopwatch.Stop();
            Console.Write("sharpSsh.Connect()   ");
            Console.WriteLine(stopwatch.ElapsedMilliseconds + " milliseconds");
            stopwatch.Restart();
            string result = sharpSsh.RunCommand("free -m");
            stopwatch.Stop();
            Console.Write("sharpSsh.RunCommand()   ");
            Console.WriteLine(stopwatch.ElapsedMilliseconds + " milliseconds");
#endif // NETFRAMEWORK

            MySshClient mySshClient_SshNet = new MySshClient(Host, Users.Regular.UserName, Users.Regular.Password, "sshnet");
            stopwatch.Restart();
            mySshClient_SshNet.Connect();
            stopwatch.Stop();
            Console.Write("mySshClient_SshNet.Connect()   ");
            Console.WriteLine(stopwatch.ElapsedMilliseconds + " milliseconds");
            stopwatch.Restart();
            string[] results1 = mySshClient_SshNet.RunCommand("free -m");
            stopwatch.Stop();
            Console.Write("mySshClient_SshNet.RunCommand()   ");
            Console.WriteLine(stopwatch.ElapsedMilliseconds + " milliseconds");

            MySshClient mySshClient_SharpSsh = new MySshClient(Host, Users.Regular.UserName, Users.Regular.Password, "sharpssh");
            stopwatch.Restart();
            mySshClient_SharpSsh.Connect();
            stopwatch.Stop();
            Console.Write("mySshClient_SharpSsh.Connect()   ");
            Console.WriteLine(stopwatch.ElapsedMilliseconds + " milliseconds");
            stopwatch.Restart();
            string[] results2 = mySshClient_SharpSsh.RunCommand("free -m");
            stopwatch.Stop();
            Console.Write("mySshClient_SharpSsh.RunCommand()   ");
            Console.WriteLine(stopwatch.ElapsedMilliseconds + " milliseconds");
        }
    }
}