# BingBongVoiceLineAPI

This is a modding framework to allow easy access for adding additional/replacing voice lines and subtitles of bing bongs responses without requiring **ANY** code to be written.


## Usage

1. Make sure you have this mod installed.
2. Create a new mod folder in your thunderstore/r2modman profile (`YourModProfile\BepInEx\plugins\YourModName`).
3. In the mod folder you just created, add a new text file named `response_sound_pack.json`, ensure the file extention is type `.json`.
4. Add any audio files you want to be BingBong responses into this folder as well.
5. Edit the `response_sound_pack.json` and include any audio files you want to be used, Here is an example config:
```json
{
  "name": "ExampleModName",
  "entries": [
    {
      "file": "ExampleMP3AudioFile.mp3",
	  "Description": "Example subtitles for bing bong"
    },
    {
      "file": "ExampleWAVAudioFile.wav"
	  //This will automatically make the subtitles blank
    }
  ]
}
``` 

Save and you are all ready to go!

### _NOTE_

- **ALL** players must have the same mod and same config settings for it to by synced between players.

_To make this easier you can create your own [thunderstore](https://thunderstore.io/c/peak/) mod following information [here](https://thunderstore.io/c/peak/create/docs/) with this mod as a dependency in the `manifest.json` file and packaged with the `response_sound_pack.json` and all audio files in the zip too._    

- You **CAN** install or create multiple mods which use this API to add custom responses. 

- Currently supports the following file types:
	- `.mp3`
	- `.wav`
	- `.ogg`
	- `.aiff`
	- `.aif`
	- `.xm`
	- `.mod`
	- `.it` 
	- `.s3m`

## Configuration

This mod has a config to change settings which supports in game config editing with [ModConfig](https://thunderstore.io/c/peak/p/PEAKModding/ModConfig/).

The config includes these settings:

- `Enable Bing Bong Voice Line API`: This allows for the mod to be enabled and disabled at any time (even in game) without disabling the mod physically.

- `Replace Bing Bong Responses`: If enabled, custom responses will replace the default Bing Bong responses instead of being added to them. 									

_NOTE: This is not synced between players so ensure all players have the same config setting._


## Future Plans

- Adding a scriptable pipeline to the API so BingBong responses can be changed based on any actions via code (possibly a similar system to `loaforcsSoundAPI`). 
- Adding config syncing between players in a lobby.
- Adding support for youtube videos via links.

## Issues

If I have missed any upgrades or you find any bugs feel free  to open an issue on the [GitHub repository](https://github.com/W1ll-Gale/BingBongVoiceLineAPI) or message me on discord (@`mrbyte.exe`) and i will try my best to fix any issues or update the mod for future updates.
