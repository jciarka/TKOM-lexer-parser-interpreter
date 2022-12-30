using Application.Models.Exceptions.ConfigurationParser;
using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;
using Application.Models.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Exceptions.SourseParser
{
    public class RuntimeNullReferenceException : RuntimeException
    {
        private readonly string? _details;

        public RuntimeNullReferenceException() : base()
        {
        }

        public RuntimeNullReferenceException(RulePosition position) : base(position)
        {
        }

        public RuntimeNullReferenceException(RulePosition position, string details) : base(position)
        {
            _details = details;
        }

        protected override string getMessage()
        {
            StringBuilder sb = new($"(LINE: {Position.Line}, column: {Position.Column}) Null reference exception");
            if (_details != null)
            {
                sb.Append(" (");
                sb.Append(_details);
                sb.Append(")");
            };

            return sb.ToString();
        }
    }
}
