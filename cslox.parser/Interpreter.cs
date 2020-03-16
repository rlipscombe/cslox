using System;
using System.Collections.Generic;

namespace cslox
{
    public class Unit
    {
        public static Unit Default { get { return null; } }
    }

    public class Environment
    {
        private Environment _enclosing;
        private Dictionary<string, object> _values = new Dictionary<string, object>();

        public Environment()
        {
            _enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            _enclosing = enclosing;
        }

        internal object Get(Token name)
        {
            object value;
            if (_values.TryGetValue(name.Lexeme, out value))
                return value;

            if (_enclosing != null)
                return _enclosing.Get(name);

            throw new RuntimeError(name, "Undefined variable '" + name.Lexeme + "'");
        }

        internal void Define(string name, object value)
        {
            _values.Add(name, value);
        }

        internal void Assign(Token name, object value)
        {
            if (_values.ContainsKey(name.Lexeme))
            {
                _values[name.Lexeme] = value;
                return;
            }

            if (_enclosing != null)
            {
                _enclosing.Assign(name, value);
                return;
            }

            if (!_values.ContainsKey(name.Lexeme))
                throw new RuntimeError(name, "Undefined variable '" + name.Lexeme + "'");
        }
    }

    public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<Unit>
    {
        private Environment _environment;
        private IErrorReporter _errors;

        public Interpreter(Environment environment, IErrorReporter errors)
        {
            _environment = environment;
            _errors = errors;
        }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (var stmt in statements)
                {
                    if (stmt != null)
                        Execute(stmt);
                }
            }
            catch (RuntimeError err)
            {
                _errors.AddRuntimeError(err);
            }
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        public object VisitBinary(Expr.Binary expr)
        {
            // We evaluate the operands left-to-right, and we're eager.
            object left = Evaluate(expr.Left);
            object right = Evaluate(expr.Right);

            switch (expr.Op.Type)
            {
                case TokenType.Greater:
                    AssertNumberOperands(expr.Op, left, right);
                    return (double)left > (double)right;
                case TokenType.GreaterEqual:
                    AssertNumberOperands(expr.Op, left, right);
                    return (double)left >= (double)right;
                case TokenType.Less:
                    AssertNumberOperands(expr.Op, left, right);
                    return (double)left < (double)right;
                case TokenType.LessEqual:
                    AssertNumberOperands(expr.Op, left, right);
                    return (double)left <= (double)right;

                case TokenType.Minus:
                    AssertNumberOperands(expr.Op, left, right);
                    return (double)left - (double)right;
                case TokenType.Plus:
                    if (left is double && right is double)
                    {
                        return (double)left + (double)right;
                    }
                    else if (left is string && right is string)
                    {
                        return (string)left + (string)right;
                    }
                    else
                        throw new RuntimeError(expr.Op, "Operands must be two numbers or two strings");
                case TokenType.Slash:
                    AssertNumberOperands(expr.Op, left, right);
                    return (double)left / (double)right;
                case TokenType.Star:
                    AssertNumberOperands(expr.Op, left, right);
                    return (double)left * (double)right;

                case TokenType.BangEqual:
                    return !IsEqual(left, right);
                case TokenType.EqualEqual:
                    return IsEqual(left, right);
            }

            throw new NotSupportedException();
        }

        public object VisitGrouping(Expr.Grouping expr)
        {
            return Evaluate(expr.Inner);
        }

        public object VisitLiteral(Expr.Literal expr)
        {
            return expr.Value;
        }

        public object VisitUnary(Expr.Unary expr)
        {
            var right = Evaluate(expr.Right);

            switch (expr.Op.Type)
            {
                case TokenType.Bang:
                    return !IsTruthy(right);
                case TokenType.Minus:
                    AssertNumberOperand(expr.Op, right);
                    return -(double)right;
            }

            throw new NotSupportedException();
        }

        // false and nil are falsey, everything else is truthy.
        private bool IsTruthy(object o)
        {
            if (o == null)
                return false;

            if (o is bool)
                return (bool)o;

            return true;
        }

        private bool IsEqual(object left, object right)
        {
            // nil is only equal to nil
            if (left == null && right == null)
                return true;
            if (left == null)
                return false;

            return left.Equals(right);
        }

        private void AssertNumberOperand(Token op, object operand)
        {
            if (operand is double)
                return;

            throw new RuntimeError(op, "Operand must be a number");
        }

        private void AssertNumberOperands(Token op, object left, object right)
        {
            if (left is double && right is double)
                return;

            throw new RuntimeError(op, "Operands must be numbers");
        }

        private string Stringify(object value)
        {
            if (value == null)
                return "nil";

            return value.ToString();
        }

        public Unit VisitPrintStmt(Stmt.Print stmt)
        {
            var value = Evaluate(stmt.Value);
            Console.WriteLine(Stringify(value));
            return Unit.Default;
        }

        public Unit VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.Value);
            return Unit.Default;
        }

        public object VisitVariable(Expr.Variable expr)
        {
            return _environment.Get(expr.Name);
        }

        public Unit VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if (stmt.Init != null)
            {
                value = Evaluate(stmt.Init);
            }

            _environment.Define(stmt.Name.Lexeme, value);
            return Unit.Default;
        }

        public object VisitAssign(Expr.Assign expr)
        {
            var value = Evaluate(expr.Value);

            _environment.Assign(expr.Name, value);
            return value;
        }

        public Unit VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.Statements, new Environment(_environment));
            return Unit.Default;
        }

        private void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            var previous = _environment;
            try
            {
                _environment = environment;

                foreach (var stmt in statements)
                {
                    Execute(stmt);
                }
            }
            finally
            {
                _environment = previous;
            }
        }
    }
}
