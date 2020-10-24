using System;
using System.Collections.Generic;
using System.Text;

namespace Life
{
    /// <summary>
    /// The exception thrown when a seed file is of an unknown/invalid version
    /// </summary>
    class SeedVersionException : Exception
    {
        /// <summary>
        /// Provides a generic message containing the invalid version found in the file
        /// </summary>
        /// <param name="version"></param>
        public SeedVersionException(string version) : base($"Invalid seed version: {version}")
        {
        }
    }
}
