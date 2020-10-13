using System;
using System.Collections.Generic;
using System.Text;

namespace Life
{
    class ParamCountException : Exception
    {
        public ParamCountException() { }

        public ParamCountException(string param, int expected, int received)
            : base($"{param} requires {expected} parameter(s). " +
                    $"Received {received}.")
        { }
    }
}
