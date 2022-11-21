using Application.Infrastructure.Lekser.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Lekser.SourceReaders
{
    public abstract class BaseSourceReader : ISourceReader, IDisposable
    {
        private long _line;
        public long Line => _line;

        private long _column;
        public long Column => _column;

        private long _linePosition;
        public long LinePosition => _linePosition;

        public long Position => getAtContentPosition();

        public BaseSourceReader()
        {
            _line = 0;
            _column = 0;
            _linePosition = 0;
        }

        protected abstract bool isEndOfContent();
        protected abstract char peekFromContent();
        protected abstract char readFromContent();
        protected abstract long getAtContentPosition();

        public char Current
        {
            get
            {
                if (isEndOfContent())
                    return CharactersHelpers.EOF;

                if (isNewLine())
                    return CharactersHelpers.NL;

                return peekFromContent();
            }
        }

        public bool Advance()
        {
            if (isEndOfContent())
                return false;

            if (trySkipNewLine())
                return true;

            incrementColumn();
            readFromContent();
            return true;
        }

        private bool trySkipNewLine()
        {
            var first = peekFromContent();

            // check first character
            if (!CharactersHelpers.NewLinesSequences.Any(x => x[0] == first))
            {
                return false;
            }

            readFromContent();
            incrementColumn();

            if (isEndOfContent())
            {
                return true;
            }

            // check second character
            var second = peekFromContent();

            if (CharactersHelpers.NewLinesSequences.Any(
                    x => x.Length == 2 && x[0] == first && x[1] == second)
                )
            {
                readFromContent();
                incrementColumn();
            }

            incrementLine();
            return true;
        }

        private bool isNewLine()
        {
            var first = peekFromContent();

            // check first character
            if (!CharactersHelpers.NewLinesSequences.Any(x => x[0] == first))
            {
                return false;
            }

            return true;
        }

        private void incrementLine()
        {
            _line++;
            _column = 0;
            _linePosition = getAtContentPosition();
        }

        private void incrementColumn()
        {
            _column++;
        }

        public virtual void Dispose() { }
    }
}
