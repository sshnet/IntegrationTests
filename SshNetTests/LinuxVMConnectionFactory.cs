using Renci.SshNet;

namespace SshNetTests
{
    public class LinuxVMConnectionFactory : IConnectionInfoFactory
    {
        private const int Port = 22;

        private const string ProxyHost = "127.0.0.1";
        private const int ProxyPort = 1234;
        private const string ProxyUserName = "test";
        private const string ProxyPassword = "123";
        private readonly string _host;
        private readonly AuthenticationMethodFactory _authenticationMethodFactory;

        public LinuxVMConnectionFactory()
        {
            _host = Hosts.Ubuntu1910Desktop;
            _authenticationMethodFactory = new AuthenticationMethodFactory();
        }

        public LinuxVMConnectionFactory(AuthenticationMethodFactory authenticationMethodFactory)
        {
            _host = Hosts.Ubuntu1910Desktop;
            _authenticationMethodFactory = authenticationMethodFactory;
        }

        public ConnectionInfo Create()
        {
            return Create(_authenticationMethodFactory.CreateRegularUserPrivateKeyAuthenticationMethod());
        }

        public ConnectionInfo Create(params AuthenticationMethod[] authenticationMethods)
        {
            return new ConnectionInfo(_host, Port, Users.Regular.UserName, authenticationMethods);
        }

        public ConnectionInfo CreateWithProxy()
        {
            return CreateWithProxy(_authenticationMethodFactory.CreateRegularUserPrivateKeyAuthenticationMethod());
        }

        public ConnectionInfo CreateWithProxy(params AuthenticationMethod[] authenticationMethods)
        {
            return new ConnectionInfo(
                _host,
                Port,
                Users.Regular.UserName,
                ProxyTypes.Socks4,
                ProxyHost,
                ProxyPort,
                ProxyUserName,
                ProxyPassword,
                authenticationMethods);
        }
    }
}

