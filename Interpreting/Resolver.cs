using Schalken.CsLox.Interpreting;
using Schalken.CsLox.Lexing;
using Schalken.CsLox.Parsing;

namespace Schalken.CsLox;

internal class Resolver(Interpreter interpreter) : IStatementVisitor, IExpressionVisitor
{
    private readonly Interpreter _interpreter = interpreter;
    private readonly Stack<Dictionary<string, bool>> _scopes = new();

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

        ResolveFunction(statement);
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

    private void ResolveFunction(FuncDecl function)
    {
        BeginScope();
        function.Parameters.ForEach(p =>
        {
            Declare(p);
            Define(p);
        });
        function.Body.ForEach(s => s.Accept(this));
        EndScope();
    }

    private void BeginScope() => _scopes.Push([]);

    private void EndScope() => _scopes.Pop();

    private void Declare(Token name)
    {
        if (_scopes.Count is 0) return;

        var scope = _scopes.Peek();
        scope[name.Lexeme.Get().ToString()] = false;
    }

    private void Define(Token name)
    {
        if (_scopes.Count is 0) return;
        var scope = _scopes.Peek();
        scope[name.Lexeme.Get().ToString()] = true;
    }
}
