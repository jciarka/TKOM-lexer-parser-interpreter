using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Types
{
    public class TypeDescription
    {
        public TypeEnum Type { get; set; }

        public string? Lexeme { get; set; }

        public Object? DefaultValue { get; set; }
    }
}
