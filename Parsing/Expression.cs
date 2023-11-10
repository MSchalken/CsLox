using Schalken.CsLox.Lexing;

namespace Schalken.CsLox.Parsing;

internal interface IExpression
{
	T Accept<T>(IExpressionVisitor<T> visitor);
	void Accept(IExpressionVisitor visitor);
}

internal sealed record Assign(Token Name, IExpression Value) : IExpression
{
	public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
	public void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
}

internal sealed record Binary(IExpression Left, Token Operator, IExpression Right) : IExpression
{
	public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
	public void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
}

internal sealed record Logical(IExpression Left, Token Operator, IExpression Right) : IExpression
{
	public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
	public void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
}

internal sealed record Unary(Token Operator, IExpression Right) : IExpression
{
	public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
	public void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
}

internal sealed record Call(IExpression Callee, Token Paren, List<IExpression> Arguments) : IExpression
{
	public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
	public void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
}

internal sealed record Get(IExpression Owner, Token Name) : IExpression
{
	public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
	public void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
}

internal sealed record Set(IExpression Owner, Token Name, IExpression Value) : IExpression
{
	public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
	public void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
}

internal sealed record Grouping(IExpression Expr) : IExpression
{
	public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
	public void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
}

internal sealed record Literal(object? Value) : IExpression
{
	public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
	public void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
}

internal sealed record Variable(Token Name) : IExpression
{
	public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
	public void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
}

internal interface IExpressionVisitor<T>
{
	T Visit(Assign expression);
	T Visit(Binary expression);
	T Visit(Logical expression);
	T Visit(Unary expression);
	T Visit(Call expression);
	T Visit(Get expression);
	T Visit(Set expression);
	T Visit(Grouping expression);
	T Visit(Literal expression);
	T Visit(Variable expression);
}
internal interface IExpressionVisitor
{
	void Visit(Assign expression);
	void Visit(Binary expression);
	void Visit(Logical expression);
	void Visit(Unary expression);
	void Visit(Call expression);
	void Visit(Get expression);
	void Visit(Set expression);
	void Visit(Grouping expression);
	void Visit(Literal expression);
	void Visit(Variable expression);
}
