using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;

namespace Application.Models.Grammar
{
    public class BlockStmt : GrammarRuleBase, IStatement, IVisitable
    {
        public IEnumerable<IStatement> Statements { get; }

        public BlockStmt(IEnumerable<IStatement> statements, RulePosition position) : base(position)
        {
            Statements = statements;
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