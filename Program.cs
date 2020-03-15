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
                RunFile(args[0]);
                return 0;
            }
            else
            {
                RunPrompt();
                return 0;
            }
        }

        static void RunFile(string path)
        {
            var text = File.ReadAllText(path);
            Run(text);
        }

        static void RunPrompt()
        {
            for (; ; )
            {
                Console.Write("> ");
                var text = Console.ReadLine();
                Run(text);
            }
        }

        static void Run(string text)
        {
            // TODO: Does this get passed in? Where *do* we abort the run?
            var errors = new ErrorReporter();
            var scanner = new Scanner(text, errors);
            var tokens = scanner.ScanTokens();
            var parser = new Parser(tokens, errors);
            var expr = parser.Parse();

            Console.WriteLine(AstPrinter.Print(expr));
        }
    }

    internal class ErrorReporter : IErrorReporter
    {
        public void AddError(int line, string message)
        {
            Console.Error.WriteLine("{0}: {1}", line, message);
            // TODO: hadError?
        }

        public void AddError(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
                Console.Error.WriteLine("{0}: at end {1}", token.Line, message);
            else
                Console.Error.WriteLine("{0}: at '{1}' {2}", token.Line, token.Lexeme, message);
        }
    }
}
