using Application.Infrastructure.ConfigurationParser;
using Application.Infrastructure.Lekser;
using Application.Infrastructure.Lekser.SourceReaders;
using Application.Infrastructure.Lexer;
using Application.Infrastructure.Presenters;
using Application.Infrastructure.SourceParser;
using Application.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application.Examples
{
    public static class ParserTestScenarios
    {
        public static void Example()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "TestFiles\\Source_nwd.txt");

            using (var reader = new FileSourceReader(path))
            using (var errorReader = new FileSourceRandomReader(path))
            {
                var lexer = new LexerEngine(
                    reader,
                    new LexerOptions
                    {
                        TypesInfo = new Models.Types.TypesInfoProvider(new string[] { "USD", "PLN", "CHF" })
                    });

                var parserEngine = new SourceParserEngine(
                    new SkipCommentsFilter(lexer),
                    new ParserOptions { TypesInfo = new Models.Types.TypesInfoProvider(new string[] { "USD", "PLN", "CHF" }) },
                    new ConsoleErrorHandler(errorReader));

                var root = parserEngine.Parse();

                var presenter = new GrammarPresenter();

                root.Accept(presenter, 0);
            }
        }
    }
}
