using System;
using System.ComponentModel;
using System.Management;

namespace SshNet.TestTools.OpenSSH
{
    public sealed class PhysicalNetworkConnectivityDisruptor : NetworkConnectivityDisruptor
    {
        public PhysicalNetworkConnectivityDisruptor(string connectionName)
        {
            ConnectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
        }

        private string ConnectionName { get; }


        public override void Start()
        {
            try
            {
                InvokeMethod(GetNetConnection(ConnectionName), "Disable");
            }
            catch (Exception ex)
            {
                throw new NetworkConnectivityDisruptorException($"Failed to disable network connection '{ConnectionName}': {ex.Message}", ex);
            }
        }

        public override void End()
        {
            try
            {
                InvokeMethod(GetNetConnection(ConnectionName), "Enable");
            }
            catch (Exception ex)
            {
                throw new NetworkConnectivityDisruptorException($"Failed to enable network connection '{ConnectionName}': {ex.Message}", ex);
            }
        }

        private ManagementObject GetNetConnection(string connectionName)
        {
            // ToDo: We could probably cache the ManagementObject
            SelectQuery wmiQuery = new SelectQuery($"SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionId = '{connectionName}'");
            ManagementObjectSearcher searchProcedure = new ManagementObjectSearcher(wmiQuery);
            foreach (ManagementObject item in searchProcedure.Get())
            {
                return item;
            }

            throw new Exception("The connection does not exist.");
        }

        private void InvokeMethod(ManagementObject networkConnection, string methodName)
        {
            var returnValue = (uint)networkConnection.InvokeMethod(methodName, null);
            if (returnValue != 0)
            {
                throw new Win32Exception(unchecked((int)returnValue));
            }
        }
    }
}
