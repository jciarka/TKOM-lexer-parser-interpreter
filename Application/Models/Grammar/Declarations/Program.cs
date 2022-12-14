using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class ProgramRoot : GrammarRuleBase
    {
        public IEnumerable<FunctionDecl> FunctionDeclarations { get; private set; }

        public ProgramRoot(IEnumerable<FunctionDecl> functionDeclaration, RulePosition position) : base(position)
        {
            FunctionDeclarations = functionDeclaration;
        }

        public void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }

        public TypeBase Accept(ITypingAnalyseVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
