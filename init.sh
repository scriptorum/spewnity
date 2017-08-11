#/bin/bash
if [[ $1 == "" ]]; then
	echo Copies Spewnity Assets and sets up a default set of asset folders in the folder specified
	echo Will overwrite Assets/Spewnity, Assets/Editor, Assets/Shaders, and .gitignore!!!!
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

echo Copying Spewnity
dir=`dirname $0`
cp -rf "${dir}/Assets/"* $path

echo Copying gitignore
cp "${dir}/.gitignore" $path

echo Creating common folders
mkdir -p "${dir}/Assets/Scenes/"
mkdir -p "${dir}/Assets/Scripts/"
mkdir -p "${dir}/Assets/Materials/"
mkdir -p "${dir}/Assets/Art/"
mkdir -p "${dir}/Assets/Audio/"
mkdir -p "${dir}/Assets/Prefabs/"
mkdir -p "${dir}/Assets/Fonts/"
