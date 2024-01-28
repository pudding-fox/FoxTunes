#!/bin/sh

ADDON="
bass_aac.dll
bass_ape.dll
bassalac.dll
bassdsd.dll
bassflac.dll
basswv.dll
"

MAIN="
x86/SQLite.Interop.dll
bass.dll
bass_gapless.dll
bassmix.dll
FoxDb.Core.dll
FoxDb.Linq.dll
FoxDb.Sql.dll
FoxDb.SQLite.dll
FoxTunes.Config.dll
FoxTunes.Core.dll
FoxTunes.DB.dll
FoxTunes.DB.SQLite.dll
FoxTunes.Launcher.exe
FoxTunes.Launcher.exe.config
FoxTunes.Logging.dll
FoxTunes.MetaData.dll
FoxTunes.MetaData.TagLib.dll
FoxTunes.Output.Bass.DirectSound.dll
FoxTunes.Output.Bass.dll
FoxTunes.Output.Bass.Gapless.dll
FoxTunes.Output.dll
FoxTunes.Scripting.dll
FoxTunes.Scripting.JS.dll
FoxTunes.UI.dll
FoxTunes.UI.Windows.dll
FoxTunes.UI.Windows.Themes.dll
log4net.config
log4net.dll
ManagedBass.dll
ManagedBass.Gapless.dll
ManagedBass.Mix.dll
Noesis.Javascript.dll
System.Data.SQLite.dll
System.Windows.Interactivity.dll
taglib-sharp.dll
"

ASIO="
bass_asio_handler.dll
bassasio.dll
FoxTunes.Output.Bass.Asio.dll
ManagedBass.Asio.dll
"

CD="
bass_gapless_cd.dll
Addon/basscd.dll
FoxTunes.Output.Bass.Cd.dll
ManagedBass.Cd.dll
ManagedBass.Gapless.Cd.dll
"

DSD="
FoxTunes.Output.Bass.Dsd.dll
ManagedBass.Dsd.dll
"

DTS="
bass_dts.dll
FoxTunes.Output.Bass.Dts.dll
ManagedBass.Dts.dll
"

SOX="
bass_sox.dll
FoxTunes.Output.Bass.Resampler.dll
ManagedBass.Sox.dll
"

WASAPI="
bass_wasapi_handler.dll
basswasapi.dll
FoxTunes.Output.Bass.Wasapi.dll
ManagedBass.Wasapi.dll
"

SQLSERVER="
FoxTunes.DB.SqlServer.dll
FoxDb.SqlServer.dll
FoxDb.SqlServer.2012.dll
"

DEPENDENCIES=" 
x86/Microsoft.VC100.CRT/msvcp100.dll
x86/Microsoft.VC100.CRT/msvcr100.dll
"

TAG=$(git describe --abbrev=0 --tags)

echo "Current version is $TAG.."

MSBUILD="/c/Program Files (x86)/MSBuild/12.0/Bin/MSBuild.exe"

if [ -f "$MSBUILD" ]
then

	rm -rf "./distribution"

	"$MSBUILD" FoxTunes.sln //t:Build //p:Configuration=Release
else 
	echo "Skipping build, not such $MSBUILD."
fi

rm -rf "./release"

mkdir -p "./release/Main"
mkdir -p "./release/Main/Addon"
mkdir -p "./release/Plugins"
mkdir -p "./release/Plugins/asio"
mkdir -p "./release/Plugins/cd"
mkdir -p "./release/Plugins/dsd"
mkdir -p "./release/Plugins/dts"
mkdir -p "./release/Plugins/sox"
mkdir -p "./release/Plugins/wasapi"
mkdir -p "./release/Plugins/sqlserver"
mkdir -p "./release/Dependencies"

echo "Creating main package.."

for file in $ADDON
do
	echo "$file"
	cp "./distribution/Release/Addon/$file" "./release/Main/Addon"
done

for file in $MAIN
do
	echo "$file"
	cp "./distribution/Release/$file" "./release/Main"
done

tar -zcvf "./release/FoxTunes-$TAG.tar.gz" -C "./release/Main" . --xform='s!^\./!!'

echo "Creating plugins package.."

for file in $ASIO
do
	echo $file
	cp "./distribution/Release/$file" "./release/Plugins/asio"
done

for file in $CD
do
	echo $file
	cp "./distribution/Release/$file" "./release/Plugins/cd"
done

for file in $DSD
do
	echo $file
	cp "./distribution/Release/$file" "./release/Plugins/dsd"
done

for file in $DTS
do
	echo $file
	cp "./distribution/Release/$file" "./release/Plugins/dts"
done

for file in $SOX
do
	echo $file
	cp "./distribution/Release/$file" "./release/Plugins/sox"
done

for file in $WASAPI
do
	echo $file
	cp "./distribution/Release/$file" "./release/Plugins/wasapi"
done

for file in $SQLSERVER
do
        echo $file
        cp "./distribution/Release/$file" "./release/Plugins/sqlserver"
done

tar -zcvf "./release/FoxTunes-$TAG-Plugins.tar.gz" -C "./release/Plugins" . --xform='s!^\./!!'

echo "Creating dependencies package.."

for file in $DEPENDENCIES
do
	echo $file
	cp "./distribution/Release/$file" "./release/Dependencies"
done

tar -zcvf "./release/FoxTunes-$TAG-Dependencies.tar.gz" -C "./release/Dependencies" . --xform='s!^\./!!'

echo "All done."

