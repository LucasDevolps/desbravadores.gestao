# Desbravadores.Gestao

Aplicação ASP.NET Core 10 para gerenciamento de usuários e autenticação JWT com arquitetura em camadas.

## Visão geral

O projeto é organizado em quatro camadas principais:

- `src/Desbravadores.Gestao.Api`: API Web ASP.NET Core
- `src/Desbravadores.Gestao.Application`: lógica de aplicação, casos de uso e validação
- `src/Desbravadores.Gestao.Domain`: entidades, DTOs, constantes e contratos de repositório
- `src/Desbravadores.Gestao.Infrastructure`: persistência EF Core, repositórios e serviços de segurança

Também há um projeto de testes em:

- `tests/Desbravadores.Gestao.UnitTests`

## Recursos principais

- Autenticação JWT com refresh token
- Criação de usuários com validação de dados
- Controle de sessão de usuários
- Permissões baseadas em políticas para rotas sensíveis
- Documentação Swagger integrada

## Requisitos

- .NET 10 SDK
- SQL Server (conexão definida por variável de ambiente)

## Configuração

Antes de executar a aplicação, defina as variáveis de ambiente no ambiente de desenvolvimento ou em um arquivo de configuração apropriado:

- `DefaultConnectionDesbravadores`: string de conexão do SQL Server
- `JWT_KEY`: chave secreta para assinatura do token JWT
- `JWT_ISSUER`: emissor do token JWT
- `JWT_AUDIENCE`: audiência do token JWT
- `Jwt_ExpiresInMinutes`: tempo de expiração do access token (opcional, padrão 60)
- `Jwt_RefreshTokenDays`: tempo de expiração do refresh token (opcional, padrão 7)

## Como executar

1. Abra a solução `Desbravadores.Gestao.slnx` no Visual Studio ou VS Code.
2. Certifique-se de que as variáveis de ambiente estejam configuradas.
3. Execute o projeto `src/Desbravadores.Gestao.Api`.
4. Acesse a interface Swagger em `https://localhost:<porta>/swagger`.

## Estrutura das rotas

- `POST /api/auth/login`: login de usuário
- `POST /api/auth/logout`: logout de usuário (requer autenticação)
- `GET /api/auth/me`: obtém os dados do usuário autenticado
- `POST /api/usuarios`: cria novo usuário
- `GET /api/usuarios`: lista usuários
- `GET /api/usuarios/{id}`: obtém usuário por UUID

## Observações

- A API está preparada para uso com autenticação JWT e políticas de autorização.
- O projeto utiliza FluentValidation para validação de requisições e EF Core para acesso a dados.

## Próximas ideias

- Dockerizar a aplicação para facilitar execução local e deployment.
- Criar uma GitHub Action para deploys automáticos.
- Implementar cenários completos de testes para garantir qualidade e confiança.

## Contato

- Lucas de Souza
- IASD Joaniza
