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
            /*
                foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (type is INativeClassPrototype)
                    {
                        proptotypes.Add((INativeClassPrototype)Activator.CreateInstance(type)!);
                    }
                }
            */

            /*            var type = typeof(INativeClassPrototype);

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && type.IsClass)
                .Select(x => (INativeClassPrototype)Activator.CreateInstance(x)!)
                .ToList();
            */


            foreach (Type mytype in Assembly.GetExecutingAssembly().GetTypes()
                .Where(mytype => mytype.GetInterfaces().Contains(typeof(INativeClassPrototype))))
            {
                proptotypes.Add((INativeClassPrototype)Activator.CreateInstance(mytype)!);
            }

            return proptotypes;
        }

        public static IEnumerable<INativeCallable> GetFunctions()
        {
            var functions = new List<INativeCallable>();

            foreach (Type mytype in Assembly.GetExecutingAssembly().GetTypes()
                .Where(mytype => mytype.GetInterfaces().Contains(typeof(INativeCallable))))
            {
                functions.Add((INativeCallable)Activator.CreateInstance(mytype)!);
            }

            return functions;
        }
    }
}
