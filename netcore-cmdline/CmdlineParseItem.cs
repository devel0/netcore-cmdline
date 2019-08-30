using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SearchAThing
{

    /// <summary>
    /// types for cmd line parser item
    /// </summary>
    public enum CmdlineParseItemType
    {
        /// <summary>
        /// command if present are mandatory to match one between commands at this parser level
        /// </summary>
        command,

        /// <summary>
        /// flag can be short, long or short/long and optional or mandatory; flag can require to have value association
        /// </summary>
        flag,

        /// <summary>
        /// parameter can be optional or mandatory and are discovered after commands, flags are parsed
        /// </summary>
        parameter,

        /// <summary>
        /// parameter array are strings at end of cmdline
        /// </summary>
        parameterArray
    }

    /// <summary>
    /// describe properties required for a command, a flag or a parameter on the syntax for the cmdline parser;
    /// enumerating returns the list of values if this is a flag with value or a parameter or a parameter array
    /// </summary>
    public class CmdlineParseItem : IEnumerable<string>
    {

        /// <summary>
        /// parser which this item belongs to
        /// </summary>
        public CmdlineParser Parser { get; private set; }

        /// <summary>
        /// type of this cmdline parser
        /// </summary>
        public CmdlineParseItemType Type { get; private set; }

        /// <summary>
        /// true if this is a command
        /// </summary>
        public bool IsCommand => Type == CmdlineParseItemType.command;

        /// <summary>
        /// true if this is a flag
        /// </summary>
        public bool IsFlag => Type == CmdlineParseItemType.flag;

        /// <summary>
        /// true if this is a parameter or array of parameters
        /// </summary>
        public bool IsParameterOrArray => Type == CmdlineParseItemType.parameter || Type == CmdlineParseItemType.parameterArray;

        /// <summary>
        /// true if this is a parameter
        /// </summary>
        public bool IsParameter => Type == CmdlineParseItemType.parameter;

        /// <summary>
        /// true is this is an array of parameters
        /// </summary>
        public bool IsParameterArray => Type == CmdlineParseItemType.parameterArray;

        /// <summary>
        /// stores actiont to execute if this global flag item matches
        /// </summary>
        public Action<CmdlineParseItem> GlobalFlagAction { get; private set; }

        /// <summary>
        /// states if this parse item is a global flag
        /// </summary>
        public bool IsGlobal => GlobalFlagAction != null;

        /// <summary>
        /// true if this item has a short name ( maybe a command, parameter or a flag with short name )
        /// </summary>
        public bool HasShortName => !string.IsNullOrWhiteSpace(ShortName);

        /// <summary>
        /// short name of this element
        /// </summary>
        public string ShortName { get; private set; }

        /// <summary>
        /// short flag final string ( without value if any )
        /// </summary>        
        public string ShortFlag => $"-{ShortName}";

        /// <summary>
        /// states if this element has a long name ( used only for flags )
        /// </summary>
        public bool HasLongName => !string.IsNullOrWhiteSpace(LongName);

        /// <summary>
        /// long name ( used only for flags )
        /// </summary>        
        public string LongName { get; private set; }

        /// <summary>
        /// states if this item have a long name ( used only for flags )
        /// </summary>        
        public string LongFlag => $"--{LongName}";

        /// <summary>
        /// short/long flag final string ( without value if any )
        /// </summary>        
        public string ShortLongFlag => $"{(HasShortName ? $"-{ShortFlag}" : "")}{((HasShortName && HasLongName) ? "," : "")}{(HasLongName ? $"--{LongFlag}" : "")}";

        /// <summary>
        /// states if this flag requires a value
        /// </summary>
        public bool HasValueName => !string.IsNullOrWhiteSpace(ValueName);

        /// <summary>
        /// symbolic name for the flag value to display in the usage
        /// </summary>        
        public string ValueName { get; private set; }

        /// <summary>
        /// description for this parse item to use in the usage or is parser description if this is a command
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// true if this item is mandatory
        /// </summary>
        public bool Mandatory { get; private set; }

        /// <summary>
        /// helper to sort items in groups with follow order Commands, Flags, Parameters, Parameters array
        /// </summary>
        public int SortOrder
        {
            get
            {
                switch (Type)
                {
                    case CmdlineParseItemType.command: return 0;
                    case CmdlineParseItemType.flag:
                        {
                            if (Mandatory) return 1; else return 2;
                        }
                    case CmdlineParseItemType.parameter: return 3;
                    case CmdlineParseItemType.parameterArray: return 4;
                    default: return 100;
                }
            }
        }

        /// <summary>
        /// custom function that user can set for parameter parser item type to define runtime completion rules
        /// </summary>
        internal Func<string, IEnumerable<string>> onCompletion = null;

        /// <summary>
        /// set a runtime completion function ( used for parameter item types )
        /// </summary>
        public void OnCompletion(Func<string, IEnumerable<string>> func)
        {
            onCompletion = func;
        }

        #region matches

        /// <summary>
        /// remove matches state and association to parser argument.
        /// used internally to re-evaluate global flags so that they can associate with sub parsers
        /// </summary>
        internal void Unmatch()
        {
            Matches = false;
            MatchParser = null;
            if (MatchArgument != null) MatchArgument.MatchedItem = null;
            MatchArgument = null;
        }

        /// <summary>
        /// associate this item to given parser and argument
        /// </summary>
        internal void Match(CmdlineParser parser, CmdlineArgument arg)
        {
            if (IsCommand) parser.Command = this;
            arg.MatchedItem = this;
            this.MatchParser = parser;
            this.Matches = true;
            this.MatchArgument = arg;
        }

        /// <summary>
        /// states if this flag have a matching with required usage
        /// </summary>
        public bool Matches { get; internal set; }

        /// <summary>
        /// used to distinguish sub parser for global flags
        /// </summary>        
        public CmdlineParser MatchParser { get; private set; }

        /// <summary>
        /// original argument with that this parse item matches
        /// </summary>
        public CmdlineArgument MatchArgument { get; private set; }

        /// <summary>
        /// helper to check if this parse item meet usage rules
        /// </summary>
        public static implicit operator bool(CmdlineParseItem item) => item.Matches;

        #endregion

        #region values        

        List<CmdlineArgument> argValues = new List<CmdlineArgument>();
        /// <summary>
        /// original cmdline argument associated as value to this item (may a flag with value or parameter)
        /// </summary>
        public IReadOnlyList<CmdlineArgument> ArgValues => argValues;

        List<string> values = new List<string>();
        /// <summary>
        /// values associated to this item (may a flag with value or parameter)
        /// </summary>
        public IReadOnlyList<string> Values => values;

        /// <summary>
        /// sets a list of cmdline arguments as values associated to this item
        /// </summary>
        internal void SetValues(IEnumerable<CmdlineArgument> valArgs)
        {
            argValues = valArgs.ToList();
            values = valArgs.Select(w => w.Argument).ToList();
        }

        /// <summary>
        /// set a single cmdline argument as value associated to this item
        /// </summary>
        internal void SetValue(CmdlineArgument valArg)
        {
            argValues = new List<CmdlineArgument>() { valArg };
            values = new List<string>() { valArg.Argument };
        }

        /// <summary>
        /// used when flag value argument mixes flag name and value
        /// </summary>
        internal void SetValue(CmdlineArgument valArg, string overrideVal)
        {
            argValues = new List<CmdlineArgument>() { valArg };
            values = new List<string>() { overrideVal };
        }

        public IEnumerator<string> GetEnumerator() => Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// helper to retrieve this item value ( used for flag value or single parameter )
        /// </summary>
        public static implicit operator string(CmdlineParseItem item) => item.FirstOrDefault();

        #endregion

        /// <summary>
        /// construct a parse item
        /// </summary>
        /// <param name="parser">parser owner</param>
        /// <param name="type">type of parse item</param>
        /// <param name="shortName">short name (command,flag,parameter)</param>
        /// <param name="longName">long name (flag)</param>
        /// <param name="valueName">value name (flag with val)</param>
        /// <param name="description">description (any item)</param>
        /// <param name="mandatory">if true and not matches a message will reported</param>
        /// <param name="globalFlagAction">if non null sets this flag as a global that can matched independently that command and other mandatory items</param>
        internal CmdlineParseItem(CmdlineParser parser, CmdlineParseItemType type,
            string shortName, string longName, string valueName, string description, bool mandatory, Action<CmdlineParseItem> globalFlagAction)
        {
            Parser = parser;
            Type = type;
            ShortName = shortName;
            LongName = longName;
            ValueName = valueName;
            Description = description;
            Mandatory = mandatory;
            GlobalFlagAction = globalFlagAction;
        }

    }

}