using Schalken.CsLox.Lexing;

namespace Schalken.CsLox;

internal class RuntimeError : Exception
{
    public Token Token { get; }
    public RuntimeError() { }
    public RuntimeError(Token token, string message)
        : base(message)
    {
        Token = token;
    }
    public RuntimeError(Token token, string message, Exception inner)
        : base(message, inner)
    {
        Token = token;
    }
}
