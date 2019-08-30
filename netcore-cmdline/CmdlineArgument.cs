namespace SearchAThing
{

    public class CmdlineArgument
    {

        public string Argument { get; private set; }

        internal CmdlineParseItem MatchedItem { get; set; }

        public bool Matched => MatchedItem != null;

        public CmdlineArgument(string arg)
        {
            Argument = arg;
        }

    }

}