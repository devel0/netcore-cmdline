# SearchAThing.CmdlineParser.AddCommand method
## AddCommand(string, string, Action<SearchAThing.CmdlineParser>)
add a command item to this parser

### Signature
```csharp
public SearchAThing.CmdlineParseItem AddCommand(string name, string description, Action<SearchAThing.CmdlineParser> builder = null)
```
### Parameters
- `name`: name of the command
- `description`: description of the command ( for the usage )
- `builder`: an optional builder to create a subparser from this command

### Returns

### Remarks

