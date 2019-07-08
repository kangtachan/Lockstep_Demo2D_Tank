#!/bin/bash
clear
dir="$(cd $(dirname ${BASH_SOURCE[0]}) && pwd)"
cd $dir
ps -ef | grep "Test.Server" | grep -v grep |awk '{print $2}' | xargs kill -9
ps -ef | grep "LPClient.app" | grep -v grep |awk '{print $2}' | xargs kill -9
open -n Build/LPClient.app
mono ../../../LockstepPlatform/Libs/Server/Lockstep.Test.Server.dll
