using Application.Infrastructure.Presenters;
using Application.Models.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar.Expressions.Terms
{
    public abstract class TypeBase
    {
        public string Name { get; }
        public TypeEnum Type { get; }

        protected TypeBase(string name, TypeEnum type)
        {
            Name = name;
            Type = type;
        }

        public abstract void Accept(IPresenterVisitor visitor, int v);
    }
}
