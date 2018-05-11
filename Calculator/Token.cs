using System;
using System.Collections.Generic;
using System.Text;
// ReSharper disable InconsistentNaming


namespace Calculator
{
  public struct TokenType
  {
    public const string ILLEGAL = "Illegal";
    public const string EOF = "EOF";

    public const string INT = "INT";

    public const string OR = "||";
    public const string AND = "&&";
    public const string PLUS = "+";
    public const string MINUS = "-";
    public const string ASTERISK = "*";
    public const string FSLASH = "/";
    public const string CARROT = "^";

    public const string LPAREN = "(";
    public const string RPAREN = ")";
  }

  public struct Token
  {
    public string Type { get; set; }
    public string Literal { get; set; }

    public override string ToString()
    {
      return $"{Type} - {Literal}";
    }
  }

}
