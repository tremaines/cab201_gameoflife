using System;
using System.Collections.Generic;
using System.Text;

namespace Life
{
    /// <summary>
    /// The exception thrown when a line in a seed file contains an error
    /// </summary>
    class SeedLineException : Exception
    {
        /// <summary>
        /// Allows for a custom message to be provided
        /// </summary>
        /// <param name="message">The custom message</param>
        /// <param name="lineNum">The line number the error occurs on</param>
        public SeedLineException(string message, int lineNum) : base($"Seed file error on line {lineNum}: {message}") 
        { }

        /// <summary>
        /// Provides a message when there is an inappropriate number of coordinates in a line
        /// </summary>
        /// <param name="lineNum">The line number the error occurs on</param>
        /// <param name="expected">The expected number of coordinates</param>
        public SeedLineException(int lineNum, int expected) 
            : base($"Seed file error on line {lineNum}: Unexpected number of coordinates (expected: {expected}).")
        { }
    }
}
