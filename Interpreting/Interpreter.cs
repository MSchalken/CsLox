using Schalken.CsLox.Lexing;
using Schalken.CsLox.Parsing;

namespace Schalken.CsLox.Interpreting;

internal class Interpreter : IStatementVisitor, IExpressionVisitor<object?>
{
    public static readonly Interpreter Instance = new();

    private readonly Environment _globals = new();

    private readonly Dictionary<IExpression, int> _locals = [];

    private Environment _environment;

    private Interpreter()
    {
        _environment = _globals;
        NativeFunctions.RegisterDefinitions(_globals);
    }

    public void Interpret(IEnumerable<IStatement> statements)
    {
        try
        {
            foreach (var statement in statements)
            {
                statement.Accept(this);
            }
        }
        catch (RuntimeError e)
        {
            Logger.Error(e);
        }
    }

    public void Resolve(IExpression expression, int depth) => _locals[expression] = depth;

    #region Statements

    public void Visit(Block statement)
    {
        ExecuteBlock(statement.Statements, new Environment(_environment));
    }

    public void ExecuteBlock(List<IStatement> statements, Environment environment)
    {
        var enclosingScope = _environment;

        try
        {
            _environment = environment;
            statements.ForEach(s => s.Accept(this));
        }
        finally
        {
            _environment = enclosingScope;
        }
    }

    public void Visit(Expression statement)
    {
        statement.Expr.Accept(this);
    }

    public void Visit(If statement)
    {
        if (IsTrue(statement.Condition.Accept(this)))
        {
            statement.ThenBranch.Accept(this);
        }
        else
        {
            statement.ElseBranch?.Accept(this);
        }
    }

    public void Visit(While statement)
    {
        while (IsTrue(statement.Condition.Accept(this)))
        {
            statement.Body.Accept(this);
        }
    }

    public void Visit(Return statement)
    {
        var value = statement.Expr?.Accept(this);
        throw new ReturnValue(value);
    }

    public void Visit(Print statement)
    {
        var value = statement.Expr.Accept(this);
        Console.WriteLine(value?.ToString() ?? "nil");
    }

    public void Visit(VarDecl statement)
    {
        var value = statement.InitExpr?.Accept(this) ?? null;
        _environment.Define(statement.Name.Lexeme.Get().ToString(), value);
    }

    public void Visit(FuncDecl statement)
    {
        var function = new UserFunction(statement, _environment);
        _environment.Define(statement.Name.Lexeme.Get().ToString(), function);
    }

    public void Visit(ClassDecl statement)
    {
        var className = statement.Name.Lexeme.Get().ToString();

        var superclass = statement.Superclass?.Accept(this);

        if (superclass is not null and not UserClass)
            throw Error(statement.Superclass!.Name, "Superclass must be a class.");

        _environment.Define(className, null);

        if (statement.Superclass is not null)
        {
            _environment = new Environment(_environment);
            _environment.Define("super", superclass);
        }

        var methods = new Dictionary<string, UserFunction>();
        foreach (var method in statement.Methods)
        {
            var methodName = method.Name.Lexeme.Get().ToString();
            var isInitializer = methodName == "init";
            methods[methodName] = new UserFunction(method, _environment, isInitializer);
        }
        var klass = new UserClass(className, superclass as UserClass, methods);

        if (statement.Superclass is not null) _environment = _environment.EnclosingScope!;

        _environment.Assign(statement.Name, klass);
    }

    #endregion

    #region Expressions

    public object? Visit(Assign expression)
    {
        var value = expression.Value.Accept(this);

        if (_locals.TryGetValue(expression, out var depth))
        {
            _environment.AssignAt(depth, expression.Name, value);
        }
        else
        {
            _globals.Assign(expression.Name, value);
        }

        return value;
    }

    public object? Visit(Binary expression)
    {
        var left = expression.Left.Accept(this);
        var right = expression.Right.Accept(this);
        var oper = expression.Operator;

