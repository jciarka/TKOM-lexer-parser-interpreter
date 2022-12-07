using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Types
{
    public class TypesInfoProvider
    {
        private readonly IEnumerable<string> _currencyTypes;

        public TypesInfoProvider(IEnumerable<string>? currencyTypes = null)
        {
            _currencyTypes = currencyTypes ?? new List<string>();
        }

        public static Dictionary<string, TypeEnum> StandardTypes => new()
        {
            { TypeName.BOOL, TypeEnum.BOOL },
            { TypeName.INT, TypeEnum.INT },
            { TypeName.DECIMAL, TypeEnum.DECIMAL },
            { TypeName.STRING, TypeEnum.STRING },
            { TypeName.ACCOUNT, TypeEnum.ACCOUNT },
            { TypeName.COLLECTION, TypeEnum.COLLECTION }
        };

        public Dictionary<string, TypeEnum> CurrencyTypes => _currencyTypes.ToDictionary(x => x, x => TypeEnum.CURRENCY);

        public Dictionary<string, TypeEnum> Types => StandardTypes.Union(CurrencyTypes)
                                                                  .ToDictionary(x => x.Key, x => x.Value);
    }
}
