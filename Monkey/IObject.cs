using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
// ReSharper disable InconsistentNaming

namespace Monkey
{
  public enum ObjectType
  {
    INTEGER,
    BOOLEAN,
    ERROR,
    RETURNVALUE,
    FUNCTION,
    STRING
  }

  public interface IObject
  {
    ObjectType Type();
    string Inspect();
  }

  public class Function : IObject
  {
    public List<Identifier> Parameters { get; set; }
    public BlockStatement Body { get; set; }
    public Environment Environment { get; set; }

    public ObjectType Type()
    {
      return ObjectType.FUNCTION;
    }

    public string Inspect()
    {
      string str = "";

      str += "fn(";
      str += string.Join(",", from param in Parameters select param.TokenLiteral());
      str += ") {\n";
      str += Body.ToString();
      str += "\n}";

      return str;
    }
  }

  public class ReturnValue : IObject
  {
    public IObject Value { get; set; }

    public ObjectType Type()
    {
      return ObjectType.RETURNVALUE;
    }

    public string Inspect()
    {
      return Value.Inspect();
    }
  }

  public class String : IObject
  {
    public string Value { get; set; }

    public ObjectType Type()
    {
      return ObjectType.STRING;
    }

    public string Inspect()
    {
      return Value;
    }
  }

  public class Integer : IObject
  {
    public long Value { get; set; }

    public ObjectType Type()
    {
      return ObjectType.INTEGER;
    }

    public string Inspect()
    {
      return Value.ToString();
    }
  }

  public class Boolean : IObject
  {
    public bool Value { get; set; }

    public ObjectType Type()
    {
      return ObjectType.BOOLEAN;
    }

    public string Inspect()
    {
      return Value.ToString();
    }
  }

  public class Error : IObject
  {
    public string Message { get; set; }

    public ObjectType Type()
    {
      return ObjectType.ERROR;
    }

    public string Inspect()
    {
      return Message;
    }
  }
}
