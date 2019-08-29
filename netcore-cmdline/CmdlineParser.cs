using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SearchAThing
{

    public class CmdlineParser
    {

        CmdlineParser _rootParser;
        public CmdlineParser RootParser
        {
            get
            {
                if (_rootParser == null)
                {
                    _rootParser = Parent == null ? this : ParentParsers.First();
                }
                return _rootParser;
            }
        }

        public CmdlineParser Parent { get; private set; }

        public IEnumerable<CmdlineParser> ParentParsers
        {
            get
            {
                var path = new List<CmdlineParser>();

                var p = Parent;

                while (p != null)
                {
                    path.Add(p);
                    p = p.Parent;
                }

                path.Reverse();

                return path;
            }
        }

        /// <summary>
        /// command that activate this parser
        /// </summary>
        public CmdlineParseItem Command { get; internal set; }

        /// <summary>
        /// list of parser from topmost to this one
        /// </summary>        
        public IEnumerable<CmdlineParser> ParserPath => ParentParsers.Union(new[] { this });

        string _Description;

        public string Description => _Description != null ? _Description : Command.Description;

        public string FriendlyName => AppDomain.CurrentDomain.FriendlyName;

        public string AppVersion
        {
            get
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fvi.FileVersion;
            }
        }

        #region colors        
        CmdlineColors _Colors;
        public CmdlineColors Colors
        {
            get
            {
                return RootParser._Colors;
            }
        }

        internal void FriendlyNameColor()
        {
            if (Colors != null) Colors.FriendlyName();
        }

        internal void CommandColor()
        {
            if (Colors != null) Colors.Commands();
        }

        internal void ParameterColor()
        {
            if (Colors != null) Colors.Parameter();
        }

        internal void FlagsColor()
        {
            if (Colors != null) Colors.Flags();
        }

        internal void DescriptionColor()
        {
            if (Colors != null) Colors.Description();
        }

        internal void ErrorColor()
        {
            if (Colors != null) Colors.Error();
        }

        internal void ResetColors()
        {
            if (Colors != null) Console.ResetColor();
        }
        #endregion

        /// <summary>
        /// this (sub)parser items
        /// </summary>
        public IReadOnlyList<CmdlineParseItem> Items => items;
        public IEnumerable<CmdlineParseItem> InheritedItems => ParentParsers.SelectMany(w => w.items);
        public IEnumerable<CmdlineParseItem> AllItems => InheritedItems.Union(Items);

        public IEnumerable<CmdlineParseItem> Commands => Items.Where(r => r.IsCommand);
        public IEnumerable<CmdlineParseItem> Flags => Items.Where(r => r.IsFlag);
        public IEnumerable<CmdlineParseItem> InheritedFlags => ParentParsers.SelectMany(w => w.Flags);
        /// <summary>
        /// inherited and this parser flags
        /// </summary>        
        public IEnumerable<CmdlineParseItem> AllFlags => InheritedFlags.Union(Flags);
        public IEnumerable<CmdlineParseItem> GlobalFlags => AllFlags.Where(r => r.GlobalFlagAction != null);
        public IEnumerable<CmdlineParseItem> Parameters => Items.Where(r => r.IsParameter);
        public IEnumerable<CmdlineParseItem> ParameterArrays => Items.Where(r => r.IsParameterArray);
        public IEnumerable<CmdlineParseItem> ParametersOrArray => Items.Where(r => r.IsParameterOrArray);

        List<CmdlineParseItem> items = new List<CmdlineParseItem>();

        Action onCmdlineMatch = null;

        CmdlineParser(CmdlineParser parent, string description = null)
        {
            Parent = parent;
            _Description = description;
        }

        public static CmdlineParser Create(string description, Action<CmdlineParser> builder, bool useColors = true)
        {
            if (useColors)
                return Create(description, builder, new CmdlineColors());
            else
                return Create(description, builder, null);
        }

        public static CmdlineParser Create(string description, Action<CmdlineParser> builder, CmdlineColors colors)
        {
            var parser = new CmdlineParser(null, description);
            parser._Colors = colors;

            builder(parser);

            return parser;
        }

        /// <summary>
        /// set action to execute when this parser cmdline matches
        /// </summary>        
        public void OnCmdlineMatch(Action action)
        {
            onCmdlineMatch = action;
        }

        public void Run(string[] args)
        {
            InternalRun(args.Select(w => new CmdlineArgument(w)).ToList());
        }

        void PrintCompletions(IEnumerable<string> values)
        {
            foreach (var x in values) System.Console.WriteLine(x);
        }

        void InternalRun(List<CmdlineArgument> args)
        {
            var showCompletion = Environment.GetEnvironmentVariable("SHOW_COMPLETIONS").Eval((e) => e != null && e == "1");

            CmdlineParseItem cmdToRun = null;

            var missingCommand = false;

            #region commands
            if (Commands.Any())
            {
                int argIdx = args.Count(w => w.MatchedItem != null);

                if (argIdx < args.Count)
                {
                    var arg = args[argIdx];

                    var qcmd = Commands.FirstOrDefault(w => w.ShortName == arg.Argument);
                    // if not found valid command checks global flags
                    if (qcmd == null)
                    {
                        if (showCompletion)
                        {
                            PrintCompletions(Commands.Select(w => w.ShortName).Where(r => r.StartsWith(arg.Argument)));
                        }
                        else
                        {
                            missingCommand = true;
                        }
                    }
                    else
                    {
                        qcmd.Match(this, arg);
                        cmdToRun = qcmd;
                    }
                }
                else
                {
                    if (showCompletion)
                    {
                        PrintCompletions(Commands.Select(w => w.ShortName));
                    }
                    else
                    {
                        missingCommand = true;
                    }
                }
            }
            #endregion

            #region flags
            if (AllFlags.Any())
            {
                var argIdxBegin = args.Count(w => w.MatchedItem != null);

                for (var argIdx = argIdxBegin; argIdx < args.Count; ++argIdx)
                {
                    var arg = args[argIdx];

                    var availFlags = AllFlags.Where(r => !r.Matches);
                    if (!availFlags.Any()) break; // all flags consumed

                    CmdlineParseItem qFlag = null;
                    var consumeNextArgAsValue = false;
                    foreach (var flag in availFlags)
                    {
                        #region short flag
                        if (flag.HasShortName)
                        {
                            if (flag.HasValueName)
                            {
                                if (arg.Argument == flag.ShortFlag)
                                {
                                    if (argIdx < args.Count - 1)
                                    {
                                        consumeNextArgAsValue = true;
                                        var valArg = args[argIdx + 1];
                                        valArg.MatchedItem = flag;
                                        flag.SetValue(valArg);
                                        qFlag = flag;
                                        break;
                                    }
                                }
                                else if (arg.Argument.StartsWith($"{flag.ShortFlag}="))
                                {
                                    flag.SetValue(arg, arg.Argument.Substring($"{flag.ShortFlag}=".Length));
                                    qFlag = flag;
                                    break;
                                }
                                else if (flag.ShortFlag.StartsWith(arg.Argument))
                                {
                                    PrintCompletions(new[] { $"{flag.ShortFlag}=" });
                                }
                            }
                            else
                            {
                                if (arg.Argument == flag.ShortFlag)
                                {
                                    qFlag = flag;
                                    break;
                                }
                                else if (flag.ShortFlag.StartsWith(arg.Argument))
                                {
                                    PrintCompletions(new[] { flag.ShortFlag });
                                }
                            }
                        }
                        #endregion

                        #region long flag
                        if (flag.HasLongName)
                        {
                            if (flag.HasValueName)
                            {
                                if (arg.Argument == flag.LongFlag)
                                {
                                    if (argIdx < args.Count - 1)
                                    {
                                        consumeNextArgAsValue = true;
                                        var valArg = args[argIdx + 1];
                                        valArg.MatchedItem = flag;
                                        flag.SetValue(valArg);
                                        qFlag = flag;
                                        break;
                                    }
                                }
                                else if (arg.Argument.StartsWith($"{flag.LongFlag}="))
                                {
                                    flag.SetValue(arg, arg.Argument.Substring($"{flag.LongFlag}=".Length));
                                    qFlag = flag;
                                    break;
                                }
                                else if (flag.LongFlag.StartsWith(arg.Argument))
                                {
                                    PrintCompletions(new[] { $"{flag.LongFlag}=" });
                                }
                            }
                            else
                            {
                                if (arg.Argument == flag.LongFlag)
                                {
                                    qFlag = flag;
                                    break;
                                }
                                else if (flag.LongFlag.StartsWith(arg.Argument))
                                {
                                    PrintCompletions(new[] { flag.LongFlag });
                                }
                            }
                        }
                        #endregion
                    }

                    if (qFlag != null)
                    {
                        qFlag.Match(this, arg);
                        if (consumeNextArgAsValue) { ++argIdx; continue; }
                    }
                }

                if (!showCompletion)
                {
                    var qMandatoryMissing = Flags.FirstOrDefault(r => r.Mandatory && !r.Matches);
                    if (qMandatoryMissing != null)
                    {
                        ErrorColor();
                        System.Console.WriteLine($"missing mandatory flag [{qMandatoryMissing.ShortLongFlag}]");
                        ResetColors();
                    }
                }
            }
            #endregion

            if (!showCompletion)
            {
                var qglobal = AllFlags.Where(r => r.IsGlobal && r.Matches).ToList();

                if (qglobal.Count == 0 && missingCommand)
                {
                    ErrorColor();
                    System.Console.WriteLine($"must specify command");
                    ResetColors();
                    PrintUsage();
                    return;
                }

                if (cmdToRun != null)
                {
                    foreach (var x in qglobal)
                    {
                        x.Unmatch();
                    }

                    cmdToRun.Parser.InternalRun(args);
                }
                if (cmdToRun == null && qglobal.Count > 0)
                {
                    foreach (var x in qglobal) x.GlobalFlagAction(x);
                    return;
                }
                if (onCmdlineMatch != null) onCmdlineMatch();
            }
        }

        #region print usage
        public void PrintUsage()
        {
            new CmdlineUsage(this).Print();
        }
        #endregion

        #region add command

        public CmdlineParseItem AddCommand(string name, string description, Action<CmdlineParser> builder = null)
        {
            var parser = new CmdlineParser(this);

            var cmd = new CmdlineParseItem(parser, CmdlineParseItemType.command,
                name, null, null, description, mandatory: false, globalFlagAction: null);

            parser.Command = cmd;

            items.Add(cmd);

            if (builder != null) builder(parser);

            return cmd;
        }

        #endregion

        #region add flag

        /// <summary>
        /// shortName or longName and valueName can be null
        /// </summary>        
        CmdlineParseItem AddFlag(string shortName, string longName, string description, string valueName, bool mandatory, Action<CmdlineParseItem> globalFlagAction)
        {
            var item = new CmdlineParseItem(this, CmdlineParseItemType.flag,
                shortName, longName, valueName, description, mandatory, globalFlagAction);

            items.Add(item);

            return item;
        }

        #region add optional flag helpers

        /// <summary>
        /// add optional short flag
        /// </summary>
        public CmdlineParseItem AddShort(string name, string description, string valueName = null, Action<CmdlineParseItem> globalFlagAction = null) =>
            AddFlag(name, null, description, valueName, mandatory: false, globalFlagAction);

        /// <summary>
        /// add optional long flag
        /// </summary>
        public CmdlineParseItem AddLong(string name, string description, string valueName = null, Action<CmdlineParseItem> globalFlagAction = null) =>
            AddFlag(null, name, description, valueName, mandatory: false, globalFlagAction);

        /// <summary>
        /// add optional short/long flag
        /// </summary>
        public CmdlineParseItem AddShortLong(string shortName, string longName, string description, string valueName = null, Action<CmdlineParseItem> globalFlagAction = null) =>
            AddFlag(shortName, longName, description, valueName, mandatory: false, globalFlagAction);

        #endregion

        #region add mandatory flag helpers

        /// <summary>
        /// add optional short flag
        /// </summary>
        public CmdlineParseItem AddMandatoryShort(string name, string description, string valueName = null) =>
            AddFlag(name, null, description, valueName, mandatory: true, globalFlagAction: null);

        /// <summary>
        /// add optional long flag
        /// </summary>
        public CmdlineParseItem AddMandatoryLong(string name, string description, string valueName = null) =>
            AddFlag(null, name, description, valueName, mandatory: true, globalFlagAction: null);

        /// <summary>
        /// add optional short/long flag
        /// </summary>
        public CmdlineParseItem AddMandatoryShortLong(string shortName, string longName, string description, string valueName = null) =>
            AddFlag(shortName, longName, description, valueName, mandatory: true, globalFlagAction: null);

        #endregion

        #endregion

        #region add parameter
        CmdlineParseItem AddParameter(string name, string description, bool mandatory)
        {
            var item = new CmdlineParseItem(this, CmdlineParseItemType.parameter,
                name, null, null, description, mandatory: mandatory, globalFlagAction: null);

            items.Add(item);

            return item;
        }

        public CmdlineParseItem AddParameter(string name, string description) => AddParameter(name, description, mandatory: false);
        public CmdlineParseItem AddMandatoryParameter(string name, string description) => AddParameter(name, description, mandatory: true);

        #endregion

        #region add parameter array

        CmdlineParseItem AddParameterArray(string name, string description, bool mandatory)
        {
            var item = new CmdlineParseItem(this, CmdlineParseItemType.parameterArray,
                name, null, null, description, mandatory: mandatory, globalFlagAction: null);

            items.Add(item);

            return item;
        }

        public CmdlineParseItem AddParameterArray(string name, string description) => AddParameterArray(name, description, mandatory: false);
        public CmdlineParseItem AddMandatoryParameterArray(string name, string description) => AddParameterArray(name, description, mandatory: true);

        #endregion

        public override string ToString()
        {
            var rows = new List<List<string>>();

            foreach (var x in AllItems.OrderBy(w => w.SortOrder))
            {
                rows.Add(new List<string>()
                {
                    x.Type.ToString(),
                    x.ShortName,
                    x.LongName,
                    x.Description,
                    x.GlobalFlagAction!=null ? "X" : "",
                    x.Mandatory ? "X" : "",
                    x.Matches ? "X" : "",
                    x.Type == CmdlineParseItemType.parameterArray ? $"[ {string.Join(",", x.ArgValues.Select(h=>$"\"{h}\""))} ]" : (string)x
                });
            }

            var left = ColumnAlignment.left;
            var center = ColumnAlignment.center;

            return rows.TableFormat(
                new[] { "TYPE", "SHORT-NAME", "LONG-NAME", "DESCRIPTION", "GLOBAL", "MANDATORY", "MATCHES", "VALUE" },
                new[] { left, left, left, left, center, center, center, left });
        }

    }

}