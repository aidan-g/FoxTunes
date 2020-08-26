#!/bin/sh

PLATFORM="
x86
x64
"

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

ADDON_MOD="
basszxtune.dll
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
V8.Net.dll
V8_Net_Proxy_x64.dll
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

SQLITE_X86="
FoxDb.SQLite.dll
FoxTunes.DB.SQLite.dll
System.Data.SQLite.dll
x86/SQLite.Interop.dll
"

SQLITE_X64="
FoxDb.SQLite.dll
FoxTunes.DB.SQLite.dll
System.Data.SQLite.dll
x64/SQLite.Interop.dll
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

MOD="
FoxTunes.Output.Bass.Mod.dll
"

CROSSFADE="
FoxTunes.Output.Bass.Crossfade.dll
ManagedBass.Crossfade.dll
bass_crossfade.dll
"

SPECTRUM="
FoxTunes.UI.Windows.Spectrum.dll
"

WAVEBAR="
FoxTunes.UI.Windows.WaveBar.dll
"

RATINGS="
FoxTunes.UI.Windows.Ratings.dll
"

STATISTICS="
FoxTunes.Statistics.dll
"

LAYOUT="
FoxTunes.UI.Windows.Layout.dll
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
crossfade
cue
dsd
dts
encoder
eq
layout
librarybrowser
logger
metadataeditor
ratings
replaygain
sox
spectrum
statistics
tools
wasapi
wavebar
windows
"

if [ -z "$1" ]
then
	TAG=$(git describe --abbrev=0 --tags)
else
	TAG="$(date +%F)-nightly"
fi

echo "Current version is $TAG.."
sleep 1
echo "3.."
sleep 1
echo "2.."
sleep 1
echo "1.."
sleep 1

rm -rf "./release"

