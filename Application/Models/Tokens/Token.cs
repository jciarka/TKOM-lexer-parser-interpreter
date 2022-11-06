using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Tokens
{
    /// <summary>
    /// Object representing single token
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Token type
        /// </summary>
        public TokenType Type { get; set; }

        /// <summary>
        /// Token position information
        /// </summary>
        public CharacterPosition? Position { get; set; }

        /// <summary>
        /// string value of token
        /// </summary>
        public string? Lexeme { get; set; }

        /// <summary>
        /// value represented by token // only literals
        /// </summary>
        public Object? Value { get; set; }

        /// <summary>
        /// Type of value representet by token - type lexeme
        /// </summary>
        public string? ValueType { get; set; }
    }
}
