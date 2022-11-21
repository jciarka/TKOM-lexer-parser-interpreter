using Application.Models.Tokens;

namespace Application.Infrastructure.Lekser
{
    public interface ILexer : IDisposable
    {
        Token Current { get; }
        bool Advance();
    }
}