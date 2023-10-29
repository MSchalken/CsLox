using Schalken.CsLox.Lexing;

namespace Schalken.CsLox.Parsing;

internal interface IExpr
{
	T Accept<T>(IExpressionVisitor<T> visitor);
}

internal sealed record Binary(IExpr Left, Token Operator, IExpr Right) : IExpr
{
	public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

internal sealed record Grouping(IExpr Expression) : IExpr
{
	public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

internal sealed record Literal(object? Value) : IExpr
{
	public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

internal sealed record Unary(Token Operator, IExpr Right) : IExpr
{
	public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

internal interface IExpressionVisitor<T>
{
	T Visit(Binary expression);
	T Visit(Grouping expression);
	T Visit(Literal expression);
	T Visit(Unary expression);
}
