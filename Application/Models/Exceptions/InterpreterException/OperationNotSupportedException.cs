using Application.Models.Exceptions.ConfigurationParser;
using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;
using Application.Models.Types;
using Application.Models.Values.BasicTypeValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Exceptions.Interpreter
{
    public class OperationNotSupportedException : RuntimeException
    {
        private string? details;

        public OperationNotSupportedException() : base()
        {
        }

        public OperationNotSupportedException(string details) : base()
        {
            this.details = details;
        }

        public OperationNotSupportedException(string details, RulePosition position) : base(position)
        {
            this.details = details;
        }

        protected override string getMessage()
        {
            return $"(LINE: {Position.Line}, column: {Position.Column}) " +
                $"Operation not supported" + (details != null ? $"({details})" : "");
        }
    }
}
