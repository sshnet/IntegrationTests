using System;
using System.IO;
using System.Threading;

namespace SshNetTests.Issue67
{
    class UnblockStreamReaderLockObject
    {
        public char[] buffer;
        public int len;
        public int pos;
    };

    public class UnblockStreamReader
    {
        const int BUFFER_SIZE = 65536;
        Thread readThread;
        UnblockStreamReaderLockObject lockObject;
        public int GetUnreadBufferLength()
        {
            return lockObject.len;
        }
        public int GetUnreadBufferPosition()
        {
            return lockObject.pos;
        }
        StreamReader streamReader;

        public UnblockStreamReader(StreamReader streamReader)
        {
            lockObject = new UnblockStreamReaderLockObject();

            lockObject.buffer = new char[BUFFER_SIZE + 1];
            lockObject.len = 0;
            lockObject.pos = 0;

            this.streamReader = streamReader;
            readThread = new Thread(this.ReadThreadProc);
            readThread.Name = "UnblockStreamReader thread";
            readThread.Start();
        }

        public void Close()
        {
            readThread.Abort();
            lock (lockObject)
            {
                lockObject.len = 0;
                lockObject.pos = 0;
            }
        }

        private void ReadThreadProc(object param)
        {
            char[] buf = new char[1];
            int readLen = 0;
            bool isSleep = false;
            try
            {
                while (true)
                {
                    lock (lockObject)
                    {
                        if (lockObject.len >= BUFFER_SIZE)
                        {
                            isSleep = true;
                        }
                    }
                    if (isSleep == true)
                    {
                        isSleep = false;
                        Thread.Sleep(10);
                        continue;
                    }
                    readLen = this.streamReader.Read(buf, 0, 1);
                    if (readLen > 0)
                    {
                        lock (lockObject)
                        {
                            if ((lockObject.pos + lockObject.len) >= BUFFER_SIZE)
                            {
                                for (int i = 0; i < lockObject.len; i++)
                                {
                                    lockObject.buffer[i] = lockObject.buffer[lockObject.pos + i];
                                }
                                lockObject.pos = 0;
                            }

                            lockObject.buffer[lockObject.pos + lockObject.len] = buf[0];

                            lockObject.len++;
                        }
                    }
                    else
                        Thread.Sleep(10);
                }
            }
            catch (Exception e)
            {
                e.ToString();
                return;
            }
        }

        public int ReadChar(ref char buf)
        {
            lock (lockObject)
            {
                if (lockObject.len == 0)
                {
                    return 0;
                }
                buf = lockObject.buffer[lockObject.pos];
                lockObject.pos++;
                lockObject.len--;
            }
            return 1;
        }

        public String ReadToEnd(bool isRemove = true)
        {
            String resultString;
            lock (lockObject)
            {
                if (lockObject.len == 0)
                {
                    return null;
                }
                resultString = new String(lockObject.buffer, lockObject.pos, lockObject.len);
                if (isRemove)
                {
                    lockObject.pos = 0;
                    lockObject.len = 0;
                }
                return resultString;
            }
        }

        public String ReadLine(char lineEndFlag = '\n')
        {
            String resultString;
            while (true)
            {
                lock (lockObject)
                {
                    if (lockObject.len == 0)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    for (int i = 0; i < lockObject.len; i++)
                    {
                        if (lockObject.buffer[lockObject.pos + i] == lineEndFlag)
                        {
                            resultString = new String(lockObject.buffer, lockObject.pos, i + 1);
                            lockObject.pos = lockObject.pos + i + 1;
                            lockObject.len = lockObject.len - i - 1;
                            return resultString;
                        }
                    }
                }
                Thread.Sleep(10);
            }
        }
    }
}