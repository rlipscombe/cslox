using System;
using System.Collections.Generic;

namespace cslox
{
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

        internal List<Token> ScanTokens()
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
                        _errors.AddScannerError(_line, "Unexpected character");
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
                _errors.AddScannerError(_line, "Unterminated string");
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
}
