namespace Schalken.CsLox.Lexing;

internal class Lexer
{
    private readonly string _input;
    private readonly List<Token> _tokens;

    private int _start = 0;
    private int _current = 0;
    private int _line = 0;

    public Lexer(string input)
    {
        _input = input;
        _tokens = new List<Token>();
    }

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            var token = ScanToken();
            if (token.HasValue) _tokens.Add(token.Value);
        }

        _tokens.Add(CreateToken(TokenType.Eof));
        return _tokens;
    }

    private bool IsAtEnd() => _current >= _input.Length;
    private char Advance() => _input.ElementAt(_current++);

    private Token CreateToken(TokenType type) => CreateToken(type, null);
    private Token CreateToken(TokenType type, object? literal) => new(type, new(_input, _start, _current - _start, _line, 0), literal);

    private static bool IsDigit(char c) => c is >= '0' and <= '9';
    private static bool IsAlpha(char c) => c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or '_';
    private static bool IsAlphaNumeric(char c) => IsDigit(c) || IsAlpha(c);

    private Token ReportIllegalCharacter()
    {
        Logger.Error(_line, "Unexpected character");
        return CreateToken(TokenType.Illegal);
    }

    private Token? NewLine()
    {
        _line++;
        return null;
    }

    private bool MatchNextChar(char c)
    {
        if (IsAtEnd()) return false;
        if (_input.ElementAt(_current) != c) return false;

        _current++;
        return true;
    }

    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        return _input.ElementAt(_current);
    }

    private char PeekNext()
    {
        if (_current + 1 >= _input.Length) return '\0';
        return _input.ElementAt(_current + 1);
    }

    private Token? ConsumeComment()
    {
        while (!IsAtEnd() && Peek() != '\n') Advance();
        return null;
    }

    private Token ReadString()
    {
        while (!IsAtEnd() && Peek() != '"')
        {
            if (Peek() == '\n') _line++;
            Advance();
        }

        if (IsAtEnd())
        {
            Logger.Error(_line, "Unterminated string.");
            return CreateToken(TokenType.Illegal);
        }

        Advance();
        var stringValue = _input.Substring(_start + 1, _current - 1);
        return CreateToken(TokenType.String, stringValue);
    }

    private Token? ReadNumber()
    {
        while (IsDigit(Peek())) Advance();

        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            Advance();
            while (IsDigit(Peek())) Advance();
        }

        var numberValue = double.Parse(_input.Substring(_start, _current));
        return CreateToken(TokenType.Number, numberValue);
    }

    private Token? ReadIdentifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();

        var identifier = _input.Substring(_start, _current);

        return CreateToken(Token.LookupIdentifier(identifier));
    }

    private Token? ScanToken()
    {
        var c = Advance();
        return c switch
        {
            '(' => CreateToken(TokenType.LeftParen),
            ')' => CreateToken(TokenType.RightParen),
            '{' => CreateToken(TokenType.LeftBrace),
            '}' => CreateToken(TokenType.RightBrace),
            ',' => CreateToken(TokenType.Comma),
            '.' => CreateToken(TokenType.Dot),
            '-' => CreateToken(TokenType.Minus),
            '+' => CreateToken(TokenType.Plus),
            ';' => CreateToken(TokenType.Semicolon),
            '*' => CreateToken(TokenType.Star),
            '!' => CreateToken(MatchNextChar('=') ? TokenType.BangEqual : TokenType.Bang),
            '=' => CreateToken(MatchNextChar('=') ? TokenType.EqualEqual : TokenType.Equal),
            '<' => CreateToken(MatchNextChar('=') ? TokenType.LessEqual : TokenType.Less),
            '>' => CreateToken(MatchNextChar('=') ? TokenType.GreaterEqual : TokenType.Greater),
            '/' when MatchNextChar('/') => ConsumeComment(),
            '/' => CreateToken(TokenType.Slash),
            ' ' => null,
            '\r' => null,
            '\t' => null,
            '\n' => NewLine(),
            '"' => ReadString(),
            >= '0' and <= '9' => ReadNumber(),
            (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or '_' => ReadIdentifier(),

            _ => ReportIllegalCharacter()
        };
    }

}