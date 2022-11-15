
namespace Application.Infrastructure.Presenters
{
    public interface IRandomSourceReader
    {
        void Dispose();
        IDictionary<long, string> ReadManyLinesFromPositions(IEnumerable<long> streamPositions);
        bool TryReadLineFromPosition(long streamPosition, out string? line);
    }
}