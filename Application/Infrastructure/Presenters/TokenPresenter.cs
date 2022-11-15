using Application.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Presenters
{
    public class TokenPresenter
    {
        public void PresentToken(Token token)
        {
            Console.WriteLine($"(Line: {token.Position!.Line} Column: {token.Position.Column}) {token.Type.ToString()} of lexeme {token.Lexeme} of value " +
                $"{ (object)token.BoolValue! ?? (object)token.IntValue! ?? (object)token.StringValue! ?? (object)token.DecimalValue! ?? token.StringValue ?? "EMPTY" }");
        }
    }
}
