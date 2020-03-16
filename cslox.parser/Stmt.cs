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

        public interface IVisitor<TResult>
        {
            TResult VisitPrintStmt(Stmt.Print stmt);
            TResult VisitExpressionStmt(Stmt.Expression stmt);
            TResult VisitVarStmt(Stmt.Var stmt);
        }
    }
}
