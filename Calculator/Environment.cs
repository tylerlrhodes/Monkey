using System;
using System.Collections.Generic;
using System.Text;

namespace Calculator
{
  public class Environment
  {
#pragma warning disable 414
    private Environment _outer = null;
#pragma warning restore 414

    private readonly Dictionary<string, IObject> _store = new Dictionary<string, IObject>();

    public (IObject, bool) Get(string name)
    {
      return _store.ContainsKey(name) ? (_store[name], true) : (null, false);
    }

    public IObject Set(string name, IObject obj)
    {
      _store[name] = obj;
      return obj;
    }

    public static Environment NewEnclosedEnvironment(Environment outer)
    {
      return new Environment {_outer = outer};
    }
  }
}
