using System;
using System.Collections.Generic;
using System.Text;

namespace Life
{
    class ParamValueException : Exception
    {
        public ParamValueException(string message) : base(message) { }


        public ParamValueException(string param, int givenVal, int lowerVal, int upperVal)
            : base($"{param} value {givenVal} is outside of the range {lowerVal} - {upperVal}.")
        { }

        public ParamValueException(string param, double givenVal, double lowerVal, double upperVal)
           : base($"{param} value {givenVal} is outside of the range {lowerVal} - {upperVal}.")
        { }
    }
}
