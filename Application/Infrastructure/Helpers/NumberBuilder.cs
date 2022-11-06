using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Helpers
{
    public class NumberBuilder
    {
        private decimal value = 0m;
        private decimal factorial = 1m;
        private int? decimalPrecision = null;

        public NumberBuilderState State { get; private set; } = NumberBuilderState.CHARACTER_REQUIRED;

        public bool IsInteger => decimalPrecision == null;

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
            else
            {
                try
                {
                    factorial *= 0.1m;
                    value += (digit * factorial);
                    decimalPrecision += 1;
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
