using Application.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Exceptions.SourseParser
{
    public class SourceParseException : ComputingException
    {
        public Token Token { get; private set; }

        public SourceParseException(Token token) : base(token.Position!)
        {
            Token = token;
        }

        public SourceParseException(Token token, string? message) : base(token!.Position!, message)
        {
            Token = token;
        }
    }
}
