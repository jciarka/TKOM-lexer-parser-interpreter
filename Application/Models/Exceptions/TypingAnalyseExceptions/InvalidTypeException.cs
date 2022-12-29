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
    public class InvalidTypeException : TypeVerifierException
    {
        public IEnumerable<TypeEnum> ExpectedTypes { get; }
        public TypeBase? CurrentType { get; }

        public InvalidTypeException(TypeBase? currentType, RulePosition position, params TypeEnum[] expectedTypes)
            : base(new CharacterPosition(position), prepareMessage(position, currentType, expectedTypes))
        {
            ExpectedTypes = expectedTypes;
            CurrentType = currentType;
        }

        private static string prepareMessage(RulePosition position, TypeBase? currentType, IEnumerable<TypeEnum> expectedTypes)
        {
            string current = currentType != null ? currentType.Type.ToString() : "void";

            return $"(LINE: {position.Line}) " +
                $"Unexpected type of expression: \"{current}\", expected \"{string.Join(" / ", expectedTypes)}\"";
        }
    }
}
