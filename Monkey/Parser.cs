using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
// ReSharper disable All

namespace Monkey
{
  public enum BindingPower
  {
    LOWEST = 10,
    EQUALS,
    OR = 20,
    AND = 30,
    COMPARISON = 35,
    SUM = 40,
    PRODUCT = 50,
    POWER = 60,
    PREFIX = 70,
    CALL,
    INDEX
  }


  public class Parser
  {
    private readonly Dictionary<TokenType, IPrefixParslet> _prefixParslets = new Dictionary<TokenType, IPrefixParslet>();
    private readonly Dictionary<TokenType, IInfixParslet> _infixParslets = new Dictionary<TokenType, IInfixParslet>();
    private Token _curToken;
    private Token _peekToken;

    private Lexer _lexer;

    public List<string> Errors { get; } = new List<string>();

    public bool HasError
    {
      get { return Errors.Count > 0 ? true : false; }
    }

    public Parser(Lexer lexer)
    {
      _lexer = lexer;

      RegisterPrefix(TokenType.INT, new IntegerParslet());
      RegisterPrefix(TokenType.MINUS, new PrefixOperatorParslet());
      RegisterPrefix(TokenType.LPAREN, new GroupedOperatorParslet());
      RegisterPrefix(TokenType.IDENT, new IdentifierParslet());
      RegisterPrefix(TokenType.FUNCTION, new FunctionLiteralParslet());
      RegisterPrefix(TokenType.IF, new IfExpressionParslet());
      RegisterPrefix(TokenType.STRING, new StringParslet());
      RegisterPrefix(TokenType.TRUE, new BooleanParslet());
      RegisterPrefix(TokenType.FALSE, new BooleanParslet());
      RegisterPrefix(TokenType.BANG, new PrefixOperatorParslet());
      RegisterPrefix(TokenType.LBRACKET, new ArrayLiteralParslet());
      RegisterPrefix(TokenType.LBRACE, new HashLiteralParslet());

      RegisterInfix(TokenType.OR, new InfixOperatorParslet(BindingPower.OR, true));
      RegisterInfix(TokenType.AND, new InfixOperatorParslet(BindingPower.AND, true));

      RegisterInfix(TokenType.EQ, new InfixOperatorParslet(BindingPower.EQUALS, false));
      RegisterInfix(TokenType.LT, new InfixOperatorParslet(BindingPower.COMPARISON, false));
      RegisterInfix(TokenType.GT, new InfixOperatorParslet(BindingPower.COMPARISON, false));
      RegisterInfix(TokenType.CARROT, new InfixOperatorParslet(BindingPower.POWER, true));
      RegisterInfix(TokenType.MINUS, new InfixOperatorParslet(BindingPower.SUM, false));
      RegisterInfix(TokenType.PLUS, new InfixOperatorParslet(BindingPower.SUM, false));
      RegisterInfix(TokenType.ASTERISK, new InfixOperatorParslet(BindingPower.PRODUCT, false));
      RegisterInfix(TokenType.FSLASH, new InfixOperatorParslet(BindingPower.SUM, false));
      RegisterInfix(TokenType.LPAREN, new ParseCallExpressionParslet(BindingPower.CALL, false));
      RegisterInfix(TokenType.LBRACKET, new IndexExpressionParslet(BindingPower.INDEX));

      NextToken();
      NextToken();
    }

    private void RegisterInfix(TokenType tokenType, IInfixParslet parslet)
    {
      _infixParslets.Add(tokenType, parslet);
    }

    private void RegisterPrefix(TokenType tokenType, IPrefixParslet parslet)
    {
      _prefixParslets.Add(tokenType, parslet);
    }

    private IPrefixParslet GetPrefixParslet(TokenType type)
    {
      if (_prefixParslets.ContainsKey(type))
        return _prefixParslets[type];

      return null;
    }

    private IInfixParslet GetInfixParslet(TokenType peekTokenType)
    {
      if (_infixParslets.ContainsKey(peekTokenType))
        return _infixParslets[peekTokenType];

      return null;
    }

