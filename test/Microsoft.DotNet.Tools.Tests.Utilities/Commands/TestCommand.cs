using Microsoft.DotNet.Cli.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.DotNet.Tools.Test.Utilities
{
    public class TestCommand
    {
        protected string _command;
        
        public TestCommand(string command)
        {
            _command = command;
        }

        public virtual CommandResult Execute(string args)
        {
            Console.WriteLine($"Executing - {_command} {args}");
            var commandResult = Command.Create(_command, args)
                .ForwardStdErr()
                .ForwardStdOut()
                .Execute();

            return commandResult;
        }
    }
}
