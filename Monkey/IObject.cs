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
    STRING,
    BUILTIN,
    ARRAY,
    HASH
  }

  public interface IObject
  {
    ObjectType Type();
    string Inspect();
  }

  public interface IHashable
  {
    int HashKey();
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

  public class String : IObject, IHashable
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

    public int HashKey()
    {
      return Value.GetHashCode();
    }
  }

  public class Integer : IObject, IHashable
  {
    public int Value { get; set; }

    public ObjectType Type()
    {
      return ObjectType.INTEGER;
    }

    public string Inspect()
    {
      return Value.ToString("N");
    }

    public int HashKey()
    {
      return Value;
    }
  }

  public class Boolean : IObject, IHashable
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

    public int HashKey()
    {
      return Value ? 1 : 0;
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

  public class Array : IObject
  {
    public List<IObject> Elements = new List<IObject>();

    public string Inspect()
    {
      return $"[{string.Join(",", Elements)}]";
    }

    public ObjectType Type()
    {
      return ObjectType.ARRAY;
    }
  }

  public class HashPair
  {
    public IObject Key { get; set; }
    public IObject Value { get; set; }
  }

  public class Hash : IObject
  {
    public Dictionary<int, HashPair> Pairs = new Dictionary<int, HashPair>();

    public string Inspect()
    {
      var strings = Pairs.Select(x => $"{x.Key}: {x.Value}");
      return "{" + string.Join(",", strings) + "}";
    }

    public ObjectType Type()
    {
      return ObjectType.HASH;
    }
  }
}
