using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class Literal<TType> : TermBase
    {
        public string Type { get; }
        public TType Value { get; }

        public Literal(string type, TType value)
        {
            Value = value;
            Type = type;
        }
    }
}
