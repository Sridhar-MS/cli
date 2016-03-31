// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.AspNetCore.Server.Kestrel.Filter;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;

namespace SampleApp
{
    public class Startup
    {
        private static string Args { get; set; }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IApplicationEnvironment env)
        {
            var ksi = app.ServerFeatures.Get<IKestrelServerInformation>();
            ksi.NoDelay = true;

            loggerFactory.AddConsole(LogLevel.Trace);

            app.UseKestrelConnectionLogging();

            app.Run(async context =>
            {
                Console.WriteLine("{0} {1}{2}{3}",
                    context.Request.Method,
                    context.Request.PathBase,
                    context.Request.Path,
                    context.Request.QueryString);
                Console.WriteLine($"Method: {context.Request.Method}");
                Console.WriteLine($"PathBase: {context.Request.PathBase}");
                Console.WriteLine($"Path: {context.Request.Path}");
                Console.WriteLine($"QueryString: {context.Request.QueryString}");

                var connectionFeature = context.Connection;
                Console.WriteLine($"Peer: {connectionFeature.RemoteIpAddress?.ToString()} {connectionFeature.RemotePort}");
                Console.WriteLine($"Sock: {connectionFeature.LocalIpAddress?.ToString()} {connectionFeature.LocalPort}");

                var content = $"Hello world!{Environment.NewLine}Received '{Args}' from command line.";
                context.Response.ContentLength = content.Length;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync(content);
            });
        }

        public static void Main(string[] args)
        {
            Args = string.Join(" ", args);
            Console.WriteLine(Args);

            var host = new WebHostBuilder()
                .UseServer("Microsoft.AspNetCore.Server.Kestrel")
                .UseUrls("http://localhost:5000")
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
