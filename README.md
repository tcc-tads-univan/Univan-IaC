# Guia de Uso do Docker Compose para o Projeto Univan-IaC

Este repositório contém o código-fonte e a configuração necessária para executar todos os serviços do projeto Univan usando Docker Compose.

## Pré-requisitos

Antes de começar, certifique-se de ter o Docker e o Docker Compose instalados no seu sistema. Você pode encontrar instruções de instalação [aqui](https://docs.docker.com/get-docker/) e [aqui](https://docs.docker.com/compose/install/).

## Clonando o Repositório

Para começar, clone este repositório usando o seguinte comando:

```
git clone git@github.com:tcc-tads-univan/Univan-IaC.git
```

Isso criará uma cópia local do repositório no seu sistema.

## Configuração

    Navegue até o diretório do projeto:
```
cd Univan-IaC
```
    Se necessário, ajuste as configurações no arquivo docker-compose.yml conforme necessário para o seu ambiente.

## Inicializando o Projeto

Agora, você está pronto para iniciar o projeto usando o Docker Compose. Execute o seguinte comando:

```bash
docker-compose up -d
```
Isso iniciará os contêineres conforme especificado no arquivo docker-compose.yml. Os contêineres serão iniciados em segundo plano (modo detached).

## Parando o Projeto

Se você precisar parar os contêineres, execute:

```bash
docker-compose down
```
Isso encerrará os contêineres e removerá os recursos associados.


## Acesso ao Aplicativo

Após iniciar o projeto, você pode acessar o swagger em: 

```bash
Univan-van: http://localhost:5000/swagger/index.html
Carpool-ms: http://localhost:5001/swagger/index.html
History-ms: http://localhost:5003/swagger/index.html
Routes-ms:  http://localhost:5004/swagger-ui/index.html
```
### Espero que este guia seja útil! Se você tiver algum problema ou dúvida, sinta-se à vontade para abrir uma issue neste repositório.

