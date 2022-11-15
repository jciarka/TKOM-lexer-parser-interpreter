using Application.Infrastructure.Lekser.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Presenters
{
    public class FileSourceRandomReader : IDisposable, IRandomSourceReader
    {
        private readonly StreamReader _reader;

        public FileSourceRandomReader(string path) : base()
        {
            _reader = new StreamReader(path);
        }

        public bool TryReadLineFromPosition(long streamPosition, out string? line)
        {
            _reader.SetPosition(streamPosition);
            line = _reader.ReadLine();
            return line != null;
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
            _reader.Close();
        }
    }
}
