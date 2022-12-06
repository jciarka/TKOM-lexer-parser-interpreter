using Application.Infrastructure.Presenters;

namespace Application.Models.Grammar
{
    public class BlockStmt : StatementBase
    {
        public IEnumerable<StatementBase> Statements { get; }

        public BlockStmt(IEnumerable<StatementBase> statements)
        {
            Statements = statements;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }

        internal void Accept(TypingAnalyserVisitor typingAnalyserVisitor)
        {
            throw new NotImplementedException();
        }
    }
}