# Desbravadores.Gestao

API REST em **ASP.NET Core 10** para gestão de usuários, autenticação JWT e controle de sessão ativo no banco de dados. O projeto foi estruturado em camadas para manter regra de negócio isolada, facilitar manutenção e deixar a evolução da API previsível.

## O que a API entrega hoje

- **Autenticação com JWT** (access token + refresh token) na rota de login.
- **Sessões rastreadas por JTI**: cada login gera sessão persistida e o token só é aceito se a sessão estiver ativa.
- **Logout com revogação de sessão**: invalida a sessão do token atual.
- **Endpoint `/auth/me`** para retornar os dados do usuário autenticado.
- **Gestão completa de usuários**: criar, listar, buscar por ID, atualizar e excluir.
- **Autorização por políticas e papéis** para restringir endpoints sensíveis.
- **Validação de entrada com FluentValidation**.
- **Documentação interativa via Swagger**.

## Arquitetura

A solução segue separação clara de responsabilidades:

- `src/Desbravadores.Gestao.Api`  
  Camada HTTP (controllers, autenticação JWT, políticas de autorização, Swagger e pipeline da aplicação).

- `src/Desbravadores.Gestao.Application`  
  Casos de uso com MediatR (commands/queries), validações e orquestração da regra de aplicação.

- `src/Desbravadores.Gestao.Domain`  
  Entidades, DTOs, contratos e enumerações de domínio.

- `src/Desbravadores.Gestao.Infrastructure`  
  Persistência com Entity Framework Core, repositórios e serviços de segurança (hash de senha e geração de token).

- `tests/Desbravadores.Gestao.UnitTests`  
  Projeto reservado para testes unitários.

## Modelo de autorização

A API utiliza autenticação **Bearer JWT** e políticas por role:

- **`MasterOnly`**: permite `DIRETORIA` e `SECRETARIA`.
- **`Financeiro`**: permite `TESOURARIA` e `DIRETORIA`.

Perfis definidos no domínio:

- `DIRETORA`
- `SECRETARIA`
- `TESOURARIA`
- `DIRETORIA`
- `DESBRAVADOR`

## Fluxo de autenticação e sessão

1. O usuário autentica via `POST /api/auth/login`.
2. A API valida credenciais, revoga sessões ativas anteriores do mesmo usuário e cria uma nova sessão.
3. O token inclui `sub` (UUID do usuário), `jti`, `email`, `name` e `role`.
4. A cada requisição autenticada, além da validação padrão do JWT, a API verifica no banco se aquela sessão (`jti`) ainda está ativa.
5. Em `POST /api/auth/logout`, a sessão atual é revogada.

## Endpoints disponíveis

### Autenticação

- `POST /api/auth/login`  
  Realiza login e retorna token + metadados de sessão.

- `POST /api/auth/logout` _(autenticado)_  
  Revoga a sessão atual.

- `GET /api/auth/me` _(autenticado)_  
  Retorna os dados do usuário autenticado.

### Usuários

- `POST /api/usuarios` _(política `MasterOnly`)_  
  Cria usuário.

- `POST /api/usuarios/publicos` _(anônimo)_  
  Cria usuário sem exigir autenticação.

- `GET /api/usuarios` _(política `Financeiro`)_  
  Lista usuários.

- `GET /api/usuarios/{id}` _(política `MasterOnly`)_  
  Busca usuário por UUID.

- `PUT /api/usuarios` _(política `MasterOnly`)_  
  Atualiza usuário.

- `DELETE /api/usuarios/{id}` _(política `MasterOnly`)_  
  Remove usuário.

## Tecnologias e padrões adotados

- .NET 10 / ASP.NET Core Web API
- MediatR
- Entity Framework Core
- FluentValidation
- JWT Bearer Authentication
- Swagger / OpenAPI
- Arquitetura em camadas (API, Application, Domain, Infrastructure)

## Configuração por variáveis de ambiente

### Banco de dados

A infraestrutura escolhe o provider com base no ambiente:

- **Development**: usa **SQL Server** com `DefaultConnectionDesbravadores`.
- **Demais ambientes**: usa **PostgreSQL** com `ConnectionStrings__DefaultConnection`.

### Segurança JWT

- `JWT_KEY` (obrigatória)
- `JWT_ISSUER` (obrigatória)
- `JWT_AUDIENCE` (obrigatória)
- `Jwt_ExpiresInMinutes` (opcional, padrão: `60`)
- `Jwt_RefreshTokenDays` (opcional, padrão: `7`)

## Como executar localmente

### Opção 1 — com .NET SDK

1. Configure as variáveis de ambiente.
2. Execute a API:

```bash
dotnet run --project src/Desbravadores.Gestao.Api
```

A aplicação sobe na porta definida por `PORT` (padrão `10000`) e expõe Swagger em `/swagger`.

### Opção 2 — com Docker Compose

```bash
docker compose up --build
```

Nesse cenário:

- SQL Server sobe em `localhost:1433`
- API sobe em `http://localhost:8080`

## Tratamento de erros

A API possui tratamento global de exceções e converte falhas de negócio em respostas HTTP consistentes:

- `UnauthorizedAccessException` → `401 Unauthorized`
- `KeyNotFoundException` / `InvalidOperationException` → `404 Not Found`
- Demais exceções → `400 Bad Request`

## Autor

Lucas de Souza  
IASD Joaniza 
