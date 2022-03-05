using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;

namespace Renci.SshNet
{
    /// <summary>
    /// Collection of different extension method
    /// </summary>
    internal static partial class Extensions
    {
        public static bool IsEqualTo(this byte[] left, byte[] right)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");

            if (left == right)
                return true;

            if (left.Length != right.Length)
                return false;

            for (var i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                    return false;
            }

            return true;
        }
    }
}
