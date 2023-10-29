using Schalken.CsLox.Lexing;

namespace Schalken.CsLox.Parsing;

internal class Parser(List<Token> tokens)
{
    private readonly List<Token> _tokens = tokens;

    private int _current = 0;

    public IExpr? Parse()
    {
        try
        {
            return Expression();
        }
        catch (ParseError)
        {
            return null;
        }
    }

    private IExpr Expression() => Equality();

    private IExpr Equality()
    {
        var expr = Comparison();

        while (Match(TokenType.BangEqual, TokenType.EqualEqual))
        {
            var oper = Previous();
            var right = Comparison();
            expr = new Binary(expr, oper, right);
        }

        return expr;
    }

    private IExpr Comparison()
    {
        var expr = Term();

        while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
        {
            var oper = Previous();
            var right = Term();
            expr = new Binary(expr, oper, right);
        }

        return expr;
    }

    private IExpr Term()
    {
        var expr = Factor();

        while (Match(TokenType.Minus, TokenType.Plus))
        {
            var oper = Previous();
            var right = Factor();
            expr = new Binary(expr, oper, right);
        }

        return expr;
    }

    private IExpr Factor()
    {
        var expr = Unary();

        while (Match(TokenType.Slash, TokenType.Star))
        {
            var oper = Previous();
            var right = Unary();
            expr = new Binary(expr, oper, right);
        }

        return expr;
    }

    private IExpr Unary()
    {
        if (Match(TokenType.Bang, TokenType.Minus))
        {
            var oper = Previous();
            var right = Unary();
            return new Unary(oper, right);
        }

        return Primary();
    }

    private IExpr Primary()
    {
        if (Match(TokenType.False)) return new Literal(false);
        if (Match(TokenType.True)) return new Literal(true);
        if (Match(TokenType.Nil)) return new Literal(null);
        if (Match(TokenType.Number, TokenType.String)) return new Literal(Previous().Literal);
        if (Match(TokenType.LeftParen))
        {
            var expr = Expression();
            Consume(TokenType.RightParen, "Expect ')' after expression.");
            return new Grouping(expr);
        }

        throw CreateParseError(Peek(), "Expect expression.");
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().Type == TokenType.Semicolon) return;

            if (Peek().Type
                is TokenType.Class
                or TokenType.Fun
                or TokenType.Var
                or TokenType.For
                or TokenType.If
                or TokenType.While
                or TokenType.Print
                or TokenType.Return
            ) return;

            Advance();
        }
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw CreateParseError(Peek(), message);
    }

    private ParseError CreateParseError(Token token, string message)
    {
        Logger.Error(token, message);
        return new();
    }

    private bool Match(params TokenType[] tokenTypes)
    {
        if (tokenTypes.Any(Check))
        {
            Advance();
            return true;
        }

        return false;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private Token Peek() => _tokens[_current];
    private Token Previous() => _tokens[_current - 1];
    private bool IsAtEnd() => Peek().Type == TokenType.Eof;
    private bool Check(TokenType type) => !IsAtEnd() && Peek().Type == type;

    private class ParseError : Exception;
}