        return oper.Type switch
        {
            TokenType.Minus => ToDouble(left, oper) - ToDouble(right, oper),
            TokenType.Slash => ToDouble(left, oper) / ToDouble(right, oper),
            TokenType.Star => ToDouble(left, oper) * ToDouble(right, oper),
            TokenType.Plus => (left, right) switch
            {
                (double leftVal, double rightVal) => leftVal + rightVal,
                (string leftVal, string rightVal) => leftVal + rightVal,
                _ => throw Error(oper, "Operands must be two numbers or two strings.")
            },
            TokenType.Greater => ToDouble(left, oper) > ToDouble(right, oper),
            TokenType.GreaterEqual => ToDouble(left, oper) >= ToDouble(right, oper),
            TokenType.Less => ToDouble(left, oper) < ToDouble(right, oper),
            TokenType.LessEqual => ToDouble(left, oper) <= ToDouble(right, oper),
            TokenType.BangEqual => !IsEqual(left, right),
            TokenType.EqualEqual => IsEqual(left, right),
            _ => throw Error(oper, "Unexpected operator in binary expression.")
        };
    }

    public object? Visit(Logical expression)
    {
        var left = expression.Left.Accept(this);
        var oper = expression.Operator;

        return oper.Type switch
        {
            TokenType.Or => IsTrue(left) ? left : expression.Right.Accept(this),
            TokenType.And => !IsTrue(left) ? left : expression.Right.Accept(this),
            _ => throw Error(oper, "Unexpected operator in logical expression.")
        };
    }

    public object? Visit(Unary expression)
    {
        var right = expression.Right.Accept(this);
        var oper = expression.Operator;

        return oper.Type switch
        {
            TokenType.Minus => -ToDouble(right, oper),
            TokenType.Bang => !IsTrue(right),
            _ => throw Error(oper, "Unexpected operator in unary expression.")
        };
    }

    public object? Visit(Call expression)
    {
        var callee = expression.Callee.Accept(this);
        var arguments = expression.Arguments.Select(arg => arg.Accept(this)).ToList();

        return callee switch
        {
            ICallable function when arguments.Count == function.Arity() =>
                function.Call(this, arguments),
            ICallable function =>
                throw Error(expression.Paren,
                    $"Expected {function.Arity()} arguments but got {arguments.Count}."),
            _ => throw Error(expression.Paren, "Can only call functions and classes.")
        };
    }

    public object? Visit(Get expression)
    {
        var owner = expression.Owner.Accept(this);

        return owner switch
        {
            UserClassInstance instance => instance.Get(expression.Name),
            _ => throw Error(expression.Name, "Only instances have properties.")
        };
    }

    public object? Visit(Set expression)
    {
        var owner = expression.Owner.Accept(this);
        var value = expression.Value.Accept(this);

        return owner switch
        {
            UserClassInstance instance => instance.Set(expression.Name, value),
            _ => throw Error(expression.Name, "Only instances have fields.")
        };
    }

    public object? Visit(This expression) => LookupVariable(expression.Keyword, expression);

    public object? Visit(Super expression)
    {
        var distance = _locals[expression];

        var superclass = _environment.GetAt(distance, "super") as UserClass;

        var instance = _environment.GetAt(distance - 1, "this") as UserClassInstance;

        if (!superclass!.TryFindMethod(expression.Method.Lexeme.Get().ToString(), out var method))
            throw Error(expression.Method, $"Undefined property '{expression.Method.Lexeme.Get().ToString()}'.");

        return method!.Bind(instance!);
    }

    public object? Visit(Grouping expression) => expression.Expr.Accept(this);

    public object? Visit(Literal expression) => expression.Value;

    public object? Visit(Variable expression) => LookupVariable(expression.Name, expression);

    #endregion

    private object? LookupVariable(Token name, IExpression expression) =>
        _locals.TryGetValue(expression, out var depth)
            ? _environment.GetAt(depth, name.Lexeme.Get().ToString())
            : _globals.Get(name);

    private static bool IsTrue(object? obj) => obj switch
    {
        null or false => false,
        _ => true
    };

    private static bool IsEqual(object? left, object? right) => (left, right) switch
    {
        (null, null) => true,
        (null, _) or (_, null) => false,
        (object objL, object objR) => objL.Equals(objR)
    };

    private static double ToDouble(object? obj, Token oper) => obj switch
    {
        double value => value,
        _ => throw Error(oper, "Operand must be a number.")
    };

    private static RuntimeError Error(Token token, string message) => new(token, message);

    public class ReturnValue(object? value) : Exception
    {
        public object? Value { get; } = value;
    }
}
