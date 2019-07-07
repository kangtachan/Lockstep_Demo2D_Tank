#!/bin/bash
dir="$(cd $(dirname ${BASH_SOURCE[0]}) && pwd)"
cd $dir
pwd
mono ../../Tools/bin/Lockstep.Tools.ExcelParser.dll ../Config/ExcelParser/Tank.json
rm -rf Assets/Scripts/Tables/*
cp -rf ../../Data/Common/Tank/AutoGen/CodeCS/* Assets/Scripts/Tables