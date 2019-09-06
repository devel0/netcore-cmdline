# CmdlineParseItem Class
**Namespace:** SearchAThing

**Inheritance:** Object → CmdlineParseItem

describe properties required for a command, a flag or a parameter on the syntax for the cmdline parser;
            enumerating returns the list of values if this is a flag with value or a parameter or a parameter array

## Signature
```csharp
public class CmdlineParseItem : System.Collections.Generic.IEnumerable<string>, System.Collections.IEnumerable
```
## Methods
|**Name**|**Summary**|
|---|---|
|[Equals](CmdlineParseItem/Equals.md)||
|[GetEnumerator](CmdlineParseItem/GetEnumerator.md)||
|[GetHashCode](CmdlineParseItem/GetHashCode.md)||
|[GetType](CmdlineParseItem/GetType.md)||
|[OnCompletion](CmdlineParseItem/OnCompletion.md)|set a runtime completion function ( used for parameter item types )|
|[ToString](CmdlineParseItem/ToString.md)||
## Properties
|**Name**|**Summary**|
|---|---|
|[ArgValues](CmdlineParseItem/ArgValues.md)|original cmdline argument associated as value to this item (may a flag with value or parameter)
|[Description](CmdlineParseItem/Description.md)|description for this parse item to use in the usage or is parser description if this is a command
|[GlobalFlagAction](CmdlineParseItem/GlobalFlagAction.md)|stores actiont to execute if this global flag item matches
|[GlobalFlagActionNested](CmdlineParseItem/GlobalFlagActionNested.md)|if false global flag action will executed immeditaly; if true execution will deferred to nested parser
|[HasLongName](CmdlineParseItem/HasLongName.md)|states if this element has a long name ( used only for flags )
|[HasShortName](CmdlineParseItem/HasShortName.md)|true if this item has a short name ( maybe a command, parameter or a flag with short name )
|[HasValueName](CmdlineParseItem/HasValueName.md)|states if this flag requires a value
|[IsCommand](CmdlineParseItem/IsCommand.md)|true if this is a command
|[IsFlag](CmdlineParseItem/IsFlag.md)|true if this is a flag
|[IsGlobal](CmdlineParseItem/IsGlobal.md)|states if this parse item is a global flag
|[IsParameter](CmdlineParseItem/IsParameter.md)|true if this is a parameter
|[IsParameterArray](CmdlineParseItem/IsParameterArray.md)|true is this is an array of parameters
|[IsParameterOrArray](CmdlineParseItem/IsParameterOrArray.md)|true if this is a parameter or array of parameters
|[LongFlag](CmdlineParseItem/LongFlag.md)|states if this item have a long name ( used only for flags )
|[LongName](CmdlineParseItem/LongName.md)|long name ( used only for flags )
|[Mandatory](CmdlineParseItem/Mandatory.md)|true if this item is mandatory
|[MatchArgument](CmdlineParseItem/MatchArgument.md)|original argument with that this parse item matches
|[Matches](CmdlineParseItem/Matches.md)|states if this flag have a matching with required usage
|[MatchParser](CmdlineParseItem/MatchParser.md)|used to distinguish sub parser for global flags
|[Parser](CmdlineParseItem/Parser.md)|parser which this item belongs to
|[ShortFlag](CmdlineParseItem/ShortFlag.md)|short flag final string ( without value if any )
|[ShortLongFlag](CmdlineParseItem/ShortLongFlag.md)|short/long flag final string ( without value if any )
|[ShortName](CmdlineParseItem/ShortName.md)|short name of this element
|[SortOrder](CmdlineParseItem/SortOrder.md)|helper to sort items in groups with follow order Commands, Flags, Parameters, Parameters array
|[Type](CmdlineParseItem/Type.md)|type of this cmdline parser
|[ValueName](CmdlineParseItem/ValueName.md)|symbolic name for the flag value to display in the usage
|[Values](CmdlineParseItem/Values.md)|values associated to this item (may a flag with value or parameter)
## Conversions
