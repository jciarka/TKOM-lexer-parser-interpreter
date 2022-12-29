using Application.Infrastructure.Interpreter;
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
        public Dictionary<FunctionSignature, FunctionDecl> FunctionDeclarations { get; private set; }

        public ProgramRoot(Dictionary<FunctionSignature, FunctionDecl> functionDeclaration, RulePosition position) : base(position)
        {
            FunctionDeclarations = functionDeclaration;
        }

        public void Accept(IVisitor visitor, int v)
        {
            visitor.Visit(this);
        }
    }
}
