using System.Text;

namespace Schalken.CsLox.Parsing;

internal class SyntaxTreePrinter : IExpressionVisitor<string>
{
    public static string Print(IExpression expression) => expression.Accept(new SyntaxTreePrinter());

    public string Visit(Binary expression)
    {
        return Parenthesize(expression.Operator.Lexeme.Get().ToString(), expression.Left, expression.Right);
    }

    public string Visit(Grouping expression)
    {
        return Parenthesize("group", expression.Expr);
    }

    public string Visit(Literal expression)
    {
        return expression.Value?.ToString() ?? "nil";
    }

    public string Visit(Unary expression)
    {
        return Parenthesize(expression.Operator.Lexeme.Get().ToString(), expression.Right);
    }

    private string Parenthesize(string name, params IExpression[] expressions)
    {
        var sb = new StringBuilder();

        sb.Append($"({name}");

        foreach (var expr in expressions)
        {
            sb.Append($" {expr.Accept(this)}");
        }

        sb.Append(')');

        return sb.ToString();
    }
}
