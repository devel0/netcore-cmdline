using System;
using SearchAThing;

namespace example_01
{
    class Program
    {
        static void Main(string[] args)
        {
            CmdlineParser.Create("sample application", (parser) =>
            {
                var xflag = parser.AddShort("x", "my first flag", "XVAL");
                var yflag = parser.AddShort("y", "my second flag", "YVAL");
                var vflag = parser.AddShortLong("v", "value", "a value flag", "VAL");
                
                parser.AddShort("h", "show usage", null, (item) => item.MatchParser.PrintUsage());

                parser.OnCmdlineMatch(() =>
                {
                    if (xflag) System.Console.WriteLine($"x flag used [{(string)xflag}]");
                    if (yflag) System.Console.WriteLine($"y flag used [{(string)yflag}]");
                    if (vflag) System.Console.WriteLine($"value specified [{(string)vflag}]");
                });

                parser.Run(args);
            });
        }
    }
}
