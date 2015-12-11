﻿using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.DotNet.Cli.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.DotNet.Tools.Test.Utilities
{
    public class DirectoryInfoAssertions
    {
        private DirectoryInfo _dirInfo;        

        public DirectoryInfoAssertions(DirectoryInfo dir)
        {
            _dirInfo = dir;                        
        }

        public AndConstraint<DirectoryInfoAssertions> Exist()
        {
            _dirInfo.Exists.Should().BeTrue();
            Execute.Assertion.ForCondition(_dirInfo.Exists)
                .FailWith("Expected directory {0} does not exist.", _dirInfo.FullName);
            return new AndConstraint<DirectoryInfoAssertions>(this);
        }

        public AndConstraint<DirectoryInfoAssertions> HaveFile(string expectedFile)
        {
            var file = _dirInfo.EnumerateFiles(expectedFile, SearchOption.TopDirectoryOnly).SingleOrDefault();
            Execute.Assertion.ForCondition(file != null)
                .FailWith("Expected File {0} cannot be found in directory {1}.", expectedFile, _dirInfo.FullName);
            return new AndConstraint<DirectoryInfoAssertions>(this);
        }

        public AndConstraint<DirectoryInfoAssertions> NotHaveFile(string expectedFile)
        {
            var file = _dirInfo.EnumerateFiles(expectedFile, SearchOption.TopDirectoryOnly).SingleOrDefault();
            Execute.Assertion.ForCondition(file == null)
                .FailWith("File {0} should not be found in directory {1}.", expectedFile, _dirInfo.FullName);
            return new AndConstraint<DirectoryInfoAssertions>(this);
        }

        public AndConstraint<DirectoryInfoAssertions> HaveFiles(IEnumerable<string> expectedFiles)
        {            
            foreach (var expectedFile in expectedFiles)
            {
                HaveFile(expectedFile);
            }

            return new AndConstraint<DirectoryInfoAssertions>(this);
        }

        public AndConstraint<DirectoryInfoAssertions> HaveDirectory(string expectedDir)
        {
            var dir = _dirInfo.EnumerateDirectories(expectedDir, SearchOption.TopDirectoryOnly).SingleOrDefault();
            Execute.Assertion.ForCondition(dir != null)
                .FailWith("Expected directory {0} cannot be found inside directory {1}.", expectedDir, _dirInfo.FullName);
            
            return new AndConstraint<DirectoryInfoAssertions>(new DirectoryInfoAssertions(dir));
        }
    }
}
