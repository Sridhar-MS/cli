using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.Tools.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.DotNet.Tools.Publish.Tests
{
    public sealed class RestoreCommand : TestCommand
    {
        public RestoreCommand()
            : base("dotnet")
        {

        }

        public override CommandResult Execute(string args="")
        {
            args = $"restore {args}";
            return base.Execute(args);
        }
    }
}
