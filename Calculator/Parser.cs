using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
// ReSharper disable All

namespace Calculator
{
  public enum BindingPower
  {
    LOWEST = 10,
    OR = 20,
    AND = 30,
    SUM = 40,
    PRODUCT = 50,
    POWER = 60,
    PREFIX = 70
  }

  interface IInfixParslet
  {
    IExpression Parse(Parser parser, IExpression left, Token token);
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

    public IExpression Parse(Parser parser, IExpression left, Token tok)
    {
      var expression = new InfixExpression()
      {
        token = tok,
        op = tok.Literal,
        left = left
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

  public class Parser
  {
    private readonly Dictionary<TokenType, IPrefixParslet> _prefixParslets = new Dictionary<TokenType, IPrefixParslet>();
    private readonly Dictionary<TokenType, IInfixParslet> _infixParslets = new Dictionary<TokenType, IInfixParslet>();
    private Token _curToken;
    private Token _peekToken;

    private Lexer _lexer;
    private List<string> _errors = new List<string>();

    public Parser(Lexer lexer)
    {
      _lexer = lexer;

      RegisterPrefix(TokenType.INT, new IntegerParslet());
      RegisterPrefix(TokenType.MINUS, new PrefixOperatorParslet());
      RegisterPrefix(TokenType.EOF, null);

      RegisterInfix(TokenType.OR, new InfixOperatorParslet(BindingPower.OR, true));
      RegisterInfix(TokenType.AND, new InfixOperatorParslet(BindingPower.AND, true));

      RegisterInfix(TokenType.CARROT, new InfixOperatorParslet(BindingPower.POWER, true));
      RegisterInfix(TokenType.MINUS, new InfixOperatorParslet(BindingPower.SUM, false));
      RegisterInfix(TokenType.PLUS, new InfixOperatorParslet(BindingPower.SUM, false));
      RegisterInfix(TokenType.ASTERISK, new InfixOperatorParslet(BindingPower.PRODUCT, false));
      RegisterInfix(TokenType.FSLASH, new InfixOperatorParslet(BindingPower.SUM, false));

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

    public IExpression ParseExpression(BindingPower bindingPower)
    {
      var prefixParslet = _prefixParslets[_curToken.Type];
      if (prefixParslet == null)
      {
        // add error logging
        return null;
      }

      var leftexp = prefixParslet.Parse(this, _curToken);

      while (!PeekTokenIs(TokenType.EOF) && bindingPower < PeekBindingPower())
      {
        var infixParslet = _infixParslets[_peekToken.Type];
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

    private IStatement ParseStatement()
    {
      return ParseExpressionStatement();
    }

    private ExpressionStatement ParseExpressionStatement()
    {
      var stmt = new ExpressionStatement();
      stmt.token = _curToken;
      stmt.Expression = ParseExpression(BindingPower.LOWEST);

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

    private bool PeekTokenIs(TokenType tokenType)
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
      _errors.Add($"Expected next token to be {tokenType} but got {_peekToken.Type} instead.");
    }
  }
}
