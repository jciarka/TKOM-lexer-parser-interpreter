using Application.Models;
using Application.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Lexer.Exceptions
{
    public class LexerException : Exception
    {
        public CharacterPosition Position { get; protected set; }

        public LexerException(CharacterPosition position)
        {
            Position = position;
        }

        public LexerException(CharacterPosition position, string? message) : base(message)
        {
            Position = position;
        }
    }
}
