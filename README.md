# FoxTunes
A modular music player and converter using the BASS framework.

The main release is [FoxTunes-2.1.3-net461.zip](https://github.com/aidan-g/FoxTunes/releases/download/2.1.3/FoxTunes-2.1.3-net461.zip)
There is also a [Microsoft Store](https://www.microsoft.com/store/productId/9MWPJTXWTXLG) package.

The wiki is [FoxTunes-wiki](https://github.com/aidan-g/FoxTunes/wiki)

![Main](Media/Screenshots/Main.PNG)

* The following input formats are supported: aac, ac3, aif, ape, dff, dsf, dts, fla, flac, kar, m4a, m4a, m4b, mac, mid, midi, mp1, mp2, mp3, mp4, oga, ogg, ogg, opus, rmi, wav, wma, wv
* The following output formats are supported: flac, m4a, mp3, ogg, opus, wv

The UI components can be relocated and removed.

![Main](Media/Screenshots/Browser.PNG)

Library, playlist and other elements can be customized using Javascript.

![Hierarchy](Media/Screenshots/HierarchyBuilder.PNG)
![Playlist](Media/Screenshots/PlaylistBuilder.PNG)

DirectSound, ASIO and WASAPI output modes are supported.

![Settings](Media/Screenshots/Settings.PNG)

Includes a mini player with optional artwork and playlist.

![Mini A](Media/Screenshots/MiniPlayerA.PNG)
![Mini B](Media/Screenshots/MiniPlayerB.PNG)

Can be used with file associations and the "Send to" explorer action. 

A Windows XP compatibile .NET 4.0 build is available, can be configured minimally.
It needs at least;
* [KB2468871](https://www.microsoft.com/en-us/download/details.aspx?id=3556) - Portable class libraries patch.
  * Microsoft keep breaking the link. Google: NDP40-KB2468871
* [msvcp100.dll and msvcr100.dll](https://github.com/aidan-g/FoxTunes/releases/download/0.8/FoxTunes-0.8-Dependencies.tar.gz) - Microsoft Visual C++ 2010

![Minimal](Media/Screenshots/Minimal.PNG)

Themable. You can create a theme library with xaml. See the FoxTunes.UI.Windows.Themes project for an example.
Two are included. I'm no artist;
* Expression Dark - A style developed by Microsoft. 
* System - Use the default appearance. Minimal resource usage.

Various optional plugins are available;

* ASIO - Low latency exclusive output, supports DSD direct.
* CD - Play audio CDs from a physical drive.
* DSD - Required for DSD direct.
* DTS - Play .dts multi channel format.
* DirectSound - Use standard windows audio session for output.
* Encoder - A converter with various output formats. 
  * Can rip CDs with the CD plugin.
  * Can up/down sample rate/depth with the Resampler plugin.
* Javascript - Use JS for scriptable elements.
* LibraryBrowser - An album art grid interface for the library. It can use lots of memory.
* Logging - Use Log4Net logging back-end. Can be configured and disabled.
* MetaDataEditor - A simple batch mode tag editor. Can embed artwork.
* ParametricEqualizer - A ten band parametric equalizer.
* Resampler - SOXR based high quality configurable resampler. Can perform up/down sampling.
* SQLite - Use SQLite for database functions.
* SimpleMetaData - A meta data provider using the file path and regular expressions. Recommended for older systems.
* SqlServer - Use Microsoft SQL Server for database functions.
* TagLib - Use TagLib for meta data functions.
* WASAPI - Windows Audio Session API output.
* Windows - Global key bindings (Multi media keys), system tray icon, system media transport controls, taskbar controls.
