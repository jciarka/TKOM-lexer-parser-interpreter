using Application.Models.Exceptions.ConfigurationParser;
using Application.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Exceptions.SourseParser
{
    public class MissingParameterException : SourceParseException
    {
        public MissingParameterException(Token current)
            : base(current, prepareMessage(current))
        {
        }

        private static string prepareMessage(Token current)
        {
            return $"(LINE: {current.Position!.Line}, COLUMN: {current.Position.Column}) " +
                $"Missing function parameter";
        }
    }
}
