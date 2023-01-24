# FoxTunes
A **portable, modular music player and converter** using the BASS framework for Windows XP/.../10/11.

The main release is [FoxTunes-2.8.1-net462.zip](https://github.com/Raimusoft/FoxTunes/releases/download/2.8.1/FoxTunes-2.8.1-net462.zip).  
For additional plugins, download [FoxTunes-2.8.1-Plugins-net462.zip](https://github.com/Raimusoft/FoxTunes/releases/download/2.8.1/FoxTunes-2.8.1-Plugins-net462.zip) (extract and copy required folders to the FoxTunes\lib directory).

There is also a [Microsoft Store](https://www.microsoft.com/store/productId/9MWPJTXWTXLG) package.

Consult the [Wiki](https://github.com/aidan-g/FoxTunes/wiki) for more informations.

![Main](Media/Screenshots/Main.PNG)

## Input/Output

* The following **input formats** are supported: aac, ac3, aif, ape, dff, dsf, dts, fla, flac, kar, m4a, m4a, m4b, mac, mid, midi, mod, mp1, mp2, mp3, mp4, oga, ogg, ogg, opus, rmi, wav, wma, wv.
* The following **output formats** are supported: flac, m4a, mp3, ogg, opus, wv.
* Gapless and fading (with crossfading) **input modes** are supported.
* DirectSound, WASAPI and ASIO **output modes** are supported.
* Replay Gain and Tempo.
* Cue sheets can be played and split using the converter.

## Interface/Layout

The UI components can be relocated and removed via the layout editor.  
You can save and switch between different layouts by activating in the settings '**Profiles**'.  
Additional windows can be added and customized.

![Main](Media/Screenshots/Browser.PNG)

## Scriptable

Library, playlist and other elements can be customized using Javascript.

![Hierarchy](Media/Screenshots/HierarchyBuilder.PNG)
![Playlist](Media/Screenshots/PlaylistBuilder.PNG)

## Themable

You can create a theme library with xaml.  
See the FoxTunes.UI.Windows.Themes project for an example.
 
Two are included. We are no artists;
* Expression Dark - A style developed by Microsoft. 
* System - Use the default appearance. Minimal resource usage.

## Mini player

Includes a mini player with optional artwork and playlist.

![Mini A](Media/Screenshots/MiniPlayerA.PNG)
![Mini B](Media/Screenshots/MiniPlayerB.PNG)

Can be used with file associations and the "Send to" explorer action. 

## Settings

A lot of settings are available depending on plugins installed.

![Settings](Media/Screenshots/Settings.PNG)

## Windows XP support

A Windows XP compatibile .NET 4.0 build can be created (raise an issue if you think this should be automated) by manually running the Nightly Build (Legacy) pipeline.
It needs at least;
* [KB2468871](https://www.microsoft.com/en-us/download/details.aspx?id=3556) - Portable class libraries patch.
  * Microsoft keep breaking the link. Google: NDP40-KB2468871

![Minimal](Media/Screenshots/Minimal.PNG)

## Optional plugins

* **Archive** - Tracks can be played directly from some archive formats: 7z, iso, rar, tar and zip.
* **ASIO** - Low latency exclusive output, supports DSD direct.
* **CD** - Play audio CDs from a physical drive.
* **CROSSFADE** - A fading input transport: https://github.com/aidan-g/BASS_CROSSFADE
* **CUE** - Play cue sheets: https://github.com/aidan-g/BASS_SUBSTREAM
  * Provides the "skip silence" feature which can trim silence from the start and end of media.
* **Discogs** - Fetch meta data and images using the discogs API.
  * Automatically fetch missing artwork.
* **DirectSound** - Use standard windows audio session for output.
* **DSD** - Required for DSD direct.
* **DTS** - Play .dts multi channel format: https://github.com/aidan-g/BASS_DTS
* **Encoder** - A converter with various output formats. 
  * Can split cue sheets with the CUE plugin.
  * Can rip CDs with the CD plugin.
  * Can up/down sample rate/depth with the Resampler plugin.
* **GAPLESS** - A "true" gapless input transport: https://github.com/aidan-g/BASS_GAPLESS
* **noesis** - Use Noesis.Javascript for scriptable elements.
* **v8net** - Use V8.Net for scriptable elements.
* **clearscript** - Use Microsoft.ClearScript for scriptable elements.  
* **Layout** - A flexible layout system with various panel types. 
* **LibraryBrowser** - An album art grid interface for the library.
* **Logging** - Use Log4Net logging back-end. Only used for debugging.
* **Lyrics** - A simple lyrics viewer, editor and auto lookup via "Chart Lyrics" provider.
* **Memory** - Play tracks from memory.
  * Improves playback over a network or other slow storage.
* **MetaDataEditor** - A simple batch mode tag editor. Can embed artwork.
* **Minidisc** - Write physical minidiscs using a compatible netmd device.
  * Uses the MD.Net library: https://github.com/aidan-g/MD.Net
* **MOD** - Play various mod music formats.
* **ParametricEqualizer** - A ten band parametric equalizer with EQ presets easily modifiable and addable (TEXT files).
* **Ratings** - 1-5 based star rating system with several controls for viewing and editing.
* **ReplayGain** - Calculate and utilize replay gain meta data for tracks and albums: https://github.com/aidan-g/BASS_REPLAY_GAIN
  * Can calculate on demand (per track) if you don't mind waiting a moment for playback.
* **Resampler** - SOXR based high quality configurable resampler. Can perform up/down sampling: https://github.com/aidan-g/BASS_SOX
* **SimpleMetaData** - A meta data provider using the file path and regular expressions. Recommended for older systems.
* **Snapping** - Enable winamp like window snapping.
* **Spectrum** - Various visualizations.
* **SQLite** - Use SQLite for database functions.
* **SqlServer** - Use Microsoft SQL Server for database functions.
* **Statistics** - Playback statistics like play count and last played date/time.
* **TagLib** - Use TagLib for meta data functions.
* **Tempo** - Adjust the tempo and pitch of media.
* **Tools** - A framework for external tools.
  * Open media with MusicBrainz Picard.
* **WASAPI** - Windows Audio Session API output.
* **WaveBar** - A wave form seek bar with mono and multi channel modes.
* **Windows** - Global key bindings (Multi media keys), system tray icon, system media transport controls, taskbar controls.
* **WPF** - Used for UI themes.

## Translations

For now only English and French translations are available.  
Please help us to translate FoxTunes in your language by submitting a pull request.

## Support the project

~We are saving up for a signing certificate and associated costs but it is very expensive.~ it's unlikely this will ever happen but I try to update the windows store app regularly which seems to address AV false positives to some extent. 

[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=BW5JUK6ZUQK7S&currency_code=GBP&source=url)
