using System;
using System.Collections.Generic;
using Xunit;

namespace cslox.tests
{
    public class PrintTests
    {
        class CaptureErrors : IErrorReporter
        {
            private List<string> _errors;

            public CaptureErrors(List<string> errors)
            {
                _errors = errors;
            }

            public void AddScannerError(int line, string message)
            {
                throw new NotImplementedException();
            }

            public void AddParserError(Token token, string message)
            {
                _errors.Add(string.Format("parser: {0}: {1}", token.Line, message));
            }

            public void AddRuntimeError(RuntimeError err)
            {
                throw new NotImplementedException();
            }
        }

        private List<string> Run(string source)
        {
            var errors = new List<string>();
            var capture = new CaptureErrors(errors);

            var scanner = new Scanner(source, capture);
            var tokens = scanner.ScanTokens();
            var parser = new Parser(tokens, capture);
            List<Stmt> program = new List<Stmt>();
            try
            {
                program = parser.Parse();
            }
            catch (ParseError e)
            {
                errors.Add(e.ToString());
            }

            var environment = new Environment();
            var interpreter = new Interpreter(environment, capture);
            try
            {
                interpreter.Interpret(program);
            }
            catch (RuntimeError e)
            {
                errors.Add(e.ToString());
            }

            return errors;
        }

        [Fact]
        public void PrintSimpleAddition()
        {
            // TODO: Console.WriteLine should be abstracted as well.
            var source = @"print 1+2;";
            Assert.Empty(Run(source));
        }

        [Fact]
        public void PrintMissingSemicolon()
        {
            var source = @"print 1+2";
            Assert.Contains("parser: 1: Expect ';' after value", Run(source));
        }

        [Fact]
        public void AssignmentInPrint()
        {
            var source = @"var a = 1;
            print a = 2;";
            Assert.Empty(Run(source));
        }
    }
}
