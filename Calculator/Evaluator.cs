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

    public IObject Eval(object node, Environment env)
    {
      IObject right;
      switch (node)
      {
        case Code c:
          return EvalProgram(c, env);

        case Identifier i:
          return EvalIdentifier(i, env);

        case LetStatement ls:
          var val = Eval(ls.Value, env);
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

      }

      return null;
    }

    private IObject EvalIdentifier(Identifier identifier, Environment env)
    {
      var (val, ok) = env.Get(identifier.Value);
      if (!ok)
        return new Error() {Message = $"identifier not found: {identifier.Value}"};
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
        return new Error() {Message = $"Unknown operator {left.Type()} {op} {right.Type()}"};
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
          return new Integer() {Value = lval + rval};
        case "-":
          return new Integer() {Value = lval - rval};
        case "*":
          return new Integer() {Value = lval * rval};
        case "/":
          return new Integer() {Value = lval / rval};
        case "^":
          return new Integer() {Value = (int)Math.Pow(lval, rval)};
        case "<":
          return NativeBoolToBoolean(lval < rval);
        case ">":
          return NativeBoolToBoolean(lval > rval);
        case "==":
          return NativeBoolToBoolean(lval == rval);
        default:
          return new Error() {Message = $"Unknown operator {left.Type()} {op} {right.Type()}"};
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
          return new Error() {Message = $"Unknown operator {op}"};
      }
    }

    private IObject EvalMinusPrefixOperatorExpression(IObject right)
    {
      if(right.Type() != ObjectType.INTEGER)
        return new Error() {Message = $"Unknown operator -{right.Type()}"};

      return new Integer() {Value = -(right as Integer).Value};
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
