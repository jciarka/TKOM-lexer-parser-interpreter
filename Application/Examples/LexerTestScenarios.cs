using Application.Infrastructure.ConfigurationParser;
using Application.Infrastructure.Lekser;
using Application.Infrastructure.Lekser.SourceReaders;
using Application.Infrastructure.Lexer;
using Application.Infrastructure.Presenters;
using Application.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application.Examples
{
    public static class LexerTestScenarios
    {
        public static void ExampleWithSourceOk_File()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "TestFiles\\Source_Ok.txt");

            using (var lexer = new LexerEngine(new FileSourceReader(path),
                 new LexerOptions { TypesInfo = new Models.Types.TypesInfoProvider(new string[] { "USD", "PLN", "CHF" }) }))
            {
                TokenPresenter presenter = new TokenPresenter();

                while (lexer.Current.Type != TokenType.EOF)
                {
                    presenter.PresentToken(lexer.Current);
                    lexer.Advance();
                }

            }
        }

        public static void ExampleWithSourceOkAndCommentFilter_File()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "TestFiles\\Source_Ok.txt");

            using (var lexer = new SkipCommentsFilter(
                    new LexerEngine(
                         new FileSourceReader(path),
                         new LexerOptions
                         {
                             TypesInfo = new Models.Types.TypesInfoProvider(new string[] { "USD", "PLN", "CHF" })
                         }
                    ))
                )
            {
                TokenPresenter presenter = new TokenPresenter();

                while (lexer.Current.Type != TokenType.EOF)
                {
                    presenter.PresentToken(lexer.Current);
                    lexer.Advance();
                }
            }
        }

        public static void ExampleWithSourceOk_String()
        {
            var content = @"
# Przykład pliku źródłowego 

int nwd(int a, int b)
{
    if(a==b)
    {
        return a;
    }
    if (a > b)
    {
        return nwd(a-b, b); # wywołania rekurencyjne
    }
    return nwd(b-a);
}

int factorial(int a)
{
    if(a == 1)
    {
        return 1;
    }

    return a * factorial(a-1);
}


void main()
{	
    int x = 3;
    int y = 10;

    print(""NWD 3 i 10 wynosi = "", nwd(x, y));  # NWD 3 i 10 wynosi = 1
    print(""Silnia 3 wynosi = "", factorial(x, y));  # Silnia 3 = 6

    var a = Account(PLN, 100000);

    # Zmniejszenie stanu konta o wartość
    a >> 100CHF;
    a >> 100 to CHF;
    a >> getAmount() to Currency(getCurrenctStr());

    # Zmniejszenie stanu konta o procent
    a %> 0.1;
    a %> getPrct();

    # Zwiększenie stanu konta o wartość
    a << 100CHF;
    a << 100 to CHF;
    a << getAmount() to Currency(getCurrenctStr());

    # Zwiększenie stanu konta o procent
    a <% 0.1;
    a <% getPrct();

    # Przelew między kontami na wartość
    a >> 100CHF >> b;
    a >> 100 to CHF >> b;
    a >> getAmount() to Currency(getCurrenctStr()) >> b;


    # Przelew między kontami na procent pierwszego z kont
    a %> 0.1 >> b;
    a %> getPrct() >> b;

    # wszystkie opisane powyżej operacje zwracaj obiekt Account
    # zawierający informację o kwocie operacji pieniężnej
    Account amount = a %> 0.1;
    print(amount.value); # 1000
    print(amount.currency); # PLN

    # Obliczenie procentowej wartości z konta
    Account prctAmount = a % 0.1;
    Account prctAmount = a % getPrct();

    var a = Account(PLN, 100000, ""NAZWA KONTA"" /* nazwa jest opcjonalna*/);
    print(a.name) # NAZWA KONTA 
    print(a.value) # 1000000 (typ decimal)
    print(a.currency) # PLN (typ Type)
    print(a.balance) # 1000000 PLN (typ PLN)

    # Dodawanie
    accounts.Add(Account(PLN, 120));
    accounts.Add(Account(USD, 10));

    # Dostęp indeksowy
    var account = Accounts[1];
    PLN balance = Accounts[1].balance;

    # Modyfikacje na indeksie
    accounts[1] = Account(USD, 0);
    accounts[1].name = ""TESTOWE"";

    # Usuwanie
    Accounts.delete(1);

    # Przeszukiwanie z delegatem
    Account first = Accounts.first(x => x.currency == USD);
    Account last = Accounts.last(x => x.currency == PLN);
    Collection<Account> usdAccounts = Accounts.where(
                                               x => x.currecy == USD);
}
";
            using (var lexer = new LexerEngine(new StringSourceReader(content),
                 new LexerOptions { TypesInfo = new Models.Types.TypesInfoProvider(new string[] { "USD", "PLN", "CHF" }) }))
            {
                var parser = new ConfigurationParserEngine(lexer);
                TokenPresenter presenter = new TokenPresenter();

                while (lexer.Current.Type != TokenType.EOF)
                {
                    presenter.PresentToken(lexer.Current);
                    lexer.Advance();
                }
            }
        }

        public static void ExampleWithConfigurationOk_File()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "TestFiles\\CurrencyConfguration_Ok.txt");

            using (var lexer = new LexerEngine(new FileSourceReader(path)))
            {
                TokenPresenter presenter = new TokenPresenter();

                while (lexer.Current.Type != TokenType.EOF)
                {
                    presenter.PresentToken(lexer.Current);
                    lexer.Advance();
                }
            }

            Console.WriteLine();

            using (var lexer = new LexerEngine(new FileSourceReader(path)))
            {
                var parser = new ConfigurationParserEngine(lexer);
                TokenPresenter presenter = new TokenPresenter();

                var configurationInfo = parser.Parse(out var issues);

                if (issues.Count() > 0)
                {
                    var errorPresenter = new ErrorConsolePresenter(new FileSourceRandomReader(path));
                    errorPresenter.Present(issues);
                }
            }
        }

        public static void ExampleWithConfigurationOk_String()
        {
            var content = @"
         USD CAD   EUR GBP   HKD CHF    JPY AUD     INR CNY;
            USD    1  1.36  1.01  0.87   7.85     1  148.74  1.56  82.73   7.3;
            CAD 0.73     1  0.74  0.64   5.77  0.74  109.27  1.15  60.78  5.36;
            EUR 0.99  1.35     1  0.86   7.76  0.99  147.04  1.54  81.78  7.22;
            GBP 1.15  1.56  1.16     1     9   1.15  170.57  1.79  94.87  8.37;
            HKD 0.13  0.17  0.13  0.11     1   0.13   18.95   0.2  10.54  0.93;
            CHF    1  1.36  1.01  0.87   0.8     1   148.5  1.56   82.6   7.29;
            JPY 0.01  0.01  0.01  0.01  0.05   0.01       1  0.01   0.56  0.05;
            AUD 0.64  0.87  0.65  0.56  5.03   0.64   95.34     1  53.03  4.68;
            INR 0.01  0.02  0.01  0.01  0.09   0.01     1.8  0.02      1  0.09;
            CNY 0.14  0.19  0.14  0.12  1.07   0.14   20.37  0.21  11.33     1;
            ";
            using (var lexer = new LexerEngine(new StringSourceReader(content)))
            {
                var parser = new ConfigurationParserEngine(lexer);
                TokenPresenter presenter = new TokenPresenter();

                while (lexer.Current.Type != TokenType.EOF)
                {
                    presenter.PresentToken(lexer.Current);
                    lexer.Advance();
                }
            }

            Console.WriteLine();

            using (var lexer = new LexerEngine(new StringSourceReader(content)))
            {
                var parser = new ConfigurationParserEngine(lexer);
                TokenPresenter presenter = new TokenPresenter();

                var configurationInfo = parser.Parse(out var issues);

                if (issues.Count() > 0)
                {
                    var errorPresenter = new ErrorConsolePresenter(new StringSourceRandomReader(content));
                    errorPresenter.Present(issues);
                }
            }
        }

        public static void ExampleWithConfigurationErrors_File()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "TestFiles\\CurrencyConfguration_Errors.txt");

            using (var lexer = new LexerEngine(new FileSourceReader(path)))
            {
                TokenPresenter presenter = new TokenPresenter();

                while (lexer.Current.Type != TokenType.EOF)
                {
                    presenter.PresentToken(lexer.Current);
                    lexer.Advance();
                }
            }

            Console.WriteLine();

            using (var lexer = new LexerEngine(new FileSourceReader(path)))
            {
                var parser = new ConfigurationParserEngine(lexer);
                TokenPresenter presenter = new TokenPresenter();

                var configurationInfo = parser.Parse(out var issues);

                if (issues.Count() > 0)
                {
                    var errorPresenter = new ErrorConsolePresenter(new FileSourceRandomReader(path));
                    errorPresenter.Present(issues);
                }
            }
        }

        public static void ExampleWithConfigurationErrors_String()
        {
            var content = @"
      USD   CAD  EURO    GBP   HKD 	 CHF      JPY   AUD    INR   CNY;
USD     1  1.36  1.01  0.87   7.85     1   148.74  1.56  82.73   7.3;
CAD	 0.73     1  0.74  0.64   5.77  0.74   109.27  1.15  60.78  5.36;
EURO 0.99  1.35     1  0.86   7.76  0.99   147.04  1.54  81.78  7.22;
GBP  1.15  test  1.16     1	     9   1.15  170.57  1.79  94.87  8.37;
HKD	 0.13  0.17  0.13  0.11      1   0.13   18.95   0.2  10.54  0.93;
CHF     1  1.36  1.01  0.87    0.8      1   148.5  1.56   82.6  7.29;
JPY	 0.01  0.01  0.01  0.01   0.05   0.01       1  0.01   0.56  0.05
AUD	 0.64  0.87  0.65  0.56   5.03   0.64   95.34     1  53.03  4.68;
INR	 0.01  0.02  0.01  0.01   0.09   0.01     1.8  0.02      1  0.09;
CNY	 0.14  0.19  0.14  0.12   1.07   0.14   20.37  0.21  11.33     1;
";
            using (var lexer = new LexerEngine(new StringSourceReader(content)))
            {
                var parser = new ConfigurationParserEngine(lexer);
                TokenPresenter presenter = new TokenPresenter();

                while (lexer.Current.Type != TokenType.EOF)
                {
                    presenter.PresentToken(lexer.Current);
                    lexer.Advance();
                }
            }

            Console.WriteLine();

            using (var lexer = new LexerEngine(new StringSourceReader(content)))
            {
                var parser = new ConfigurationParserEngine(lexer);
                TokenPresenter presenter = new TokenPresenter();

                var configurationInfo = parser.Parse(out var issues);

                if (issues.Count() > 0)
                {
                    var errorPresenter = new ErrorConsolePresenter(new StringSourceRandomReader(content));
                    errorPresenter.Present(issues);
                }
            }
        }
    }
}
