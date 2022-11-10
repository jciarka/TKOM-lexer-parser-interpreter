using Application.Models.Tokens;

namespace Application.Infrastructure.Lekser
{
    public interface ILexer
    {
        Token Future(int index);
        Token Peek();
        Token? Previous();
        Token? Previous(int index);
        Token Read();
    }
}