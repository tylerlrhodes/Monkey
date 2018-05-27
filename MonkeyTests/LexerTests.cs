using System;
using System.Collections.Generic;
using Xunit;
using Monkey;

namespace MonkeyTests
{
  public class LexerTests
  {
    [Fact]
    public void Test2()
    {
      const string input = @"""String test""";

      var expected = new List<Token>()
      {
        new Token()
        {
          Type = TokenType.STRING,
          Literal = "String test"
        }
      };

      var lexer = new Lexer(input);

      foreach (var token in expected)
      {
        var curToken = lexer.NextToken();

        Assert.Equal(token, curToken);
      }
    }

    [Fact]
    public void Test1()
    {
      const string input = "4 + 3 - 2 * 5 / 10 + (2 + 3)^3 || 1 && 2";

      List<Token> expected = new List<Token>
      {
        new Token()
        {
          Type = TokenType.INT,
          Literal = "4"
        },
        new Token()
        {
          Type = TokenType.PLUS,
          Literal = "+"
        },
        new Token()
        {
          Type = TokenType.INT,
          Literal = "3"
        },
        new Token()
        {
          Type = TokenType.MINUS,
          Literal = "-"
        },
        new Token()
        {
          Type = TokenType.INT,
          Literal = "2"
        },
        new Token()
        {
          Type = TokenType.ASTERISK,
          Literal = "*"
        },
        new Token()
        {
          Type = TokenType.INT,
          Literal = "5"
        },
        new Token()
        {
          Type = TokenType.FSLASH,
          Literal = "/"
        },
        new Token()
        {
          Type = TokenType.INT,
          Literal = "10"
        },
        new Token()
        {
          Type = TokenType.PLUS,
          Literal = "+"
        },
        new Token()
        {
          Type = TokenType.LPAREN,
          Literal = "("
        },
        new Token()
        {
          Type = TokenType.INT,
          Literal = "2"
        },
        new Token()
        {
          Type = TokenType.PLUS,
          Literal = "+"
        },
        new Token()
        {
          Type = TokenType.INT,
          Literal = "3"
        },
        new Token()
        {
          Type = TokenType.RPAREN,
          Literal = ")"
        },
        new Token()
        {
          Type = TokenType.CARROT,
          Literal = "^"
        },
        new Token()
        {
          Type = TokenType.INT,
          Literal = "3"
        },
        new Token()
        {
          Type = TokenType.OR,
          Literal = "||"
        },
        new Token()
        {
          Type = TokenType.INT,
          Literal = "1"
        },
        new Token()
        {
          Type = TokenType.AND,
          Literal = "&&"
        },
        new Token()
        {
          Type = TokenType.INT,
          Literal = "2"
        },
        new Token()
        {
          Type = TokenType.EOF,
          Literal = ""
        }
      };

      Lexer lexer = new Lexer(input);

      foreach (var t in expected)
      {
        var curToken = lexer.NextToken();

        Console.WriteLine(curToken.ToString() + " " + t.ToString());
        Assert.Equal(t, curToken);
      }
    }
  }
}
