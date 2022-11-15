namespace Application.Models.Grammar
{
    public class FuntionDecl : GrammarRuleBase
    {
        public string Type { get; }
        public string Name { get; }
        public IEnumerable<Parameter> Parameters { get; }
        public BlockStmt Block { get; }

        public FuntionDecl(string type, string name, IEnumerable<Parameter> parameters, BlockStmt block)
        {
            Type = type;
            Name = name;
            Parameters = parameters;
            Block = block;
        }
    }
}