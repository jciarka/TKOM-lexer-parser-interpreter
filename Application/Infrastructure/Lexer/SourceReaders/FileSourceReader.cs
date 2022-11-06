using Application.Infrastructure.Lekser.Extentions;
using Application.Infrastructure.Lekser.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Lekser.SourceReaders
{
    public class FileSourceReader : BaseSourceReader, IDisposable
    {
        private readonly StreamReader _reader;

        public FileSourceReader(string path) : base()
        {
            _reader = new StreamReader(path);
        }

        protected override long getAtContentPosition()
        {
            return _reader.GetPosition();
        }

        protected override bool isEndOfContent()
        {
            return _reader.EndOfStream;
        }

        protected override char peekFromContent()
        {
            return !isEndOfContent() ? (char)_reader.Peek() : CharactersHelpers.EOF;
        }

        protected override char readFromContent()
        {
            return !isEndOfContent() ? (char)_reader.Read() : CharactersHelpers.EOF;
        }

        public override void Dispose()
        {
            _reader.Close();
        }
    }
}
