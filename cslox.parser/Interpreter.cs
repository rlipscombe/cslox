using System;
using System.Collections.Generic;
using System.Linq;

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

        internal object GetAt(int distance, string name)
        {
            return GetAncestor(distance)._values[name];
        }

        private Environment GetAncestor(int distance)
        {
            var environment = this;
            for (int i = 0; i < distance; ++i)
                environment = environment._enclosing;

            return environment;
        }

        internal void AssignAt(int distance, Token name, object value)
        {
            GetAncestor(distance)._values[name.Lexeme] = value;
        }
    }

    public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<Unit>
    {
        private Environment _globals = new Environment();
        private IErrorReporter _errors;

        private Dictionary<Expr, int> _locals = new Dictionary<Expr, int>();

        private Environment _environment;

        public Interpreter(Environment globals, IErrorReporter errors)
        {
            _globals = globals;
            _environment = _globals;
            _errors = errors;
        }

        public Environment Globals { get { return _globals; } }

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
                case TokenType.Percent:
                    AssertNumberOperands(expr.Op, left, right);
                    return (double)left % (double)right;

                case TokenType.BangEqual:
                    return !IsEqual(left, right);
                case TokenType.EqualEqual:
                    return IsEqual(left, right);
            }

            throw new NotSupportedException();
        }

        internal void Resolve(Expr expr, int depth)
        {
            _locals.Add(expr, depth);
        }

        public object VisitLogical(Expr.Logical expr)
        {
            // We always evaluate the left expression.
            var left = Evaluate(expr.Left);

            if (expr.Op.Type == TokenType.Or)
            {
                // For 'or', we can short-circuit if the left is truthy.
                if (IsTruthy(left))
                    return left;
            }
            else if (expr.Op.Type == TokenType.And)
            {
                // For 'and', we can short-circuit if the left is falsy.
                if (!IsTruthy(left))
                    return left;
            }

            // Returning the truthy (or falsy) value, rather than strict
            // 'true' or 'false' allow us to do 'var x = y or default;'
            return Evaluate(expr.Right);
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
            return LookupVariable(expr.Name, expr);
        }

        private object LookupVariable(Token name, Expr expr)
        {
            int distance;
            if (_locals.TryGetValue(expr, out distance))
                return _environment.GetAt(distance, name.Lexeme);
            else
                return _globals.Get(name);
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

            int distance;
            if (_locals.TryGetValue(expr, out distance))
                _environment.AssignAt(distance, expr.Name, value);
            else
                _globals.Assign(expr.Name, value);

            return value;
        }

        public Unit VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.Statements, new Environment(_environment));
            return Unit.Default;
        }

        // Needs to be public, because it's also used for executing functions.
        public void ExecuteBlock(List<Stmt> statements, Environment environment)
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

        public Unit VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.Then);
            }
            else
            {
                if (stmt.Else != null)
                {
                    Execute(stmt.Else);
                }
            }

            return Unit.Default;
        }

        public Unit VisitWhileStmt(Stmt.While stmt)
        {
            while (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.Body);
            }

            return Unit.Default;
        }

        public object VisitCall(Expr.Call expr)
        {
            var callee = Evaluate(expr.Callee);

            // Assuming 'Select' is left-to-right, we'll evaluate arguments left-to-right.
            // In C, this is undefined, so... /shrug.
            var arguments = expr.Arguments
                                .Select(arg => Evaluate(arg))
                                .ToList();

            var callable = callee as ILoxCallable;
            if (callable != null)
            {
                // JavaScript discards extra arguments and sets missing ones to 'undefined'. We'll be stricter.
                if (arguments.Count != callable.Arity)
                {
                    throw new RuntimeError(expr.Paren,
                                            string.Format("Expected {0} arguments but got {1}",
                                                            callable.Arity, arguments.Count));
                }

                return callable.Call(this, arguments);
            }
            else
                throw new RuntimeError(expr.Paren, "Can only call functions and classes");
        }

        public Unit VisitFunction(Stmt.Function stmt)
        {
            // Capture the environment at declaration time.
            var function = new LoxFunction(_environment, stmt.Parameters, stmt.Body);
            _environment.Define(stmt.Name.Lexeme, function);
            return Unit.Default;
        }

        public object VisitFunction(Expr.Function expr)
        {
            // Capture the environment at declaration time.
            var function = new LoxFunction(_environment, expr.Parameters, expr.Body);
            return function;
        }

        public Unit VisitReturn(Stmt.Return stmt)
        {
            object result = null;
            if (stmt.Value != null)
                result = Evaluate(stmt.Value);

            throw new Return(result);
        }
    }

    public class Return : Exception
    {
        public Return(object result)
        {
            Result = result;
        }

        public object Result { get; }
    }

    // TODO: We've not used 'Lox' in any other names (that's literally what namespaces are for)...?
    interface ILoxCallable
    {
        object Call(Interpreter interpreter, List<object> arguments);
        int Arity { get; }
    }

    class LoxFunction : ILoxCallable
    {
        public LoxFunction(Environment closure, List<Token> parameters, List<Stmt> body)
        {
            // By capturing the closure here, this is the environment at
            // declaration time. This is lexical closure. This is NOT what
            // JS does.
            Closure = closure;
            Parameters = parameters;
            Body = body;
        }

        Environment Closure { get; }
        List<Token> Parameters { get; }
        List<Stmt> Body { get; }

        public int Arity => Parameters.Count;

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            // Our environment is nested in the closure.
            var environment = new Environment(Closure);
            foreach (var binding in Enumerable.Zip(Parameters, arguments))
            {
                environment.Define(binding.First.Lexeme, binding.Second);
            }

            try
            {
                interpreter.ExecuteBlock(Body, environment);
            }
            catch (Return ret)
            {
                return ret.Result;
            }

            return null;
        }
    }
}
