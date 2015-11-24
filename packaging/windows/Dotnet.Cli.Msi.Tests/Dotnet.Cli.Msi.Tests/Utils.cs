using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dotnet.Cli.Msi.Tests
{
    class Utils
    {
        internal static bool ExistsOnPath(string fileName)
        {
            var paths = Environment.GetEnvironmentVariable("PATH");
            return paths
                .Split(';')
                .Any(path => File.Exists(Path.Combine(path, fileName)));
        }
    }
}
