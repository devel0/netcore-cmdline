using SearchAThing;
using System.Linq;

namespace example_01
{
    class Program
    {
        static void Main(string[] args)
        {
            CmdlineParser.Create("sample application", (parser) =>
            {
                var cmd1 = parser.AddCommand("cmd1", "sample command 1");
                var cmd2 = parser.AddCommand("cmd2", "sample command 2");

                var flag1 = parser.AddShortLong("f1", "test-flag1", "sample flag 1");
                var flag2 = parser.AddShortLong("f2", "test-flag2", "sample flag 2");
                var flag3 = parser.AddShortLong("f3", "my-flag3", "sample flag 3");

                parser.AddShort("h", "show usage", null, (item) => item.MatchParser.PrintUsage());

                var param = parser.AddParameter("item", "an odd number between 51 and 63");
                param.OnCompletion((str) =>
                {
                    var validSet = Enumerable.Range(50, 15).Where(r => r % 2 != 0).Select(w => w.ToString());

                    return validSet.Where(r => r.StartsWith(str));
                });

                parser.OnCmdlineMatch(() =>
                {
                });

                parser.Run(args);
            });
        }
    }
}