namespace Application.Models.Grammar
{
    public class FunctionCallExpr : TermBase
    {
        public string Name { get; }
        public IEnumerable<ArgumentBase> Arguments { get; }

        public FunctionCallExpr(string name, IEnumerable<ArgumentBase> arguments)
        {
            Name = name;
            Arguments = arguments;
        }
    }
}