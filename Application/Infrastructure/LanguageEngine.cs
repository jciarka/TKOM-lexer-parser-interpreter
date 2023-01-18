using Application.Infrastructure.ConfigurationParser;
using Application.Infrastructure.Interpreter;
using Application.Infrastructure.Lekser;
using Application.Infrastructure.Lekser.SourceReaders;
using Application.Infrastructure.Lexer;
using Application.Infrastructure.Presenters;
using Application.Infrastructure.SourceParser;
using Application.Models.ConfigurationParser;

namespace Application.Infrastructure
{
    public class LanguageEngine
    {
        private readonly string sourcePath;
        private readonly string? configurationPath;

        public LanguageEngine(string sourcePath, string? configurationPath)
        {
            this.sourcePath = sourcePath;
            this.configurationPath = configurationPath;
        }

        public void Execute()
        {
            CurrencyTypesInfo? currencyInfo;

            if (configurationPath != null)
            {
                if (!computeConfiguration(out currencyInfo))
                {
                    Console.WriteLine("Configuration computing executed with errors.");
                    return;
                }
            }
            else
            {
                currencyInfo = new CurrencyTypesInfo();
            }

            computeSource(currencyInfo!);
        }

        private bool computeSource(CurrencyTypesInfo curencyInfo)
        {
            using (var errorReader = new FileSourceRandomReader(sourcePath))
            using (var reader = new FileSourceReader(sourcePath))
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
                    Console.WriteLine("Parsing executed with errors.");
                    return false;
                }

                var interpreter = new InterpreterEngine(
                        new ConsoleErrorHandler(errorReader),
                        new InterpreterEngineOptions { CurrencyTypesInfo = curencyInfo }
                    );

                interpreter.InterpretProgram(root);

                if (errorHandler.ErrorCount() > 0)
                {
                    Console.WriteLine("Execution finished with runtime error.");
                    return false;
                }

                return true;
            }
        }

        private bool computeConfiguration(out CurrencyTypesInfo? configuration)
        {
            using (var errorReader = new FileSourceRandomReader(configurationPath!))
            using (var reader = new FileSourceReader(configurationPath!))
            {
                var configurationErrorHandler = new ConsoleErrorHandler(errorReader);

                var lexer = new LexerEngine(reader);

                var parser = new ConfigurationParserEngine(lexer, configurationErrorHandler);

                TokenPresenter presenter = new TokenPresenter();

                configuration = parser.Parse();

                return configurationErrorHandler.ErrorCount() == 0;
            }
        }
    }
}
