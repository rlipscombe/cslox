using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace cslox
{
    class Program
    {
        static int Main(string[] args)
        {
            foreach (var arg in args)
            {
                Console.WriteLine(arg);
            }

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

    class Scanner
    {
        private string _source;
        private IErrorReporter _errors;

        private List<Token> _tokens = new List<Token>();

        private int _start = 0;
        private int _current = 0;
        private int _line = 1;

        static IDictionary<string, TokenType> _keywords = new Dictionary<string, TokenType>();

        static Scanner()
        {
            _keywords.Add("and", TokenType.And);
            _keywords.Add("class", TokenType.And);
            _keywords.Add("else", TokenType.And);
            _keywords.Add("false", TokenType.False);
            _keywords.Add("for", TokenType.For);
            _keywords.Add("fun", TokenType.Fun);
            _keywords.Add("if", TokenType.If);
            _keywords.Add("nil", TokenType.Nil);
            _keywords.Add("or", TokenType.Or);
            _keywords.Add("print", TokenType.Print);
            _keywords.Add("return", TokenType.Return);
            _keywords.Add("super", TokenType.Super);
            _keywords.Add("this", TokenType.This);
            _keywords.Add("true", TokenType.True);
            _keywords.Add("var", TokenType.Var);
            _keywords.Add("while", TokenType.While);
        }

        internal Scanner(string source, IErrorReporter errors)
        {
            _source = source;
            _errors = errors;
        }

        internal IEnumerable<Token> ScanTokens()
        {
            while (!IsEOF())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, "", null, _line));
            return _tokens;
        }

        private bool IsEOF()
        {
            return _current >= _source.Length;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                // Single character.
                case '(': AddToken(TokenType.LeftParen); break;
                case ')': AddToken(TokenType.RightParen); break;
                case '{': AddToken(TokenType.LeftBrace); break;
                case '}': AddToken(TokenType.RightBrace); break;
                case ',': AddToken(TokenType.Comma); break;
                case '.': AddToken(TokenType.Dot); break;
                case '-': AddToken(TokenType.Minus); break;
                case '+': AddToken(TokenType.Plus); break;
                case ';': AddToken(TokenType.Semicolon); break;
                case '*': AddToken(TokenType.Star); break;

                // Single character lookahead.
                case '!': AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang); break;
                case '=': AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal); break;
                case '<': AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less); break;
                case '>': AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater); break;

                // Slash or single-line comment.
                case '/':
                    {
                        if (Match('/'))
                        {
                            while (Peek() != '\n' && !IsEOF())
                                Advance();
                        }
                        else
                        {
                            AddToken(TokenType.Slash);
                        }
                    }
                    break;

                // Whitespace
                case ' ':
                case '\r':
                case '\t':
                    break;

                case '\n':
                    ++_line;
                    break;

                // String literals
                case '"': ScanString(); break;

                default:
                    if (IsDigit(c))
                    {
                        ScanNumber();
                    }
                    else if (IsAlpha(c))
                    {
                        ScanIdentifier();
                    }
                    else
                    {
                        // Don't throw exceptions for the error, because
                        // we want to keep going if possible.
                        _errors.AddError(_line, "Unexpected character");
                    }
                    break;
            }
        }

        private char Advance()
        {
            return _source[_current++];
        }

        private bool Match(char ch)
        {
            if (IsEOF())
                return false;

            if (_source[_current] != ch)
                return false;

            ++_current;
            return true;
        }

        private char Peek()
        {
            if (IsEOF())
                return '\0';

            return _source[_current];
        }

        private char PeekNext()
        {
            if (_current + 1 >= _source.Length)
                return '\0';

            return _source[_current];
        }

        private void ScanString()
        {
            while (Peek() != '"' && !IsEOF())
            {
                if (Peek() == '\n')
                    ++_line;
                Advance();
            }

            if (IsEOF())
            {
                _errors.AddError(_line, "Unterminated string");
                return;
            }

            Advance();

            var value = _source.Substring(_start + 1, _current - _start - 2);
            AddToken(TokenType.String, value);
        }

        private bool IsDigit(char ch)
        {
            return ch >= '0' && ch <= '9';
        }

        private void ScanNumber()
        {
            while (IsDigit(Peek()))
                Advance();

            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();

                while (IsDigit(Peek()))
                    Advance();
            }

            AddToken(TokenType.Number, Double.Parse(_source.Substring(_start, _current - _start)));
        }

        private bool IsAlpha(char ch)
        {
            return (ch >= 'a' && ch <= 'z') ||
                    (ch >= 'A' && ch <= 'Z') ||
                    ch == '_';
        }

        private void ScanIdentifier()
        {
            while (IsAlphaNumeric(Peek()))
                Advance();

            var text = _source.Substring(_start, _current - _start);
            TokenType type;
            if (_keywords.TryGetValue(text, out type))
            {
                AddToken(type);
            }
            else
            {
                AddToken(TokenType.Identifier);
            }
        }

        private bool IsAlphaNumeric(char ch)
        {
            return IsAlpha(ch) || IsDigit(ch);
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object literal)
        {
            var text = _source.Substring(_start, _current - _start);
            _tokens.Add(new Token(type, text, literal, _line));
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

    enum TokenType
    {
        // Single-character tokens
        LeftParen, RightParen,
        LeftBrace, RightBrace,
        Comma, Dot,
        Minus, Plus,
        Semicolon,
        Slash, Star,

        // One *or* two character tokens
        Bang, BangEqual,
        Equal, EqualEqual,
        Greater, GreaterEqual,
        Less, LessEqual,

        // Literals
        Identifier, String, Number,

        // Keywords
        And, Class, Else, False, Fun, For, If, Nil, Or,
        Print, Return, Super, This, True, Var, While,

        EOF
    }

    class Token
    {
        private TokenType _type;
        private string _lexeme;
        private object _literal;
        private int _line;

        internal Token(TokenType type, string lexeme, object literal, int line)
        {
            _type = type;
            _lexeme = lexeme;
            _literal = literal;
            _line = line;
        }

        public string Lexeme
        {
            get { return _lexeme; }
        }

        public override string ToString()
        {
            return string.Format("{0}: {1} {2} {3}", _line, _type, _lexeme, _literal);
        }
    }

    internal interface IErrorReporter
    {
        void AddError(int line, string message);
    }

    abstract class Expr
    {
        public abstract TResult Accept<TResult>(IVisitor<TResult> visitor);


        internal class Binary : Expr
        {
            public Binary(Expr left, Token op, Expr right)
            {
                Left = left;
                Op = op;
                Right = right;
            }

            public Expr Left { get; }
            public Token Op { get; }
            public Expr Right { get; }

            public override TResult Accept<TResult>(IVisitor<TResult> visitor)
            {
                return visitor.VisitBinary(this);
            }
        }

        internal class Literal : Expr
        {
            public override TResult Accept<TResult>(IVisitor<TResult> visitor)
            {
                return visitor.VisitLiteral(this);
            }
        }

        internal interface IVisitor<TResult>
        {
            TResult VisitBinary(Expr.Binary expr);
            TResult VisitLiteral(Expr.Literal expr);
        }
    }

    class AstPrinter : Expr.IVisitor<string>
    {
        public string VisitBinary(Expr.Binary expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);
        }

        public string VisitLiteral(Expr.Literal expr)
        {
            throw new NotImplementedException();
        }

        string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        string Parenthesize(string name, params Expr[] exprs)
        {
            var builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (var expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }

            builder.Append(")");
            return builder.ToString();
        }
    }
}
