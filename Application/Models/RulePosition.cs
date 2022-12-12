namespace Application.Models.Grammar
{
    public class RulePosition
    {
        public RulePosition(CharacterPosition position)
        {
            Line = position.Line;
            Column = position.Column;
            Position = position.Position;
            LinePosition = position.LinePosition;
        }

        public RulePosition(long line, long column, long position, long linePosition)
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