    public IExpression ParseExpression(BindingPower bindingPower)
    {
      var prefixParslet = GetPrefixParslet(_curToken.Type);
      if (prefixParslet == null)
      {
        Errors.Add($"No Prefix Parslet for {_curToken.Type}");

        return null;
      }

      var leftexp = prefixParslet.Parse(this, _curToken);

      while (!PeekTokenIs(TokenType.EOF) && bindingPower < PeekBindingPower())
      {
        var infixParslet = GetInfixParslet(_peekToken.Type);
        if (infixParslet == null)
        {
          return leftexp;
        }

        NextToken();

        leftexp = infixParslet.Parse(this, leftexp, _curToken);
      }

      return leftexp;
    }

    private BindingPower PeekBindingPower()
    {
      if (!_infixParslets.ContainsKey(_peekToken.Type)) return BindingPower.LOWEST;

      IInfixParslet parslet = _infixParslets[_peekToken.Type];
      return parslet.GetBindingPower();
    }

    public Code ParseCode()
    {
      Code code = new Code();

      while (!CurTokenIs(TokenType.EOF))
      {
        IStatement statement = ParseStatement();
        if (statement != null)
          code.Statements.Add(statement);
        NextToken();
      }

      return code;
    }

    public IStatement ParseStatement()
    {
      switch (_curToken.Type)
      {
        case TokenType.LET:
          return ParseLetStatement();
        case TokenType.RETURN:
          return ParseReturnStatement();
        default:
          return ParseExpressionStatement();
      }
    }

    private IStatement ParseLetStatement()
    {
      var stmt = new LetStatement() { token = _curToken };

      if (!ExpectPeek(TokenType.IDENT))
        return null;

      stmt.Name = new Identifier() { token = _curToken, Value = _curToken.Literal };

      if (!ExpectPeek(TokenType.ASSIGN))
        return null;

      NextToken();

      stmt.Value = ParseExpression(BindingPower.LOWEST);

      return stmt;
    }

    private ExpressionStatement ParseExpressionStatement()
    {
      var stmt = new ExpressionStatement();
      stmt.token = _curToken;
      stmt.Expression = ParseExpression(BindingPower.LOWEST);

      return stmt;
    }

    private ReturnStatement ParseReturnStatement()
    {
      var stmt = new ReturnStatement() { token = _curToken };

      NextToken();

      stmt.ReturnValue = ParseExpression(BindingPower.LOWEST);

      return stmt;
    }

    public void NextToken()
    {
      _curToken = _peekToken;
      _peekToken = _lexer.NextToken();
    }

    private bool CurTokenIs(TokenType tokenType)
    {
      return _curToken.Type == tokenType;
    }

    public bool PeekTokenIs(TokenType tokenType)
    {
      return _peekToken.Type == tokenType;
    }

    public bool ExpectPeek(TokenType tokenType)
    {
      if (PeekTokenIs(tokenType))
      {
        NextToken();
        return true;
      }
      else
      {
        PeekError(tokenType);
        return false;
      }
    }

    private void PeekError(TokenType tokenType)
    {
      Errors.Add($"Expected next token to be {tokenType} but got {_peekToken.Type} instead.");
    }

    public Token CurrentToken() => _curToken;

  }

  interface IInfixParslet
  {
    IExpression Parse(Parser parser, IExpression function, Token token);
    BindingPower GetBindingPower();
  }

  class InfixOperatorParslet : IInfixParslet
  {
    private BindingPower _bindingPower;

    public BindingPower GetBindingPower() => _bindingPower;

    private bool _isRight;

    public InfixOperatorParslet(BindingPower bindingPower, bool isRight)
    {
      _bindingPower = bindingPower;
      _isRight = isRight;
    }

    public IExpression Parse(Parser parser, IExpression function, Token tok)
    {
      var expression = new InfixExpression()
      {
        token = tok,
        op = tok.Literal,
        left = function
      };

      parser.NextToken();

      expression.right = parser.ParseExpression(_bindingPower - (_isRight ? 1 : 0));

      return expression;
    }
  }

  interface IPrefixParslet
  {
    IExpression Parse(Parser parser, Token token);
  }

