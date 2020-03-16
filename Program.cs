using System;
using System.IO;

namespace cslox
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: cslox [scriptfile]");
                return 64;
            }
            else if (args.Length == 1)
            {
                return RunFile(args[0]);
            }
            else
            {
                RunPrompt();
                return 0;
            }
        }

        static int RunFile(string path)
        {
            var text = File.ReadAllText(path);

            var errors = new ErrorReporter();
            Run(text, errors);
            if (errors.HadError)
                return 65;
            if (errors.HadRuntimeError)
                return 70;
            return 0;
        }

        static void RunPrompt()
        {
            for (; ; )
            {
                Console.Write("> ");
                var text = Console.ReadLine();
                var errors = new ErrorReporter();
                Run(text, errors);
            }
        }

        static void Run(string text, IErrorReporter errors)
        {
            var scanner = new Scanner(text, errors);
            var tokens = scanner.ScanTokens();
            var parser = new Parser(tokens, errors);
            var expr = parser.Parse();

            Console.WriteLine(AstPrinter.Print(expr));

            // TODO: For persisting variables in the REPL, we'll need a single instance of this.
            var interpreter = new Interpreter(errors);
            interpreter.Interpret(expr);
        }
    }

    internal class ErrorReporter : IErrorReporter
    {
        public bool HadError { get; internal set; }
        public bool HadRuntimeError { get; internal set; }

        public void AddScannerError(int line, string message)
        {
            Console.Error.WriteLine("{0}: {1}", line, message);
            HadError = true;
        }

        public void AddParserError(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
                Console.Error.WriteLine("{0}: at end {1}", token.Line, message);
            else
                Console.Error.WriteLine("{0}: at '{1}' {2}", token.Line, token.Lexeme, message);
            HadError = true;
        }

        public void AddRuntimeError(RuntimeError err)
        {
            Console.Error.WriteLine("{0}: {1}", err.Op.Line, err.Message);
            HadRuntimeError = true;
        }
    }
}
