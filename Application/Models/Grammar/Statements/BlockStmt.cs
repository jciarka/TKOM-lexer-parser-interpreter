using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;

namespace Application.Models.Grammar
{
    public class BlockStmt : StatementBase
    {
        public IEnumerable<StatementBase> Statements { get; }

        public BlockStmt(IEnumerable<StatementBase> statements, RulePosition position) : base(position)
        {
            Statements = statements;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }

        public override TypeBase Accept(ITypingAnalyseVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}