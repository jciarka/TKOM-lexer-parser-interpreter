using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Types
{
    public class TypesInfoProvider
    {
        private readonly IEnumerable<TypeDescription> _currencyTypes;

        public TypesInfoProvider(IEnumerable<string>? currencyTypes = null)
        {
            _currencyTypes = currencyTypes?.Select(x => new TypeDescription()
            {
                Type = TypeEnum.EXTERNAL,
                DefaultValue = 0,
                Lexeme = x.ToUpper(),
            }) ?? new List<TypeDescription>();
        }

        public static IEnumerable<TypeDescription> StandardTypes => new List<TypeDescription>()
        {
            new TypeDescription()
            {
                Type = TypeEnum.BOOL,
                Lexeme = "bool",
                DefaultValue = false,
            },
            new TypeDescription()
            {
                Type = TypeEnum.INT,
                Lexeme = "int",
                DefaultValue = 0,
            },
            new TypeDescription()
            {
                Type = TypeEnum.DECIMAL,
                Lexeme = "decimal",
                DefaultValue = 0M,
            },
            new TypeDescription()
            {
                Type = TypeEnum.STRING,
                Lexeme = "string",
                DefaultValue = (string?)null,
            },
            new TypeDescription()
            {
                Type = TypeEnum.ACCOUNT,
                Lexeme = "Account",
                DefaultValue = null,
            }
        };

        public IEnumerable<TypeDescription> ExternalTypes => _currencyTypes;

        public IEnumerable<TypeDescription> Types => StandardTypes.Union(_currencyTypes);
    }
}
