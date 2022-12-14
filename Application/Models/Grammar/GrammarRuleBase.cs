using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public abstract class GrammarRuleBase // : IVisitable
    {
        protected GrammarRuleBase(RulePosition position)
        {
            Position = position;
        }

        public RulePosition Position { get; }
    }
}
