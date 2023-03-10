using Application.Models;
using Application.Models.Exceptions;
using Application.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Lexer.Exceptions
{
    public class LexerException : ComputingException
    {
        public LexerException(CharacterPosition position) : base(position)
        {
        }

        public LexerException(CharacterPosition position, string? message) : base(position, message)
        {
        }
    }
}
