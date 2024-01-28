# FoxTunes
A modular music player using the BASS framework.

The main release is [FoxTunes-2.0.4-net461.zip](https://github.com/aidan-g/FoxTunes/releases/download/2.0.4/FoxTunes-2.0.4-net461.zip)

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

Can be used with file associations and the "Send to" explorer action. 

A Windows XP compatibile .NET 4.0 build is available, can be configured minimally.
It needs at least;
* [KB2468871](http://support.microsoft.com/kb/2468871) - Portable class libraries patch.
* [msvcp100.dll and msvcr100.dll](https://github.com/aidan-g/FoxTunes/releases/download/0.8/FoxTunes-0.8-Dependencies.tar.gz) - Microsoft Visual C++ 2010

![Minimal](Media/Screenshots/Minimal.PNG)

Various optional plugins are available;

* ASIO - Low latency exclusive output, supports DSD direct.
* CD - Play audio CDs from a physical drive.
* DSD - Required for DSD direct.
* DTS - Play .dts multi channel format.
* DirectSound - Use standard windows audio session for output.
* Javascript - Use JS for scriptable elements.
* Logging - Use Log4Net logging back-end. Can be configured and disabled.
* ParametricEqualizer - A ten band parametric equalizer.
* Resampler - SOXR based high quality configurable resampler. Can perform up/down sampling.
* SQLite - Use SQLite for database functions.
* SimpleMetaData - A meta data provider using the file path and regular expressions. Recommended for older systems.
* SqlServer - Use Microsoft SQL Server for database functions.
* TagLib - Use TagLib for meta data functions.
* WASAPI - Windows Audio Session API output.
* Windows - Global key bindings (Multi media keys), system tray icon, system media transport controls, taskbar controls.