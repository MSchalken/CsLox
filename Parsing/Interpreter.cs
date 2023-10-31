using System.Diagnostics;
using Schalken.CsLox.Lexing;

namespace Schalken.CsLox.Parsing;

internal class Interpreter : IExpressionVisitor<object?>
{
    private static readonly Interpreter _instance = new();
    private Interpreter() { }

    public static void Interpret(IExpr expression)
    {
        try
        {
            var value = expression.Accept(_instance);
            Console.WriteLine(value?.ToString() ?? "nil");
        }
        catch (RuntimeError e)
        {
            Logger.Error(e);
        }
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
            _ => throw new UnreachableException()
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
            _ => throw new UnreachableException()
        };
    }

    public object? Visit(Grouping expression) => expression.Expression.Accept(this);

    public object? Visit(Literal expression) => expression.Value;

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
