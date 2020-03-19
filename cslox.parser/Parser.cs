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

        // program :: declaration* EOF
        public List<Stmt> Parse()
        {
            var statements = new List<Stmt>();
            while (!IsEOF())
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        // declaration :: funDecl | varDecl | statement ;
        private Stmt Declaration()
        {
            try
            {
                if (MatchAny(TokenType.Fun))
                    return FunctionDeclaration("function");
                if (MatchAny(TokenType.Var))
                    return VarDeclaration();

                return Statement();
            }
            catch (ParseError)
            {
                Synchronize();
                return null;
            }
        }

        // funDecl :: "fun" function ;
        // function :: IDENTIFIER "(" parameters? ")" block ;
        // parameters :: IDENTIFIER ( "," IDENTIFIER )* ;
        private Stmt FunctionDeclaration(string kind)
        {
            var name = Consume(TokenType.Identifier, "Expect " + kind + " name");
            Consume(TokenType.LeftParen, "Expect '(' after " + kind + " name");
            var parameters = FunctionParameters();
            Consume(TokenType.RightParen, "Expect ')' after parameters");
            Consume(TokenType.LeftBrace, "Expect '{' before " + kind + " body");
            var body = Block();
            return new Stmt.Function(name, parameters, body);
        }

        private List<Token> FunctionParameters()
        {
            var parameters = new List<Token>();
            if (!Check(TokenType.RightParen))
            {
                do
                {
                    if (parameters.Count >= 255) {
                        _errors.AddParserError(Peek(), "Cannot have more than 255 parameters");
                    }

                    parameters.Add(Consume(TokenType.Identifier, "Expect parameter name"));
                } while (MatchAny(TokenType.Comma));
            }

            return parameters;
        }

        // varDecl :: "var" IDENTIFIER ( "=" expression )? ";" ;
        private Stmt VarDeclaration()
        {
            var name = Consume(TokenType.Identifier, "Expect variable name");

            Expr init = new Expr.Literal(null);
            if (MatchAny(TokenType.Equal))
            {
                init = Expression();
            }

            Consume(TokenType.Semicolon, "Expect ';' after variable declaration");
            return new Stmt.Var(name, init);
        }

        // statement :: exprStmt | forStmt | ifStmt | printStmt | returnStmt | whileStmt | block ;
        private Stmt Statement()
        {
            if (MatchAny(TokenType.If))
                return IfStatement();
            if (MatchAny(TokenType.For))
                return ForStatement();
            if (MatchAny(TokenType.Print))
                return PrintStatement();
            if (MatchAny(TokenType.Return))
                return ReturnStatement();
            if (MatchAny(TokenType.While))
                return WhileStatement();
            if (MatchAny(TokenType.LeftBrace))
                return BlockStatement();

            return ExpressionStatement();
        }

        // exprStmt :: expression ";" ;
        private Stmt ExpressionStatement()
        {
            var expr = Expression();
            Consume(TokenType.Semicolon, "Expect ';' after expression");
            return new Stmt.Expression(expr);
        }

        // ifStmt :: "if" "(" expression ")" statement ( "else" statement )? ;
        private Stmt IfStatement()
        {
            Consume(TokenType.LeftParen, "Expect '(' after 'if'");
            var condition = Expression();
            Consume(TokenType.RightParen, "Expect ')' after if condition");

            var thenBranch = Statement();
            Stmt elseBranch = null;
            if (MatchAny(TokenType.Else))
            {
                elseBranch = Statement();
            }

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private Stmt PrintStatement()
        {
            var expr = Expression();
            Consume(TokenType.Semicolon, "Expect ';' after value");
            return new Stmt.Print(expr);
        }

        // returnStmt :: "return" expression? ";" ;
        private Stmt ReturnStatement()
        {
            var keyword = Previous();

            // Expression is optional
            Expr value = null;
            if (!Check(TokenType.Semicolon))
            {
                value = Expression();
            }

            Consume(TokenType.Semicolon, "Expect ';' after return value");
            return new Stmt.Return(keyword, value);
        }

        // whileStmt :: "while" "(" expression ")" statement ;
        private Stmt WhileStatement()
        {
            Consume(TokenType.LeftParen, "Expect '(' after 'while'");
            var condition = Expression();
            Consume(TokenType.RightParen, "Expect ')' after while condition");

            var body = Statement();
            return new Stmt.While(condition, body);
        }

        // forStmt :: "for" "(" ( varDecl | exprStmt | "; ")
        //                      expression? ";"
        //                      expression? ")" statement ;
        private Stmt ForStatement()
        {
            Consume(TokenType.LeftParen, "Expect '(' after 'for'");

            // Get the initializer.
            Stmt init;
            if (MatchAny(TokenType.Semicolon))
                init = null;
            else if (MatchAny(TokenType.Var))
                init = VarDeclaration();
            else
                init = ExpressionStatement();

            // Get the condition.
            Expr condition = null;
            if (!Check(TokenType.Semicolon))
                condition = Expression();

            Consume(TokenType.Semicolon, "Expect ';' after loop condition");

            // Get the increment.
            Expr increment = null;
            if (!Check(TokenType.RightParen))
                increment = Expression();

            Consume(TokenType.RightParen, "Expect ')' after for clauses");

            var body = Statement();

            // Desugar by creating a while loop instead.
            if (increment != null)
            {
                body = new Stmt.Block(
                    new List<Stmt> {
                        body,
                        new Stmt.Expression(increment)
                    });
            }

            if (condition == null)
                condition = new Expr.Literal(true);

            body = new Stmt.While(condition, body);

            if (init != null)
            {
                body = new Stmt.Block(
                    new List<Stmt> {
                        init,
                        body
                    });
            }

            return body;
        }

        // block :: "{" declaration* "}" ;
        private Stmt BlockStatement()
        {
            return new Stmt.Block(Block());
        }

        private List<Stmt> Block()
        {
            var statements = new List<Stmt>();

            while (!Check(TokenType.RightBrace) && !IsEOF())
            {
                var stmt = Declaration();
                if (stmt != null)
                {
                    statements.Add(stmt);
                }
            }

            Consume(TokenType.RightBrace, "Expect '}' after block");
            return statements;
        }

        // expression :: assignment ;
        private Expr Expression()
        {
            return Assignment();
        }

        // assignment :: IDENTIFIER "=" assignment
        //             | logic_or ;
        private Expr Assignment()
        {
            var expr = Or();

            if (MatchAny(TokenType.Equal))
            {
                var equals = Previous();
                var value = Assignment();

                if (expr is Expr.Variable)
                {
                    var name = ((Expr.Variable)expr).Name;
                    return new Expr.Assign(name, value);
                }

                _errors.AddParserError(equals, "Invalid assignment target");
            }

            return expr;
        }

        // logic_or :: logic_and ( "or" logic_and )* ;
        private Expr Or()
        {
            var expr = And();

            while (MatchAny(TokenType.Or))
            {
                var op = Previous();
                var right = And();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        // logic_and :: equality ( "and" equality )* ;
        private Expr And()
        {
            var expr = Equality();

            while (MatchAny(TokenType.And))
            {
                var op = Previous();
                var right = And();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
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

        // multiplication :: unary ( ( "*" | "/" | "%" ) unary )* ;
        private Expr Multiplication()
        {
            return LeftBinary(Unary, new TokenType[] { TokenType.Star, TokenType.Slash, TokenType.Percent }, Unary);
        }

        // unary :: ( "!" | "-" ) unary | call ;
        private Expr Unary()
        {
            if (MatchAny(TokenType.Bang, TokenType.Minus))
            {
                var op = Previous();
                var right = Unary();
                return new Expr.Unary(op, right);
            }

            return Call();
        }

        // call :: primary ( "(" arguments? ")" )* ;
        // arguments :: expression ( "," expression )* ;
        private Expr Call()
        {
            var expr = Primary();

            for (;;)
            {
                if (MatchAny(TokenType.LeftParen))
                    expr = FinishCall(expr);
                else
                    break;
            }

            return expr;
        }

        private Expr FinishCall(Expr callee)
        {
            var arguments = new List<Expr>();
            if (!Check(TokenType.RightParen))
            {
                do {
                    // Arbitrary limit; not really needed.
                    if (arguments.Count >= 255) {
                        // Note that it'll report the same error for each of the extra arguments.
                        _errors.AddParserError(Peek(), "Cannot have more than 255 arguments");
                    }

                    arguments.Add(Expression());
                } while (MatchAny(TokenType.Comma));
            }

            var paren = Consume(TokenType.RightParen, "Expect ')' after arguments");
            return new Expr.Call(callee, paren, arguments);
        }

        // primary :: "false" | "true" | "nil" | NUM | STR | IDENTIFIER | "(" expression ")" ;
        private Expr Primary()
        {
            if (MatchAny(TokenType.False)) return new Expr.Literal(false);
            if (MatchAny(TokenType.True)) return new Expr.Literal(false);
            if (MatchAny(TokenType.Nil)) return new Expr.Literal(null);

            if (MatchAny(TokenType.Number, TokenType.String))
            {
                return new Expr.Literal(Previous().Literal);
            }

            if (MatchAny(TokenType.Identifier))
            {
                return new Expr.Variable(Previous());
            }

            if (MatchAny(TokenType.LeftParen))
            {
                var expr = Expression();
                Consume(TokenType.RightParen, "Expect ')' after expression");
                return new Expr.Grouping(expr);
            }

            if (MatchAny(TokenType.Fun))
            {
                Consume(TokenType.LeftParen, "Expect '(' after fun");
                var location = Previous();
                var parameters = FunctionParameters();
                Consume(TokenType.RightParen, "Expect ')' after parameters");
                Consume(TokenType.LeftBrace, "Expect '{' before fun body");
                var body = Block();
                return new Expr.Function(location, parameters, body);
            }

            _errors.AddParserError(Peek(), "Expect expression");
            throw new ParseError();
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

            _errors.AddParserError(Peek(), message);
            throw new ParseError();
        }

        private void Synchronize()
        {
            Advance();

            while (!IsEOF())
            {
                if (Previous().Type == TokenType.Semicolon)
                    return;

                switch (Peek().Type)
                {
                    case TokenType.Class:
                    case TokenType.Fun:
                    case TokenType.Var:
                    case TokenType.For:
                    case TokenType.If:
                    case TokenType.While:
                    case TokenType.Print:
                    case TokenType.Return:
                        return;
                }

                Advance();
            }
        }
    }
}
