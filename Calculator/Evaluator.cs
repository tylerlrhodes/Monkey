using System;
using System.Collections.Generic;
using System.Text;
// ReSharper disable All

namespace Calculator
{
  public class Evaluator
  {
    private static Boolean TRUE = new Boolean() { Value = true };
    private static Boolean FALSE = new Boolean() { Value = false };

    public IObject Eval(INode node, Environment env)
    {
      IObject right;
      IObject val;
      switch (node)
      {
        case Code c:
          return EvalProgram(c, env);

        case BlockStatement bs:
          return EvalBlockStatement(bs, env);

        case ReturnStatement rs:
          val = Eval(rs.ReturnValue, env);
          if (val is Error)
            return val;
          return new ReturnValue() { Value = val };

        case Identifier i:
          return EvalIdentifier(i, env);

        case LetStatement ls:
          val = Eval(ls.Value, env);
          if (val is Error)
            return val;
          env.Set(ls.Name.Value, val);
          break;

        case ExpressionStatement es:
          return Eval(es.Expression, env);

        case IntegerLiteral il:
          return new Integer() { Value = il.Value };

        case PrefixExpression pe:
          right = Eval(pe.right, env);
          if (right is Error)
            return right;
          return EvalPrefixExpression(pe.op, right);

        case InfixExpression ie:
          var left = Eval(ie.left, env);
          if (left is Error)
            return left;
          right = Eval(ie.right, env);
          if (right is Error)
            return right;
          return EvalInfixExpression(ie.op, left, right);

        case IfExpression ie:
          return EvalIfExpression(ie, env);

        case FunctionLiteral fl:
          var parameters = fl.Parameters;
          var body = fl.Body;
          return new Function() { Parameters = parameters, Body = body, Environment = env };

        case CallExpression ce:
          var function = Eval(ce.Function, env);
          if (function is Error)
            return function;
          var args = EvalExpressions(ce.Arguments, env);
          if (args.Count == 1 && args[0] is Error)
          {
            return args[0];
          }
          return ApplyFunction(function as Function, args);
      }

      return null;
    }

    private IObject EvalIfExpression(IfExpression ie, Environment env)
    {
      var condition = Eval(ie.Condition, env);
      if (condition is Error)
        return condition;

      if (IsTruthy(condition))
      {
        return Eval(ie.Consequence, env);
      }
      else if (ie.Alternative != null)
      {
        return Eval(ie.Alternative, env);
      }
      else
      {
        return null;
      }
    }

    private bool IsTruthy(IObject condition)
    {
      switch (condition)
      {
        case Boolean b:
          return b == TRUE ? true : false;

        case Integer i:
          return i.Value == 0 ? false : true;

        default:
          return true;
      }
    }

    private Environment ExtendFunctionEnv(Function function, List<IObject> args)
    {
      var env = Environment.NewEnclosedEnvironment(function.Environment);
      for (int i = 0; i < function.Parameters.Count; i++)
      {
        env.Set(function.Parameters[i].Value, args[i]);
      }
      return env;
    }

    private IObject ApplyFunction(Function function, List<IObject> args)
    {
      if (!(function is Function))
      {
        return new Error() { Message = $"not a function {function.Type()}" };
      }

      if (args.Count != function.Parameters.Count)
        return new Error() { Message = "Invalid argument count." };

      var extendedEnv = ExtendFunctionEnv(function, args);
      var evaluated = Eval(function.Body, extendedEnv);

      return UnwrapReturnValue(evaluated);

    }

    private IObject UnwrapReturnValue(IObject evaluated)
    {
      if (evaluated is ReturnValue)
        return (evaluated as ReturnValue).Value;

      return evaluated;
    }

    private List<IObject> EvalExpressions(List<IExpression> arguments, Environment env)
    {
      var result = new List<IObject>();
      foreach (var arg in arguments)
      {
        var evaluated = Eval(arg, env);
        if (evaluated is Error)
        {
          return new List<IObject>() { evaluated };
        }
        result.Add(evaluated);
      }
      return result;
    }

    private IObject EvalBlockStatement(BlockStatement bs, Environment env)
    {
      IObject result = null;
      foreach (var stmt in bs.Statements)
      {
        result = Eval(stmt, env);
        if (result != null)
        {
          if (result is ReturnValue || result is Error)
            return result;
        }
      }
      return result;
    }

    private IObject EvalIdentifier(Identifier identifier, Environment env)
    {
      var (val, ok) = env.Get(identifier.Value);
      if (!ok)
        return new Error() { Message = $"identifier not found: {identifier.Value}" };
      return val;
    }

    private IObject EvalInfixExpression(string op, IObject left, IObject right)
    {
      if (left.Type() == ObjectType.INTEGER && right.Type() == ObjectType.INTEGER)
      {
        return EvalIntegerInfixExpression(op, left, right);
      }
      else if (op == "==")
      {
        return NativeBoolToBoolean(left == right);
      }
      else if (op == "!=")
      {
        // todo
      }
      else if (left.Type() == ObjectType.BOOLEAN && right.Type() == ObjectType.BOOLEAN && op == "&&")
      {
        return NativeBoolToBoolean((left as Boolean).Value && (right as Boolean).Value);
      }
      else
      {
        return new Error() { Message = $"Unknown operator {left.Type()} {op} {right.Type()}" };
      }
      return null;
    }

    private IObject EvalIntegerInfixExpression(string op, IObject left, IObject right)
    {
      var lval = (left as Integer).Value;
      var rval = (right as Integer).Value;

      switch (op)
      {
        case "+":
          return new Integer() { Value = lval + rval };
        case "-":
          return new Integer() { Value = lval - rval };
        case "*":
          return new Integer() { Value = lval * rval };
        case "/":
          return new Integer() { Value = lval / rval };
        case "^":
          return new Integer() { Value = (int)Math.Pow(lval, rval) };
        case "<":
          return NativeBoolToBoolean(lval < rval);
        case ">":
          return NativeBoolToBoolean(lval > rval);
        case "==":
          return NativeBoolToBoolean(lval == rval);
        default:
          return new Error() { Message = $"Unknown operator {left.Type()} {op} {right.Type()}" };
      }
    }

    private IObject NativeBoolToBoolean(bool b)
    {
      return b ? TRUE : FALSE;
    }

    private IObject EvalPrefixExpression(string op, IObject right)
    {
      switch (op)
      {
        case "-":
          return EvalMinusPrefixOperatorExpression(right);
        default:
          return new Error() { Message = $"Unknown operator {op}" };
      }
    }

    private IObject EvalMinusPrefixOperatorExpression(IObject right)
    {
      if (right.Type() != ObjectType.INTEGER)
        return new Error() { Message = $"Unknown operator -{right.Type()}" };

      return new Integer() { Value = -(right as Integer).Value };
    }

    private IObject EvalProgram(Code node, Environment env)
    {
      IObject result = null;

      foreach (var stmt in node.Statements)
      {
        result = Eval(stmt, env);
        switch (result)
        {
          case Integer i:
            result = i;
            break;
          case Boolean b:
            result = b;
            break;
          case Error e:
            result = e;
            break;
          default:
            result = null;
            break;
        }
      }

      return result;
    }
  }
}
