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

            var environment = new Environment();
            Globals.Register(environment);

            var errors = new ErrorReporter(path);
            Run(text, environment, errors);
            if (errors.Count != 0)
                return 65;  // ??
            return 0;
        }

        static void RunPrompt()
        {
            var environment = new Environment();
            for (; ; )
            {
                Console.Write("> ");
                var text = Console.ReadLine();
                var errors = new ErrorReporter("interactive");
                Run(text, environment, errors);
            }
        }

        static void Run(string text, Environment environment, IErrorReporter errors)
        {
            var scanner = new Scanner(text, errors);
            var tokens = scanner.ScanTokens();
            if (errors.Count == 0)
            {
                var parser = new Parser(tokens, errors);
                var program = parser.Parse();
                if (errors.Count == 0)
                {
                    Console.WriteLine(ProgramPrinter.Print(program));

                    // TODO: Feels kinda weird to be creating the interpreter
                    // before the resolver. Maybe there's an intermediate data structure
                    // that could be passed from one to the other?
                    var interpreter = new Interpreter(environment, errors);
                    var resolver = new Resolver(interpreter, errors);
                    resolver.Resolve(program);

                    if (errors.Count == 0)
                        interpreter.Interpret(program);
                }
            }
        }
    }

    internal class ErrorReporter : IErrorReporter
    {
        string _file;
        int _count = 0;

        public ErrorReporter(string file)
        {
            _file = file;
        }

        public void AddScannerError(int line, string message)
        {
            Console.Error.WriteLine("{0}:{1}: {2}", _file, line, message);
            ++_count;
        }

        public void AddParserError(Token token, string message)
        {
            AddError(token, message);
        }

        public void AddRuntimeError(RuntimeError err)
        {
            Console.Error.WriteLine("{0}:{1}: {2}", _file, err.Op.Line, err.Message);
            ++_count;
        }

        public void AddResolverError(Token name, string message)
        {
            AddError(name, message);
        }

        private void AddError(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
                Console.Error.WriteLine("{0}:{1}: at end {2}", _file, token.Line, message);
            else
                Console.Error.WriteLine("{0}:{1}: at '{2}' {3}", _file, token.Line, token.Lexeme, message);
            ++_count;
        }

        public int Count
        {
            get { return _count; }
        }
    }
}