  class StringParslet : IPrefixParslet
  {
    public IExpression Parse(Parser parser, Token token)
    {
      return new StringLiteral()
      {
        token = token,
        Value = token.Literal
      };
    }
  }

  class IntegerParslet : IPrefixParslet
  {
    public IExpression Parse(Parser parser, Token token)
    {
      return new IntegerLiteral()
      {
        token = token,
        Value = int.Parse(token.Literal)
      };
    }
  }

  class PrefixOperatorParslet : IPrefixParslet
  {
    public IExpression Parse(Parser parser, Token token)
    {
      parser.NextToken();

      IExpression operand = parser.ParseExpression(BindingPower.PREFIX);

      return new PrefixExpression()
      {
        token = token,
        op = token.Literal,
        right = operand
      };
    }
  }

  public class BooleanParslet : IPrefixParslet
  {
    public IExpression Parse(Parser parser, Token token)
    {
      return new BooleanLiteral()
      {
        token = token,
        Value = bool.Parse(token.Literal)
      };
    }
  }

  public class IfExpressionParslet : IPrefixParslet
  {
    public IExpression Parse(Parser parser, Token token)
    {
      var expr = new IfExpression() { token = parser.CurrentToken() };

      if (!parser.ExpectPeek(TokenType.LPAREN))
        return null;

      parser.NextToken();
      expr.Condition = parser.ParseExpression(BindingPower.LOWEST);

      if (!parser.ExpectPeek(TokenType.RPAREN))
        return null;

      if (!parser.ExpectPeek(TokenType.LBRACE))
        return null;

      expr.Consequence = new BlockStatementParslet().Parse(parser);

      if (parser.PeekTokenIs(TokenType.ELSE))
      {
        parser.NextToken();

        if (!parser.PeekTokenIs(TokenType.LBRACE) && !parser.PeekTokenIs(TokenType.IF))
          return null;

        if (parser.PeekTokenIs(TokenType.LBRACE))
        {
          parser.NextToken();
          expr.Alternative = new BlockStatementParslet().Parse(parser);
        }
        else
        {
          parser.NextToken();
          expr.Alternative = parser.ParseExpression(BindingPower.LOWEST);
        }
      }

      return expr;
    }
  }

  public class ParseCallExpressionParslet : IInfixParslet
  {
    private BindingPower _bindingPower;

    public BindingPower GetBindingPower() => _bindingPower;

    private bool _isRight;

    public ParseCallExpressionParslet(BindingPower bp, bool isRight)
    {
      _bindingPower = bp;
      _isRight = isRight;
    }

    public IExpression Parse(Parser parser, IExpression function, Token token)
    {
      var exp = new CallExpression() { token = token, Function = function };
      exp.Arguments = ParseCallArguments(parser);
      return exp;
    }

    private List<IExpression> ParseCallArguments(Parser parser)
    {
      var args = new List<IExpression>();

      if (parser.PeekTokenIs(TokenType.RPAREN))
      {
        parser.NextToken();
        return args;
      }

      parser.NextToken();

      args.Add(parser.ParseExpression(BindingPower.LOWEST));

      while (parser.PeekTokenIs(TokenType.COMMA))
      {
        parser.NextToken();
        parser.NextToken();
        args.Add(parser.ParseExpression(BindingPower.LOWEST));
      }

      if (!parser.ExpectPeek(TokenType.RPAREN))
        return null;

      return args;
    }
  }

  public class FunctionLiteralParslet : IPrefixParslet
  {
    public IExpression Parse(Parser parser, Token token)
    {
      var literal = new FunctionLiteral() { token = token };

      if (!parser.ExpectPeek(TokenType.LPAREN))
        return null;

      literal.Parameters = ParseFunctionParameters(parser);

      if (!parser.ExpectPeek(TokenType.LBRACE))
        return null;

      literal.Body = new BlockStatementParslet().Parse(parser);

      return literal;
    }

