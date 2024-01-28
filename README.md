# FoxTunes
A modular music player using the BASS framework.

![Main](https://github.com/aidan-g/FoxTunes/blob/master/Media/Screenshots/Main.PNG)

Library, playlist and other elements can be customized using Javascript.

![Hierarchy](https://github.com/aidan-g/FoxTunes/blob/master/Media/Screenshots/HierarchyBuilder.PNG)
![Playlist](https://github.com/aidan-g/FoxTunes/blob/master/Media/Screenshots/PlaylistBuilder.PNG)

DirectSound, ASIO and WASAPI output modes are supported.

![Settings](https://github.com/aidan-g/FoxTunes/blob/master/Media/Screenshots/Settings.PNG)

Includes a mini player with optional artwork and playlist.

![Mini A](https://github.com/aidan-g/FoxTunes/blob/master/Media/Screenshots/MiniPlayerA.PNG)
![Mini B](https://github.com/aidan-g/FoxTunes/blob/master/Media/Screenshots/MiniPlayerB.PNG)

A Windows XP compatibile .NET 4.0 build is available, can be configured minimally.

![Minimal](https://github.com/aidan-g/FoxTunes/blob/master/Media/Screenshots/Minimal.PNG)

Stock plugins;

1) SQLite - Use SQLite for database functions.
2) TagLib - Use TagLib for meta data functions.
3) DirectSound - Use standard windows audio session for output.
4) Javascript - Use JS for scriptable elements.
5) Logging - Use Log4Net logging back-end. Can be configured and disabled.
6) Config - Use binary config file.

Various optional plugins are available;

1) Windows - Global key bindings (Multi media keys), system tray icon.
2) SqlServer - Use Microsoft SQL Server for database functions.
3) SimpleMetaData - A meta data provider using the file path and regular expressions. Recommended for older systems.
4) ASIO - Low latency exclusive output, supports DSD direct.
5) WASAPI - Windows Audio Session API output.
6) DSD - Required for DSD direct.
7) DTS - Play .dts multi channel format.
8) CD - Play audio CDs from a physical drive.
9) Resampler - SOXR based high quality configurable resampler. Can perform up/down sampling.
