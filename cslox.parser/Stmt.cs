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

        public interface IVisitor<TResult>
        {
            TResult VisitPrintStmt(Stmt.Print stmt);
            TResult VisitExpressionStmt(Stmt.Expression stmt);
            TResult VisitVarStmt(Stmt.Var stmt);
            TResult VisitBlockStmt(Stmt.Block stmt);
            TResult VisitIfStmt(Stmt.If stmt);
        }
    }
}
