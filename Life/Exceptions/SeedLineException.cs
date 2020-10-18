using System;
using System.Collections.Generic;
using System.Text;

namespace Life
{
    class SeedLineException : Exception
    {
        public SeedLineException(string message, int lineNum) : base($"Seed file error on line {lineNum}: {message}") 
        { }

        public SeedLineException(int lineNum, int expected) 
            : base($"Seed file error on line {lineNum}: Unexpected number of coordinates (expected: {expected}).")
        { }
    }
}
