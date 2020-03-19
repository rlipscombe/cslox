using System.Collections.Generic;

namespace cslox
{
    public abstract class Stmt
    {
        public abstract TResult Accept<TResult>(IVisitor<TResult> visitor);

        public class Print : Stmt
        {
            public Print(Expr value)
            {
                Value = value;
            }

            public Expr Value { get; }

            public override TResult Accept<TResult>(IVisitor<TResult> visitor)
            {
                return visitor.VisitPrintStmt(this);
            }
        }

        public class Expression : Stmt
        {
            public Expression(Expr value)
            {
                Value = value;
            }

            public Expr Value { get; }

            public override TResult Accept<TResult>(IVisitor<TResult> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }
        }

        public class Var : Stmt
        {
            public Var(Token name, Expr init)
            {
                Name = name;
                Init = init;
            }

            public Token Name { get; }
            public Expr Init { get; }

            public override TResult Accept<TResult>(IVisitor<TResult> visitor)
            {
                return visitor.VisitVarStmt(this);
            }
        }

        public class Block : Stmt
        {
            public Block(List<Stmt> statements)
            {
                Statements = statements;
            }

            public List<Stmt> Statements { get; }

            public override TResult Accept<TResult>(IVisitor<TResult> visitor)
            {
                return visitor.VisitBlockStmt(this);
            }
        }

        public class If : Stmt
        {
            public If(Expr condition, Stmt thenBranch, Stmt elseBranch)
            {
                Condition = condition;
                Then = thenBranch;
                Else = elseBranch;
            }

            public Expr Condition { get; }
            public Stmt Then { get; }
            public Stmt Else { get; }

            public override TResult Accept<TResult>(IVisitor<TResult> visitor)
            {
                return visitor.VisitIfStmt(this);
            }
        }

        public class While : Stmt
        {
            public While(Expr condition, Stmt body)
            {
                Condition = condition;
                Body = body;
            }

            public Expr Condition { get; }
            public Stmt Body { get; }

            public override TResult Accept<TResult>(IVisitor<TResult> visitor)
            {
                return visitor.VisitWhileStmt(this);
            }
        }

        public class Function : Stmt
        {
            public Function(Token name, List<Token> parameters, List<Stmt> body)
            {
                Name = name;
                Parameters = parameters;
                Body = body;
            }

            public Token Name { get; }
            public List<Token> Parameters { get; }
            public List<Stmt> Body { get; }

            public override TResult Accept<TResult>(IVisitor<TResult> visitor)
            {
                return visitor.VisitFunction(this);
            }
        }

        public class Return : Stmt
        {
            public Return(Token keyword, Expr value)
            {
                Keyword = keyword;
                Value = value;
            }

            public Token Keyword { get; }
            public Expr Value { get; }

            public override TResult Accept<TResult>(IVisitor<TResult> visitor)
            {
                return visitor.VisitReturn(this);
            }
        }

        public interface IVisitor<TResult>
        {
            // TODO: Naming is inconsistent here (VisitFooStmt vs VisitFoo).
            TResult VisitPrintStmt(Stmt.Print stmt);
            TResult VisitExpressionStmt(Stmt.Expression stmt);
            TResult VisitVarStmt(Stmt.Var stmt);
            TResult VisitBlockStmt(Stmt.Block stmt);
            TResult VisitIfStmt(Stmt.If stmt);
            TResult VisitWhileStmt(Stmt.While stmt);
            TResult VisitFunction(Stmt.Function stmt);
            TResult VisitReturn(Return @return);
        }
    }
}
