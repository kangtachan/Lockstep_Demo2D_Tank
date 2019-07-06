#!/bin/bash
dir="$(cd $(dirname ${BASH_SOURCE[0]}) && pwd)"
cd $dir
pwd
result="$(mono ../../Tools/bin/Lockstep.Tools.ExcelParser.dll Tank.json)"

rm -rf Assets/Scripts/Tables/*
cp -rf ../../Data/Common/Tank/AutoGen/CodeCS/* Assets/Scripts/Tables