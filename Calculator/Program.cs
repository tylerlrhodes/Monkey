using System;

namespace Calculator
{
  class Program
  {
    enum Test
    {
      NumOne
    }

    static void Main(string[] args)
    {
      Console.WriteLine("Calculator 1.0");

      while (true)
      {
        Console.Write(">>");
        var expr = Console.ReadLine();

        Lexer lexer = new Lexer(expr);
        Parser parser = new Parser(lexer);

        Code code = parser.ParseCode();

        Console.WriteLine(code);
      }
    }
  }
}
