﻿using Application.Infrastructure.ConfigurationParser;
using Application.Infrastructure.Lekser;
using Application.Infrastructure.Lekser.SourceReaders;

Console.WriteLine("Hello, World!");

var content = @"
                     USD   CAD   EUR 	GBP   HKD 	 CHF    JPY   AUD 	  INR  CNY;
                USD	   1  1.36  1.01  0.87   7.85     1  148.74  1.56  82.73   7.3;
                CAD	0.73     1  0.74  0.64   5.77  0.74  109.27  1.15  60.78  5.36;
                EUR	0.99  1.35     1  0.86   7.76  0.99  147.04  1.54  81.78  7.22;
                GBP	1.15  1.56  1.16     1	   9   1.15  170.57	 1.79  94.87  8.37;
                HKD	0.13  0.17  0.13  0.11     1   0.13   18.95   0.2  10.54  0.93;
                CHF	   1  1.36  1.01  0.87  7.84      1   148.5  1.56   82.6  7.29;
                JPY	0.01  0.01  0.01  0.01  0.05   0.01       1	 0.01   0.56  0.05;
                AUD	0.64  0.87  0.65  0.56  5.03   0.64   95.34     1  53.03  4.68;
                INR	0.01  0.02  0.01  0.01  0.09   0.01     1.8	 0.02      1  0.09;
                CNY	0.14  0.19  0.14  0.12  1.07   0.14   20.37  0.21  11.33     1;
            ";


var lexer = new LexerEngine(new StringSourceReader(content));
var parser = new ConfigurationParserEngine(lexer);

var configurationInfo = parser.Parse(out var issues);