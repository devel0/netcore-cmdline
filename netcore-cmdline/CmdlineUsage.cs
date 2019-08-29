using System.Linq;
using System.Text;
using static System.Math;

namespace SearchAThing
{

    public class CmdlineUsage
    {

        #region colors        
        void FriendlyNameColor() => Parser.FriendlyNameColor();
        void CommandColor() => Parser.CommandColor();
        void ParameterColor() => Parser.ParameterColor();
        void FlagsColor() => Parser.FlagsColor();
        void DescriptionColor() => Parser.DescriptionColor();
        void ResetColors() => Parser.ResetColors();
        #endregion

        public CmdlineParser Parser { get; private set; }

        public CmdlineUsage(CmdlineParser parser)
        {
            Parser = parser;
        }

        int width = 0;

        void SweepFlags(bool onlyComputeWidth, bool inherited)
        {
            var flags = inherited ? Parser.InheritedFlags : Parser.Flags;

            foreach (var fg in flags.Where(w => w.GlobalFlagAction == null).GroupBy(w => w.Mandatory))
            {

                if (!onlyComputeWidth)
                {
                    if (inherited) System.Console.Write("(inherited) ");
                    if (fg.Key)
                    {
                        System.Console.WriteLine("Mandatory flags:");
                    }
                    else
                    {
                        System.Console.WriteLine("Optional flags:");
                    }
                }

                foreach (var f in fg)
                {
                    var sb = new StringBuilder();

                    sb.Append("  ");
                    if (f.HasShortName) sb.Append($"-{f.ShortName}");
                    if (f.HasLongName)
                    {
                        if (f.HasShortName) sb.Append(",");
                        sb.Append($"--{f.LongName}");
                    }
                    if (f.HasValueName) sb.Append($"={f.ValueName}");

                    if (onlyComputeWidth) width = Max(width, sb.Length);
                    else
                    {
                        FlagsColor();
                        System.Console.Write(sb.ToString().Align(width));
                        ResetColors();
                        System.Console.WriteLine($"   {f.Description}");
                    }
                }

                if (!onlyComputeWidth) System.Console.WriteLine();
            }

            if (inherited)
            {
                if (Parser.GlobalFlags.Any())
                {
                    if (!onlyComputeWidth)
                    {
                        System.Console.WriteLine("Global flags:");
                    }

                    foreach (var f in Parser.GlobalFlags)
                    {
                        var sb = new StringBuilder();

                        sb.Append("  ");
                        if (f.HasShortName) sb.Append($"-{f.ShortName}");
                        if (f.HasLongName)
                        {
                            if (f.HasShortName) sb.Append(",");
                            sb.Append($"--{f.LongName}");
                        }
                        if (f.HasValueName) sb.Append($"={f.ValueName}");

                        if (onlyComputeWidth) width = Max(width, sb.Length);
                        else
                        {
                            FlagsColor();
                            System.Console.Write(sb.ToString().Align(width));
                            ResetColors();
                            System.Console.WriteLine($"   {f.Description}");
                        }
                    }

                    if (!onlyComputeWidth) System.Console.WriteLine();
                }
            }
        }

        void SweepItems(bool onlyComputeWidth)
        {
            if (Parser.Commands.Any())
            {
                if (!onlyComputeWidth)
                {
                    System.Console.WriteLine("Commands:");
                }
                foreach (var cmd in Parser.Commands)
                {
                    var sb = new StringBuilder();

                    CommandColor();
                    sb.Append($"  {cmd.ShortName}");
                    ResetColors();

                    if (onlyComputeWidth)
                    {
                        width = Max(width, sb.Length);
                    }
                    else
                    {
                        CommandColor();
                        System.Console.Write(sb.ToString().Align(width));
                        ResetColors();
                        System.Console.WriteLine($"   {cmd.Description}");
                    }
                }
                if (!onlyComputeWidth) System.Console.WriteLine();
            }

            SweepFlags(onlyComputeWidth, inherited: false);
            SweepFlags(onlyComputeWidth, inherited: true);

            if (Parser.ParametersOrArray.Any())
            {
                if (!onlyComputeWidth)
                {
                    System.Console.WriteLine($"Parameters");
                }

                foreach (var param in Parser.ParametersOrArray)
                {
                    var sb = new StringBuilder();

                    sb.Append("  ");
                    sb.Append(param.ShortName);
                    if (param.Type == CmdlineParseItemType.parameterArray) sb.Append("...");

                    if (onlyComputeWidth)
                    {
                        width = Max(width, sb.Length);
                    }
                    else
                    {
                        ParameterColor();
                        System.Console.Write(sb.ToString().Align(width));
                        ResetColors();
                        System.Console.WriteLine($"   {param.Description}");
                    }
                }
            }
        }

        public void Print()
        {
            System.Console.WriteLine();
            {
                {
                    System.Console.Write($"Usage: ");

                    FriendlyNameColor();
                    System.Console.Write($"{Parser.FriendlyName}");

                    CommandColor();
                    foreach (var pp in Parser.ParentParsers)
                    {
                        System.Console.Write($" {pp.Command.ShortName}");
                    }
                    if (Parser.Commands.Any()) System.Console.Write(" COMMAND");

                    FlagsColor();
                    if (Parser.Flags.Any()) System.Console.Write(" FLAGS");

                    ParameterColor();
                    foreach (var param in Parser.Parameters)
                    {
                        var optBegin = param.Mandatory ? "" : "[";
                        var optEnd = param.Mandatory ? "" : "]";
                        System.Console.Write($" {optBegin}{param.ShortName}{optEnd}");
                    }
                    if (Parser.ParameterArrays.Any())
                    {
                        var param = Parser.ParameterArrays.First();

                        var optBegin = param.Mandatory ? "" : "[";
                        var optEnd = param.Mandatory ? "" : "]";
                        System.Console.Write($" {optBegin}{param.ShortName}{optEnd}...");
                    }
                    ResetColors();

                    System.Console.WriteLine();
                }

                System.Console.WriteLine();

                DescriptionColor();
                System.Console.Write($"{Parser.Description}");
                ResetColors();
                System.Console.WriteLine();

                System.Console.WriteLine();

                SweepItems(onlyComputeWidth: true);
                SweepItems(onlyComputeWidth: false);
            }
            System.Console.WriteLine();
        }

    }

}