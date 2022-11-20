using Application.Models.Tokens;

namespace Application.Infrastructure.Lekser
{
    public interface ILexer : IDisposable
    {
        Token Peek();
        Token Read();
    }
}