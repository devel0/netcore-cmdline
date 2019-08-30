using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SearchAThing
{

    public enum CmdlineParseItemType
    {
        command,
        flag,
        parameter,
        parameterArray
    }

    public class CmdlineParseItem : IEnumerable<string>
    {

        public CmdlineParser Parser { get; private set; }

        public CmdlineParseItemType Type { get; private set; }
        public bool IsCommand => Type == CmdlineParseItemType.command;
        public bool IsFlag => Type == CmdlineParseItemType.flag;
        public bool IsParameterOrArray => Type == CmdlineParseItemType.parameter || Type == CmdlineParseItemType.parameterArray;
        public bool IsParameter => Type == CmdlineParseItemType.parameter;
        public bool IsParameterArray => Type == CmdlineParseItemType.parameterArray;

        public Action<CmdlineParseItem> GlobalFlagAction { get; private set; }
        public bool IsGlobal => GlobalFlagAction != null;

        public bool HasShortName => !string.IsNullOrWhiteSpace(ShortName);
        public string ShortName { get; private set; }
        public string ShortFlag => $"-{ShortName}";

        public bool HasLongName => !string.IsNullOrWhiteSpace(LongName);
        public string LongName { get; private set; }
        public string LongFlag => $"--{LongName}";

        public string ShortLongFlag => $"{(HasShortName ? ShortFlag : "")}{((HasShortName && HasLongName) ? "," : "")}{(HasLongName ? LongFlag : "")}";

        public bool HasValueName => !string.IsNullOrWhiteSpace(ValueName);
        public string ValueName { get; private set; }

        public string Description { get; private set; }

        public bool Mandatory { get; private set; }

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

        internal Func<string, IEnumerable<string>> onCompletion = null;

        public void OnCompletion(Func<string, IEnumerable<string>> func)
        {
            onCompletion = func;
        }

        #region matches

        internal void Unmatch()
        {
            Matches = false;
            MatchParser = null;
            if (MatchArgument != null) MatchArgument.MatchedItem = null;
            MatchArgument = null;
        }

        internal void Match(CmdlineParser parser, CmdlineArgument arg)
        {
            if (IsCommand) parser.Command = this;
            arg.MatchedItem = this;
            this.MatchParser = parser;
            this.Matches = true;
            this.MatchArgument = arg;            
        }

        public bool Matches { get; internal set; }

        /// <summary>
        /// used to distinguish sub parser for global flags
        /// </summary>        
        public CmdlineParser MatchParser { get; private set; }

        public CmdlineArgument MatchArgument { get; private set; }

        public static implicit operator bool(CmdlineParseItem item) => item.Matches;

        #endregion

        #region values        

        List<CmdlineArgument> argValues = new List<CmdlineArgument>();
        public IReadOnlyList<CmdlineArgument> ArgValues => argValues;

        List<string> values = new List<string>();
        public IReadOnlyList<string> Values => values;

        internal void SetValues(IEnumerable<CmdlineArgument> valArgs)
        {
            argValues = valArgs.ToList();
            values = valArgs.Select(w => w.Argument).ToList();
        }

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

        public static implicit operator string(CmdlineParseItem item) => item.FirstOrDefault();

        #endregion        

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