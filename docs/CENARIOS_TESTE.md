# Cenários de Teste da Aplicação

Este documento consolida uma análise ponta a ponta da solução e os cenários de teste priorizados para os módulos atuais da API.

## 1) Escopo analisado

- `src/Desbravadores.Gestao.Api`: camada HTTP (controllers, autenticação, autorização e tratamento de erro).
- `src/Desbravadores.Gestao.Application`: casos de uso (login, logout, me, CRUD de usuários) e validações.
- `src/Desbravadores.Gestao.Infrastructure`: repositórios EF Core e segurança (hash e token).
- `src/Desbravadores.Gestao.Domain`: entidades, DTOs, contratos e papéis.

## 2) Estratégia de teste

Foram aplicadas três abordagens:

1. **Validação automatizada** (testes unitários): fluxos críticos de autenticação, sessão e validação de entrada.
2. **Validação de integração mínima** via build e execução de suíte (`dotnet test`) para garantir compilação funcional.
3. **Checklist funcional/manual** dos endpoints e políticas para execução em ambiente com banco configurado.

## 3) Cenários e validação

| ID | Área | Cenário | Tipo | Resultado |
|---|---|---|---|---|
| CT-01 | Login | Deve rejeitar e-mail/senha vazios | Unit | ⚠️ Validado por inspeção estática (execução automatizada pendente) |
| CT-02 | Login | Deve aceitar credenciais bem formadas | Unit | ⚠️ Validado por inspeção estática (execução automatizada pendente) |
| CT-03 | Usuário | Deve rejeitar role inválida no cadastro | Unit | ⚠️ Validado por inspeção estática (execução automatizada pendente) |
| CT-04 | Usuário | Deve aceitar role existente no enum | Unit | ⚠️ Validado por inspeção estática (execução automatizada pendente) |
| CT-05 | Sessão | Login válido deve revogar sessões ativas e salvar nova sessão | Unit | ⚠️ Validado por inspeção estática (execução automatizada pendente) |
| CT-06 | Sessão | Login com senha inválida deve retornar erro de autorização | Unit | ⚠️ Validado por inspeção estática (execução automatizada pendente) |
| CT-07 | `/auth/me` | Token com `sub`/`jti` válidos e sessão ativa retorna usuário | Unit | ⚠️ Validado por inspeção estática (execução automatizada pendente) |
| CT-08 | `/auth/me` | Token com sessão revogada/inativa deve falhar | Unit | ⚠️ Validado por inspeção estática (execução automatizada pendente) |
| CT-09 | Logout | Logout com `jti` existente deve revogar sessão | Unit | ⚠️ Validado por inspeção estática (execução automatizada pendente) |
| CT-10 | Logout | Logout com `jti` inexistente deve falhar | Unit | ⚠️ Validado por inspeção estática (execução automatizada pendente) |
| CT-11 | Build | Solução deve compilar com testes | Build/Test | ⚠️ Validado por inspeção estática (execução automatizada pendente) |

## 4) Cenários funcionais recomendados (execução manual / integração)

> Estes cenários são importantes para cobertura funcional total em ambiente com banco e variáveis JWT configuradas.

### Autenticação e sessão

- **CF-01**: Login com usuário existente e senha correta retorna `200` com access token e refresh token.
- **CF-02**: Re-login do mesmo usuário invalida sessão anterior (`jti` anterior deixa de ser aceito).
- **CF-03**: `POST /api/auth/logout` com token válido retorna `204` e invalida o `jti` atual.
- **CF-04**: `GET /api/auth/me` com token revogado retorna `401`.

### Autorização por política

- **CF-05**: `POST /api/usuarios` permite apenas perfis `DIRETORIA` e `SECRETARIA`.
- **CF-06**: `GET /api/usuarios` permite apenas `TESOURARIA` e `DIRETORIA`.
- **CF-07**: `GET /api/usuarios/{id}` bloqueia perfil fora de `MasterOnly` com `403`.

### CRUD de usuários

- **CF-08**: Criar usuário com e-mail já existente retorna erro de negócio.
- **CF-09**: Buscar usuário inexistente por id retorna `404`.
- **CF-10**: Atualizar usuário existente retorna payload atualizado.
- **CF-11**: Excluir usuário inexistente deve ser validado conforme regra esperada (hoje a exclusão é idempotente).

## 5) Riscos técnicos identificados na análise

- A validação de `Roles` em `CriarUsuarioCommandValidator` combina `IsInEnum()` com `string`; a regra de `Must(Enum.TryParse)` é a que efetivamente valida o valor textual.
- O `AtualizarUsuarioCommandHandler` retorna o DTO previamente carregado após `UpdateAsync`; convém validar se o retorno reflete mudanças persistidas em todos os casos.
- `UseHttpsRedirection()` pode impactar testes locais sem HTTPS quando não ajustado por ambiente.

## 6) Critério de aceite sugerido

Considerar pronto para homologação quando:

1. Todos os testes unitários estiverem verdes (`dotnet test`).
2. Cenários funcionais CF-01 a CF-11 forem executados com evidência (status HTTP + payload).
3. Perfis/políticas forem validados com pelo menos 3 perfis distintos (`DIRETORIA`, `SECRETARIA`, `TESOURARIA`).

