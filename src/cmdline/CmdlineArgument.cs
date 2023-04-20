namespace SearchAThing.Cmdline;

/// <summary>
/// Encloses command line argument string and relate to matching parse item if any
/// </summary>
public class CmdlineArgument
{

    /// <summary>
    /// cmdline argument string
    /// </summary>
    public string Argument { get; private set; }

    /// <summary>
    /// non null if a parser item matches
    /// </summary>
    internal CmdlineParseItem MatchedItem { get; set; }

    /// <summary>
    /// true if a parser item matches
    /// </summary>
    public bool Matched => MatchedItem != null;

    /// <summary>
    /// construct a cmdline argument from cmdline arg string
    /// </summary>
    public CmdlineArgument(string arg)
    {
        Argument = arg;
    }

}
