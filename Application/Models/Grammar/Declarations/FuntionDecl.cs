using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;

namespace Application.Models.Grammar
{
    public class FunctionDecl : GrammarRuleBase
    {
        public TypeBase? Type { get; }
        public string Name { get; }
        public IEnumerable<Parameter> Parameters { get; }
        public BlockStmt Block { get; }

        public FunctionDecl(string name, IEnumerable<Parameter> parameters, BlockStmt block, TypeBase? type = null)
        {
            Type = type;
            Name = name;
            Parameters = parameters;
            Block = block;
        }

        internal TypeBase? Accept(TypingAnalyserVisitor typingAnalyserVisitor)
        {
            throw new NotImplementedException();
        }

        public void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }
    }
}