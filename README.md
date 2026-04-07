# Configuration

```json
{
    "OutputOptions": {
        "OutputPath": "<Full path To HTML output file>"
    },
    "DatabaseConfigurationOptions": {
        "Username": "<Database username>",
        "Password": "<Database password>",
        "Host": "<Database IP / hostname>",
        "Port": "<Database port>",
        "Database": "<Database name>"
    },
    "ConfiguredSetsOptions": {
        "ConfiguredSets": [<"comma", "seperated", "list", "of", "standard", "setcodes">],
        "ConfiguredCommanderSets": [<"comma", "seperated", "list", "of", "commander", "setcodes">],   
        "HydrateSetData": <true/false>,
        "SpecialGuestsEnabled": <true/false>,
        "SpecialGuestRangesBySet": {
            "<setCode>": "<lower-bound-collector-number>, <upper-bound-collector-number>" 
        },
        "SpecialGuestRatesBySet": {
            "<setCode>": "<numerator>,<denominator>"
        }
    }
}
```