namespace cslox
{
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

    public class Token
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
}
