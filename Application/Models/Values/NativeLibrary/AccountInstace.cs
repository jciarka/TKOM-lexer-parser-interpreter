using Application.Models.ConfigurationParser;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Types;
using Application.Models.Values.BasicTypeValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Values.NativeLibrary
{
    public class AccountInstace : Instance
    {
        public AccountInstace(IClass @class) : base(@class)
        {
        }

        public IValue Withdraw(CurrencyTypesInfo currencyInfo, IValue amount, AccountInstace? toAccount = null)
        {
            var thisCurrency = ((GenericType)Class.Type).ParametrisingType;
            var oryginalAmountValue = (CurrencyValue)amount;

            CurrencyValue thisCurrencyAmountValue;
            if (!thisCurrency.Name.Equals(oryginalAmountValue.Type.Name))
            {
                thisCurrencyAmountValue = (CurrencyValue)oryginalAmountValue.To(new TypeValue(thisCurrency), currencyInfo);
            }
            else
            {
                thisCurrencyAmountValue = oryginalAmountValue;
            }

            SetProperty(
                "Ballance",
                ((DecimalValue)GetProperty("Ballance"))
                    .Sub(thisCurrencyAmountValue.To(new TypeValue(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL)), null!))
            );

            if (toAccount != null)
            {
                ((AccountInstace)toAccount).Fund(currencyInfo, amount);
            }

            return new NullValue();
        }

        public IValue Fund(CurrencyTypesInfo currencyInfo, IValue amount)
        {
            var thisCurrency = ((GenericType)Class.Type).ParametrisingType;
            var oryginalAmountValue = (CurrencyValue)amount;

            CurrencyValue thisCurrencyAmountValue;
            if (!thisCurrency.Name.Equals(oryginalAmountValue.Type.Name))
            {
                thisCurrencyAmountValue = (CurrencyValue)oryginalAmountValue.To(new TypeValue(thisCurrency), currencyInfo);
            }
            else
            {
                thisCurrencyAmountValue = oryginalAmountValue;
            }

            SetProperty(
                "Ballance",
                ((DecimalValue)GetProperty("Ballance"))
                    .Add(thisCurrencyAmountValue.To(new TypeValue(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL)), null!))
            );

            return new NullValue();
        }

        public IValue PrctOf(IValue prct)
        {
            DecimalValue dPrct;
            if (prct is IntValue)
            {
                dPrct = (DecimalValue)prct.To(new TypeValue(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL)), null!);
            }
            else if (prct is DecimalValue)
            {
                dPrct = (DecimalValue)prct;
            }
            else
            {
                throw new NotSupportedException();
            }

            return new DecimalValue(((DecimalValue)GetProperty("Ballance")).Value * dPrct.Value / 100);
        }
    }
}
