using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class DeclarationStmt : GrammarRuleBase, IStatement, IVisitable
    {
        public TypeBase? Type { get; set; }
        public Identifier Identifier { get; }
        public IExpression? Expression { get; }

        public DeclarationStmt(Identifier identifier, RulePosition position, IExpression? expression = null, TypeBase? type = null) : base(position)
        {
            Type = type;
            Identifier = identifier;
            Expression = expression;
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
