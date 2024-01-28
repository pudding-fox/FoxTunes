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
bassmidi.dll
bassopus.dll
basswma.dll
basswv.dll
"

LAUNCHER="
FoxTunes.Launcher.exe
FoxTunes.Launcher.exe.config
"

LIB="
FoxDb.Core.dll
FoxDb.Linq.dll
FoxDb.Sql.dll
FoxTunes.Core.dll
FoxTunes.DB.dll
FoxTunes.MetaData.dll
FoxTunes.Output.dll
FoxTunes.Scripting.dll
FoxTunes.UI.dll
Microsoft.Threading.Tasks.Extensions.Desktop.dll
Microsoft.Threading.Tasks.Extensions.dll
Microsoft.Threading.Tasks.dll
Microsoft.Windows.Shell.dll
System.IO.dll
System.Runtime.dll
System.Threading.Tasks.dll
System.Windows.Interactivity.dll
"

CONF="
FoxTunes.Config.dll
"

WPF="
FoxTunes.UI.Windows.Themes.dll
FoxTunes.UI.Windows.dll
"

BASS="
FoxTunes.Output.Bass.DirectSound.dll
FoxTunes.Output.Bass.Gapless.dll
FoxTunes.Output.Bass.dll
ManagedBass.Gapless.dll
ManagedBass.Mix.dll
ManagedBass.Fx.dll
ManagedBass.dll
bass.dll
bass_fx.dll
bass_gapless.dll
bass_inmemory_handler.dll
bassmix.dll
"

JS="
FoxTunes.Scripting.JS.dll
Noesis.Javascript.dll
msvcp100.dll
msvcr100.dll
"

WINDOWS="
FoxTunes.Core.Windows.dll
FoxTunes.Core.Windows.UWP.dll
"

ASIO="
FoxTunes.Output.Bass.Asio.dll
ManagedBass.Asio.dll
bass_asio_handler.dll
bassasio.dll
"

CD="
FoxTunes.Output.Bass.Cd.dll
ManagedBass.Cd.dll
ManagedBass.Gapless.Cd.dll
bass_gapless_cd.dll
basscd.dll
"

DSD="
FoxTunes.Output.Bass.Dsd.dll
ManagedBass.Dsd.dll
bass_inmemory_handler_dsd.dll
"

DTS="
FoxTunes.Output.Bass.Dts.dll
ManagedBass.Dts.dll
bass_dts.dll
"

SOX="
FoxTunes.Output.Bass.Resampler.dll
ManagedBass.Sox.dll
bass_sox.dll
"

WASAPI="
FoxTunes.Output.Bass.Wasapi.dll
ManagedBass.Wasapi.dll
bass_wasapi_handler.dll
basswasapi.dll
"

SQLITE="
FoxDb.SQLite.dll
FoxTunes.DB.SQLite.dll
System.Data.SQLite.dll
x86/SQLite.Interop.dll
"

SQLSERVER="
FoxDb.SqlServer.2012.dll
FoxDb.SqlServer.dll
FoxTunes.DB.SqlServer.dll
"

TAGLIBMETADATA="
FoxTunes.MetaData.TagLib.dll
taglib-sharp.dll
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

ENCODER="
FoxTunes.Encoder.Bass.exe
FoxTunes.Encoder.Bass.exe.config
"

ENCODERS="
flac.exe
flac_license.txt
lame.exe
lame_license.txt
nsutil.dll
oggenc2.exe
opusenc.exe
opusenc_license.txt
refalac.exe
sox.exe
sox_license.txt
wavpack.exe
wavpack_license.txt
"

EQ="
FoxTunes.Output.Bass.ParametricEqualizer.dll
"

TOOLS="
FoxTunes.Tools.dll
"

LOG="
FoxTunes.Logging.dll
"

REPLAYGAIN="
FoxTunes.Output.Bass.ReplayGain.exe
ManagedBass.ReplayGain.dll
bass_replay_gain.dll
"

CUE="
FoxTunes.Output.Bass.Cue.dll
bass_substream_handler.dll
"

MINIMAL="
bass
conf
js
sqlite
taglibmetadata
wpf
"

BUNDLED="
asio
cd
cue
dsd
dts
encoder
eq
librarybrowser
logger
metadataeditor
replaygain
sox
tools
wasapi
windows
"

