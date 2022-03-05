using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SshNetTests.Issue67
{
    public class MySshClient : IDisposable
    {
        public MySshClient(string host, string userName, string password, string sshStreamType)
        {
            _host = host;
            _userName = userName;
            _password = password;
            _sshStreamType = sshStreamType;
        }

        private string _host;
        private string _userName;
        private string _password;
        private string _sshStreamType;
        private int _noResponseTimeoutSeconds = 60;

        private Component component = new Component();
        private bool disposed = false;

        ~MySshClient()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    component.Dispose();
                }

                this.Close();

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private ISshStream SshStream
        {
            get;
            set;
        }

        public void Connect()
        {
            SshStream = SshStreamFactory.CreateSshStream(_sshStreamType);
            SshStream.Connect(_host, _userName, _password);

            try
            {
                UnblockStreamReader = new UnblockStreamReader(SshStream.GetStreamReader());
                InitEnv();
            }
            catch (Exception ex)
            {
                this.Close();
                throw ex;
            }
        }

        protected virtual void InitEnv()
        {
            string[] response = null;
            this.SshStream.Write("set +o vi");
            this.SshStream.Write("set +o viraw");
            this.SshStream.Write("export PROMPT_COMMAND=");
            this.SshStream.Write("export PS1=" + Prompt);
            response = this.ReadResponse("export PS1=" + Prompt + "\r\n", _noResponseTimeoutSeconds);
            response = this.ReadResponse(Prompt, _noResponseTimeoutSeconds);

            string helloMessage = "Hello this is test message!";
            this.SshStream.Write("echo '" + helloMessage + "'");
            response = this.ReadResponse(helloMessage + "\r\n", _noResponseTimeoutSeconds);
            response = this.ReadResponse(Prompt, _noResponseTimeoutSeconds);

            this.SshStream.Write("stty columns 512");
            response = this.ReadResponse(Prompt, _noResponseTimeoutSeconds);

            this.SshStream.Write("stty rows 24");
            response = this.ReadResponse(Prompt, _noResponseTimeoutSeconds);

            this.SshStream.Write("export LANG=en_US.UTF-8");
            response = this.ReadResponse(Prompt, _noResponseTimeoutSeconds);

            this.SshStream.Write("export NLS_LANG=American_America.ZHS16GBK");
            response = this.ReadResponse(Prompt, _noResponseTimeoutSeconds);

            this.SshStream.Write("unalias grep");
            response = this.ReadResponse(Prompt, _noResponseTimeoutSeconds);
        }

        protected virtual void Close()
        {
            if (UnblockStreamReader != null)
            {
                UnblockStreamReader.Close();
                UnblockStreamReader = null;
            }
            if (SshStream != null)
            {
                SshStream.Close();
                SshStream = null;
            }
        }

        protected UnblockStreamReader UnblockStreamReader
        {
            get;
            private set;
        }

        public String Prompt
        {
            get
            {
                return "[SHINE_COMMAND_PROMPT]";
            }
        }

        public void Write(string data)
        {
            if (SshStream == null)
            {
                this.Connect();
            }
            if (UnblockStreamReader.GetUnreadBufferLength() > 0)
            {
                UnblockStreamReader.ReadToEnd();
            }
            this.SshStream.Write(data);
        }

        public string[] ReadResponse(string prompt, int noResponseTimeoutSeconds)
        {
            List<UntilInfo> untilInfoList = new List<UntilInfo>() { new UntilInfo(prompt) };
            string[] response = UnblockStreamUtility.ReadUntil(UnblockStreamReader, untilInfoList, noResponseTimeoutSeconds);
            return response;
        }

        public string[] ReadResponse(List<UntilInfo> untilInfoList, int noResponseTimeoutSeconds)
        {
            string[] response = UnblockStreamUtility.ReadUntil(UnblockStreamReader, untilInfoList, noResponseTimeoutSeconds);
            return response;
        }

        public string[] RunCommand(string command)
        {
            SshStream.Write(command);
            return ReadResponse(Prompt, _noResponseTimeoutSeconds);
        }
    }
}