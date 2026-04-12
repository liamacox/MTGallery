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
## Ouput Options
* `OutputPath` - The fully qualified path to your desired report output file. 

## Database Configuration Options
* `Username` - Database user username.
* `Password` - Database user password.
* `Host` - IP or Hostname of the database server.
* `Port` - Database server port.
* `Database` - Name of the desired database. 

## Configured Sets Options
* `ConfiguredSets` - List of **non-commander** sets you want to be able to draw from.
* `ConfiguredCommanderSets` - List of **commander** sets you want to be able to load into your card pool. The program does not "draw" from commander sets. It instead adds one of each card into your pool of pulled cards, as if you had purchased one of each commander precon for the set.
* `HydrateSetData` - Set to `true` to populate the `set_data` table with the cards from each configured set. Keep this option set to `false` at all times unless you made changes to the `ConfiguredSetsOptions` as querying Scryfall and updating the database significantly increases start up time. 
* `SpecialGuestsEnabled` - Set to `true` to enable adding special guest cards (SPG) to packs if possible. Set to `false` to disable this functionality. 
* `SpecialGuestRangesBySet` - A dictionary with set codes as keys and special guest collector number ranges as values. For example, the special guest collector number range for Lorwyn Eclipsed (ecl) is \[129, 148\]. In order to generate special guest cards for a given set, it **must** be included in this dictionary.
* `SpecialGuestRatesBySet` - A dictionary with set codes as keys and special guest pull rates as values. For example, the special guest rate for Lorwyn Eclipsed (ecl) is 1 in 55 packs and 15 in 1000 packs for Bloomburrow (blb). In order to generate special guest cards for a given set, it **must** be included in this dictionary.

# Notice
Portions of MTGallery are unofficial Fan Content permitted under the Wizards of the Coast Fan Content Policy. The literal and graphical information presented in this program about Magic: The Gathering, including card images, is copyright Wizards of the Coast, LLC. MTGallery is not produced by or endorsed by Wizards of the Coast.

For all other content, see [LICENSE.md](LICENSE.md).