# FoxTunes
A modular music player using the BASS framework.

The main release is [FoxTunes-1.5-net461.zip](https://github.com/aidan-g/FoxTunes/releases/download/1.5/FoxTunes-1.5-net461.zip)

![Main](Media/Screenshots/Main.PNG)

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

A Windows XP compatibile .NET 4.0 build is available, can be configured minimally.
It needs at least;
* [KB2468871](http://support.microsoft.com/kb/2468871) - Portable class libraries patch.
* [msvcp100.dll and msvcr100.dll](https://github.com/aidan-g/FoxTunes/releases/download/0.8/FoxTunes-0.8-Dependencies.tar.gz) - Microsoft Visual C++ 2010

![Minimal](Media/Screenshots/Minimal.PNG)

Stock plugins;

* SQLite - Use SQLite for database functions.
* TagLib - Use TagLib for meta data functions.
* DirectSound - Use standard windows audio session for output.
* Javascript - Use JS for scriptable elements.
* Logging - Use Log4Net logging back-end. Can be configured and disabled.
* Config - Use binary config file.

Various optional plugins are available;

* Windows - Global key bindings (Multi media keys), system tray icon.
* SqlServer - Use Microsoft SQL Server for database functions.
* SimpleMetaData - A meta data provider using the file path and regular expressions. Recommended for older systems.
* ASIO - Low latency exclusive output, supports DSD direct.
* WASAPI - Windows Audio Session API output.
* DSD - Required for DSD direct.
* DTS - Play .dts multi channel format.
* CD - Play audio CDs from a physical drive.
* Resampler - SOXR based high quality configurable resampler. Can perform up/down sampling.
