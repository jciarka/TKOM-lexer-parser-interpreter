using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Lekser.SourceReaders
{
    /// <summary>
    /// Abstracion over different types of text sourecs end specyfic to machine encodings
    /// </summary>
    public interface ISourceReader : IDisposable
    {
        /// <summary>
        /// Number of line that cursor is currently at
        /// </summary>
        public long Line { get; }

        /// <summary>
        /// Number of character in current line that cursor is currently at
        /// </summary>
        public long Column { get; }

        /// <summary>
        /// Stream position the cursor is currently at
        /// </summary>
        public long Position { get; }

        /// <summary>
        /// Stream position of first byte of current line
        /// </summary>
        public long LinePosition { get; }

        /// <summary>
        /// Reads the character the cursor is currently at and moves cursor one character forward
        /// </summary>
        /// <returns>Character the cursor is currently at</returns>
        public char Current { get; }

        /// <summary>
        /// Reads the character the cursor is currently at without moving the cursor
        /// </summary>
        /// <returns>Character the cursor is currently at</returns>
        public bool Advance();
    }
}
