using System;
using System.Collections.Generic;
using Xunit;

namespace cslox.tests
{
    public class PrintTests
    {
        class CaptureErrors : IErrorReporter
        {
            private List<string> _errors = new List<string>();

            public void AddParserError(Token token, string message)
            {
                _errors.Add(string.Format("parser: {0} {1}", token, message));
            }

            public void AddRuntimeError(RuntimeError err)
            {
                throw new NotImplementedException();
            }

            public void AddScannerError(int line, string message)
            {
                throw new NotImplementedException();
            }

            public List<string> ToList()
            {
                return _errors;
            }
        }

        private List<string> Run(string source)
        {
            var errors = new CaptureErrors();

            var scanner = new Scanner(source, errors);
            var tokens = scanner.ScanTokens();
            var parser = new Parser(tokens, errors);
            List<Stmt> program = new List<Stmt>();
            try
            {
                program = parser.Parse();
            }
            catch (ParseError)
            {
                // TODO?
            }

            var environment = new Environment();
            var interpreter = new Interpreter(environment, errors);
            try
            {
                interpreter.Interpret(program);
            }
            catch (RuntimeError)
            {
                // TODO: Capture this too?
            }

            return errors.ToList();
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
            Assert.Contains("parser: 1: EOF   Expect ';' after value", Run(source));
        }
    }
}
