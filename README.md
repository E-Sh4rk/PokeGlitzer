# PokeGlitzer

If you are looking for a regular save editor (no glitch species, no raw data manipulation),
you should use [PKHeX](https://github.com/kwsch/PKHeX).

## Running the app

The following dependencies are required in order to run this application:

- Windows (recommended), or MacOS/Linux
- .NET Runtime 7.0: https://dotnet.microsoft.com/download/dotnet/7.0

You can then download the last version of this app in the [release section](https://github.com/E-Sh4rk/PokeGlitzer/releases).

To run this app:
- On Windows, just open `PokeGlitzer.exe`
- On Linux/MacOS, you can use the command `dotnet PokeGlitzer.dll`

## Features

The following ROMs are compatible with this save editor:
- Emerald (any language)
- Ruby/Sapphire (any language)
- RedFire/LeafGreen (any language)

In any case, **please backup your save before using this save editor**.

The following features are implemented:
- Viewing and editing Pokemons in the party
- Viewing and editing Pokemons in the boxes
- Viewing and editing PC box names
- Synchronizing data with a running instance of Bizhawk
- Simulating Glitzer Popping (manually with a given offset, or automatically)

## Synchronization with Bizhawk

It is possible to synchronize the save editor with Bizhawk in order see/modify in live
the content of your party/boxes. This feature is **only available on Windows** as Bizhawk does not
support LUA scripting on Linux/MacOS.

This feature requires a recent version of Bizhawk: at least version 2.7,
or you can also build it from the [master](https://github.com/TASVideos/BizHawk/tree/master) channel
(or any version after commit [e79d33bcfdd9dac596dce0d60bf7c8621d92ce62](https://github.com/TASVideos/BizHawk/tree/e79d33bcfdd9dac596dce0d60bf7c8621d92ce62)).

You can then synchronize the level editor with Bizhawk by doing the following:
1. Open a Pokemon ROM (generation 3) with a recent version of Bizhawk
2. Open the `LUA console` and load the script `Misc/bizhawk_synchronize_*.lua` (in this repo) corresponding to your version.
Note that this script is only available for Emerald US and JP for now, but you can easily adapt it to work with other versions.
3. Open the save editor and click on `Sync -> Start synchronization`

## Building the app

If you want to build the app yourself, you should first clone this repo
(`git clone https://github.com/E-Sh4rk/PokeGlitzer.git`).

Then:
- On Windows, just open `PokeGlitzer.sln` using Visual Studio 2022
- On Linux/MacOS, you should be able to build it with the command `dotnet build PokeGlitzer.sln -c Release`
