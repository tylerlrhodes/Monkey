using System;
using System.Collections.Generic;
using System.Text;

namespace Calculator
{
  public class Lexer
  {
    private readonly string _input;

    private int _position = 0;
    private int _readPosition = 0;

    private char _ch;

    public Lexer(string input)
    {
      _input = input;
      ReadChar();
    }

    public Token NextToken()
    {
      Token token = new Token();

      SkipWhiteSpaces();

      switch (_ch)
      {
        case '|':
          if (PeekChar() == '|')
          {
            ReadChar();
            token = NewToken(TokenType.OR, "||");
          }
          else
          {
            SetInvalidToken(token);
          }
          break;
        case '&':
          if (PeekChar() == '&')
          {
            ReadChar();
            token = NewToken(TokenType.AND, "&&");
          }
          else
          {
            SetInvalidToken(token);
          }
          break;
        case '+':
          token = NewToken(TokenType.PLUS, _ch.ToString());
          break;
        case '-':
          token = NewToken(TokenType.MINUS, _ch.ToString());
          break;
        case '/':
          token = NewToken(TokenType.FSLASH, _ch.ToString());
          break;
        case '*':
          token = NewToken(TokenType.ASTERISK, _ch.ToString());
          break;
        case '^':
          token = NewToken(TokenType.CARROT, _ch.ToString());
          break;
        case '(':
          token = NewToken(TokenType.LPAREN, _ch.ToString());
          break;
        case ')':
          token = NewToken(TokenType.RPAREN, _ch.ToString());
          break;
        case (char)0:
          token = NewToken(TokenType.EOF, "");
          break;

        default:
          if (char.IsDigit(_ch))
          {
            token.Type = TokenType.INT;
            token.Literal = ReadNumber();
            return token;
          }
          else
          {
            SetInvalidToken(token);
          }
          break;
      }

      ReadChar();

      return token;
    }

    private void SetInvalidToken(Token token)
    {
      token.Type = TokenType.ILLEGAL;
      token.Literal = _ch.ToString();
    }

    private void ReadChar()
    {
      if (_readPosition > _input.Length-1)
      {
        _ch = (char) 0;
      }
      else
      {
        _ch = _input[_readPosition];
      }
      _position = _readPosition;
      _readPosition++;
    }

    private void SkipWhiteSpaces()
    {
      while (_ch == ' ' || _ch == '\t' || _ch == '\n' || _ch == '\r')
        ReadChar();
    }

    private char PeekChar()
    {
      if (_readPosition > _input.Length)
        return (char) 0;
      else
      {
        return _input[_readPosition];
      }
    }

    private string ReadIdentifier()
    {
      throw new NotImplementedException();
    }

    private string ReadNumber()
    {
      int position = _position;
      while (char.IsDigit(_ch))
        ReadChar();
      return _input.Substring(position, _position - position);
    }

    private Token NewToken(TokenType tokenType, string literal)
    {
      return new Token
      {
        Type = tokenType,
        Literal = literal
      };
    }
  }
}
