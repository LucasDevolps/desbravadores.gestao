# Wiki técnica — Desbravadores Gestão API

> Esta wiki foi escrita **somente com base no código existente no repositório**, descrevendo o que já está implementado hoje.

## 1) Visão geral

A solução implementa uma API REST em ASP.NET Core com foco em:

- autenticação com JWT;
- controle de sessão ativa por `jti` salvo em banco;
- logout com revogação da sessão atual;
- endpoint para dados do usuário autenticado;
- CRUD de usuários com autorização por políticas.

A entrada principal está em `Program.cs`, com configuração de controllers, autenticação, autorização, DI das camadas e Swagger. A aplicação sobe por padrão na porta `10000` (ou valor de `PORT`).

## 2) Estrutura de projetos (camadas)

- `src/Desbravadores.Gestao.Api`
  - controllers HTTP;
  - configuração JWT e políticas de autorização;
  - configuração Swagger;
  - middleware global de tratamento de exceções.

- `src/Desbravadores.Gestao.Application`
  - casos de uso (commands/queries via MediatR);
  - validações (FluentValidation);
  - interfaces de serviços e repositórios de aplicação.

- `src/Desbravadores.Gestao.Domain`
  - entidades de domínio (`Usuario`, `UsuarioSessao`);
  - enum de perfis (`Roles`);
  - DTOs compartilhados.

- `src/Desbravadores.Gestao.Infrastructure`
  - `AppDbContext` (EF Core);
  - implementação dos repositórios;
  - `TokenService` (geração JWT e refresh token);
  - `PasswordHasher` com BCrypt.

- `tests/Desbravadores.Gestao.UnitTests`
  - projeto de testes já criado, com teste placeholder.

## 3) Modelo de domínio e persistência

### 3.1 Entidade `Usuario`

Campos persistidos:

- `Id` (int, PK);
- `Uuid` (Guid, único);
- `Nome`;
- `Email`;
- `Senha` (hash BCrypt);
- `DataCriacao` (`DateOnly`, UTC na criação);
- `Role` (enum, salvo como string no banco).

Relacionamento: `Usuario` 1:N `UsuarioSessao`.

### 3.2 Entidade `UsuarioSessao`

Campos persistidos:

- `Id` (PK);
- `Uuid` (Guid, alternate key);
- `UsuarioId` (FK);
- `Jti` (único);
- `RefreshToken` (único);
- `AccessTokenExpiraEm`;
- `RefreshTokenExpiraEm`;
- `Revogado` + `DataRevogacao`;
- `DataCriacao`.

### 3.3 Mapeamento EF Core

No `AppDbContext`:

- tabela `Usuarios` e `UsuarioSessoes`;
- índices únicos para `Usuario.Uuid`, `UsuarioSessao.Jti` e `UsuarioSessao.RefreshToken`;
- `Role` com conversão para `string`;
- cascata no delete de sessão quando usuário é removido.

## 4) Perfis e autorização

### 4.1 Perfis (`Roles`)

Enum atual:

- `DIRETORA`
- `SECRETARIA`
- `TESOURARIA`
- `DIRETORIA`
- `DESBRAVADOR`

### 4.2 Políticas de autorização

Políticas configuradas:

- `MasterOnly` → permite `DIRETORIA` e `SECRETARIA`.
- `Financeiro` → permite `TESOURARIA` e `DIRETORIA`.

## 5) Autenticação JWT e sessão ativa

### 5.1 Geração do token

No login, o `TokenService` gera:

- `access_token` assinado com `JWT_KEY`;
- `refresh_token` aleatório (Base64 com 64 bytes);
- `jti` novo;
- expiração do access token (`Jwt_ExpiresInMinutes`, default `60`);
- expiração do refresh (`Jwt_RefreshTokenDays`, default `7`).

Claims incluídas:

- `sub` = `Usuario.Uuid`;
- `email`;
- `jti`;
- `user_id`;
- `name`;
- `role`.

### 5.2 Validação de token em cada request autenticada

Além da validação padrão JWT (`issuer`, `audience`, assinatura e expiração), há checagem de sessão ativa no banco:

1. extrai `sub` e `jti` do token;
2. localiza usuário por `Uuid`;
3. valida existência de sessão ativa com mesmo `UsuarioId + jti`, não revogada e não expirada.

Se falhar, a autenticação é rejeitada.

### 5.3 Regras de login e logout

