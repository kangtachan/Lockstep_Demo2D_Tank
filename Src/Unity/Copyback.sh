#!/bin/bash
dir="$(cd $(dirname ${BASH_SOURCE[0]}) && pwd)"
cd $dir
pwd
mono ../../Tools/bin/Lockstep.Tools.CopySourceFiles.dll ../../Tools/Config/CopySourceFiles/Tank2Common.json