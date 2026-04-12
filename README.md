# About
MTGallery is a simple Magic: The Gathering pack generator and card repository. The generator supports any set that can be found on [Scryfall](https://scryfall.com/) and generates a simple HTML report that can be easily shared with others.  

# Dependencies
* PostgreSQL
  * The program takes care of creating all of the necessary tables, however you must manually insert the JSON pull rates you want to use into the `pull_rates` table. This can be accomplished using the SQL command `INSERT INTO pull_rates VALUES (<set_code>, <json_data>);`. Some sample pull rates data can be found in [SetData](SetData).
* If you want to use this program, you must also change the configuration source file path in [src/MTGallery/Program.cs](src/MTGallery/Program.cs) on line #9.
* See the below section for creating your JSON configuration file.

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
            "<setCode>": "<lower-bound-collector-number>,<upper-bound-collector-number>" 
        },
        "SpecialGuestRatesBySet": {
            "<setCode>": "<numerator>,<denominator>"
        }
    }
}
```

# Notice
Portions of MTGallery are unofficial Fan Content permitted under the Wizards of the Coast Fan Content Policy. The literal and graphical information presented in this program about Magic: The Gathering, including card images, is copyright Wizards of the Coast, LLC. MTGallery is not produced by or endorsed by Wizards of the Coast.

For all other content, see [LICENSE.md](LICENSE.md).