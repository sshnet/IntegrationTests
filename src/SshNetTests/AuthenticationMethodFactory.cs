﻿using System;
using System.IO;
using Renci.SshNet;

namespace SshNetTests
{
    public class AuthenticationMethodFactory
    {
        public PasswordAuthenticationMethod CreatePowerUserPasswordAuthenticationMethod()
        {
            var user = Users.Admin;
            return new PasswordAuthenticationMethod(user.UserName, user.Password);
        }

        public PrivateKeyAuthenticationMethod CreateRegularUserPrivateKeyAuthenticationMethod()
        {
            var privateKeyFile = GetPrivateKey("SshNetTests.resources.client.id_rsa");
            return new PrivateKeyAuthenticationMethod(Users.Regular.UserName, privateKeyFile);
        }

        public PrivateKeyAuthenticationMethod CreateRegularUserPrivateKeyAuthenticationMethodWithBadKey()
        {
            var privateKeyFile = GetPrivateKey("SshNetTests.resources.client.id_noaccess.rsa");
            return new PrivateKeyAuthenticationMethod(Users.Regular.UserName, privateKeyFile);
        }

        public PasswordAuthenticationMethod CreateRegulatUserPasswordAuthenticationMethod()
        {
            return new PasswordAuthenticationMethod(Users.Regular.UserName, Users.Regular.Password);
        }

        public PasswordAuthenticationMethod CreateRegularUserPasswordAuthenticationMethodWithBadPassword()
        {
            return new PasswordAuthenticationMethod(Users.Regular.UserName, "xxx");
        }

        public KeyboardInteractiveAuthenticationMethod CreateRegularUserKeyboardInteractiveAuthenticationMethod()
        {
            var keyboardInteractive = new KeyboardInteractiveAuthenticationMethod(Users.Regular.UserName);
            keyboardInteractive.AuthenticationPrompt += (sender, args) =>
                {
                    foreach (var authenticationPrompt in args.Prompts)
                        authenticationPrompt.Response = Users.Regular.Password;
                };
            return keyboardInteractive;
        }


        private PrivateKeyFile GetPrivateKey(string resourceName)
        {
            using (var stream = GetResourceStream(resourceName))
            {
                return new PrivateKeyFile(stream);
            }
        }

        private Stream GetResourceStream(string resourceName)
        {
            var type = GetType();
            var resourceStream = type.Assembly.GetManifestResourceStream(resourceName);
            if (resourceStream == null)
            {
                throw new ArgumentException($"Resource '{resourceName}' not found in assembly '{type.Assembly.FullName}'.", nameof(resourceName));
            }
            return resourceStream;
        }
    }
}