    private List<Identifier> ParseFunctionParameters(Parser parser)
    {
      var identifiers = new List<Identifier>();

      if (parser.PeekTokenIs(TokenType.RPAREN))
      {
        parser.NextToken();
        return identifiers;
      }

      parser.NextToken();

      var ident = new Identifier() { token = parser.CurrentToken(), Value = parser.CurrentToken().Literal };
      identifiers.Add(ident);

      while (parser.PeekTokenIs(TokenType.COMMA))
      {
        parser.NextToken();
        parser.NextToken();
        ident = new Identifier() { token = parser.CurrentToken(), Value = parser.CurrentToken().Literal };
        identifiers.Add(ident);
      }

      if (!parser.ExpectPeek(TokenType.RPAREN))
        return null;

      return identifiers;
    }
  }

  public class BlockStatementParslet
  {
    public BlockStatement Parse(Parser parser)
    {
      var block = new BlockStatement() { token = parser.CurrentToken(), Statements = new List<IStatement>() };

      parser.NextToken();

      while (parser.CurrentToken().Type != TokenType.RBRACE && parser.CurrentToken().Type != TokenType.EOF)
      {
        var stmt = parser.ParseStatement();
        if (stmt != null)
          block.Statements.Add(stmt);
        parser.NextToken();
      }

      return block;
    }
  }

  public class IdentifierParslet : IPrefixParslet
  {
    public IExpression Parse(Parser parser, Token token)
    {
      return new Identifier() { token = token, Value = token.Literal };
    }
  }

  public class GroupedOperatorParslet : IPrefixParslet
  {
    public IExpression Parse(Parser parser, Token token)
    {
      parser.NextToken();

      var exp = parser.ParseExpression(BindingPower.LOWEST);

      if (!parser.ExpectPeek(TokenType.RPAREN))
      {
        return null;
      }

      return exp;
    }
  }

  public class IndexExpressionParslet : IInfixParslet
  {
    private BindingPower _bindingPower;

    public BindingPower GetBindingPower() => _bindingPower;

    public IndexExpressionParslet(BindingPower bp)
    {
      _bindingPower = bp;
    }

    public IExpression Parse(Parser parser, IExpression function, Token token)
    {
      var exp = new IndexExpression()
      {
        token = token,
        Left = function
      };

      parser.NextToken();
      exp.Index = parser.ParseExpression(BindingPower.LOWEST);

      if (!parser.ExpectPeek(TokenType.RBRACKET))
        return null;

      return exp;
    }
  }

  public class HashLiteralParslet : IPrefixParslet
  {
    public IExpression Parse(Parser parser, Token token)
    {
      var hash = new HashLiteral
      {
        token = token,
        Pairs = new Dictionary<IExpression, IExpression>()
      };

      while (!parser.PeekTokenIs(TokenType.RBRACE))
      {
        parser.NextToken();
        var key = parser.ParseExpression(BindingPower.LOWEST);

        if (!parser.ExpectPeek(TokenType.COLON))
          return null;

        parser.NextToken();
        var value = parser.ParseExpression(BindingPower.LOWEST);

        hash.Pairs[key] = value;

        if (!parser.PeekTokenIs(TokenType.RBRACE) && !parser.ExpectPeek(TokenType.COMMA))
          return null;
      }

      if (!parser.ExpectPeek(TokenType.RBRACE))
        return null;

      return hash;
    }
  }

  public class ArrayLiteralParslet : IPrefixParslet
  {
    public IExpression Parse(Parser parser, Token token)
    {
      return new ArrayLiteral
      {
        token = token,
        Elements = ParseExpressionList(parser)
      };
    }

    private List<IExpression> ParseExpressionList(Parser parser)
    {
      var args = new List<IExpression>();

      if (parser.PeekTokenIs(TokenType.RBRACKET))
      {
        parser.NextToken();
        return args;
      }

      parser.NextToken();

      args.Add(parser.ParseExpression(BindingPower.LOWEST));

      while (parser.PeekTokenIs(TokenType.COMMA))
      {
        parser.NextToken();
        parser.NextToken();
        args.Add(parser.ParseExpression(BindingPower.LOWEST));
      }

      if (!parser.ExpectPeek(TokenType.RBRACKET))
        return null;

      return args;
    }
  }
}
