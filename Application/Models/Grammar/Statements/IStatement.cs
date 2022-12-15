using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public interface IStatement : IVisitable
    {
        public RulePosition Position { get; }
    }
}
