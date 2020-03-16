using System;

namespace cslox
{
    public class RuntimeError : Exception
    {
        public RuntimeError(Token op, string message)
        {
            Op = op;
            Message = message;
        }

        public Token Op { get; private set; }
        public new string Message { get; private set; }
    }
}
