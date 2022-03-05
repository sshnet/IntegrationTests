using Renci.SshNet;

namespace SshNetTests
{
    public class LinuxAdminConnectionFactory : IConnectionInfoFactory
    {
        private const int Port = 22;

        public ConnectionInfo Create()
        {
            var user = Users.Admin;
            return new ConnectionInfo(Hosts.Ubuntu1910Desktop, Port, user.UserName, new PasswordAuthenticationMethod(user.UserName, user.Password));
        }

        public ConnectionInfo Create(params AuthenticationMethod[] authenticationMethods)
        {
            throw new System.NotImplementedException();
        }

        public ConnectionInfo CreateWithProxy()
        {
            throw new System.NotImplementedException();
        }

        public ConnectionInfo CreateWithProxy(params AuthenticationMethod[] authenticationMethods)
        {
            throw new System.NotImplementedException();
        }
    }
}

