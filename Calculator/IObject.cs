using System;
using System.Collections.Generic;
using System.Text;
// ReSharper disable InconsistentNaming

namespace Calculator
{
  public enum ObjectType
  {
    INTEGER,
    BOOLEAN,
    ERROR,
    RETURNVALUE
  }

  public interface IObject
  {
    ObjectType Type();
    string Inspect();
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

  public class Integer : IObject
  {
    public int Value { get; set; }

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