- **Login**: revoga todas as sessões ativas anteriores do usuário e cria uma nova sessão persistida.
- **Logout**: busca sessão pelo `jti` do token atual e marca como revogada.

## 6) Endpoints existentes

Base route: `/api`

## 6.1 Auth

### `POST /api/auth/login`

Request body:

```json
{
  "email": "usuario@dominio.com",
  "senha": "123456"
}
```

Validações:

- e-mail obrigatório e formato válido;
- senha obrigatória.

Resposta (`200 OK`):

```json
{
  "token": {
    "accessToken": "...",
    "refreshToken": "...",
    "jti": "...",
    "accessTokenExpiraEm": "...",
    "refreshTokenExpiraEm": "..."
  },
  "expiracao": "...",
  "nome": "...",
  "email": "..."
}
```

---

### `POST /api/auth/logout` (autenticado)

- Extrai `jti` do token atual.
- Revoga a sessão correspondente.

Resposta: `204 No Content`.

---

### `GET /api/auth/Me` (autenticado)

- Extrai `sub` e `jti` dos claims.
- Retorna dados do usuário autenticado se a sessão estiver ativa.

Resposta (`200 OK`): `UsuarioDTO`.

> Observação: no código a rota foi declarada como `Me` (M maiúsculo), ou seja `/api/auth/Me`.

## 6.2 Usuários

Controller protegido por `[Authorize]`; exceção: criação pública.

### `POST /api/usuarios` (`MasterOnly`)

Request body:

```json
{
  "nome": "Nome",
  "email": "email@dominio.com",
  "senha": "123456",
  "roles": "DESBRAVADOR"
}
```

Validações:

- `nome`, `email`, `senha` obrigatórios;
- `email` válido;
- `roles` deve ser um valor do enum `Roles`.

Resposta: `201 Created` com `{ id }` (UUID).

---

### `POST /api/usuarios/publicos` (anônimo)

Mesma estrutura de criação de usuário, porém sem autenticação.

Resposta: `201 Created` com `{ id }`.

---

### `GET /api/usuarios` (`Financeiro`)

Retorna lista de usuários (`UsuarioDTO`), atualmente com:

- `id`
- `nome`
- `email`
- `dataCriacao`

---

### `GET /api/usuarios/{id}` (`MasterOnly`)

Busca usuário por UUID.

Resposta: `200 OK` com `UsuarioDTO`.

---

### `PUT /api/usuarios` (`MasterOnly`)

Body atual esperado pelo command:

```json
{
  "command": {
    "uuid": "GUID",
    "nome": "...",
    "email": "...",
    "senha": "...",
    "role": "DESBRAVADOR"
  }
}
```

Resposta: `200 OK` com `UsuarioDTO`.

---

### `DELETE /api/usuarios/{id}` (`MasterOnly`)

Remove usuário por UUID.

Resposta: `204 No Content`.

## 7) Tratamento global de exceções

Mapeamento em `Program.cs`:

- `UnauthorizedAccessException` → `401 Unauthorized`;
- `KeyNotFoundException` e `InvalidOperationException` → `404 Not Found`;
- demais exceções → `400 Bad Request` com mensagem genérica.

## 8) Configuração por variáveis de ambiente

### 8.1 JWT (obrigatórias)

- `JWT_KEY`
- `JWT_ISSUER`
- `JWT_AUDIENCE`

### 8.2 JWT (opcionais)

- `Jwt_ExpiresInMinutes` (default `60`)
- `Jwt_RefreshTokenDays` (default `7`)

### 8.3 Banco de dados

Seleção de provider no `AddInfrastructure`:

- `ASPNETCORE_ENVIRONMENT=Development` → SQL Server com `DefaultConnectionDesbravadores`;
- demais ambientes → PostgreSQL com `ConnectionStrings__DefaultConnection`.

## 9) Execução local

### 9.1 Com .NET SDK

```bash
dotnet run --project src/Desbravadores.Gestao.Api
```

API sobe em `http://0.0.0.0:${PORT}` (default `10000`), Swagger em `/swagger`.

### 9.2 Com Docker Compose

```bash
docker compose up --build
```

No compose atual:

- SQL Server exposto em `localhost:1433`;
- API mapeada em `http://localhost:8080` (porta interna `10000`).

## 10) Itens presentes no repositório que não estão implementados como feature funcional

- Não há endpoint para refresh token exposto nos controllers.
- O arquivo HTTP de exemplo ainda aponta para `weatherforecast`, rota que não existe nos controllers atuais.
