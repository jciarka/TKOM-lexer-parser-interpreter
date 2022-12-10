using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Values.NativeLibrary
{
    public static class NativeLibraryProvider
    {
        public static IEnumerable<INativeClassPrototype> GetClassPrototypes()
        {
            var proptotypes = new List<INativeClassPrototype>();

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type is INativeClassPrototype)
                {
                    proptotypes.Add((INativeClassPrototype)Activator.CreateInstance(type)!);
                }
            }

            return proptotypes;
        }

        public static IEnumerable<INativeCallable> GetFunctions()
        {
            var functions = new List<INativeCallable>();

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type is INativeCallable)
                {
                    functions.Add((INativeCallable)Activator.CreateInstance(type)!);
                }
            }

            return functions;
        }
    }
}
