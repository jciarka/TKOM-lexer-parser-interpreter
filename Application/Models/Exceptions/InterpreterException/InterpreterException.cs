using Application.Models.Grammar;
using System.Reflection;

namespace Application.Models.Exceptions.SourseParser
{
    public abstract class RuntimeException : ComputingException
    {
        public void SetPosition(RulePosition position)
        {
            Position = new CharacterPosition(position);
            PropertyInfo prop = GetType().GetProperty("Message")!;
            prop.SetValue(this, getMessage());
        }

        protected abstract string getMessage();

        public RuntimeException() : base(new CharacterPosition())
        {
        }

        public RuntimeException(RulePosition position) : base(new CharacterPosition(position))
        {
        }
    }
}
