using System.Text;

namespace cslox
{
    public class AstPrinter : Expr.IVisitor<string>
    {
        public static string Print(Expr expr)
        {
            var printer = new AstPrinter();
            return printer.PrintInner(expr);
        }

        string PrintInner(Expr expr)
        {
            return expr.Accept(this);
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
            return expr.Value.ToString();
        }

        public string VisitGrouping(Expr.Grouping expr)
        {
            return Parenthesize(expr.Inner);
        }
    }
}
