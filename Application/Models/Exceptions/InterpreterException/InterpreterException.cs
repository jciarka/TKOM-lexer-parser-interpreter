using Application.Models.Grammar;
using System.Reflection;

namespace Application.Models.Exceptions.Interpreter
{
    public abstract class RuntimeException : ComputingException
    {
        public bool HasPosition { get; private set; }

        public IList<GrammarRuleBase> ProgramStackTrace { get; private set; } = new List<GrammarRuleBase>();

        public void AddToStackTrace(GrammarRuleBase grammarRule)
        {
            if (!HasPosition)
            {
                SetPosition(grammarRule.Position);
            }

            ProgramStackTrace.Add(grammarRule);
        }

        public void SetPosition(RulePosition position)
        {
            HasPosition = true;
            Position = new CharacterPosition(position);
        }

        public override string Message
        {
            get
            {
                return getMessage();
            }
        }

        protected abstract string getMessage();

        public RuntimeException() : base(new CharacterPosition())
        {
            HasPosition = false;
        }

        public RuntimeException(RulePosition position) : base(new CharacterPosition(position))
        {
            HasPosition = true;
        }
    }
}
