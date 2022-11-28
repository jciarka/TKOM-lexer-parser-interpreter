using Application.Models.Tokens;

namespace Application.Infrastructure.Lekser
{
    public interface ILexer
    {
        Token Current { get; }
        bool Advance();
    }
}