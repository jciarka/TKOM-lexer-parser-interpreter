using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Values.BasicTypeValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Values
{
    public static class ValuesFactory
    {
        public static IValue GetDefaultValue(TypeBase type)
        {
            switch (type.Type)
            {
                case Types.TypeEnum.INT:
                    return new IntValue();

                case Types.TypeEnum.DECIMAL:
                    return new DecimalValue();

                case Types.TypeEnum.BOOL:
                    return new BoolValue();

                case Types.TypeEnum.STRING:
                    return new StringValue();

                case Types.TypeEnum.ACCOUNT:
                    return new Reference(type);

                case Types.TypeEnum.TYPE:
                    return new TypeValue();

                case Types.TypeEnum.COLLECTION:
                    return new Reference(type);

                case Types.TypeEnum.CURRENCY:
                    return new CurrencyValue(type.Name);

                case Types.TypeEnum.VOID:
                    throw new NotSupportedException();
            }
            throw new NotSupportedException();
        }
    }
}
