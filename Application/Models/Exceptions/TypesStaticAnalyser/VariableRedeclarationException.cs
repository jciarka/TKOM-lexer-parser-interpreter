using Application.Models.Exceptions.SourseParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Exceptions.SourseParser
{
    public class VariableRedeclarationException : TypesAnalyserException
    {
        public VariableRedeclarationException(CharacterPosition position)
            : base(position, prepareMessage(position))
        {
        }

        private static string prepareMessage(CharacterPosition position)
        {
            return $"(LINE: {position.Line}, COLUMN: {position.Column}) " +
                $"Variable redeclaration";
        }
    }
}
