using System;
using System.Collections.Generic;
using System.Text;

namespace Life
{
    /// <summary>
    /// The exception thrown when a parameter value is outside the correct range or not of the correct type
    /// </summary>
    /// <author>
    /// Tremaine Stroebel
    /// </author>
    /// <date>
    /// October 2020
    /// </date>
    class ParamValueException : Exception
    {
        /// <summary>
        /// A basic constructer, takes a single string argument
        /// </summary>
        /// <param name="message">A string argument to be provided as the exception message</param>
        public ParamValueException(string message) : base(message) { }

        /// <summary>
        /// A counstructor that takes several parameters to form a predefined message
        /// </summary>
        /// <param name="param">The option the exception is being thrown for</param>
        /// <param name="givenVal">The value provided by the user</param>
        /// <param name="lowerVal">The lowest value allowable</param>
        /// <param name="upperVal">The highest value allowable</param>
        public ParamValueException(string param, int givenVal, int lowerVal, int upperVal)
            : base($"{param} value {givenVal} is outside of the range {lowerVal} - {upperVal}.")
        { }

        /// <summary>
        /// A counstructor that takes several parameters to form a predefined message
        /// </summary>
        /// <param name="param">The option the exception is being thrown for</param>
        /// <param name="givenVal">The value provided by the user</param>
        /// <param name="lowerVal">The lowest value allowable</param>
        /// <param name="upperVal">The highest value allowable</param>
        public ParamValueException(string param, double givenVal, double lowerVal, double upperVal)
           : base($"{param} value {givenVal} is outside of the range {lowerVal} - {upperVal}.")
        { }
    }
}
