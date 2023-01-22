# TELESTO (Telegram Service for Triggernometry Operations)

This plugin allows Triggernometry (and other programs, why not) some means to interact with the game directly.

## How does it work

The plugin starts listening to JSON payloads on HTTP POST on a specific port, and from there it will just execute whatever commands are sent by the external application.

## How to install

You will find the plugin in my Dalamud plugin repository at https://github.com/paissaheavyindustries/Dalamud-Repo! Follow the instructions there on how to use the repository.

## In-game usage and configuration

* Type `/telesto` to open the configuration UI
* General settings
  * "Start endpoint on launch" controls whether the HTTP endpoint is available when the plugin loads or not
  * "HTTP POST endpoint" is the HTTP endpoint

## Support / Discord

There's a publicly available Discord server for announcements, suggestions, and questions - feel free to join at:

https://discord.gg/6f9MY55

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

`EnableDoodle`: Enables a doodle to be rendered. For examples on doodle creation, scroll down below.

`DisableDoodle`: Disables a single doodle.
Example: `{ "version": 1, "id": 123456, "type": "DisableDoodle", "payload": { "name": "meow3" } }`

`DisableDoodleRegex`: Disables multiple doodles by matching their IDs with the supplied regex.
Example: `{ "version": 1, "id": 123456, "type": "DisableDoodleRegex", "payload": { "regex": "meow.*" } }`

### Doodles

Doodles come currently in three types:

`text`: Render text  
`line`: Draw a line  
`circle`: Draw a circle

#### Properties generic to all doodles

`name`: Name of the doodle  
`type`: Type of the doodle  
`r`: Color red component (0.0 - 1.0), supports numeric expressions  
`g`: Color green component (0.0 - 1.0), supports numeric expressions  
`b`: Color blue component (0.0 - 1.0), supports numeric expressions  
`a`: Color alpha component (0.0 - 1.0), supports numeric expressions  
`expiresin`: Number of milliseconds the doodle is alive

#### Text doodle

`size`: Size of the text
`position`: Top left coordinate of the text (top left aligned)
`text`: Text to display

This example would place "HELLO WORLD!" at the bottom of the nearest market board: `{ "version": 1, "id": 123456, "type": "EnableDoodle", "payload": { "name": "meow2", "type": "text", "r": "1", "g": "1", "expiresin": "1000000", "text": "HELLO WORLD!", "size": "20", "position": { "coords": "entity", "name": "Market Board" } } }`

#### Line doodle

`thickness`: Thickness of the line
`start`: Start coordinate of the line (scroll down for coordinate specifications)
`end`: End coordinate of the line (scroll down for coordinate specifications)

This example would draw a blinking line from the nearest summoning bell to the nearest market board: `{ "version": 1, "id": 123456, "type": "EnableDoodle", "payload": { "name": "meow", "type": "line", "r": "1", "g": "1", "thickness": "3", "a": "abs(sin(${_systemtimems} / 100.0))", "start": { "coords": "entity", "name": "Summoning Bell" }, "end": { "coords": "entity", "name": "Market Board" } } }`

#### Circle doodle

`radius`: Radius of the circle  
`filled`: Determines if the circle is filled (`true`) or not (`false`)  
`position`: Center coordinate of the circle (scroll down for coordinate specifications)

This example would draw a circle at the bottom of the nearest market board: `{ "version": 1, "id": 123456, "type": "EnableDoodle", "payload": { "name": "meow3", "type": "circle", "r": "1", "g": "1", "expiresin": "1000000", "radius": "20", "position": { "coords": "entity", "name": "Market Board" } } }`

### Coordinate specification

Coordinate specifications for some telegram follow the following format:

`"position": { "coords": "...", ... }`

Where `coords` can be one of:

`screen`: Screen space coordinates; properties are `x`, `y`, and `z`, of which all support numeric expressions  
`world`: Game world coordinates; properties are `x`, `y`, and `z`, of which all support numeric expressions  
`entity`: Latched onto a game object/entity; properties are `id` to specify an object by hex ID string, or `name` to specify object by name

So to display something at screen coordinates 100, 20, you would say:

`"position": { "coords": "screen", "x": "100", "y": "20" }`

To display something relative to a game object in the world, you could try:

`"position": { "coords": "world", "x": "${_ffxiventity[DEADBEEF].x} + 10", "y": "${_ffxiventity[DEADBEEF].y}", "z": "${_ffxiventity[DEADBEEF].z}" }`

Or if you just want an easy way to latch something onto a game object, you can just specify its ID or name:

`"position": { "coords": "entity", "id": "DEADBEEF" }`  
`"position": { "coords": "entity", "name": "My'anime Char'name" }`
