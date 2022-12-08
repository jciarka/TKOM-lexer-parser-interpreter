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

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!obj.GetType().IsSubclassOf(typeof(TypeBase)))
            {
                return false;
            }

            return Name.Equals(((TypeBase)obj).Name);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }
}
