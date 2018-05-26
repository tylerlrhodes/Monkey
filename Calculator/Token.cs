using System;
using System.Collections.Generic;
using System.Text;
// ReSharper disable InconsistentNaming


namespace Calculator
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

    IDENT
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
        default:
          return TokenType.IDENT;
      }
    }
  }

}
