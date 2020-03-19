using System.Collections.Generic;

namespace cslox
{
    public abstract class Expr
    {
        public abstract TResult Accept<TResult>(IVisitor<TResult> visitor);

        public class Unary : Expr
        {
            public Unary(Token op, Expr right)
            {
                Op = op;
                Right = right;
            }

            public Token Op { get; }
            public Expr Right { get; }

            public override TResult Accept<TResult>(IVisitor<TResult> visitor)
            {
                return visitor.VisitUnary(this);
            }
        }

        public class Binary : Expr
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

        public class Logical : Expr
        {
            public Logical(Expr left, Token op, Expr right)
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
                return visitor.VisitLogical(this);
            }
        }

        public class Literal : Expr
        {
            public Literal(object value)
            {
                Value = value;
            }

            public object Value { get; }

            public override TResult Accept<TResult>(IVisitor<TResult> visitor)
            {
                return visitor.VisitLiteral(this);
            }
        }

        public class Grouping : Expr
        {
            public Grouping(Expr inner)
            {
                Inner = inner;
            }

            public Expr Inner { get; }

            public override TResult Accept<TResult>(IVisitor<TResult> visitor)
            {
                return visitor.VisitGrouping(this);
            }
        }

        public class Variable : Expr
        {
            public Variable(Token name)
            {
                Name = name;
            }

            public Token Name { get; }

            public override TResult Accept<TResult>(IVisitor<TResult> visitor)
            {
                return visitor.VisitVariable(this);
            }
        }

        public class Assign : Expr
        {
            public Assign(Token name, Expr value)
            {
                Name = name;
                Value = value;
            }

            public Token Name { get; }
            public Expr Value { get; }

            public override TResult Accept<TResult>(IVisitor<TResult> visitor)
            {
                return visitor.VisitAssign(this);
            }
        }

        public class Call : Expr
        {
            public Call(Expr callee, Token paren, List<Expr> arguments)
            {
                Callee = callee;
                Paren = paren;
                Arguments = arguments;
            }

            public Expr Callee { get; }
            public Token Paren { get; }
            public List<Expr> Arguments { get; }

            public override TResult Accept<TResult>(IVisitor<TResult> visitor)
            {
                return visitor.VisitCall(this);
            }
        }

        public class Function : Expr
        {
            public Function(Token fun, List<Token> parameters, List<Stmt> body)
            {
                Fun = fun;
                Parameters = parameters;
                Body = body;
            }

            public Token Fun { get; }
            public List<Token> Parameters { get; }
            public List<Stmt> Body { get; }

            public override TResult Accept<TResult>(IVisitor<TResult> visitor)
            {
                return visitor.VisitFunction(this);
            }
        }

        public interface IVisitor<TResult>
        {
            TResult VisitBinary(Binary expr);
            TResult VisitLogical(Logical logical);
            TResult VisitUnary(Unary expr);
            TResult VisitLiteral(Literal expr);
            TResult VisitGrouping(Grouping expr);
            TResult VisitVariable(Variable expr);
            TResult VisitAssign(Assign expr);
            TResult VisitCall(Call call);
            TResult VisitFunction(Function function);
        }
    }
}
