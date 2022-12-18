using Application.Infrastructure.Presenters;
using Application.Models.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar.Expressions.Terms
{
    public class TypeType : TypeBase
    {
        public TypeBase OfType { get; }

        public TypeType(TypeBase ofType) : base(TypeName.TYPE, TypeEnum.TYPE)
        {
            OfType = ofType;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
