using Application.Models.Types;

namespace Application.Infrastructure.SourceParser
{
    public class ParserOptions
    {
        public TypesInfoProvider TypesInfo { get; set; } = new TypesInfoProvider();
    }
}