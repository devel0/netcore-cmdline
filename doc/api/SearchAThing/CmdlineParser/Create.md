# SearchAThing.CmdlineParser.Create method
## Create(string, Action<SearchAThing.CmdlineParser>, bool)
create main parser

### Signature
```csharp
public static SearchAThing.CmdlineParser Create(string description, Action<SearchAThing.CmdlineParser> builder, bool useColors = True)
```
### Parameters
- `description`: program description
- `builder`: action to configure and run the parser
- `useColors`: true to use colors

### Returns

### Remarks


<p>&nbsp;</p>
<p>&nbsp;</p>
<hr/>

## Create(string, Action<SearchAThing.CmdlineParser>, CmdlineColors)
create main parser

### Signature
```csharp
public static SearchAThing.CmdlineParser Create(string description, Action<SearchAThing.CmdlineParser> builder, CmdlineColors colors)
```
### Parameters
- `description`: program description
- `builder`: action to configure and run the parser
- `colors`: custom color object or null to disable

### Returns

### Remarks

