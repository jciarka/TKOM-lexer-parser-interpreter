using Application.Infrastructure.Presenters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar.Expressions.Terms
{
    public class GenericType : TypeBase
    {
        public string Name { get; set; }
        public TypeBase ParametrisingType { get; set; }

        public GenericType(string name, TypeBase parametrisingType)
        {
            Name = name;
            ParametrisingType = parametrisingType;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }
    }
}
