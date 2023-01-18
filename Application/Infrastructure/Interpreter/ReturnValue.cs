using Application.Models.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Interpreter
{
    public class ReturnValue : Exception
    {
        public IValue Value { get; }

        public ReturnValue(IValue value)
        {
            Value = value;
        }
    }
}
