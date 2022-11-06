using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Helpers
{
    public class NumberBuilder
    {
        private decimal value = 1;

        public NumberBuilderState State { get; private set; } = NumberBuilderState.CHARACTER_REQUIRED;

        public bool IsInteger => decimalPrecision == null;

        private int? decimalPrecision = null;

        public bool tryAppend(char character)
        {
            if (State == NumberBuilderState.INVALID)
            {
                return false;
            }

            if (character.Equals('.'))
            {
                if (IsInteger)
                {
                    decimalPrecision = 0;
                    State = NumberBuilderState.CHARACTER_REQUIRED;
                    return true;
                }

                if (!IsInteger || State == NumberBuilderState.CHARACTER_REQUIRED)
                {
                    State = NumberBuilderState.INVALID;
                    return false;
                }
            }

            if (!char.IsDigit(character))
            {
                State = NumberBuilderState.INVALID;
                return false;
            }

            int digit = character - '0';

            if (IsInteger)
            {
                try
                {
                    value = 10 * value + digit;
                    State = NumberBuilderState.VALID;
                    return true;
                }
                catch (OverflowException)
                {
                    State = NumberBuilderState.OVERFLOWED;
                    return false;
                }
            }

            if (IsInteger)
            {
                try
                {
                    value = 10 * value + digit;
                    State = NumberBuilderState.VALID;
                    return true;
                }
                catch (OverflowException)
                {
                    State = NumberBuilderState.OVERFLOWED;
                    return false;
                }
            }
            else
            {
                try
                {
                    decimalPrecision += 1;
                    value = value + Decimal.Divide(digit, 10 ^ (int)decimalPrecision!);
                    State = NumberBuilderState.VALID;
                    return true;
                }
                catch (OverflowException)
                {
                    State = NumberBuilderState.OVERFLOWED;
                    return false;
                }
            }
        }

        public int ToInteger()
        {
            return (int)value;
        }

        public decimal ToDecimal()
        {
            return value;
        }
    }

    public enum NumberBuilderState
    {
        VALID,
        INVALID,
        OVERFLOWED,
        EMPTY,
        CHARACTER_REQUIRED
    }
}
