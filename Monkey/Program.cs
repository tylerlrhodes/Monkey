using System;
using System.IO;

namespace Calculator
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Calculator 1.0");

      if (args.Length != 1)
      {
        DoREPL();
      }
      else
      {
        RunFile(args[0]);
      }
    }

    private static void RunFile(string file)
    {

      var text = File.ReadAllText(file);

      var env = new Environment();

      var lexer = new Lexer(text);
      var parser = new Parser(lexer);

      var code = parser.ParseCode();

      if (parser.HasError)
      {
        foreach (var err in parser.Errors)
        {
          Console.WriteLine(err);
        }
        return;
      }

      var eval = new Evaluator();

      var result = eval.Eval(code, env);

      Console.WriteLine(result == null ? "No output.." : $"Result = {result.Inspect()}");
    }

    // ReSharper disable once InconsistentNaming
    static void DoREPL()
    {
      var env = new Environment();

      while (true)
      {
        Console.Write(">>");
        var expr = Console.ReadLine();
        var text = "";

        while (expr != ";")
        {
          text += expr;
          expr = Console.ReadLine();
        }

        var lexer = new Lexer(text);
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

        var result = eval.Eval(code, env);

        Console.WriteLine(result == null ? "Enter next line.." : $"Result = {result.Inspect()}");
      }
    }
  }
}
