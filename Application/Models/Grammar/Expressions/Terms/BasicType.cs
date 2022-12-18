using Application.Infrastructure.Presenters;
using Application.Models.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar.Expressions.Terms
{
    public class BasicType : TypeBase
    {
        public BasicType(string name, TypeEnum type) : base(name, type)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
