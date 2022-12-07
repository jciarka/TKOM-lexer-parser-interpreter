using Application.Infrastructure.Presenters;
using Application.Models.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar.Expressions.Terms
{
    public class GenericType : TypeBase
    {
        public TypeBase ParametrisingType { get; set; }

        public GenericType(string name, TypeBase parametrisingType) : base(name, TypeEnum.GENERIC)
        {
            ParametrisingType = parametrisingType;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }
    }
}
