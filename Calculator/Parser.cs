using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
// ReSharper disable All

namespace Calculator
{
  public enum Precedence
  {
    LOWEST = 0,
    SUM,
    PRODUCT,
    POWER,
    PREFIX
  }

  interface IInfixParslet
  {
    IExpression Parse(Parser parser, IExpression left, Token token);
    Precedence GetPrecedence();
  }

  class InfixOperatorParslet : IInfixParslet
  {
    private Precedence _precedence;

    public Precedence GetPrecedence() => _precedence;

    private bool _isRight;

    public InfixOperatorParslet(Precedence precedence, bool isRight)
    {
      _precedence = precedence;
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

      expression.right = parser.ParseExpression(_precedence - (_isRight ? 1 : 0));

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

      IExpression operand = parser.ParseExpression(Precedence.PREFIX);

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
    private readonly Dictionary<string, IPrefixParslet> _prefixParslets = new Dictionary<string, IPrefixParslet>();
    private readonly Dictionary<string, IInfixParslet> _infixParslets = new Dictionary<string, IInfixParslet>();
    private Token _curToken;
    private Token _peekToken;

    private Lexer _lexer;
    private List<string> _errors = new List<string>();

    public Parser(Lexer lexer)
    {
      _lexer = lexer;

      RegisterPrefix(TokenType.INT, new IntegerParslet());
      RegisterPrefix(TokenType.MINUS, new PrefixOperatorParslet());

      RegisterInfix(TokenType.CARROT, new InfixOperatorParslet(Precedence.POWER, true));
      RegisterInfix(TokenType.MINUS, new InfixOperatorParslet(Precedence.SUM, false));
      RegisterInfix(TokenType.PLUS, new InfixOperatorParslet(Precedence.SUM, false));
      RegisterInfix(TokenType.ASTERISK, new InfixOperatorParslet(Precedence.PRODUCT, false));
      RegisterInfix(TokenType.FSLASH, new InfixOperatorParslet(Precedence.SUM, false));

      NextToken();
      NextToken();
    }

    private void RegisterInfix(string tokenType, IInfixParslet parslet)
    {
      _infixParslets.Add(tokenType, parslet);
    }

    private void RegisterPrefix(string tokenType, IPrefixParslet parslet)
    {
      _prefixParslets.Add(tokenType, parslet);
    }

    public IExpression ParseExpression(Precedence precedence)
    {
      var prefixParslet = _prefixParslets[_curToken.Type];
      if (prefixParslet == null)
      {
        // add error logging
        return null;
      }

      var leftexp = prefixParslet.Parse(this, _curToken);

      while (!PeekTokenIs(TokenType.EOF) && precedence < PeekPrecedence())
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

    private Precedence PeekPrecedence()
    {
      IInfixParslet parslet = _infixParslets[_peekToken.Type];
      return parslet.GetPrecedence();
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
      stmt.Expression = ParseExpression(Precedence.LOWEST);

      return stmt;
    }

    public void NextToken()
    {
      _curToken = _peekToken;
      _peekToken = _lexer.NextToken();
    }

    private bool CurTokenIs(string tokenType)
    {
      return _curToken.Type == tokenType;
    }

    private bool PeekTokenIs(string tokenType)
    {
      return _peekToken.Type == tokenType;
    }

    public bool ExpectPeek(string tokenType)
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

    private void PeekError(string tokenType)
    {
      _errors.Add($"Expected next token to be {tokenType} but got {_peekToken.Type} instead.");
    }
  }
}
