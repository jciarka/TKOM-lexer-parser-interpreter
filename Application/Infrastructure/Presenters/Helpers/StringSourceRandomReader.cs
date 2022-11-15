using Application.Infrastructure.Lekser.Extentions;
using Application.Infrastructure.Lekser.Helpers;
using Application.Infrastructure.Lekser.SourceReaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Presenters
{
    public class StringSourceRandomReader : IRandomSourceReader, IDisposable
    {
        public readonly string _content;

        public StringSourceRandomReader(string content)
        {
            _content = content;
        }

        public bool TryReadLineFromPosition(long streamPosition, out string? line)
        {
            StringBuilder builder = new();

            while (!isNewLine(streamPosition) && streamPosition < _content.Length)
            {
                builder.Append(_content[(int)streamPosition++]);
            }

            if (builder.Length == 0)
            {
                line = null;
                return false;
            }

            line = builder.ToString();
            return true;
        }

        private bool isNewLine(long position)
        {
            var first = _content[(int)position];

            if (!CharactersHelpers.NewLinesSequences.Any(x => x[0] == first))
            {
                return false;
            }

            return true;
        }

        public IDictionary<long, string> ReadManyLinesFromPositions(IEnumerable<long> streamPositions)
        {
            IDictionary<long, string> lines = new Dictionary<long, string>();
            var distinctPositions = streamPositions.Distinct().OrderBy(x => x);

            foreach (var position in streamPositions)
            {
                if (TryReadLineFromPosition(position, out string? line))
                {
                    lines.Add(position, line!);
                }
            }

            return lines;
        }

        public void Dispose()
        {

        }
    }
}
