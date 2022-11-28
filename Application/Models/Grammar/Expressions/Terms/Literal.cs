﻿using Application.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class Literal : TermBase
    {
        public string Type { get; }
        public bool? BoolValue { get; set; }
        public string? StringValue { get; set; }
        public int? IntValue { get; set; }
        public decimal? DecimalValue { get; set; }

        public Literal(Token token)
        {
            BoolValue = token.BoolValue;
            StringValue = token.StringValue;
            IntValue = token.IntValue;
            DecimalValue = token.DecimalValue;
            Type = token.ValueType!;
        }

        public Literal(string type, bool? boolValue = null, string? stringValue = null, int? intValue = null, decimal? decimalValue = null)
        {
            BoolValue = boolValue;
            StringValue = stringValue;
            IntValue = intValue;
            DecimalValue = decimalValue;
            Type = type;
        }
    }
}