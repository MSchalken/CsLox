namespace Schalken.CsLox.Lexing;

internal readonly record struct Token(TokenType Type, Lexeme Lexeme, object? Literal)
{
    public override readonly string ToString() => $"{Type} {Lexeme} {Literal}";

    public static TokenType LookupIdentifier(string identifier) => identifier switch
    {
        "and" => TokenType.And,
        "class" => TokenType.Class,
        "else" => TokenType.Else,
        "false" => TokenType.False,
        "for" => TokenType.For,
        "fun" => TokenType.Fun,
        "if" => TokenType.If,
        "nil" => TokenType.Nil,
        "or" => TokenType.Or,
        "print" => TokenType.Print,
        "return" => TokenType.Return,
        "super" => TokenType.Super,
        "this" => TokenType.This,
        "true" => TokenType.True,
        "var" => TokenType.Var,
        "while" => TokenType.While,
        _ => TokenType.Identifier
    };
}

internal readonly record struct Lexeme(string Source, int Index, int Length, int Line, int Column)
{
    private readonly Lazy<string> _lexemeAsString = new(() => new string(Source.AsSpan(Index, Length)));

    public readonly ReadOnlySpan<char> AsSpan() => Source.AsSpan(Index, Length);

    public override readonly string ToString() => _lexemeAsString.Value;
}
