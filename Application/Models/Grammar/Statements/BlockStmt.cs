namespace Application.Models.Grammar
{
    public class BlockStmt : StatementBase
    {
        public IEnumerable<StatementBase> Statements { get; }

        public BlockStmt(IEnumerable<StatementBase> statements)
        {
            Statements = statements;
        }
    }
}