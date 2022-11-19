using Application.Models.Exceptions.ConfigurationParser;
using Application.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Exceptions.SourseParser
{
    public class ExpectedEofException : SourceParseException
    {
        public ExpectedEofException(Token token) : base(token, prepareMessage(token))
        {
        }

        private static string prepareMessage(Token token)
        {
            return $"(LINE: {token.Position!.Line}, COLUMN: {token.Position.Column}) " +
                $"Expected end of file, other token instead: \"{token.Type}\"";
        }
    }
}
