using System;
using System.Collections.Generic;
using System.Text;

namespace Life
{

    class SeedVersionException : Exception
    {
        public SeedVersionException(string version) : base($"Invalid seed version: {version}")
        {
        }
    }
}
