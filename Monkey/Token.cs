using System;
using System.Collections.Generic;
using System.Text;
// ReSharper disable InconsistentNaming


namespace Monkey
{
  public enum TokenType
  {
    ILLEGAL,
    EOF,

    INT,

    ASSIGN,
    EQ,
    OR,
    AND,
    LT,
    GT,

    PLUS,
    MINUS,
    ASTERISK,
    FSLASH,
    CARROT,

    LPAREN,
    RPAREN,

    LET,

    IDENT,

    LBRACE,
    RBRACE,

    IF,
    ELSE,
    FUNCTION,
    RETURN,
    COMMA,
    STRING,
    TRUE,
    FALSE
  }

  public struct Token
  {
    public TokenType Type { get; set; }
    public string Literal { get; set; }

    public override string ToString()
    {
      return $"{Type} - {Literal}";
    }

    public static TokenType LookupIdent(string ident)
    {
      switch (ident)
      {
        case "let":
          return TokenType.LET;
        case "return":
          return TokenType.RETURN;
        case "fn":
          return TokenType.FUNCTION;
        case "if":
          return TokenType.IF;
        case "else":
          return TokenType.ELSE;
        case "true":
          return TokenType.TRUE;
        case "false":
          return TokenType.FALSE;
        default:
          return TokenType.IDENT;
      }
    }
  }

}
