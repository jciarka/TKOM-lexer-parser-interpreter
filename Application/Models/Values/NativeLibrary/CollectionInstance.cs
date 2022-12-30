using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Values.NativeLibrary
{
    public class CollectionInstance : Instance
    {
        public List<IValue> Values { get; } = new();

        public CollectionInstance(IClass @class) : base(@class)
        {
        }
    }
}
