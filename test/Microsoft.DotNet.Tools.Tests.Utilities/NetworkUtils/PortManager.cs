using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace Microsoft.DotNet.Tools.Test.Utilities
{
    public static class PortManager
    {
        private static int _nextPort = 8001;

        public static int GetPort()
        {
            Interlocked.Increment(ref _nextPort);
            while (!IsPortFree(_nextPort))
            {
                Interlocked.Increment(ref _nextPort);
            }
            
            return _nextPort;
        }
        
        private static bool IsPortFree(int port)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
               if (tcpi.LocalEndPoint.Port==port)
               {
                   return false;
               }
            }
            
            return true;
        }
    }
}
