using System;

namespace SearchAThing
{

    public class CmdlineColors
    {
        public Action FriendlyName = () =>
        {
            Console.ForegroundColor = ConsoleColor.White;
        };        

        public Action Commands = () =>
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
        };        

        public Action Flags = () =>
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
        };        

        public Action Parameter = () =>
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
        };

        public Action Description = () =>
        {
            Console.ForegroundColor = ConsoleColor.White;            
        };

        public Action Error = () =>
        {
            Console.ForegroundColor = ConsoleColor.Red;
        };
    }

}
