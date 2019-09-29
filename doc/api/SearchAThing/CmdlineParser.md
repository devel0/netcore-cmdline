# CmdlineParser Class
**Namespace:** SearchAThing

**Inheritance:** Object → CmdlineParser

cmdline parser tool

## Signature
```csharp
public class CmdlineParser
```
## Methods
|**Name**|**Summary**|
|---|---|
|[AddCommand](CmdlineParser/AddCommand.md)|add a command item to this parser|
|[AddLong](CmdlineParser/AddLong.md)|add optional long flag|
|[AddMandatoryLong](CmdlineParser/AddMandatoryLong.md)|add mandatory long flag|
|[AddMandatoryParameter](CmdlineParser/AddMandatoryParameter.md)|add mandatory parameter item to this parser|
|[AddMandatoryParameterArray](CmdlineParser/AddMandatoryParameterArray.md)|add mandatory parameter array item to this parser|
|[AddMandatoryShort](CmdlineParser/AddMandatoryShort.md)|add mandatory short flag|
|[AddMandatoryShortLong](CmdlineParser/AddMandatoryShortLong.md)|add mandatory short/long flag|
|[AddParameter](CmdlineParser/AddParameter.md)|add optional parameter item to this parser|
|[AddParameterArray](CmdlineParser/AddParameterArray.md)|add optional parameter array item to this parser|
|[AddShort](CmdlineParser/AddShort.md)|add optional short flag|
|[AddShortLong](CmdlineParser/AddShortLong.md)|add optional short/long flag|
|[Create](CmdlineParser/Create.md) (static)|create main parser|
|[Create](CmdlineParser/Create.md#createstring-actionsearchathingcmdlineparser-cmdlinecolors-bool) (static)|create main parser|
|[Equals](CmdlineParser/Equals.md)||
|[GetHashCode](CmdlineParser/GetHashCode.md)||
|[GetType](CmdlineParser/GetType.md)||
|[OnCmdlineMatch](CmdlineParser/OnCmdlineMatch.md)|set action to execute when this parser cmdline matches|
|[PrintUsage](CmdlineParser/PrintUsage.md)|print the usage based on current parser configuration|
|[Run](CmdlineParser/Run.md)|execute the parser ( must called once from top parser builder )|
|[ToString](CmdlineParser/ToString.md)|build a table with all parser item details, matches and parsed values ( for debug purpose )|
## Properties
|**Name**|**Summary**|
|---|---|
|[AllFlags](CmdlineParser/AllFlags.md)|inherited and this parser flags
|[AllItems](CmdlineParser/AllItems.md)|parent and this items
|[AppVersion](CmdlineParser/AppVersion.md)|app version utility
|[Colors](CmdlineParser/Colors.md)|colors set ( this can be changed from the Create method )
|[Command](CmdlineParser/Command.md)|command that activate this parser
|[Commands](CmdlineParser/Commands.md)|this parser commands
|[Description](CmdlineParser/Description.md)|description of this parser ( automatically retrieved from command if this is a subparser )
|[Flags](CmdlineParser/Flags.md)|this parser flags
|[FriendlyName](CmdlineParser/FriendlyName.md)|assembly friendly name ( used for Usage )
|[GlobalFlags](CmdlineParser/GlobalFlags.md)|global flags
|[InheritedFlags](CmdlineParser/InheritedFlags.md)|only parent parsers flags
|[InheritedItems](CmdlineParser/InheritedItems.md)|only parents items
|[Items](CmdlineParser/Items.md)|this (sub)parser items
|[ParameterArrays](CmdlineParser/ParameterArrays.md)|parameter with array mode
|[Parameters](CmdlineParser/Parameters.md)|parameters
|[ParametersOrArray](CmdlineParser/ParametersOrArray.md)|all parameters single or array mode
|[Parent](CmdlineParser/Parent.md)|parent parset ( null for topmost parser )
|[ParentParsers](CmdlineParser/ParentParsers.md)|parent parsers enum
|[ParserPath](CmdlineParser/ParserPath.md)|list of parser from topmost to this one
|[RootParser](CmdlineParser/RootParser.md)|topmost parser
## Conversions
