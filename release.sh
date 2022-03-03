#!/bin/sh

PLATFORM="
x86
x64
"

TARGET="
net40
net462
"

if [ -z "$1" ]
then
	version=$(git describe --abbrev=0 --tags)
else
	version="$(date +%F)-nightly"
fi

echo "Current version is $version.."
sleep 1
echo "3.."
sleep 1
echo "2.."
sleep 1
echo "1.."
sleep 1

cd "./release"
"./FoxTunes.Release.exe" "$version"
cd ..

rm -f ./release/*.zip

for platform in $PLATFORM
do
	for target in $TARGET
	do

		if [ ! -d "./release/$platform/$target" ]
		then
			echo "Source was not built: ${platform}/${target}"
			continue
		fi

		cd "./release/$platform/$target/Main"

		echo "Setting the release type to default..";
		sed -i 's/key="ReleaseType"\s\+value=".*"/key="ReleaseType" value="Default"/' "FoxTunes.Launcher.exe.config"

		"../../../../.7z/7za.exe" a "FoxTunes-$version-$target-$platform.zip" "*.*" -r

		mv "./FoxTunes-$version-$target-$platform.zip" "../../../"

		cd ..
		cd ..
		cd ..
		cd ..

		echo "Creating minimal package.."

		cd "./release/$platform/$target/Minimal"

		echo "Setting the release type to minimal..";
		sed -i 's/key="ReleaseType"\s\+value=".*"/key="ReleaseType" value="Minimal"/' "FoxTunes.Launcher.exe.config"

		"../../../../.7z/7za.exe" a "FoxTunes-$version-$target-$platform-Minimal.zip" "*.*" -r

		mv "./FoxTunes-$version-$target-$platform-Minimal.zip" "../../../"

		cd ..
		cd ..
		cd ..
		cd ..

		echo "Creating plugins package.."

		cd "./release/$platform/$target/Plugins"

		"../../../../.7z/7za.exe" a "FoxTunes-$version-Plugins-$target-$platform.zip" "*.*" -r

		mv "./FoxTunes-$version-Plugins-$target-$platform.zip" "../../../"

		cd ..
		cd ..
		cd ..
		cd ..

	done
	echo
done
echo

echo "All done."

