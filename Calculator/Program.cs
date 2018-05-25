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

        var lexer = new Lexer(expr);
        var parser = new Parser(lexer);

        var code = parser.ParseCode();

        Console.WriteLine(code);

        if (parser.HasError)
        {
          foreach (var err in parser.Errors)
          {
            Console.WriteLine(err);
          }
          continue;
        }

        var eval = new Evaluator();

        var result = eval.Eval(code, new Environment());

        Console.WriteLine($"Result = {result.Inspect()}\n");
      }
    }
  }
}
