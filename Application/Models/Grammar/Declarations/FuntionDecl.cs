namespace Application.Models.Grammar
{
    public class FunctionDecl : GrammarRuleBase
    {
        public string Type { get; }
        public string Name { get; }
        public IEnumerable<Parameter> Parameters { get; }
        public BlockStmt Block { get; }

        public FunctionDecl(string type, string name, IEnumerable<Parameter> parameters, BlockStmt block)
        {
            Type = type;
            Name = name;
            Parameters = parameters;
            Block = block;
        }
    }
}