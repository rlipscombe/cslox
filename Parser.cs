using System;
using System.Collections.Generic;

namespace cslox
{
    public class Parser
    {
        List<Token> _tokens;
        private int _current = 0;

        private IErrorReporter _errors;

        public Parser(List<Token> tokens, IErrorReporter errors)
        {
            _tokens = tokens;
            _errors = errors;
        }

        public Expr Parse()
        {
            return Expression();
        }

        // expression :: equality
        private Expr Expression()
        {
            return Equality();
        }

        private Expr LeftBinary(Func<Expr> leftP, TokenType[] tokens, Func<Expr> rightP)
        {
            Expr expr = leftP();

            while (MatchAny(tokens))
            {
                var op = Previous();
                var right = rightP();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        // equality :: comparison ( ( "!=" | "==" ) comparison )* ;
        private Expr Equality()
        {
            return LeftBinary(Comparison, new TokenType[] { TokenType.BangEqual, TokenType.EqualEqual }, Comparison);
        }

        // comparison :: addition ( ( ">" | ">=" | "<" | "<=" ) addition )* ;
        private Expr Comparison()
        {
            return LeftBinary(Addition, new TokenType[] {
                TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual}, Addition);
        }

        // addition :: multiplication ( ( "-" | "+" ) multiplication )* ;
        private Expr Addition()
        {
            return LeftBinary(Multiplication, new TokenType[] { TokenType.Minus, TokenType.Plus }, Multiplication);
        }

        // multiplication :: unary ( ( "*" | "/" ) unary )* ;
        private Expr Multiplication()
        {
            return LeftBinary(Unary, new TokenType[] { TokenType.Star, TokenType.Slash }, Unary);
        }

        // unary :: ( "!" | "-" ) unary | primary ;
        private Expr Unary()
        {
            if (MatchAny(TokenType.Bang, TokenType.Minus))
            {
                var op = Previous();
                var right = Unary();
                return new Expr.Unary(op, right);
            }

            return Primary();
        }

        // primary :: NUM | STR | "false" | "true" | "nil" | "(" expression ")" ;
        private Expr Primary()
        {
            if (MatchAny(TokenType.False)) return new Expr.Literal(false);
            if (MatchAny(TokenType.True)) return new Expr.Literal(false);
            if (MatchAny(TokenType.Nil)) return new Expr.Literal(null);

            if (MatchAny(TokenType.Number, TokenType.String))
            {
                return new Expr.Literal(Previous().Literal);
            }

            if (MatchAny(TokenType.LeftParen))
            {
                var expr = Expression();
                Consume(TokenType.RightParen, "Expect ')' after expression");
                return new Expr.Grouping(expr);
            }

            _errors.AddError(Peek(), "Expect expression");
            throw new ParseException();
        }

        private bool MatchAny(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsEOF())
                return false;
            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (!IsEOF())
                ++_current;

            return Previous();
        }

        private bool IsEOF()
        {
            return Peek().Type == TokenType.EOF;
        }

        private Token Peek()
        {
            return _tokens[_current];
        }

        private Token Previous()
        {
            return _tokens[_current - 1];
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type))
                return Advance();

            _errors.AddError(Peek(), message);
            throw new ParseException();
        }
    }
}
