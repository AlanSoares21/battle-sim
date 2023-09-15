## Created by chat GPT
import os
import json
from PIL import Image

def create_assets_folder(folder_path):
    print("creating assets with images in " + folder_path)
    # Lista todos os arquivos .png no diretório fornecido
    png_files = [file for file in os.listdir(folder_path) if file.endswith('.png') and not file.startswith('assets')]

    # Cria um dicionário para armazenar as informações de cada imagem
    asset_info = {}
    
    print("handling " + str(len(png_files)) + " files")
    if len(png_files) == 0:
        print("no files found in " + folder_path + " to be handled")
        return
    
    # Inicializa o arquivo assets.png
    current_position = 0
    assetFileHeight = 0
    assetFileWidth = 0
    for png_file in png_files:
        file_path = os.path.join(folder_path, png_file)
        image = Image.open(file_path)
        assetFileHeight += image.size[1]
        if (image.size[0] > assetFileWidth):
            assetFileWidth = image.size[0]

    # Cria uma nova imagem vazia para armazenar todas as imagens .png
    assets_image = Image.new('RGBA', (assetFileWidth, assetFileHeight), (0, 0, 0, 0))

    for png_file in png_files:
        file_path = os.path.join(folder_path, png_file)

        # Abre a imagem original
        image = Image.open(file_path)

        # Armazena as informações da imagem no dicionário
        asset_info[png_file[:-4]] = {
            'size': { 'width': image.size[0], 'height': image.size[1] },
            'start': { 'x': 0, 'y': current_position }
        }

        # Adiciona a imagem original à imagem de assets.png
        assets_image.paste(image, (0, current_position))

        # Atualiza a posição atual para a próxima imagem
        current_position += image.height

    # Salva a imagem de assets.png
    assets_image.save(os.path.join(folder_path, 'assets.png'))

    # Salva as informações no arquivo assets.map.json
    with open(os.path.join(folder_path, 'assets.map.json'), 'w') as json_file:
        json.dump(asset_info, json_file, indent=4)
    print("assets files created")

folder_path = ''

try:
    folder_path = os.environ['AssetsFolder']
except:
    print("Error when try get the AssetsFolder variable")
    exit()

if (len(folder_path) == 0):
    print('you should provide the AssetsFolder variable')
create_assets_folder(folder_path)