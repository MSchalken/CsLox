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
            if (Match(TokenType.Fun)) return FuncDeclaration("function");
            if (Match(TokenType.Class)) return ClassDeclaration();

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

    private FuncDecl FuncDeclaration(string kind)
    {
        var name = Consume(TokenType.Identifier, $"Expect {kind} name.");
        Consume(TokenType.LeftParen, $"Expect '(' after {kind} name.");

        var parameters = new List<Token>();

        if (!Check(TokenType.RightParen))
        {
            do
            {
                if (parameters.Count > 255)
                {
                    Logger.Error(Peek(), "Can't have more than 255 parameters.");
                }

                var parameter = Consume(TokenType.Identifier, "Expect parameter name.");
                parameters.Add(parameter);
            } while (Match(TokenType.Comma));
        }

        Consume(TokenType.RightParen, "Expect ')' after parameters.");
        Consume(TokenType.LeftBrace, $"Expect '{{' before {kind} body.");

        var body = BlockStatement();
        return new FuncDecl(name, parameters, body.Statements);
    }

    private ClassDecl ClassDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expect class name.");

        var superclass = Match(TokenType.Less)
            ? new Variable(Consume(TokenType.Identifier, "Expect superclass name."))
            : null;


        Consume(TokenType.LeftBrace, "Expect '{' before class body.");

        var methods = new List<FuncDecl>();

        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            methods.Add(FuncDeclaration("method"));
        }

        Consume(TokenType.RightBrace, "Expect '}' after class body.");
        return new ClassDecl(name, superclass, methods);
    }

    private IStatement Statement()
    {
        if (Match(TokenType.LeftBrace)) return BlockStatement();
        if (Match(TokenType.If)) return IfStatement();
        if (Match(TokenType.While)) return WhileStatement();
        if (Match(TokenType.For)) return ForStatement();
        if (Match(TokenType.Return)) return ReturnStatement();
        if (Match(TokenType.Print)) return PrintStatement();

        return ExpressionStatement();
    }

    private Block BlockStatement()
    {
        var statements = new List<IStatement>();

        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            var decl = Declaration();
            if (decl is not null)
            {
                statements.Add(decl);
            }
        }

        Consume(TokenType.RightBrace, "Expect '}' after block.");

        return new Block(statements);
    }

    private If IfStatement()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'if'.");
        var condition = Expression();
        Consume(TokenType.RightParen, "Expect ')' after if condition.");

        var thenBranch = Statement();
        var elseBranch = Match(TokenType.Else)
            ? Statement()
            : null;

        return new If(condition, thenBranch, elseBranch);
    }

    private While WhileStatement()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'while'.");
        var condition = Expression();
        Consume(TokenType.RightParen, "Expect ')' after while condition.");

        var body = Statement();

        return new While(condition, body);
    }

    private IStatement ForStatement()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'for'.");

        IStatement? initializer =
            Match(TokenType.Semicolon) ? null
            : Match(TokenType.Var) ? VarDeclaration()
            : ExpressionStatement();

        IExpression? condition =
            Check(TokenType.Semicolon) ? null
            : Expression();
        Consume(TokenType.Semicolon, "Expect ';' after loop condition.");

        IExpression? continuation =
            Check(TokenType.RightParen) ? null
            : Expression();
        Consume(TokenType.RightParen, "Expect ')' after loop continuation.");

        var body = Statement();

        if (continuation is not null)
        {
            body = new Block([body, new Expression(continuation)]);
        }

        condition ??= new Literal(true);
        body = new While(condition, body);

        if (initializer is not null)
        {
            body = new Block([initializer, body]);
        }

        return body;
    }

    private Return ReturnStatement()
    {
        var keyword = Previous();
        var value = Check(TokenType.Semicolon)
            ? null
            : Expression();

        Consume(TokenType.Semicolon, "Expect ';' after return value.");
        return new Return(keyword, value);
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
        var expr = Or();

        if (Match(TokenType.Equal))
        {
            var equals = Previous();
            var value = Assignment();

            if (expr is Variable varExpr)
            {
                return new Assign(varExpr.Name, value);
            }
            else if (expr is Get getExpr)
            {
                return new Set(getExpr.Owner, getExpr.Name, value);
            }

            Logger.Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private IExpression Or()
    {
        var expr = And();

        while (Match(TokenType.Or))
        {
            var oper = Previous();
            var right = And();
            expr = new Logical(expr, oper, right);
        }

        return expr;
    }

    private IExpression And()
    {
        var expr = Equality();

        while (Match(TokenType.And))
        {
            var oper = Previous();
            var right = Equality();
            expr = new Logical(expr, oper, right);
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

        return Call();
    }

    private IExpression Call()
    {
        var expr = Primary();

        while (true)
        {
            if (Match(TokenType.LeftParen))
            {
                expr = CallArguments(expr);
            }
            else if (Match(TokenType.Dot))
            {
                var name = Consume(TokenType.Identifier, "Expect property name after '.'.");
                expr = new Get(expr, name);
            }
            else
            {
                return expr;
            }
        }
    }

    private Call CallArguments(IExpression callee)
    {
        var arguments = new List<IExpression>();

        if (!Check(TokenType.RightParen))
        {
            do
            {
                if (arguments.Count > 255)
                {
                    Logger.Error(Peek(), "Can't have more than 255 arguments.");
                }

                arguments.Add(Expression());
            } while (Match(TokenType.Comma));
        }

        var paren = Consume(TokenType.RightParen, "Expect ')' after arguments.");
        return new Call(callee, paren, arguments);
    }

    private IExpression Primary()
    {
        if (Match(TokenType.False)) return new Literal(false);
        if (Match(TokenType.True)) return new Literal(true);
        if (Match(TokenType.Nil)) return new Literal(null);
        if (Match(TokenType.Number, TokenType.String)) return new Literal(Previous().Literal);
        if (Match(TokenType.Identifier)) return new Variable(Previous());
        if (Match(TokenType.This)) return new This(Previous());
        if (Match(TokenType.Super))
        {
            var keyword = Previous();
            Consume(TokenType.Dot, "Expect '.' after 'super'.");
            var method = Consume(TokenType.Identifier, "Expect superclass methods name.");
            return new Super(keyword, method);
        }
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
