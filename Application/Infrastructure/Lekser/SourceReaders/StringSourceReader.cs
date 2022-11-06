using Application.Infrastructure.Lekser.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Lekser.SourceReaders
{
    public class StringSourceReader : BaseSourceReader
    {
        private readonly string _content;
        private int stringPosition;

        public StringSourceReader(string content) : base()
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
        }

        protected override long getAtContentPosition()
        {
            return stringPosition;
        }

        protected override bool isEndOfContent()
        {
            return stringPosition >= _content.Length;
        }

        protected override char peekFromContent()
        {
            return !isEndOfContent() ? _content[stringPosition] : CharactersHelpers.EOF;
        }

        protected override char readFromContent()
        {
            return !isEndOfContent() ? _content[stringPosition++] : CharactersHelpers.EOF;
        }
    }
}
