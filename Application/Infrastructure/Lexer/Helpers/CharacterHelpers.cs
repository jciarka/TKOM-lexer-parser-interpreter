using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Lekser.Helpers
{
    public static class CharactersHelpers
    {
        public static IEnumerable<byte[]> NewLinesSequences = new List<byte[]>()
        {
            new byte[] { 0x0A }, // Unix
            new byte[] { 0x0D, 0x0A }, // Windows
            new byte[] { 0x0D }, // Commodore
            new byte[] { 0x1E }, // QNX
            new byte[] { 0x0A, 0x0D }, // Acron BBC
            new byte[] { 0x9B }, // Atari
            new byte[] { 0x15 }, // IBM
        };

        public static char EOF => (char)0x05;

        public static char NL => '\n';
    }
}
