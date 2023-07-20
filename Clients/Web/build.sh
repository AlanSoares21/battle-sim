#!/bin/bash
if [ -n "$CodeSrc" ]; then
    echo "Changing working dir to $CodeSrc"
    cd $CodeSrc
fi
# building image with source code
npm install --all
npm run build