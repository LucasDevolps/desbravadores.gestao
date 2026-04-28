# Desbravadores.Gestao

API REST em **ASP.NET Core 10** para autenticação JWT, controle de sessão ativa via banco de dados e gestão de usuários com autorização por políticas.

## Funcionalidades implementadas

- Login com JWT (`access token` + `refresh token`).
- Controle de sessão por `jti` persistido no banco.
- Revogação de sessão no logout.
- Endpoint de identidade do usuário autenticado (`/api/auth/Me`).
- Endpoint para renovação de sessão/token (`/api/auth/refresh`).
- CRUD de usuários com regras de autorização.
- Validação de entrada com FluentValidation.
- Swagger/OpenAPI com tema dark customizado.

## Arquitetura da solução

- `src/Desbravadores.Gestao.Api`  
  Camada HTTP: controllers, autenticação/autorização, Swagger, pipeline e tratamento global de exceções.

- `src/Desbravadores.Gestao.Application`  
  Casos de uso (MediatR), validações e contratos de aplicação.

- `src/Desbravadores.Gestao.Domain`  
  Entidades, DTOs e enumerações de domínio.

- `src/Desbravadores.Gestao.Infrastructure`  
  EF Core (`AppDbContext`), repositórios, hash de senha e geração de tokens.

- `tests/Desbravadores.Gestao.UnitTests`  
  Projeto de testes unitários.

## Perfis e políticas de autorização

### Roles disponíveis

- `DIRETORA`
- `SECRETARIA`
- `TESOURARIA`
- `DIRETORIA`
- `DESBRAVADOR`

### Políticas configuradas

- `MasterOnly`: `DIRETORIA` e `SECRETARIA`.
- `Financeiro`: `TESOURARIA` e `DIRETORIA`.

## Fluxo de autenticação e sessão

1. Usuário autentica em `POST /api/auth/login`.
2. Sessões ativas anteriores do usuário são revogadas.
3. Nova sessão é criada no banco com `jti`, refresh token e expiração.
4. Em cada request autenticada, além da validação JWT, a API valida se a sessão está ativa no banco.
5. `POST /api/auth/logout` revoga a sessão atual.
6. `GET /api/auth/refresh` gera novos tokens para sessão autenticada.

## Endpoints atuais

## Autenticação (`/api/auth`)

- `POST /login` (anônimo)
- `POST /logout` (autenticado)
- `GET /Me` (autenticado)
- `GET /refresh` (autenticado)

## Usuários (`/api/usuarios`)

> O controller é protegido por `[Authorize]`.

- `POST /` — política `MasterOnly`
- `GET /` — política `Financeiro`
- `GET /{id:guid}` — política `MasterOnly`
- `PUT /` — política `MasterOnly`
- `DELETE /{id:guid}` — política `MasterOnly`

> Observação: o endpoint `POST /api/usuarios/publicos` está comentado no código neste momento.

## Variáveis de ambiente

## JWT

Obrigatórias:

- `JWT_KEY`
- `JWT_ISSUER`
- `JWT_AUDIENCE`

Opcionais:

- `Jwt_ExpiresInMinutes` (padrão: `60`)
- `Jwt_RefreshTokenDays` (padrão: `7`)

## Banco de dados

A seleção do provider é por ambiente:

- `ASPNETCORE_ENVIRONMENT=Development`:
  - usa **SQL Server**
  - connection string em `DefaultConnectionDesbravadores`

- demais ambientes:
  - usa **PostgreSQL**
  - connection string em `ConnectionStrings__DefaultConnection`

## Execução local

## Com .NET SDK

1. Defina as variáveis de ambiente obrigatórias.
2. Execute:

```bash
dotnet run --project src/Desbravadores.Gestao.Api
```

Com profile local, a aplicação usa `http://localhost:5009` (e `https://localhost:7269` no profile https). Fora de desenvolvimento, usa `PORT` (padrão `10000`).

## Com Docker Compose

```bash
docker compose up --build
```

Serviços do compose atual:

- SQL Server: `localhost:1433`
- Nginx (proxy da API): `http://localhost:8080`

## Tratamento global de erros

Mapeamentos configurados no pipeline:

- `UnauthorizedAccessException` → `401 Unauthorized`
- `KeyNotFoundException` → `404 Not Found`
- `InvalidOperationException` → `400 Bad Request`
- Demais exceções → `400 Bad Request` com mensagem genérica

## Autor

Lucas de Souza  
IASD Joaniza
