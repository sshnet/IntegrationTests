using Renci.SshNet;

namespace SshNetTests
{
    public interface IConnectionInfoFactory
    {
        ConnectionInfo Create();
        ConnectionInfo Create(params AuthenticationMethod[] authenticationMethods);
        ConnectionInfo CreateWithProxy();
        ConnectionInfo CreateWithProxy(params AuthenticationMethod[] authenticationMethods);
    }
}
