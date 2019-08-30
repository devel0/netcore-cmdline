using System;

namespace SearchAThing
{

    /// <summary>
    /// cmdline color configurator;
    /// action fields can be changed to customize if foreground, background should changed
    /// </summary>
    public class CmdlineColors
    {

        /// <summary>
        /// color for assembly friendly name in the usage line
        /// </summary>
        public Action FriendlyName = () =>
        {
            Console.ForegroundColor = ConsoleColor.White;
        };

        /// <summary>
        /// color for commands
        /// </summary>
        public Action Commands = () =>
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
        };

        /// <summary>
        /// color for flags
        /// </summary>
        public Action Flags = () =>
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
        };

        /// <summary>
        /// color for parameter
        /// </summary>
        public Action Parameter = () =>
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
        };

        /// <summary>
        /// color for parser description
        /// </summary>
        public Action Description = () =>
        {
            Console.ForegroundColor = ConsoleColor.White;
        };

        /// <summary>
        /// color for error reporting when parser doesn't matches
        /// </summary>
        public Action Error = () =>
        {
            Console.ForegroundColor = ConsoleColor.Red;
        };
    }

}
