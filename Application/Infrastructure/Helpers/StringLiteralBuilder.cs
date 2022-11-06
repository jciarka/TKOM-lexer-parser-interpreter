using Application.Infrastructure.Lekser.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Helpers
{
    public class StringLiteralBuilder
    {
        private StringBuilder _builder;

        private bool escaped = false;

        private int length = 0;
        public int Length => length;

        public LiteralBuilderState State { get; private set; } = LiteralBuilderState.EMPTY;

        public StringLiteralBuilder()
        {
            _builder = new StringBuilder();
        }

        public override string ToString()
        {
            return _builder.ToString();
        }

        public bool tryAppend(char letter)
        {
            length++;

            if (State == LiteralBuilderState.EMPTY && !letter.Equals('\"'))
            {
                State = LiteralBuilderState.INVALID;
                return false;
            }

            if (State == LiteralBuilderState.INVALID || State == LiteralBuilderState.VALID)
            {
                State = LiteralBuilderState.INVALID;
                return false;
            }

            if (escaped)
            {
                if (letter.Equals('n'))
                {
                    _builder.Append('\n');
                }
                else if (letter.Equals('t'))
                {
                    _builder.Append('\t');
                }
                else if (letter.Equals('f'))
                {
                    _builder.Append('\f');
                }
                else if (letter.Equals('b'))
                {
                    _builder.Append('\b');
                }
                else if (letter.Equals('v'))
                {
                    _builder.Append('\v');
                }
                else if (letter.Equals('\''))
                {
                    _builder.Append('\'');
                }
                else if (letter.Equals('\\'))
                {
                    _builder.Append('\\');
                }
                else
                {
                    _builder.Append(letter);
                }

                escaped = false;

                return true;
            }

            if (letter.Equals('"'))
            {
                if (State == LiteralBuilderState.EMPTY)
                {
                    State = LiteralBuilderState.IN_PROGRESS;
                    return true;
                }
                else
                {
                    State = LiteralBuilderState.VALID;
                    return true;
                }
            }

            if (letter.Equals(CharactersHelpers.EOF) || letter.Equals(CharactersHelpers.NL))
            {
                State = LiteralBuilderState.INVALID;
                return false;
            }

            if (letter.Equals('\\'))
            {
                escaped = true;
                return true;
            }

            _builder.Append(letter);
            return true;
        }
    }

    public enum LiteralBuilderState
    {
        EMPTY,
        IN_PROGRESS,
        VALID,
        INVALID,
    }
}