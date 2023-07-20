#!/bin/bash
if [ -z $AssetsUrl ]; then
        echo "You should provide the url to download the assets"
        exit 1;
fi
whoami
curl $AssetsUrl > assets.zip
unzip assets.zip -d extracted
mv extracted/**/*.png public/assets/
pip install Pillow
python create_assets.py