# TELESTO (Telegram Service for Triggernometry Operations)

This plugin allows Triggernometry (and other programs, why not) some means to interact with the game directly.

## How does it work

The plugin starts listening to JSON payloads on HTTP POST on a specific port, and from there it will just execute whatever commands are sent by the external application.

## (X) JSON!

The general form is `{ "version": 1, "id": x, "type": y, "payload": z }`, where:

* x is a number identifier for the telegram (not currently in use, may be used later in replies)
* y is the telegram type
* z is the payload relevant to the telegram

### Telegram types

`PrintMessage`: Prints a message in chat log.
Example: `{ "version": 1, "id": 123456, "type": "PrintMessage", "payload": { "message": "hello world" } }`

`PrintError`: Prints an error message in chat log.
Example: `{ "version": 1, "id": 123456, "type": "PrintError", "payload": { "message": "hello world" } }`

`ExecuteCommand`: Executes an ingame command.
Example: `{ "version": 1, "id": 123456, "type": "ExecuteCommand", "payload": { "command": "/mk attack1 <1>" } }`

`OpenMap`: Opens a map and sets a flag marker. `coords` can be either `world` for ingame coords or `raw` for raw map coordinates.
Example: `{ "version": 1, "id": 123456, "type": "OpenMap", "payload": { "territory": 160, "map": 108, "coords": "world", "x": 12, "y": 12 } }`

`Bundle`: A bundle of telegrams.
Example: `{ "version": 1, "id": 123456, "type": "Bundle", "payload": [ { "id": 123456, "type": "PrintMessage", "payload": { "message": "hello world" } }, { "id": 123456, "type": "ExecuteCommand", "payload": { "command": "/mk attack1 <1>" } } ] }`

## In-game usage and configuration

* Type `/telesto` to open the configuration UI
* General settings
  * "Start endpoint on launch" controls whether the HTTP endpoint is available when the plugin loads or not
  * "HTTP POST endpoint" is the HTTP endpoint