for platform in $PLATFORM
do
	for target in $TARGET
	do

		mkdir -p "./release/$platform/$target/Main"
		mkdir -p "./release/$platform/$target/Main/lib"
		mkdir -p "./release/$platform/$target/Plugins"
		mkdir -p "./release/$platform/$target/Plugins/asio"
		mkdir -p "./release/$platform/$target/Plugins/bass"
		mkdir -p "./release/$platform/$target/Plugins/bass/addon"
		mkdir -p "./release/$platform/$target/Plugins/cd"
		mkdir -p "./release/$platform/$target/Plugins/conf"
		mkdir -p "./release/$platform/$target/Plugins/crossfade"
		mkdir -p "./release/$platform/$target/Plugins/cue"
		mkdir -p "./release/$platform/$target/Plugins/dsd"
		mkdir -p "./release/$platform/$target/Plugins/dts"
		mkdir -p "./release/$platform/$target/Plugins/encoder"
		mkdir -p "./release/$platform/$target/Plugins/encoder/encoders"
		mkdir -p "./release/$platform/$target/Plugins/eq"
		mkdir -p "./release/$platform/$target/Plugins/js"
		mkdir -p "./release/$platform/$target/Plugins/layout"
		mkdir -p "./release/$platform/$target/Plugins/librarybrowser"
		mkdir -p "./release/$platform/$target/Plugins/logger"
		mkdir -p "./release/$platform/$target/Plugins/metadataeditor"
		mkdir -p "./release/$platform/$target/Plugins/mod"
		mkdir -p "./release/$platform/$target/Plugins/mod/addon"
		mkdir -p "./release/$platform/$target/Plugins/ratings"
		mkdir -p "./release/$platform/$target/Plugins/replaygain"
		mkdir -p "./release/$platform/$target/Plugins/simplemetadata"
		mkdir -p "./release/$platform/$target/Plugins/sox"
		mkdir -p "./release/$platform/$target/Plugins/spectrum"
		mkdir -p "./release/$platform/$target/Plugins/sqlite"
		mkdir -p "./release/$platform/$target/Plugins/sqlserver"
		mkdir -p "./release/$platform/$target/Plugins/statistics"
		mkdir -p "./release/$platform/$target/Plugins/taglibmetadata"
		mkdir -p "./release/$platform/$target/Plugins/tools"
		mkdir -p "./release/$platform/$target/Plugins/wasapi"
		mkdir -p "./release/$platform/$target/Plugins/wavebar"
		mkdir -p "./release/$platform/$target/Plugins/windows"
		mkdir -p "./release/$platform/$target/Plugins/wpf"

		echo "Creating plugins package.."
		echo

		echo "Creating plugin: windows"
		for file in $WINDOWS
		do
			if [ ! -f "./distribution/$platform/$target/$file" ]
			then
				echo "SKIPPING $file" 
				continue
			fi
			echo $file
			cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/windows"
		done
		echo

		echo "Creating plugin: asio"
		for file in $ASIO
		do
			echo $file
			cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/asio"
		done
		echo

		echo "Creating plugin: cd"
		for file in $CD
		do
			echo $file
			cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/cd"
		done
		echo

		echo "Creating plugin: dsd"
		for file in $DSD
		do
			echo $file
			cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/dsd"
		done
		echo

		echo "Creating plugin: dts"
		for file in $DTS
		do
			echo $file
			cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/dts"
		done
		echo

		echo "Creating plugin: sox"
		for file in $SOX
		do
			echo $file
			cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/sox"
		done
		echo

		echo "Creating plugin: wasapi"
		for file in $WASAPI
		do
			echo $file
			cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/wasapi"
		done
		echo

		echo "Creating plugin: sqlserver"
		for file in $SQLSERVER
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/sqlserver"
		done
		echo

		echo "Creating plugin: simplemetadata"
		for file in $SIMPLEMETADATA
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/simplemetadata"
		done
		echo

		echo "Creating plugin: librarybrowser"
		for file in $LIBRARYBROWSER
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/librarybrowser"
		done
		echo

		echo "Creating plugin: metadataeditor"
		for file in $METADATAEDITOR
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/metadataeditor"
		done
		echo

		echo "Creating plugin: encoder"
		for file in $ENCODER
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/encoder"
		done
		echo

		echo "Installing encoders (bass)"
		for file in $ENCODERS
		do
				echo $file
				cp "./distribution/$platform/$target/Encoders/$file" "./release/$platform/$target/Plugins/encoder/encoders"
		done
		echo

		echo "Creating plugin: eq"
		for file in $EQ
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/eq"
		done
		echo

		echo "Creating plugin: tools"
		for file in $TOOLS
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/tools"
		done
		echo
	
		echo "Creating plugin: js"
		for file in $JS
		do
				if [ ! -f "./distribution/$platform/$target/$file" ]
				then
					echo "SKIPPING $file" 
					continue
				fi
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/js"
		done
		echo
	
		echo "Creating plugin: bass"
		for file in $BASS
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/bass"
		done
		echo
	
		echo "Installing addons (bass)"
		for file in $ADDON
		do
			echo "$file"
			cp "./distribution/$platform/$target/Addon/$file" "./release/$platform/$target/Plugins/bass/addon"
		done
		echo
	
		echo "Creating plugin: sqlite"
		if [ $platform = "x86" ]
		then
			SQLITE=$SQLITE_X86
		elif [ $platform = "x64" ]
		then
			SQLITE=$SQLITE_X64
		else
			echo "Unsupported platform: ${platform}"
		fi
		for file in $SQLITE
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/sqlite"
		done
		echo
	
		echo "Creating plugin: taglibmetadata"
		for file in $TAGLIBMETADATA
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/taglibmetadata"
		done
		echo

		echo "Creating plugin: logger"
		for file in $LOG
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/logger"
		done
		echo

		echo "Creating plugin: conf"
		for file in $CONF
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/conf"
		done
		echo

		echo "Creating plugin: wpf"
		for file in $WPF
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/wpf"
		done
		echo

		echo "Creating plugin: replaygain"
		for file in $REPLAYGAIN
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/replaygain"
		done
		echo

		echo "Creating plugin: cue"
		for file in $CUE
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/cue"
		done
		echo

		echo "Creating plugin: mod"
		for file in $MOD
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/mod"
		done
		echo

		echo "Installing addons (mod)"
		for file in $ADDON_MOD
		do
			echo "$file"
			cp "./distribution/$platform/$target/Addon/$file" "./release/$platform/$target/Plugins/mod/addon"
		done
		echo

		echo "Creating plugin: crossfade"
		for file in $CROSSFADE
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/crossfade"
		done
		echo

		echo "Creating plugin: spectrum"
		for file in $SPECTRUM
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/spectrum"
		done
		echo

		echo "Creating plugin: wavebar"
		for file in $WAVEBAR
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/wavebar"
		done
		echo

		echo "Creating plugin: ratings"
		for file in $RATINGS
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/ratings"
		done
		echo

		echo "Creating plugin: statistics"
		for file in $STATISTICS
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/statistics"
		done
		echo

		echo "Creating plugin: layout"
		for file in $LAYOUT
		do
				echo $file
				cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Plugins/layout"
		done
		echo

		cd "./release/$platform/$target/Plugins"

		"../../../../.7z/7za.exe" a "FoxTunes-$TAG-Plugins-$target-$platform.zip" "*.*" -r

		mv "./FoxTunes-$TAG-Plugins-$target-$platform.zip" "../../../"

		cd ..
		cd ..
		cd ..
		cd ..
	
		echo "Creating main package.."

		for file in $LAUNCHER
		do
			echo "$file"
			cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Main"
		done

		for file in $LIB
		do
			if [ ! -f "./distribution/$platform/$target/$file" ]
			then
				echo "SKIPPING $file" 
				continue
			fi
			echo "$file"
			cp "./distribution/$platform/$target/$file" "./release/$platform/$target/Main/lib"
		done
	
		for minimal in $MINIMAL
		do
			echo "Installing plugin (Minimal): $minimal"
			mkdir -p "./release/$platform/$target/Main/lib/$minimal"
			cp -r "./release/$platform/$target/Plugins/$minimal/"* "./release/$platform/$target/Main/lib/$minimal"
		done

		cd "./release/$platform/$target/Main"

		echo "Setting the release type to minimal..";
		sed -i 's/key="ReleaseType"\s\+value=".*"/key="ReleaseType" value="Minimal"/' "FoxTunes.Launcher.exe.config"

		"../../../../.7z/7za.exe" a "FoxTunes-$TAG-$target-$platform-Minimal.zip" "*.*" -r

		mv "./FoxTunes-$TAG-$target-$platform-Minimal.zip" "../../../"

		cd ..
		cd ..
		cd ..
		cd ..

		for bundled in $BUNDLED
		do
			echo "Installing plugin (Bundled): $bundled"
			mkdir -p "./release/$platform/$target/Main/lib/$bundled"
			cp -r "./release/$platform/$target/Plugins/$bundled/"* "./release/$platform/$target/Main/lib/$bundled"
		done

		cd "./release/$platform/$target/Main"

		echo "Setting the release type to default..";
		sed -i 's/key="ReleaseType"\s\+value=".*"/key="ReleaseType" value="Default"/' "FoxTunes.Launcher.exe.config"

		"../../../../.7z/7za.exe" a "FoxTunes-$TAG-$target-$platform.zip" "*.*" -r

		mv "./FoxTunes-$TAG-$target-$platform.zip" "../../../"

		cd ..
		cd ..
		cd ..
		cd ..

	done
	echo
done
echo

echo "All done."

