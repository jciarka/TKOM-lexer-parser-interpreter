namespace Application.Models.Grammar
{
    public class Lambda : ArgumentBase
    {
        public IEnumerable<Parameter> Parameters { get; }
        public BlockStmt Block { get; }

        public Lambda(IEnumerable<Parameter> parameters, BlockStmt block)
        {
            Parameters = parameters;
            Block = block;
        }
    }
}