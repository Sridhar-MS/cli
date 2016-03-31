using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Microsoft.DotNet.ProjectModel;
using FluentAssertions;

namespace Microsoft.DotNet.Tools.Test.Utilities
{
    public class NetworkHelper
    {
        // in milliseconds
        private const int PingTimeout = 10000;
        
        private static Queue<TcpListener> s_PortPool = new Queue<TcpListener>();
        
        public static string Localhost { get; } = "http://localhost";
        
        public static bool IsServerUp(string url)
        {
            using(var ping = new Ping())
            {
                var pingReply = ping.SendPingAsync(url, PingTimeout).Result;
                return pingReply.Status == IPStatus.Success;
            }
        }
        
        public static void TestGetRequest(string url, string expectedResponse)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                
                HttpResponseMessage response = client.GetAsync("").Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    responseString.Should().Contain(expectedResponse);                    
                }
            }
        }
        
        public static int GetFreePort()
        {
            lock(s_PortPool)
            {
                if(s_PortPool.Count == 0)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        var tcpl = new TcpListener(IPAddress.Loopback, 0);
                        tcpl.Start();
                        s_PortPool.Enqueue(tcpl);
                    }
                }
                
                var currentTcpl = s_PortPool.Dequeue();
                var port = ((IPEndPoint)currentTcpl.LocalEndpoint).Port;
                currentTcpl.Stop();
                return port;
            }
        }
        
        public static string GetLocalhostUrlWithFreePort()
        {
            return $"http://{Localhost}:{GetFreePort()}";
        }
    }
}