using System;
using System.Collections.Generic;
using System.Linq;
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
        Fn = (objects) =>
        {
          var str = objects.Aggregate("", (current, @object) => current + @object.Inspect());

          System.Console.WriteLine(str);

          return new Integer() { Value = 0 };
        }
      };
      BuiltInFunctions["toStr"] = new BuiltIn()
      {
        Fn = (objects) =>
        {
          if (objects.Count != 1)
            return new Error() {Message = "Wrong number of arguments provided to toStr()"};
          
          return new String() { Value = objects[0].Inspect() };
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
