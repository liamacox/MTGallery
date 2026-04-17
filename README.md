# About
MTGallery is a simple Magic: The Gathering pack generator and card repository. The generator supports any set that can be found on [Scryfall](https://scryfall.com/) and generates a simple HTML report that can be easily shared with others.  

## Supported Sets
This generator supports creating packs for the following sets:
* Bloomburrow (blb)
* Lorwyn Eclipsed (ecl)

Commander sets can be configured as shown in the below configuration section.

# Dependencies
* PostgreSQL
  * The program takes care of creating all of the necessary tables.
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
        "ConfiguredCommanderSets": [<"comma", "seperated", "list", "of", "commander", "setcodes">],   
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
* `ConfiguredCommanderSets` - List of **commander** sets you want to be able to load into your card pool. The program does not "draw" from commander sets. It instead adds one of each card into your pool of pulled cards, as if you had purchased one of each commander precon for the set.

### Example `ConfiguredSetsOptions` Configuration
```json
"ConfiguredSetsOptions": {
    "ConfiguredCommanderSets": ["ecc", "blc"],
}
```

# Notice
Portions of MTGallery are unofficial Fan Content permitted under the Wizards of the Coast Fan Content Policy. The literal and graphical information presented in this program about Magic: The Gathering, including card images, is copyright Wizards of the Coast, LLC. MTGallery is not produced by or endorsed by Wizards of the Coast.

For all other content, see [LICENSE.md](LICENSE.md).