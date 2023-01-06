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
    public class ReferenceOutOfRangeException : RuntimeException
    {
        private int index;

        public ReferenceOutOfRangeException(int index) : base()
        {
            this.index = index;
        }

        public ReferenceOutOfRangeException(int index, RulePosition position) : base(position)
        {
            this.index = index;
        }

        protected override string getMessage()
        {
            return $"(LINE: {Position.Line}, column: {Position.Column}) " +
                $"Index reference beyond collection size (index {index})";
        }
    }
}
