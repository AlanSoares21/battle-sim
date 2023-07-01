# Battle Simulator
Inicialmente, o propósito principal desse projeto era poder simular batalhas de um jogo específico e por isso o nome "Battle Simulator", porém, essa motivação foi abandonada com o tempo.

## Requirements
- Docker
- ... só isso mesmo.


## Run
Se você deseja rodar em sua máquina, clone o repositório e execute os scripts `build-server.sh` e `build-webclient.sh` para gerar as imagens do servidor e do cliente.
Para configurar as variáveis de ambiente, execute:
- `export JwtSecret=sua-secret-key`
- `export ApiUrl=http://localhost:3002`
- `export SiteUrl=http://localhost:3000`

Para finalizar, execute os scripts `run-server.sh` e `run-webclient.sh`. 
Obs: devido a um problema na forma como o WebClient é rodado você terá que esperar por um tempo antes de acessar http://localhhost:3000 e poder jogar no seu servidor local.
