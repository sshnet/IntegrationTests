namespace SshNet.TestTools.OpenSSH
{
    public abstract class NetworkConnectivityDisruptor
    {
        public abstract void Start();

        public abstract void End();

        public static NetworkConnectivityDisruptor Create()
        {
            // ToDo: Here we decide which environment we are running in... somehow :)
            // Maybe environment variables: easy to setup and persist independently of the code?
//            return new DockerNetworkConnectivityDisruptor(containerName: "sshnet");
            return new PhysicalNetworkConnectivityDisruptor(connectionName: "Management");
        }
    }
}
