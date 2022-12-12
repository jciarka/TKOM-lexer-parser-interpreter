using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public abstract class TermBase : ExpressionBase
    {
        protected TermBase(RulePosition position) : base(position)
        {
        }
    }
}
