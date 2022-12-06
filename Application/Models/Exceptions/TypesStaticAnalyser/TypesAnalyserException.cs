using Application.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Exceptions.SourseParser
{
    public class TypesAnalyserException : ComputingException
    {
        public TypesAnalyserException(CharacterPosition position) : base(position)
        {

        }

        public TypesAnalyserException(CharacterPosition position, string? message) : base(position, message)
        {

        }
    }
}
