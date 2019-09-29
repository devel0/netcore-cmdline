# SearchAThing.CmdlineParser.Create method
## Create(string, Action<SearchAThing.CmdlineParser>, bool, bool)
create main parser

### Signature
```csharp
public static SearchAThing.CmdlineParser Create(string description, Action<SearchAThing.CmdlineParser> builder, bool useColors = True, bool unescapeArguments = False)
```
### Parameters
- `description`: program description
- `builder`: action to configure and run the parser
- `useColors`: true to use colors
- `unescapeArguments`: true to unescape arguments ( eg. newlines in argument strings )

### Returns

### Remarks


<p>&nbsp;</p>
<p>&nbsp;</p>
<hr/>

## Create(string, Action<SearchAThing.CmdlineParser>, CmdlineColors, bool)
create main parser

### Signature
```csharp
public static SearchAThing.CmdlineParser Create(string description, Action<SearchAThing.CmdlineParser> builder, CmdlineColors colors, bool unescapeArguments)
```
### Parameters
- `description`: program description
- `builder`: action to configure and run the parser
- `colors`: custom color object or null to disable
- `unescapeArguments`: true to unescape arguments ( eg. newlines in argument strings )

### Returns

### Remarks

