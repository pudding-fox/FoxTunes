#!/bin/sh

TARGET="
net40
net461
"

ADDON="
bass_aac.dll
bass_ac3.dll
bass_ape.dll
bassalac.dll
bassdsd.dll
bassflac.dll
basswv.dll
bassmidi.dll
basswma.dll
"

MAIN="
x86/SQLite.Interop.dll
bass.dll
bass_gapless.dll
bass_inmemory_handler.dll
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
log4net.dll
ManagedBass.dll
ManagedBass.Gapless.dll
ManagedBass.Mix.dll
Noesis.Javascript.dll
System.Data.SQLite.dll
System.IO.dll
System.Runtime.dll
System.Threading.Tasks.dll
System.Windows.Interactivity.dll
taglib-sharp.dll
Microsoft.Threading.Tasks.dll
Microsoft.Threading.Tasks.Extensions.Desktop.dll
Microsoft.Threading.Tasks.Extensions.dll
Microsoft.Windows.Shell.dll
"

WINDOWS="
FoxTunes.Core.Windows.dll
"

ASIO="
bass_asio_handler.dll
bassasio.dll
FoxTunes.Output.Bass.Asio.dll
ManagedBass.Asio.dll
"

CD="
bass_gapless_cd.dll
basscd.dll
FoxTunes.Output.Bass.Cd.dll
ManagedBass.Cd.dll
ManagedBass.Gapless.Cd.dll
"

DSD="
bass_inmemory_handler_dsd.dll
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

SIMPLEMETADATA="
FoxTunes.MetaData.FileName.dll
"

LIBRARYBROWSER="
FoxTunes.UI.Windows.LibraryBrowser.dll
"

METADATAEDITOR="
FoxTunes.UI.Windows.MetaDataEditor.dll
"

BUNDLED="
windows
asio
cd
dsd
dts
sox
wasapi
librarybrowser
metadataeditor
"

TAG=$(git describe --abbrev=0 --tags)

echo "Current version is $TAG.."
sleep 1
echo "3.."
sleep 1
echo "2.."
sleep 1
echo "1.."
sleep 1

rm -rf "./release"

for target in $TARGET
do

	mkdir -p "./release/$target/Main"
	mkdir -p "./release/$target/Main/Addon"
	mkdir -p "./release/$target/Plugins"
	mkdir -p "./release/$target/Plugins/windows"
	mkdir -p "./release/$target/Plugins/asio"
	mkdir -p "./release/$target/Plugins/cd"
	mkdir -p "./release/$target/Plugins/dsd"
	mkdir -p "./release/$target/Plugins/dts"
	mkdir -p "./release/$target/Plugins/sox"
	mkdir -p "./release/$target/Plugins/wasapi"
	mkdir -p "./release/$target/Plugins/sqlserver"
	mkdir -p "./release/$target/Plugins/simplemetadata"
	mkdir -p "./release/$target/Plugins/librarybrowser"
	mkdir -p "./release/$target/Plugins/metadataeditor"

	echo "Creating main package.."

	for file in $ADDON
	do
		echo "$file"
		cp "./distribution/$target/Addon/$file" "./release/$target/Main/Addon"
	done

	for file in $MAIN
	do
		if [ ! -f "./distribution/$target/$file" ]
		then
			echo "SKIPPING $file" 
			continue
		fi
		echo "$file"
		cp "./distribution/$target/$file" "./release/$target/Main"
	done

	cd "./release/$target/Main"

	"../../../.7z/7za.exe" a "FoxTunes-$TAG-$target-Minimal.zip" "*.*" -r

	mv "./FoxTunes-$TAG-$target-Minimal.zip" "../../"

	cd ..
	cd ..
	cd ..

	echo "Creating plugins package.."

	for file in $WINDOWS
	do
		echo $file
		cp "./distribution/$target/$file" "./release/$target/Plugins/windows"
	done

	for file in $ASIO
	do
		echo $file
		cp "./distribution/$target/$file" "./release/$target/Plugins/asio"
	done

	for file in $CD
	do
		echo $file
		cp "./distribution/$target/$file" "./release/$target/Plugins/cd"
	done

	for file in $DSD
	do
		echo $file
		cp "./distribution/$target/$file" "./release/$target/Plugins/dsd"
	done

	for file in $DTS
	do
		echo $file
		cp "./distribution/$target/$file" "./release/$target/Plugins/dts"
	done

	for file in $SOX
	do
		echo $file
		cp "./distribution/$target/$file" "./release/$target/Plugins/sox"
	done

	for file in $WASAPI
	do
		echo $file
		cp "./distribution/$target/$file" "./release/$target/Plugins/wasapi"
	done

	for file in $SQLSERVER
	do
			echo $file
			cp "./distribution/$target/$file" "./release/$target/Plugins/sqlserver"
	done

	for file in $SIMPLEMETADATA
	do
			echo $file
			cp "./distribution/$target/$file" "./release/$target/Plugins/simplemetadata"
	done

	for file in $LIBRARYBROWSER
    do
            echo $file
            cp "./distribution/$target/$file" "./release/$target/Plugins/librarybrowser"
    done

	for file in $METADATAEDITOR
    do
            echo $file
            cp "./distribution/$target/$file" "./release/$target/Plugins/metadataeditor"
    done

	cd "./release/$target/Plugins"

	"../../../.7z/7za.exe" a "FoxTunes-$TAG-Plugins-$target.zip" "*.*" -r

	mv "./FoxTunes-$TAG-Plugins-$target.zip" "../../"

	cd ..
	cd ..
	cd ..

	for bundled in $BUNDLED
	do
		echo "Installing plugin: $bundled"
		mkdir -p "./release/$target/Main/$bundled"
		cp "./release/$target/Plugins/$bundled/"* "./release/$target/Main/$bundled"
	done

	cd "./release/$target/Main"

	"../../../.7z/7za.exe" a "FoxTunes-$TAG-$target.zip" "*.*" -r

	mv "./FoxTunes-$TAG-$target.zip" "../../"

	cd ..
	cd ..
	cd ..

done

echo "All done."

