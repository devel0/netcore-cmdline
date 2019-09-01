using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SearchAThing
{

    /// <summary>
    /// cmdline parser tool
    /// </summary>
    public class CmdlineParser
    {

        CmdlineParser _rootParser;
        /// <summary>
        /// topmost parser
        /// </summary>        
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

        /// <summary>
        /// parent parset ( null for topmost parser )
        /// </summary>
        public CmdlineParser Parent { get; private set; }

        /// <summary>
        /// parent parsers enum
        /// </summary>
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

        /// <summary>
        /// description of this parser ( automatically retrieved from command if this is a subparser )
        /// </summary>
        public string Description => _Description != null ? _Description : Command.Description;

        /// <summary>
        /// assembly friendly name ( used for Usage )
        /// </summary>
        public string FriendlyName => AppDomain.CurrentDomain.FriendlyName;

        /// <summary>
        /// app version utility
        /// </summary>
        public string AppVersion
        {
            get
            {
                var assembly = System.Reflection.Assembly.GetCallingAssembly();
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fvi.FileVersion;
            }
        }

        #region colors        
        CmdlineColors _Colors;
        /// <summary>
        /// colors set ( this can be changed from the Create method )
        /// </summary>        
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

        /// <summary>
        /// only parents items
        /// </summary>        
        public IEnumerable<CmdlineParseItem> InheritedItems => ParentParsers.SelectMany(w => w.items);

        /// <summary>
        /// parent and this items
        /// </summary>    
        public IEnumerable<CmdlineParseItem> AllItems => InheritedItems.Union(Items);

        /// <summary>
        /// this parser commands
        /// </summary>
        public IEnumerable<CmdlineParseItem> Commands => Items.Where(r => r.IsCommand);

        /// <summary>
        /// this parser flags
        /// </summary>        
        public IEnumerable<CmdlineParseItem> Flags => Items.Where(r => r.IsFlag);

        /// <summary>
        /// only parent parsers flags
        /// </summary>        
        public IEnumerable<CmdlineParseItem> InheritedFlags => ParentParsers.SelectMany(w => w.Flags);

        /// <summary>
        /// inherited and this parser flags
        /// </summary>        
        public IEnumerable<CmdlineParseItem> AllFlags => InheritedFlags.Union(Flags);

        /// <summary>
        /// global flags
        /// </summary>        
        public IEnumerable<CmdlineParseItem> GlobalFlags => AllFlags.Where(r => r.GlobalFlagAction != null);

        /// <summary>
        /// parameters
        /// </summary>        
        public IEnumerable<CmdlineParseItem> Parameters => Items.Where(r => r.IsParameter);

        /// <summary>
        /// parameter with array mode
        /// </summary>        
        public IEnumerable<CmdlineParseItem> ParameterArrays => Items.Where(r => r.IsParameterArray);

        /// <summary>
        /// all parameters single or array mode
        /// </summary>        
        public IEnumerable<CmdlineParseItem> ParametersOrArray => Items.Where(r => r.IsParameterOrArray);

        List<CmdlineParseItem> items = new List<CmdlineParseItem>();

        Action onCmdlineMatch = null;

        CmdlineParser(CmdlineParser parent, string description = null)
        {
            Parent = parent;
            _Description = description;
        }

        /// <summary>
        /// create main parser
        /// </summary>
        /// <param name="description">program description</param>
        /// <param name="builder">action to configure and run the parser</param>
        /// <param name="useColors">true to use colors</param>
        public static CmdlineParser Create(string description, Action<CmdlineParser> builder, bool useColors = true)
        {
            if (useColors)
                return Create(description, builder, new CmdlineColors());
            else
                return Create(description, builder, null);
        }

        /// <summary>
        /// create main parser
        /// </summary>
        /// <param name="description">program description</param>
        /// <param name="builder">action to configure and run the parser</param>
        /// <param name="colors">custom color object or null to disable</param>
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

        bool showCompletion;

        /// <summary>
        /// execute the parser ( must called once from top parser builder )
        /// </summary>
        public void Run(string[] args)
        {
            var skipArgs = 0;
            // SHOW_COMPLETIONS=1 ( from bash completion will include program name, so we skip first arg )
            // SHOW_COMPLETIONS=2 ( from debug cmdline so don't skip because first arg is first arg )
            var showCompletionEnv = Environment.GetEnvironmentVariable("SHOW_COMPLETIONS");
            if (showCompletionEnv != null)
            {
                if (showCompletionEnv == "1") skipArgs = 1;
            }

            var cmdlineMatches = InternalRun(args.Select(w => new CmdlineArgument(w)).Skip(skipArgs).ToList());

            foreach (var x in cmdlineMatches)
            {
                x();
            }
        }

        void PrintCompletions(IEnumerable<string> values)
        {
            foreach (var x in values) System.Console.WriteLine(x);
        }

        IEnumerable<Action> InternalRun(List<CmdlineArgument> args)
        {
            showCompletion = Environment.GetEnvironmentVariable("SHOW_COMPLETIONS").Eval((e) => e != null && (e == "1" || e == "2"));

            CmdlineParseItem cmdToRun = null;

            var missingCommand = false;
            var missingFlag = false;

            #region flags
            if (AllFlags.Any())
            {
                foreach (var (arg, argIdx) in args.WithIndex())
                {
                    if (arg.Matched) continue;

                    var availFlags = AllFlags.Where(r => !r.Matches);
                    if (!availFlags.Any()) break; // all flags consumed

                    CmdlineParseItem qFlag = null;

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
                        if (!qFlag.GlobalFlagActionNested && qFlag.GlobalFlagAction != null)
                        {
                            qFlag.GlobalFlagAction(qFlag);
                            qFlag.GlobalFlagActionExecuted = true;
                        }
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
                        PrintUsage();
                        missingFlag = true;
                    }
                }
            }
            #endregion

            #region commands
            if (Commands.Any())
            {
                var arg = args.FirstOrDefault(w => !w.Matched);

                if (arg != null)
                {
                    var qcmd = Commands.FirstOrDefault(w => w.ShortName == arg.Argument);
                    // if not found valid command checks global flags
                    if (qcmd == null)
                    {
                        missingCommand = true;

                        if (showCompletion)
                        {
                            PrintCompletions(Commands.Select(w => w.ShortName).Where(r => r.StartsWith(arg.Argument) && r != arg.Argument));
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
                    missingCommand = true;

                    if (showCompletion)
                    {
                        PrintCompletions(Commands.Select(w => w.ShortName));
                    }
                }
            }
            #endregion

            CmdlineParseItem missingParameter = null;

            #region parameters
            if (ParametersOrArray.Any())
            {
                foreach (var param in Parameters)
                {
                    var arg = args.FirstOrDefault(r => !r.Matched);

                    if (arg == null)
                    {
                        if (showCompletion)
                        {
                            if (param.onCompletion != null && !missingCommand) PrintCompletions(param.onCompletion(""));
                        }
                        else
                        {
                            if (param.Mandatory)
                                missingParameter = param;
                            break;
                        }
                    }
                    else
                    {
                        var skipCompletion = !showCompletion;

                        if (showCompletion)
                        {
                            if (param.onCompletion != null && !missingCommand)
                            {
                                var completions = param.onCompletion(arg.Argument).Where(r => r != arg.Argument);
                                if (completions.Count() > 0)
                                    PrintCompletions(completions);
                                else
                                    skipCompletion = true;
                            }
                        }

                        if (skipCompletion)
                        {
                            param.Match(this, arg);
                            param.SetValue(arg);
                        }
                    }
                }

                var parr = ParameterArrays.FirstOrDefault();

                if (parr != null)
                {
                    var parrArgs = new List<CmdlineArgument>();
                    while (true)
                    {
                        var arg = args.FirstOrDefault(r => !r.Matched);
                        if (arg == null)
                        {
                            if (showCompletion)
                            {
                                if (parr.onCompletion != null && !missingCommand)
                                    PrintCompletions(parr.onCompletion(""));
                            }
                            else
                            {
                                if (parr.Mandatory && parrArgs.Count == 0)
                                    missingParameter = parr;
                            }
                            break;
                        }
                        else
                        {
                            var skipCompletion = !showCompletion;

                            if (showCompletion)
                            {
                                if (parr.onCompletion != null && !missingCommand)
                                {
                                    var completions = parr.onCompletion(arg.Argument).Where(r => r != arg.Argument);
                                    if (completions.Count() > 0)
                                    {
                                        PrintCompletions(completions);
                                        break;
                                    }
                                    else
                                        skipCompletion = true;
                                }
                            }

                            if (skipCompletion)
                            {
                                parr.Match(this, arg);
                                parrArgs.Add(arg);
                            }
                        }
                    }
                    parr.SetValues(parrArgs);
                }
            }
            #endregion

            var qglobal = AllFlags.Where(r => r.IsGlobal && r.GlobalFlagActionNested && r.Matches).ToList();

            if (!showCompletion && qglobal.Count == 0 && missingCommand)
            {
                ErrorColor();
                System.Console.WriteLine($"missing command");
                ResetColors();
                PrintUsage();
                yield break;
            }

            if (!showCompletion && missingParameter != null)
            {
                ErrorColor();
                System.Console.WriteLine($"missing required parameter [{missingParameter.ShortName}]");
                ResetColors();
                PrintUsage();
                yield break;
            }

            if (!showCompletion && onCmdlineMatch != null && qglobal.Count == 0 && !missingFlag) yield return onCmdlineMatch;

            if (cmdToRun != null)
            {
                var qGlobalToremove = new List<CmdlineParseItem>();
                foreach (var x in qglobal)
                {
                    if (!x.GlobalFlagActionNested)
                    {
                        qGlobalToremove.Add(x);
                    }
                    else
                        x.Unmatch();
                }
                foreach (var x in qGlobalToremove) qglobal.Remove(x);

                if (!missingFlag)
                {
                    foreach (var x in cmdToRun.Parser.InternalRun(args))
                    {
                        yield return x;
                    }
                }
                yield break;
            }
            if (cmdToRun == null && qglobal.Count > 0)
            {
                foreach (var x in qglobal.Where(r => !r.GlobalFlagActionExecuted)) x.GlobalFlagAction(x);
                yield break;
            }
        }

        #region print usage
        /// <summary>
        /// print the usage based on current parser configuration
        /// </summary>
        public void PrintUsage()
        {
            new CmdlineUsage(this).Print();
        }
        #endregion

        #region add command
        /// <summary>
        /// add a command item to this parser
        /// </summary>
        /// <param name="name">name of the command</param>
        /// <param name="description">description of the command ( for the usage )</param>
        /// <param name="builder">an optional builder to create a subparser from this command</param>        
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
        CmdlineParseItem AddFlag(string shortName, string longName, string description, string valueName, bool mandatory, Action<CmdlineParseItem> globalFlagAction, bool globalFlagActionNested)
        {
            var item = new CmdlineParseItem(this, CmdlineParseItemType.flag,
                shortName, longName, valueName, description, mandatory, globalFlagAction, globalFlagActionNested);

            items.Add(item);

            return item;
        }

        #region add optional flag helpers

        /// <summary>
        /// add optional short flag
        /// </summary>
        public CmdlineParseItem AddShort(string name, string description, string valueName = null, Action<CmdlineParseItem> globalFlagAction = null, bool globalFlagActionNested = true) =>
            AddFlag(name, null, description, valueName, mandatory: false, globalFlagAction, globalFlagActionNested);

        /// <summary>
        /// add optional long flag
        /// </summary>
        public CmdlineParseItem AddLong(string name, string description, string valueName = null, Action<CmdlineParseItem> globalFlagAction = null, bool globalFlagActionNested = true) =>
            AddFlag(null, name, description, valueName, mandatory: false, globalFlagAction, globalFlagActionNested);

        /// <summary>
        /// add optional short/long flag
        /// </summary>
        public CmdlineParseItem AddShortLong(string shortName, string longName, string description, string valueName = null, Action<CmdlineParseItem> globalFlagAction = null, bool globalFlagActionNested = true) =>
            AddFlag(shortName, longName, description, valueName, mandatory: false, globalFlagAction, globalFlagActionNested);

        #endregion

        #region add mandatory flag helpers

        /// <summary>
        /// add mandatory short flag
        /// </summary>
        public CmdlineParseItem AddMandatoryShort(string name, string description, string valueName = null) =>
            AddFlag(name, null, description, valueName, mandatory: true, globalFlagAction: null, globalFlagActionNested: true);

        /// <summary>
        /// add mandatory long flag
        /// </summary>
        public CmdlineParseItem AddMandatoryLong(string name, string description, string valueName = null) =>
            AddFlag(null, name, description, valueName, mandatory: true, globalFlagAction: null, globalFlagActionNested: true);

        /// <summary>
        /// add mandatory short/long flag
        /// </summary>
        public CmdlineParseItem AddMandatoryShortLong(string shortName, string longName, string description, string valueName = null) =>
            AddFlag(shortName, longName, description, valueName, mandatory: true, globalFlagAction: null, globalFlagActionNested: true);

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

        /// <summary>
        /// add optional parameter item to this parser
        /// </summary>
        /// <param name="name">name of this parameter ( used in Usage )</param>
        /// <param name="description">description of this parameter ( used in Usage )</param>        
        public CmdlineParseItem AddParameter(string name, string description) => AddParameter(name, description, mandatory: false);

        /// <summary>
        /// add mandatory parameter item to this parser
        /// </summary>
        /// <param name="name">name of this parameter ( used in Usage )</param>
        /// <param name="description">description of this parameter ( used in Usage )</param>    
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

        /// <summary>
        /// add optional parameter array item to this parser
        /// </summary>
        /// <param name="name">name of this parameter array ( used in Usage )</param>
        /// <param name="description">description of this parameter array ( used in Usage )</param>        
        public CmdlineParseItem AddParameterArray(string name, string description) => AddParameterArray(name, description, mandatory: false);

        /// <summary>
        /// add mandatory parameter array item to this parser
        /// </summary>
        /// <param name="name">name of this parameter array ( used in Usage )</param>
        /// <param name="description">description of this parameter array ( used in Usage )</param>    
        public CmdlineParseItem AddMandatoryParameterArray(string name, string description) => AddParameterArray(name, description, mandatory: true);

        #endregion

        /// <summary>
        /// build a table with all parser item details, matches and parsed values ( for debug purpose )
        /// </summary>
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
                    x.Type == CmdlineParseItemType.parameterArray ?
                        ((x.ArgValues.Count>0) ? $"[ {string.Join(",", x.ArgValues.Select(h=>$"\"{h.Argument}\""))} ]" : "") :
                        (string)x
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