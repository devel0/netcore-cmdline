using SearchAThing;

namespace example_01
{
    class Program
    {
        static void Main(string[] args)
        {
            CmdlineParser.Create("sample application", (parser) =>
            {
                var cmdConfig = parser.AddCommand("config", "configuration", (pConfig) =>
                {
                    var cmdConfigShow = pConfig.AddCommand("show", "show current config");
                    var cmdConfigUpdate = pConfig.AddCommand("update", "update config item", (pConfigUpdate) =>
                    {
                        var param = pConfigUpdate.AddMandatoryParameter("var=value", "assign value to var");
                        pConfigUpdate.OnCmdlineMatch(() =>
                        {
                            System.Console.WriteLine($"setting [{(string)param}]");
                        });
                    });

                    pConfig.OnCmdlineMatch(() =>
                    {
                        if (cmdConfigShow) System.Console.WriteLine($"showing configuration...");
                    });
                });                
                
                parser.AddShort("h", "show usage", null, (item) => item.MatchParser.PrintUsage());                

                parser.OnCmdlineMatch(() =>
                {
                });

                parser.Run(args);
            });
        }
    }
}