using System;
using System.Collections.Generic;

namespace cslox
{
    public class Resolver : Expr.IVisitor<Unit>, Stmt.IVisitor<Unit>
    {
        IErrorReporter _errors;

        enum Resolution { NotReady, Ready };

        /// Java's 'Stack' has more features than .NET's, so...
        class ScopeStack
        {
            List<Dictionary<string, Resolution>> _list = new List<Dictionary<string, Resolution>>();

            public int Count
            {
                get { return _list.Count; }
            }

            internal IDictionary<string, Resolution> Peek()
            {
                return _list[_list.Count - 1];
            }

            internal Dictionary<string, Resolution> At(int i)
            {
                return _list[i];
            }

            internal void Push(Dictionary<string, Resolution> dictionary)
            {
                _list.Add(dictionary);
            }

            internal void Pop()
            {
                _list.RemoveAt(_list.Count - 1);
            }
        }

        Interpreter _interpreter;
        ScopeStack _scopes = new ScopeStack();

        enum FunctionType { None, Function };
        FunctionType _currentFunction = FunctionType.None;

        public Resolver(Interpreter interpreter, IErrorReporter errors)
        {
            _interpreter = interpreter;
            _errors = errors;
        }

        public Unit VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.Statements);
            EndScope();
            return Unit.Default;
        }

        public Unit VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.Name);
            if (stmt.Init != null)
                Resolve(stmt.Init);
            Define(stmt.Name);
            return Unit.Default;
        }

        public Unit VisitVariable(Expr.Variable expr)
        {
            // Because we recurse into the initializer expression (while the scope has the variable)
            // in 'NotReady'), we can use this to check that the initializer expression doesn't refer
            // to this variable.
            Resolution res;
            if (_scopes.Count != 0 && _scopes.Peek().TryGetValue(expr.Name.Lexeme, out res) && res == Resolution.NotReady)
            {
                _errors.AddResolverError(expr.Name, "Cannot read local variable in its own initializer");
            }

            ResolveLocal(expr, expr.Name);
            return Unit.Default;
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            for (var i = _scopes.Count - 1; i >= 0; i--)
            {
                var scope = _scopes.At(i);
                if (scope.ContainsKey(name.Lexeme))
                {
                    _interpreter.Resolve(expr, _scopes.Count - 1 - i);
                    return;
                }
            }

            // Assume global.
        }

        private void Declare(Token name)
        {
            if (_scopes.Count == 0)
                return;

            var scope = _scopes.Peek();
            if (scope.ContainsKey(name.Lexeme))
            {
                _errors.AddResolverError(name, "Variable with this name already declared in this scope");
            }

            scope.Add(name.Lexeme, Resolution.NotReady);
        }

        private void Define(Token name)
        {
            if (_scopes.Count == 0)
                return;

            var scope = _scopes.Peek();
            scope[name.Lexeme] = Resolution.Ready;
        }

        public void Resolve(List<Stmt> statements)
        {
            foreach (var statement in statements)
            {
                Resolve(statement);
            }
        }

        private void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        private void BeginScope()
        {
            _scopes.Push(new Dictionary<string, Resolution>());
        }

        private void EndScope()
        {
            _scopes.Pop();
        }

        public Unit VisitBinary(Expr.Binary expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return Unit.Default;
        }

        public Unit VisitLogical(Expr.Logical logical)
        {
            Resolve(logical.Left);
            Resolve(logical.Right);
            return Unit.Default;
        }

        public Unit VisitUnary(Expr.Unary expr)
        {
            Resolve(expr.Right);
            return Unit.Default;
        }

        public Unit VisitLiteral(Expr.Literal expr)
        {
            return Unit.Default;
        }

        public Unit VisitGrouping(Expr.Grouping expr)
        {
            Resolve(expr.Inner);
            return Unit.Default;
        }

        public Unit VisitAssign(Expr.Assign expr)
        {
            Resolve(expr.Value);
            ResolveLocal(expr, expr.Name);
            return Unit.Default;
        }

        public Unit VisitCall(Expr.Call call)
        {
            Resolve(call.Callee);
            foreach (var arg in call.Arguments)
                Resolve(arg);

            return Unit.Default;
        }

        public Unit VisitFunction(Expr.Function function)
        {
            ResolveFunction(function);
            return Unit.Default;
        }

        public Unit VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.Value);
            return Unit.Default;
        }

        public Unit VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.Value);
            return Unit.Default;
        }

        public Unit VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Then);
            if (stmt.Else != null)
                Resolve(stmt.Else);
            return Unit.Default;
        }

        public Unit VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Body);
            return Unit.Default;
        }

        public Unit VisitFunction(Stmt.Function stmt)
        {
            Declare(stmt.Name);
            Define(stmt.Name);
            ResolveFunction(stmt, FunctionType.Function);
            return Unit.Default;
        }

        private void ResolveFunction(Stmt.Function stmt, FunctionType type)
        {
            ResolveFunction(type, stmt.Parameters, stmt.Body);
        }

        private void ResolveFunction(FunctionType type, List<Token> parameters, List<Stmt> body)
        {
            var enclosingFunction = _currentFunction;
            _currentFunction = type;
            BeginScope();
            foreach (var param in parameters)
            {
                Declare(param);
                Define(param);
            }

            Resolve(body);
            EndScope();
            _currentFunction = enclosingFunction;
        }

        private void ResolveFunction(Expr.Function expr)
        {
            ResolveFunction(FunctionType.Function, expr.Parameters, expr.Body);
        }

        public Unit VisitReturn(Stmt.Return stmt)
        {
            if (_currentFunction == FunctionType.None)
                _errors.AddResolverError(stmt.Keyword, "Cannot return from top-level code");

            if (stmt.Value != null)
                Resolve(stmt.Value);

            return Unit.Default;
        }
    }
}
