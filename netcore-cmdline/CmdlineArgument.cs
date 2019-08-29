namespace SearchAThing
{

    public class CmdlineArgument
    {

        public string Argument { get; private set; }

        internal CmdlineParseItem MatchedItem { get; set; }
        
        public CmdlineArgument(string arg)
        {
            Argument = arg;
        }

    }

}