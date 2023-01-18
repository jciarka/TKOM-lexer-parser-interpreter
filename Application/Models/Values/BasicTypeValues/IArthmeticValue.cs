using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Values.BasicTypeValues
{
    public interface IArthmeticValue
    {
        public IValue Add(IValue other);
        public IValue Sub(IValue other);
        public IValue Mul(IValue other);
        public IValue Div(IValue other);
    }
}
