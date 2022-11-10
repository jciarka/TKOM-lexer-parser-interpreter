using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Exceptions
{
    public class ComputingException : Exception
    {
        public CharacterPosition Position { get; protected set; }

        public ComputingException(CharacterPosition position)
        {
            Position = position;
        }

        public ComputingException(CharacterPosition position, string? message) : base(message)
        {
            Position = position;
        }
    }
}
