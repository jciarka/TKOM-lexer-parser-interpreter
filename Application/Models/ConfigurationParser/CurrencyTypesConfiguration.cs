using Application.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.ConfigurationParser
{
    public class CurrencyTypesInfo
    {
        public List<string> currencyTypes = new();
        public Dictionary<(string CFrom, string CTo), decimal> currencyConvertions = new();
    }
}
