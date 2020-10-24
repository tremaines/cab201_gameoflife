using System;
using System.Collections.Generic;
using System.Text;

namespace Life
{
    /// <summary>
    /// The exception thrown when the incorrect number of parameters is provided for an option
    /// </summary>
    /// <author>
    /// Tremaine Stroebel
    /// </author>
    /// <date>
    /// October 2020
    /// </date>
    class ParamCountException : Exception
    {
        /// <summary>
        /// A basic constructer, takes a single string argument
        /// </summary>
        /// <param name="message">A string argument to be provided as the exception message</param>
        public ParamCountException(string message) : base(message) { }

        /// <summary>
        /// A counstructor that takes several parameters to form a predefined message
        /// </summary>
        /// <param name="param">The option the exception is being thrown for</param>
        /// <param name="expected">The expected number of parameters</param>
        /// <param name="received">The received number of paramets</param>
        public ParamCountException(string param, int expected, int received)
            : base($"{param} requires {expected} parameter(s). " +
                    $"Received {received}.")
        { }
    }
}
