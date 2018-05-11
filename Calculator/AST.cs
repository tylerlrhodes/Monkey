using System;
using System.Collections.Generic;
using System.Text;
// ReSharper disable InconsistentNaming

namespace Calculator
{
  public interface INode
  {
    string TokenLiteral();
    string ToString();
  }

  public interface IStatement : INode
  {
    void StatementNode();
  }

  public interface IExpression : INode
  {
    void ExpressionNode();
  }

  public class Code : INode
  {
    public List<IStatement> Statements { get; set; } = new List<IStatement>();

    public string TokenLiteral()
    {
      if (Statements.Count > 0)
        return Statements[0].TokenLiteral();
      else
      {
        return "";
      }
    }

    public override string ToString()
    {
      string str = "";
      foreach (var statement in Statements)
      {
        str += statement.ToString() + "\n";
      }
      return str;
    }
  }

  public class ExpressionStatement : IStatement
  {
    public Token token { get; set; }
    public IExpression Expression { get; set; }

    public string TokenLiteral()
    {
      return token.Literal;
    }

    public override string ToString()
    {
      if (Expression != null)
        return Expression.ToString();
      return "";
    }

    public void StatementNode()
    {
      throw new NotImplementedException();
    }
  }

  public class IntegerLiteral : IExpression
  {
    public Token token { get; set; }
    public int Value { get; set; }

    public string TokenLiteral()
    {
      return token.Literal;
    }

    public override string ToString()
    {
      return token.Literal;
    }

    public void ExpressionNode()
    {
      throw new NotImplementedException();
    }
  }

  public class PrefixExpression : IExpression
  {
    public Token token { get; set; }
    public string op { get; set; }
    public IExpression right { get; set; }


    public string TokenLiteral()
    {
      return token.Literal;
    }

    public override string ToString()
    {
      return "(" + op + right.ToString() + ")";
    }

    public void ExpressionNode()
    {
      throw new NotImplementedException();
    }
  }

  public class InfixExpression : IExpression
  {
    public Token token { get; set; }
    public IExpression left { get; set; }
    public string op { get; set; }
    public IExpression right { get; set; }


    public string TokenLiteral()
    {
      return token.Literal;
    }

    public override string ToString()
    {
      return "(" + left.ToString() + " " + op + " " + right.ToString() + ")";
    }

    public void ExpressionNode()
    {
      throw new NotImplementedException();
    }
  }


}
