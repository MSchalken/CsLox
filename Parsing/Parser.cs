using Schalken.CsLox.Lexing;

namespace Schalken.CsLox.Parsing;

internal class Parser(List<Token> tokens)
{
    private readonly List<Token> _tokens = tokens;

    private int _current = 0;

    public IEnumerable<IStatement> Parse()
    {
        while (!IsAtEnd())
        {
            var decl = Declaration();
            if (decl is not null)
            {
                yield return decl;
            }
        }
    }

    #region Statement Parsing

    private IStatement? Declaration()
    {
        try
        {
            if (Match(TokenType.Var)) return VarDeclaration();

            return Statement();
        }
        catch (ParseError)
        {
            Synchronize();
            return null;
        }
    }

    private VarDecl VarDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expect variable name.");

        IExpression? init = null;

        if (Match(TokenType.Equal))
        {
            init = Expression();
        }

        Consume(TokenType.Semicolon, "Expect ';' after variable declaration.");
        return new VarDecl(name, init);
    }

    private IStatement Statement()
    {
        if (Match(TokenType.Print)) return PrintStatement();

        return ExpressionStatement();
    }

    private Print PrintStatement()
    {
        var expr = Expression();
        Consume(TokenType.Semicolon, "Expect ';' after statement.");
        return new Print(expr);
    }

    private Expression ExpressionStatement()
    {
        var expr = Expression();
        Consume(TokenType.Semicolon, "Expect ';' after statement.");
        return new Expression(expr);
    }

    #endregion

    #region Expression Parsing

    private IExpression Expression() => Assignment();

    private IExpression Assignment()
    {
        var expr = Equality();

        if (Match(TokenType.Equal))
        {
            var equals = Previous();
            var value = Assignment();

            if (expr is Variable varExpr)
            {
                return new Assign(varExpr.Name, value);
            }

            Logger.Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private IExpression Equality()
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

    private IExpression Comparison()
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

    private IExpression Term()
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

    private IExpression Factor()
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

    private IExpression Unary()
    {
        if (Match(TokenType.Bang, TokenType.Minus))
        {
            var oper = Previous();
            var right = Unary();
            return new Unary(oper, right);
        }

        return Primary();
    }

    private IExpression Primary()
    {
        if (Match(TokenType.False)) return new Literal(false);
        if (Match(TokenType.True)) return new Literal(true);
        if (Match(TokenType.Nil)) return new Literal(null);
        if (Match(TokenType.Number, TokenType.String)) return new Literal(Previous().Literal);
        if (Match(TokenType.Identifier)) return new Variable(Previous());
        if (Match(TokenType.LeftParen))
        {
            var expr = Expression();
            Consume(TokenType.RightParen, "Expect ')' after expression.");
            return new Grouping(expr);
        }

        throw CreateParseError(Peek(), "Expect expression.");
    }

    #endregion

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
