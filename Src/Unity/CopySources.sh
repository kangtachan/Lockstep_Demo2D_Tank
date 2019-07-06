#!/bin/bash
dir="$(cd $(dirname ${BASH_SOURCE[0]}) && pwd)"
cd $dir
pwd
mono ../../Tools/bin/Lockstep.Tools.CopySourceFiles.dll ../../Tools/Config/CopySourceFiles/Common2Tank.json