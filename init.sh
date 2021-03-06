#/bin/bash
if [[ $1 == "" ]]; then
	echo Copies Spewnity Assets and sets up a default structure of asset folders in the unity project folder specified
	echo Usage: init.sh path-to-unity-folder
	exit
fi

function badFolder {
	echo Missing Unity Folder: $1
	echo Aborting...
	exit
}

echo Verifying $1 is a Unity project folder
path="$1/ProjectSettings/"
if [[ ! -d "$path" ]]; then
	badFolder $path
fi
path="$1/Assets/"
if [[ ! -d "$path" ]]; then
	badFolder $path
fi

dir=`dirname $0`

echo Copying gitignore
cp "${dir}/.gitignore" $1

echo Copying Assets
cp -r "$dir/Assets/Spewnity" "$1/Assets/"

echo Creating common folders
mkdir -p "$1/Assets/Scenes/"
mkdir -p "$1/Assets/Scripts/"
mkdir -p "$1/Assets/Materials/"
mkdir -p "$1/Assets/Art/"
mkdir -p "$1/Assets/Audio/"
mkdir -p "$1/Assets/Prefabs/"
mkdir -p "$1/Assets/Fonts/"
mkdir -p "$1/Raw/AudioRecs"