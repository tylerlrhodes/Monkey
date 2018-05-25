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

    OR,
    AND,
    PLUS,
    MINUS,
    ASTERISK,
    FSLASH,
    CARROT,

    LPAREN,
    RPAREN
  }

  public struct Token
  {
    public TokenType Type { get; set; }
    public string Literal { get; set; }

    public override string ToString()
    {
      return $"{Type} - {Literal}";
    }
  }

}
