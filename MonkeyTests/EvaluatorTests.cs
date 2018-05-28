using System;
using System.Collections.Generic;
using System.Text;
using Monkey;
using Xunit;

namespace MonkeyTests
{
  public class EvaluatorTests
  {
    [Fact]
    public void Test1()
    {
      var text = @"
let x = 10
let func = fn(x) {
  if (x == 0)
  {
    return 0
  }
  return x + func(x-1)
}

func(x)
";
      var lexer = new Lexer(text);
      var parser = new Parser(lexer);

      var code = parser.ParseCode();

      var env = new Monkey.Environment();
      var evaluator = new Evaluator();

      var result = evaluator.Eval(code, env);

      Assert.NotNull(result);
      Assert.Equal(55, (result as Integer).Value);
    }
  }
}
