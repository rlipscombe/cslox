using System.Collections.Generic;
using System.Text;

namespace cslox
{
    public class ProgramPrinter : Expr.IVisitor<string>, Stmt.IVisitor<string>
    {
        public static string Print(List<Stmt> program)
        {
            var printer = new ProgramPrinter();
            return printer.PrintInner(program);
        }

        private string PrintInner(List<Stmt> program)
        {
            var builder = new StringBuilder();
            foreach (var stmt in program)
            {
                if (stmt != null)
                    builder.Append(stmt.Accept(this));
            }

            return builder.ToString();
        }

        string Parenthesize(Expr expr)
        {
            var builder = new StringBuilder();

            builder.Append("(");
            builder.Append(expr.Accept(this));
            builder.Append(")");
            return builder.ToString();
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

        string Parenthesize(string name, string name2, params Expr[] exprs)
        {
            var builder = new StringBuilder();

            builder.Append("(").Append(name).Append(" ").Append(name2);
            foreach (var expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }

            builder.Append(")");
            return builder.ToString();
        }

        public string VisitBinary(Expr.Binary expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);
        }

        public string VisitUnary(Expr.Unary expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Right);
        }

        public string VisitLiteral(Expr.Literal expr)
        {
            return expr.Value != null ? expr.Value.ToString() : "nil";
        }

        public string VisitGrouping(Expr.Grouping expr)
        {
            return Parenthesize(expr.Inner);
        }

        public string VisitPrintStmt(Stmt.Print stmt)
        {
            return Parenthesize("print", stmt.Value);
        }

        public string VisitExpressionStmt(Stmt.Expression stmt)
        {
            return stmt.Value.Accept(this);
        }

        public string VisitVariable(Expr.Variable expr)
        {
            return expr.Name.Lexeme;
        }

        public string VisitVarStmt(Stmt.Var stmt)
        {
            return Parenthesize("var", stmt.Name.Lexeme, stmt.Init);
        }

        public string VisitAssign(Expr.Assign expr)
        {
            return Parenthesize("set", expr.Name.Lexeme, expr.Value);
        }

        public string VisitBlockStmt(Stmt.Block stmt)
        {
            var builder = new StringBuilder();
            builder.Append("(block ");

            foreach (var s in stmt.Statements)
            {
                builder.Append(s.Accept(this));
            }

            builder.Append(")");
            return builder.ToString();
        }
    }
}
