using System;
using System.Collections.Generic;
using System.Text;

namespace Monkey
{
  public static class BuiltIns
  {
    public static Dictionary<string, BuiltIn> BuiltInFunctions = new Dictionary<string, BuiltIn>();

    static BuiltIns()
    {
      BuiltInFunctions["puts"] = new BuiltIn()
      {
        Fn = (List<IObject> objects) =>
        {
          var str = "";
          foreach (var @object in objects)
          {
            str += @object.Inspect();
          }

          System.Console.WriteLine(str);

          return new Integer() { Value = 0 };
        }
      };
    }
  }

  public class BuiltIn : IObject
  {
    public Func<List<IObject>, IObject> Fn;

    public ObjectType Type()
    {
      return ObjectType.BUILTIN;
    }

    public string Inspect()
    {
      return "Builtin Function";
    }
  }
}
