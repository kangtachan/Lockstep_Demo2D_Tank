#!/bin/bash
echo " ------------LockstepPlatform Setup -------------"
dir="$(cd $(dirname ${BASH_SOURCE[0]}) && pwd)"
cd $dir
pwd
_count=5
echo " ------------(1/$(_count))Build LPEngine ...-------------"
_idx=$_idx + 1
#msbuild /property:Configuration=Debug /p:WarningLevel=0 /verbosity:minimal ../LockstepPlatform/LPEngine.sln
#echo " ------------(2/$(_count))Build LockstepPlatform ...-------------"
#msbuild /property:Configuration=Debug /p:WarningLevel=0 /verbosity:minimal ../LockstepPlatform/LockstepPlatform.sln

echo " ------------(3/$(_count))Copy libs  ...-------------"
mkdir -p Libs
cp -rf ../LockstepPlatform/Libs/ ./Libs/

echo " ------------(4/$(_count))Copy Tools  ...-------------"
mkdir -p ./Tools/bin/
mkdir -p ./Tools/Src/
cp -rf ../LockstepPlatform/Tools/bin/ ./Tools/bin/
cp -rf ../LockstepPlatform/Tools/Config/ ./Tools/Config/
cp -rf ../LockstepPlatform/Tools/Src/*ECS* ./Tools/Src/
cp -rf ../LockstepPlatform/Tools/Build*.sh ./Tools/

echo " ------------(5/$(_count))Build LPGame ...-------------"
msbuild /property:Configuration=Debug /p:WarningLevel=0 /verbosity:minimal ./LPGame.sln


echo " ------------(6/$(_count))Prepare Client CopySources ...-------------"
cd Src/Unity
pwd
./CopySources.sh

echo " ------------(7/$(_count))Prepare Client GenConfig ...-------------"
./GenConfig.sh

echo "Setup done :)"
sleep 3