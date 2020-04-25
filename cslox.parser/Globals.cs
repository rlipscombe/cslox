using System;
using System.Collections.Generic;

namespace cslox
{
    public static class Globals
    {
        public static void Register(Environment environment)
        {
            environment.Define("clock", new Clock());
        }

        // TODO: Could probably do this with 'dynamic'...?
        class Clock : ILoxCallable
        {
            public int Arity {
                get { return 0; }
            }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            public override string ToString()
            {
                return "<fun clock()>";
            }
        }
    }
}
