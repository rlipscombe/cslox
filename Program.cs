using System;
using System.IO;

namespace cslox
{
    class Program
    {
        static int Main(string[] args)
        {
            var expr = new Expr.Binary(
                new Expr.Unary(
                    new Token(TokenType.Minus, "-", null, 1),
                    new Expr.Literal(123)),
                new Token(TokenType.Star, "*", null, 1),
                new Expr.Grouping(
                    new Expr.Literal(45.67)));

            Console.WriteLine(AstPrinter.Print(expr));

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

            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
        }
    }

    internal class ErrorReporter : IErrorReporter
    {
        public void AddError(int line, string message)
        {
            Console.Error.WriteLine("{0}: {1}", line, message);
            // TODO: hadError?
        }
    }
}
