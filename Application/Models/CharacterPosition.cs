using Application.Models.Grammar;

namespace Application.Models
{
    public class CharacterPosition
    {
        public CharacterPosition()
        {

        }

        public CharacterPosition(RulePosition position)
        {
            Line = position.Line;
            Column = position.Column;
            LinePosition = position.LinePosition;
            Position = position.Position;
        }

        public CharacterPosition(long line, long column, long position, long linePosition)
        {
            Line = line;
            Column = column;
            Position = position;
            LinePosition = linePosition;
        }

        public long Line { get; set; }
        public long Column { get; set; }
        public long Position { get; set; }
        public long LinePosition { get; set; }
    }
}
