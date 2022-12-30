using Application.Infrastructure.ConfigurationParser;
using Application.Infrastructure.Interpreter;
using Application.Infrastructure.Lekser;
using Application.Infrastructure.Lekser.SourceReaders;
using Application.Infrastructure.Lexer;
using Application.Infrastructure.Presenters;
using Application.Infrastructure.SourceParser;
using Application.Models.ConfigurationParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application.Examples
{
    public class InterpreterTestScenarions
    {
        public static void InterperterExample()
        {
            var conf_path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "TestFiles\\CurrencyConfguration_Ok.txt");

            CurrencyTypesInfo curencyInfo;

            using (var errorReader = new FileSourceRandomReader(conf_path))
            using (var reader = new FileSourceReader(conf_path))
            {
                var configurationErrorHandler = new ConsoleErrorHandler(errorReader);

                var lexer = new LexerEngine(reader);

                var parser = new ConfigurationParserEngine(lexer, configurationErrorHandler);

                TokenPresenter presenter = new TokenPresenter();

                curencyInfo = parser.Parse();
            }

            var source_path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "TestFiles\\Interpret_basic.txt");

            using (var errorReader = new FileSourceRandomReader(source_path))
            using (var reader = new FileSourceReader(source_path))
            {
                var errorHandler = new ConsoleErrorHandler(errorReader);

                var lexer = new LexerEngine(
                    reader,
                    new LexerOptions
                    {
                        TypesInfo = new Models.Types.TypesInfoProvider(curencyInfo.currencyTypes)
                    });

                var parserEngine = new SourceParserEngine(
                    new SkipCommentsFilter(lexer),
                    new ParserOptions { TypesInfo = new Models.Types.TypesInfoProvider(curencyInfo.currencyTypes) },
                    errorHandler);

                var root = parserEngine.Parse();

                var typingAnalyser = new TypeVerifier(errorHandler);
                typingAnalyser.Visit(root);

                if (errorHandler.ErrorCount() > 0)
                {
                    return;
                }

                var interpreter = new InterpreterEngine(
                        new ConsoleErrorHandler(errorReader),
                        new InterpreterEngineOptions { CurrencyTypesInfo = curencyInfo }
                    );

                interpreter.InterpretProgram(root);
            }
        }
    }
}

