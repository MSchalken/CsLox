using Schalken.CsLox.Lexing;
using Schalken.CsLox.Parsing;

namespace Schalken.CsLox.Interpreting;

internal class Interpreter : IStatementVisitor, IExpressionVisitor<object?>
{
    private static readonly Interpreter _instance = new();
    private Interpreter() { }

    public static void Interpret(IEnumerable<IStatement> statements)
    {
        try
        {
            foreach (var statement in statements)
            {
                statement.Accept(_instance);
            }
        }
        catch (RuntimeError e)
        {
            Logger.Error(e);
        }
    }

    #region Statements

    public void Visit(Expression statement)
    {
        statement.Expr.Accept(this);
    }

    public void Visit(Print statement)
    {
        var value = statement.Expr.Accept(this);
        Console.WriteLine(value?.ToString() ?? "nil");
    }

    #endregion

    #region Expressions

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

    public object? Visit(Grouping expression) => expression.Expr.Accept(this);

    public object? Visit(Literal expression) => expression.Value;

    #endregion

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
}
