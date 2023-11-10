using Schalken.CsLox.Interpreting;
using Schalken.CsLox.Lexing;
using Schalken.CsLox.Parsing;

namespace Schalken.CsLox;

internal class Resolver(Interpreter interpreter) : IStatementVisitor, IExpressionVisitor
{
    private enum FunctionType
    {
        None,
        Function,
        Initializer,
        Method
    }

    private enum ClassType
    {
        None,
        Class
    }

    private readonly Interpreter _interpreter = interpreter;

    private readonly Stack<Dictionary<string, bool>> _scopes = new();

    private FunctionType _currentFunction = FunctionType.None;

    private ClassType _currentClass = ClassType.None;

    public void Resolve(IEnumerable<IStatement> statements)
    {
        foreach (var statement in statements)
        {
            statement.Accept(this);
        }
    }

    #region Statements

    public void Visit(Block statement)
    {
        BeginScope();
        statement.Statements.ForEach(s => s.Accept(this));
        EndScope();
    }

    public void Visit(Expression statement)
    {
        statement.Expr.Accept(this);
    }

    public void Visit(If statement)
    {
        statement.Condition.Accept(this);
        statement.ThenBranch.Accept(this);
        statement.ElseBranch?.Accept(this);
    }

    public void Visit(While statement)
    {
        statement.Condition.Accept(this);
        statement.Body.Accept(this);
    }

    public void Visit(Return statement)
    {
        if (_currentFunction is FunctionType.None)
            Logger.Error(statement.Keyword, "Can't return from top-level code.");

        if (_currentFunction is FunctionType.Initializer && statement.Expr is not null)
            Logger.Error(statement.Keyword, "Can't return a value from an initializer.");

        statement.Expr?.Accept(this);
    }

    public void Visit(Print statement)
    {
        statement.Expr.Accept(this);
    }

    public void Visit(VarDecl statement)
    {
        Declare(statement.Name);
        statement.InitExpr?.Accept(this);
        Define(statement.Name);
    }

    public void Visit(FuncDecl statement)
    {
        Declare(statement.Name);
        Define(statement.Name);

        ResolveFunction(statement, FunctionType.Function);
    }

    public void Visit(ClassDecl statement)
    {
        var enclosingClass = _currentClass;
        _currentClass = ClassType.Class;

        Declare(statement.Name);
        Define(statement.Name);

        BeginScope();
        _scopes.Peek()["this"] = true;

        statement.Methods.ForEach(m =>
        {
            var declarationType =
                m.Name.Lexeme.Get().ToString() == "init"
                ? FunctionType.Initializer
                : FunctionType.Method;
            ResolveFunction(m, declarationType);
        });

        EndScope();

        _currentClass = enclosingClass;
    }

    #endregion

    #region Expressions

    public void Visit(Assign expression)
    {
        expression.Value.Accept(this);
        ResolveLocal(expression, expression.Name);
    }

    public void Visit(Binary expression)
    {
        expression.Left.Accept(this);
        expression.Right.Accept(this);
    }

    public void Visit(Logical expression)
    {
        expression.Left.Accept(this);
        expression.Right.Accept(this);
    }

    public void Visit(Unary expression)
    {
        expression.Right.Accept(this);
    }

    public void Visit(Call expression)
    {
        expression.Callee.Accept(this);
        expression.Arguments.ForEach(a => a.Accept(this));
    }

    public void Visit(Get expression)
    {
        expression.Owner.Accept(this);
    }

    public void Visit(Set expression)
    {
        expression.Owner.Accept(this);
        expression.Value.Accept(this);
    }

    public void Visit(This expression)
    {
        if (_currentClass is ClassType.None)
        {
            Logger.Error(expression.Keyword, "Can't use 'this' outside of a class.");
            return;
        }

        ResolveLocal(expression, expression.Keyword);
    }

    public void Visit(Grouping expression)
    {
        expression.Expr.Accept(this);
    }

    public void Visit(Literal expression)
    {
    }

    public void Visit(Variable expression)
    {
        var variableName = expression.Name.Lexeme.Get().ToString();
        if (_scopes.Count is not 0
            && _scopes.Peek().TryGetValue(variableName, out var defined)
            && !defined)
        {
            Logger.Error(expression.Name, "Can't read local variable in its own initializer.");
        }
        ResolveLocal(expression, expression.Name);
    }

    #endregion

    private void ResolveLocal(IExpression expression, Token name)
    {
        var variableName = name.Lexeme.Get().ToString();

        foreach (var (scope, index) in _scopes.Select((scope, index) => (scope, index)))
        {
            if (scope.ContainsKey(variableName))
            {
                _interpreter.Resolve(expression, index);
                return;
            }
        }
    }

    private void ResolveFunction(FuncDecl function, FunctionType functionType)
    {
        var enclosingFunction = _currentFunction;
        _currentFunction = functionType;
        BeginScope();
        function.Parameters.ForEach(p =>
        {
            Declare(p);
            Define(p);
        });
        function.Body.ForEach(s => s.Accept(this));
        EndScope();
        _currentFunction = enclosingFunction;
    }

    private void BeginScope() => _scopes.Push([]);

    private void EndScope() => _scopes.Pop();

    private void Declare(Token name)
    {
        if (_scopes.Count is 0) return;

        var scope = _scopes.Peek();
        if (!scope.TryAdd(name.Lexeme.Get().ToString(), false))
        {
            Logger.Error(name, "A variable with this name already exists in this scope.");
        }
    }

    private void Define(Token name)
    {
        if (_scopes.Count is 0) return;
        var scope = _scopes.Peek();
        scope[name.Lexeme.Get().ToString()] = true;
    }
}
