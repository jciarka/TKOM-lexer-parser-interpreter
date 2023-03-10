using Application.Models.Types;
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

        public bool? BoolValue { get; set; }
        public string? StringValue { get; set; }
        public int? IntValue { get; set; }
        public decimal? DecimalValue { get; set; }

        /// <summary>
        /// Type of value representet by token
        /// </summary>    
        public string? ValueType { get; set; }
    }
}
