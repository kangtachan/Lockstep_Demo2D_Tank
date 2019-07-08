#!/bin/bash
dir="$(cd $(dirname ${BASH_SOURCE[0]}) && pwd)"
cd $dir
pwd
echo " ------------ Build LPGame ...-------------"
msbuild /property:Configuration=Debug /p:WarningLevel=0 /verbosity:minimal ./LPGame.sln

sleep 3