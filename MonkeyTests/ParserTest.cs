using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Monkey;

namespace MonkeyTests
{
  public class ParserTest
  {
    [Fact]
    public void TestPrefix1()
    {
      Lexer lexer = new Lexer("42");

      Parser parser = new Parser(lexer);

      var code = parser.ParseCode();

      System.Console.WriteLine(code);

      Assert.Equal(new IntegerLiteral() {token = new Token() {Literal = "42", Type = TokenType.INT}, Value = 42}.ToString(), code.Statements[0].ToString());
    }

    [Fact]
    public void TestPrefix2()
    {
      Lexer lexer = new Lexer("---42");

      Parser parser = new Parser(lexer);

      var code = parser.ParseCode();

      System.Console.WriteLine(code);

      Assert.Equal("(-(-(-42)))", code.Statements[0].ToString());
    }

    [Fact]
    public void TestPrefix3()
    {
      Lexer lexer = new Lexer("---42 + 42 * 10");

      Parser parser = new Parser(lexer);

      var code = parser.ParseCode();

      System.Console.WriteLine(code);

      Assert.Equal("((-(-(-42))) + (42 * 10))", code.Statements[0].ToString());
    }

    [Fact]
    public void TestPrefix4()
    {
      Lexer lexer = new Lexer("---42 + 42 * 10^3^4");

      Parser parser = new Parser(lexer);

      var code = parser.ParseCode();

      System.Console.WriteLine(code);

      Assert.Equal("((-(-(-42))) + (42 * (10 ^ (3 ^ 4))))", code.Statements[0].ToString());
    }

    [Fact]
    public void TestPrefix5()
    {
      Lexer lexer = new Lexer(@"2 + 4
10");

      Parser parser = new Parser(lexer);

      var code = parser.ParseCode();

      System.Console.WriteLine(code);

      Assert.Equal("(2 + 4)\n10\n", code.ToString());
    }

    [Fact]
    public void TestPrefix6()
    {
      var lexer = new Lexer(@"""String Test""");
      var parser = new Parser(lexer);

      var code = parser.ParseCode();
      System.Console.WriteLine(code);

      Assert.Equal("String Test\n", code.ToString());
    }
  }
}
