using Application.Models.Types;

namespace Application.Infrastructure.Lekser
{
    public class LexerOptions
    {
        public TypesInfoProvider? TypesInfo { get; set; } = new TypesInfoProvider();
        public int LiteralMaxLength { get; set; } = 255;
    }
}