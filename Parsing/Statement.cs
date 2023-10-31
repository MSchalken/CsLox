namespace Schalken.CsLox.Parsing;

internal interface IStatement
{
	T Accept<T>(IStatementVisitor<T> visitor);
	void Accept(IStatementVisitor visitor);
}

internal sealed record Expression(IExpression Expr) : IStatement
{
	public T Accept<T>(IStatementVisitor<T> visitor) => visitor.Visit(this);
	public void Accept(IStatementVisitor visitor) => visitor.Visit(this);
}

internal sealed record Print(IExpression Expr) : IStatement
{
	public T Accept<T>(IStatementVisitor<T> visitor) => visitor.Visit(this);
	public void Accept(IStatementVisitor visitor) => visitor.Visit(this);
}

internal interface IStatementVisitor<T>
{
	T Visit(Expression statement);
	T Visit(Print statement);
}
internal interface IStatementVisitor
{
	void Visit(Expression statement);
	void Visit(Print statement);
}