TAG=$(git describe --abbrev=0 --tags)
#TAG="$(date +%F)-nightly"

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
	mkdir -p "./release/$target/Main/lib"
	mkdir -p "./release/$target/Plugins"
	mkdir -p "./release/$target/Plugins/asio"
	mkdir -p "./release/$target/Plugins/bass"
	mkdir -p "./release/$target/Plugins/bass/addon"
	mkdir -p "./release/$target/Plugins/cd"
	mkdir -p "./release/$target/Plugins/cue"
	mkdir -p "./release/$target/Plugins/conf"
	mkdir -p "./release/$target/Plugins/dsd"
	mkdir -p "./release/$target/Plugins/dts"
	mkdir -p "./release/$target/Plugins/encoder"
	mkdir -p "./release/$target/Plugins/encoder/encoders"
	mkdir -p "./release/$target/Plugins/eq"
	mkdir -p "./release/$target/Plugins/js"
	mkdir -p "./release/$target/Plugins/librarybrowser"
	mkdir -p "./release/$target/Plugins/logger"
	mkdir -p "./release/$target/Plugins/metadataeditor"
	mkdir -p "./release/$target/Plugins/replaygain"
	mkdir -p "./release/$target/Plugins/simplemetadata"
	mkdir -p "./release/$target/Plugins/sox"
	mkdir -p "./release/$target/Plugins/sqlite"
	mkdir -p "./release/$target/Plugins/sqlserver"
	mkdir -p "./release/$target/Plugins/taglibmetadata"
	mkdir -p "./release/$target/Plugins/tools"
	mkdir -p "./release/$target/Plugins/wasapi"
	mkdir -p "./release/$target/Plugins/windows"
	mkdir -p "./release/$target/Plugins/wpf"

	echo "Creating plugins package.."
	echo

	echo "Creating plugin: windows"
	for file in $WINDOWS
	do
		if [ ! -f "./distribution/$target/$file" ]
		then
			echo "SKIPPING $file" 
			continue
		fi
		echo $file
		cp "./distribution/$target/$file" "./release/$target/Plugins/windows"
	done
	echo

	echo "Creating plugin: asio"
	for file in $ASIO
	do
		echo $file
		cp "./distribution/$target/$file" "./release/$target/Plugins/asio"
	done
	echo

	echo "Creating plugin: cd"
	for file in $CD
	do
		echo $file
		cp "./distribution/$target/$file" "./release/$target/Plugins/cd"
	done
	echo

	echo "Creating plugin: dsd"
	for file in $DSD
	do
		echo $file
		cp "./distribution/$target/$file" "./release/$target/Plugins/dsd"
	done
	echo

	echo "Creating plugin: dts"
	for file in $DTS
	do
		echo $file
		cp "./distribution/$target/$file" "./release/$target/Plugins/dts"
	done
	echo

	echo "Creating plugin: sox"
	for file in $SOX
	do
		echo $file
		cp "./distribution/$target/$file" "./release/$target/Plugins/sox"
	done
	echo

	echo "Creating plugin: wasapi"
	for file in $WASAPI
	do
		echo $file
		cp "./distribution/$target/$file" "./release/$target/Plugins/wasapi"
	done
	echo

	echo "Creating plugin: sqlserver"
	for file in $SQLSERVER
	do
			echo $file
			cp "./distribution/$target/$file" "./release/$target/Plugins/sqlserver"
	done
	echo

	echo "Creating plugin: simplemetadata"
	for file in $SIMPLEMETADATA
	do
			echo $file
			cp "./distribution/$target/$file" "./release/$target/Plugins/simplemetadata"
	done
	echo

	echo "Creating plugin: librarybrowser"
	for file in $LIBRARYBROWSER
    do
            echo $file
            cp "./distribution/$target/$file" "./release/$target/Plugins/librarybrowser"
    done
	echo

	echo "Creating plugin: metadataeditor"
	for file in $METADATAEDITOR
    do
            echo $file
            cp "./distribution/$target/$file" "./release/$target/Plugins/metadataeditor"
    done
	echo

	echo "Creating plugin: encoder"
	for file in $ENCODER
    do
            echo $file
            cp "./distribution/$target/$file" "./release/$target/Plugins/encoder"
    done
	echo

	echo "Installing encoders (bass)"
	for file in $ENCODERS
    do
            echo $file
            cp "./distribution/$target/Encoders/$file" "./release/$target/Plugins/encoder/encoders"
    done
	echo

	echo "Creating plugin: eq"
    for file in $EQ
    do
            echo $file
            cp "./distribution/$target/$file" "./release/$target/Plugins/eq"
    done
	echo

	echo "Creating plugin: tools"
	for file in $TOOLS
    do
            echo $file
            cp "./distribution/$target/$file" "./release/$target/Plugins/tools"
    done
	echo
	
	echo "Creating plugin: js"
	for file in $JS
    do
            echo $file
            cp "./distribution/$target/$file" "./release/$target/Plugins/js"
    done
	echo
	
	echo "Creating plugin: bass"
	for file in $BASS
    do
            echo $file
            cp "./distribution/$target/$file" "./release/$target/Plugins/bass"
    done
	echo
	
	echo "Installing addons (bass)"
	for file in $ADDON
	do
		echo "$file"
		cp "./distribution/$target/Addon/$file" "./release/$target/Plugins/bass/addon"
	done
	echo
	
	echo "Creating plugin: sqlite"
	for file in $SQLITE
    do
            echo $file
            cp "./distribution/$target/$file" "./release/$target/Plugins/sqlite"
    done
	echo
	
	echo "Creating plugin: taglibmetadata"
	for file in $TAGLIBMETADATA
    do
            echo $file
            cp "./distribution/$target/$file" "./release/$target/Plugins/taglibmetadata"
    done
	echo

	echo "Creating plugin: logger"
	for file in $LOG
    do
            echo $file
            cp "./distribution/$target/$file" "./release/$target/Plugins/logger"
    done
	echo

	echo "Creating plugin: conf"
	for file in $CONF
    do
            echo $file
            cp "./distribution/$target/$file" "./release/$target/Plugins/conf"
    done
	echo

	echo "Creating plugin: wpf"
	for file in $WPF
    do
            echo $file
            cp "./distribution/$target/$file" "./release/$target/Plugins/wpf"
    done
	echo

	echo "Creating plugin: replaygain"
	for file in $REPLAYGAIN
    do
            echo $file
            cp "./distribution/$target/$file" "./release/$target/Plugins/replaygain"
    done
	echo

	echo "Creating plugin: cue"
	for file in $CUE
    do
            echo $file
            cp "./distribution/$target/$file" "./release/$target/Plugins/cue"
    done
	echo

	cd "./release/$target/Plugins"

	"../../../.7z/7za.exe" a "FoxTunes-$TAG-Plugins-$target.zip" "*.*" -r

	mv "./FoxTunes-$TAG-Plugins-$target.zip" "../../"

	cd ..
	cd ..
	cd ..
	
	echo "Creating main package.."

	for file in $LAUNCHER
	do
		echo "$file"
		cp "./distribution/$target/$file" "./release/$target/Main"
	done

	for file in $LIB
	do
		if [ ! -f "./distribution/$target/$file" ]
		then
			echo "SKIPPING $file" 
			continue
		fi
		echo "$file"
		cp "./distribution/$target/$file" "./release/$target/Main/lib"
	done
	
	for minimal in $MINIMAL
	do
		echo "Installing plugin (Minimal): $minimal"
		mkdir -p "./release/$target/Main/lib/$minimal"
		cp -r "./release/$target/Plugins/$minimal/"* "./release/$target/Main/lib/$minimal"
	done

	cd "./release/$target/Main"

	echo "Setting the release type to minimal..";
	sed -i 's/key="ReleaseType"\s\+value=".*"/key="ReleaseType" value="Minimal"/' "FoxTunes.Launcher.exe.config"

	"../../../.7z/7za.exe" a "FoxTunes-$TAG-$target-Minimal.zip" "*.*" -r

	mv "./FoxTunes-$TAG-$target-Minimal.zip" "../../"

	cd ..
	cd ..
	cd ..

	for bundled in $BUNDLED
	do
		echo "Installing plugin (Bundled): $bundled"
		mkdir -p "./release/$target/Main/lib/$bundled"
		cp -r "./release/$target/Plugins/$bundled/"* "./release/$target/Main/lib/$bundled"
	done

	cd "./release/$target/Main"

	echo "Setting the release type to default..";
	sed -i 's/key="ReleaseType"\s\+value=".*"/key="ReleaseType" value="Default"/' "FoxTunes.Launcher.exe.config"

	"../../../.7z/7za.exe" a "FoxTunes-$TAG-$target.zip" "*.*" -r

	mv "./FoxTunes-$TAG-$target.zip" "../../"

	cd ..
	cd ..
	cd ..

done
echo

echo "All done."

