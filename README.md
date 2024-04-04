# Arctic.Background

## Configuration Variables
| Variable name | Possible Values/format | Datatype | Description
| --- | --- | --- |----------- |
|STORETYPE|memory/json|string| Type of datastorage for scrapped data
|RESULTOUTPUTFOLDER||string| Path to output folder for json storage. Only applicable if STORETYPE is json
|OVERRIDEDATA|true/false|boolean|Set to true if data that is already parsed should be reparsed
|RUNMODE| hh:mm:ss | timestamp| How long between each time the docker container should parse websites. Default 48 hours if nothing is set. Example value 48:00:00
|PARSETYPES|competition/puzzle|string|Types of parses that should be run. Values are seperated with ','.

## Example startup with json file store