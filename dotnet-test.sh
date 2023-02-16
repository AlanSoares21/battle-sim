#!/bin/bash
DotnetTestsPassedCode=0
cd Engine/Tests
dotnet test
if [ $? != $DotnetTestsPassedCode ]; then
    echo "Error on test Engine. Dotnet Test Code: $?"
    exit 127;
fi
echo "Succes on test Engine"
cd ../../Server/Tests
dotnet test
if [ $? != $DotnetTestsPassedCode ]; then
    echo "Error on test Server. Dotnet Test Code: $?"
    exit 127;
fi 