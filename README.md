
# Battle Simulator
Inicialmente, o propósito principal desse projeto era poder simular batalhas de um jogo específico e por isso o nome "Battle Simulator", porém, essa motivação foi abandonada com o tempo.

## Requirements
- Docker
- Disponibilizar uma URL para um arquivo .zip contendo os assets do jogo. 
Você pode encontrar assets usados durante o desenvolvimento em https://drive.google.com/drive/folders/1JFAnP7LytnR-3OEc99azKfu0CGtDNcPt
Ao clicar em "fazer o download", o navegador irá baixar um arquivo .zip, é possível utilizar a url que o navegador usa para fazer esse download.

## Run
Se você deseja rodar em sua máquina, primeiro, clone o repositório.                                                  
Abra o terminal e configure as variáveis de ambiente executando:                       
- `export JwtSecret=sua-secret-key`
- `export ApiUrl=http://localhost:3002`
- `export SiteUrl=http://localhost:3000` 
- `export AssetsUrl=http://assets-server.com/files/assets.zip`
Não se esqueça de alterar o valor de `JwtSecret` e `AssetsUrl`!                                                          

Execute os scripts `build-server.sh` e `build-webclient.sh` para gerar as imagens do servidor e do cliente.

Para finalizar, execute os scripts `run-server.sh` e `run-webclient.sh`. 

