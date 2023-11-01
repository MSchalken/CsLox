using Schalken.CsLox.Lexing;

namespace Schalken.CsLox.Parsing;

internal interface IStatement
{
	T Accept<T>(IStatementVisitor<T> visitor);
	void Accept(IStatementVisitor visitor);
}

internal sealed record Block(List<IStatement> Statements) : IStatement
{
	public T Accept<T>(IStatementVisitor<T> visitor) => visitor.Visit(this);
	public void Accept(IStatementVisitor visitor) => visitor.Visit(this);
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

internal sealed record VarDecl(Token Name, IExpression? InitExpr) : IStatement
{
	public T Accept<T>(IStatementVisitor<T> visitor) => visitor.Visit(this);
	public void Accept(IStatementVisitor visitor) => visitor.Visit(this);
}

internal interface IStatementVisitor<T>
{
	T Visit(Block statement);
	T Visit(Expression statement);
	T Visit(Print statement);
	T Visit(VarDecl statement);
}
internal interface IStatementVisitor
{
	void Visit(Block statement);
	void Visit(Expression statement);
	void Visit(Print statement);
	void Visit(VarDecl statement);
}
