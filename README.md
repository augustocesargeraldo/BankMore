# BankMore — README

> Repositório com as duas APIs do exercício: **BankMore ContaCorrente API** e **BankMore Transferencia API**.

---

## Visão geral

Este repositório contém dois microsserviços (APIs) que implementam as funcionalidades do exercício **BankMore**: cadastro/autenticação/inativação, movimentações em conta corrente, transferência entre contas e consulta de saldo.

Principais decisões arquiteturais adotadas:

- **Domain-Driven Design (DDD)**: o domínio financeiro (conta corrente, movimento, transferência) está modelado na camada `Domain`, com entidades e regras de negócio coesas.
- **Clean Architecture / Hexagonal**: separação clara entre **API (entry points)**, **Application (UseCases / Services / QueryHandlers)**, **Domain** e **Infrastructure (persistência, integração HTTP)**. Os UseCases e QueryHandlers atuam como portas/serviços da aplicação.
- **CQRS (Command / Query Responsibility Segregation)**: comandos (criar conta, movimentar, transferir, inativar) e queries (consultar saldo, obter conta) são separados em handlers/UseCases distintos.
- **SOLID**: design orientado a princípios SOLID para facilitar testes, manutenção e extensão.
- **Design Patterns usados**:
  - **Repository / Mapper** (adaptação dos acessos ao DB via Dapper)
  - **Adapter** para integração entre APIs (Transferencia → ContaCorrente)
  - **Factory / Builder** para criação de objetos de domínio (quando aplicável)
  - **Idempotency Key pattern** (para garantir idempotência nas operações que podem ser repetidas)

> Observação: este projeto não utiliza `MediatR`, optando por uma implementação direta para manter simplicidade e foco didático.

---

## Principais características implementadas

- Endpoints documentados com **Swagger** para cada API.
- **Autenticação JWT** em todas as APIs: todos os endpoints exigem token válido (`Authorize`), exceto o endpoint de criação de sessão (login) que é público para obtenção do token.
- **Idempotência**: todos os endpoints que alteram estado (exceto endpoints de consulta e o endpoint de criação de sessão) foram implementados para suportar idempotência.
- Integração entre APIs: a **Transferencia API** realiza chamadas HTTP para a **ContaCorrente API** (endpoint base durante desenvolvimento: `https://localhost:7087/`) e **repassa o token JWT** no header `Authorization`.
- Persistência: **SQLite** + **Dapper** (scripts de criação de tabelas foram fornecidos e as bases criadas).
- Testes: existe um projeto de testes unitários cobrindo UseCases e validações principais. A parte opcional (Sistema de Tarifas / Kafka) **não** foi implementada.

---

## Estrutura do repositório (resumida)

```
/BankMore
  /src
    /BankMore.ContaCorrente.Api        -> Web API (Swagger, controllers)
    /BankMore.ContaCorrente.Application -> UseCases, DTOs, Validators
    /BankMore.ContaCorrente.Domain      -> Entidades e regras de negócio
    /BankMore.ContaCorrente.Infra       -> Repositórios (Dapper), migrations, scripts

    /BankMore.Transferencia.Api        -> Web API (controller Transferencia)
    /BankMore.Transferencia.Application
    /BankMore.Transferencia.Domain
    /BankMore.Transferencia.Infra

  /tests
    /BankMore.ContaCorrente.Tests
    /BankMore.Transferencia.Tests

  /data
   - ContaCorrente.db
   - Transferencia.db

  /scripts
    - contacorrente.sql
    - transferencia.sql

  docker-compose.yaml
  README.md
```

---

## Endpoints (desenvolvimento)

### BankMore ContaCorrente API (Swagger em desenvolvimento):
> https://localhost:7087/swagger/index.html

**ContasCorrentes**
- `POST /api/contas-correntes` — Criar conta corrente (requisição autenticada, recebe número da conta e senha).
- `PATCH /api/contas-correntes/status` — Inativar conta (requisição autenticada, valida senha)
- `POST /api/contas-correntes/movimentos` — Registrar movimento (requisição autenticada, débito/crédito). Suporta `idRequisicao`.
- `GET /api/contas-correntes/saldo` — Consultar saldo (requisição autenticada)
- `GET /api/contas-correntes/{idcontacorrente}` — Obter conta por id (requisição autenticada)
- `GET /api/contas-correntes/numero/{numeroConta}` — Obter conta por número (requisição autenticada)

**Sessoes**
- `POST /api/sessoes` — Criar sessão / Login (gera JWT contendo a identificação da conta corrente). Este endpoint é público (não exige token).


### BankMore Transferencia API (Swagger em desenvolvimento):
> https://localhost:7260/swagger/index.html

**Transferencia**
- `POST /api/Transferencia` — Realiza transferência entre contas da mesma instituição (requisição autenticada). Repassa `Authorization` e `idRequisicao` para a ContaCorrente API quando necessário.

---

## Fluxo de Transferência (resumido)

1. Cliente chama `POST /api/Transferencia` na Transferencia API com body contendo `idRequisicao`, `numeroContaDestino` e `valor` e com header `Authorization: Bearer {token}`.
2. Transferencia API valida token (autenticidade/expiração) e valida regras básicas (conta existente e ativa, valor positivo).
3. Transferencia API chama ContaCorrente API:
   - Primeiro: **débito** na conta logada → `POST /api/contas-correntes/movimentos` com `idRequisicao` e passando o JWT.
   - Segundo: **crédito** na conta destino → `POST /api/contas-correntes/movimentos` com `idRequisicao` e passando o JWT.
4. Em caso de falha na segunda chamada (crédito), Transferencia API tenta **estornar** executando uma chamada de débito inverso.
5. Persistência da transferência na tabela `TRANSFERENCIA`.
6. Retorna `HTTP 204` em caso de sucesso.

> Observações: as chamadas entre APIs usam `HttpClient` e o token JWT é repassado no header `Authorization` para manter o contexto de segurança.

---

## Idempotência

- A idempotência é garantida pela tabela `idempotencia`, que possui as colunas `requisicao` e `resultado`.
- Implementação para operações que alteram estado: criação de conta, movimentação, transferência, inativação.
- Por padrão, o serviço verifica se uma mesma requisição já foi processada e, se sim, retorna o resultado previamente armazenado sem executar novamente a operação de negócio.
- Em alguns casos, o request inclui um `idRequisicao` para diferenciar chamadas distintas que, de outro modo, teriam o mesmo conteúdo de requisição.

---

## Segurança

- As senhas das contas são armazenadas de forma segura usando um hash `SHA-256` combinado com um `salt` aleatório.
- Cada conta tem sua própria coluna `salt` na tabela contacorrente.
- Ao criar a conta, a senha fornecida é combinada com o `salt` e armazenada como hash na coluna `senha`.

Isso garante que mesmo que o banco seja acessado indevidamente, as senhas originais não possam ser recuperadas diretamente.

---

## Banco de dados e scripts

- Banco utilizado em dev: **SQLite**.
- Acesso via **Dapper** para simplicidade e performance nas queries.
- Os scripts de criação das tabelas foram recebidos e utilizados (veja `/scripts`).

Tabelas principais:
- `CONTACORRENTE` (idcontacorrente, numero, nome, ativo, senha, salt)
- `MOVIMENTO` (idmovimento, idcontacorrente, datamovimento, tipomovimento, valor)
- `TRANSFERENCIA` (idtransferencia, idcontacorrente_origem, idcontacorrente_destino, datamovimento, valor)
- `IDEMPOTENCY` (chave_idempotencia, requisicao, resultado)

---

## Tratamento de erros e códigos (padrão usado)

As APIs retornam JSON com campos: `mensagens` e `tipoFalha` quando aplicável.

Principais `errorType` usados em validações:
- `USER_UNAUTHORIZED` — Falha de autenticação (HTTP 403)
- `INVALID_ACCOUNT` — Conta inexistente (HTTP 400)
- `INACTIVE_ACCOUNT` — Conta inativa (HTTP 400)
- `INVALID_VALUE` — Valor inválido/negativo (HTTP 400)
- `INVALID_TYPE` — Tipo de movimento inválido (HTTP 400)

Exemplo de resposta de erro:

```json
{
  "message": "Conta corrente não encontrada",
  "errorType": "INVALID_ACCOUNT"
}
```

---

## Execução local

### Requisitos
- .NET 8 SDK
- Docker (opcional para docker-compose)

### Rodando localmente (modo dev)
1. Ajuste `appsettings.json` das APIs com a string do SQLite (ex.: `Data Source=./data/ContaCorrente.db`, `Data Source=./data/Transferencia.db`).
2. O projeto já inclui bases de dados preparadas em `/data/ContaCorrente.db` e `/data/Transferencia.db`. Basta ajustar o caminho absoluto dessas bases no appsettings.json. Existe uma conta cadastrada para possibilitar criar a sessão na API de ContaCorrente:            
   {
     "numero": 1,
     "senha": "teste123"
   }
3. Subir as APIs via `dotnet run --project src/BankMore.ContaCorrente.Api` e `dotnet run --project src/BankMore.Transferencia.Api`.
4. Swagger estarão disponíveis em:
   - ContaCorrente: `https://localhost:7087/swagger/index.html`
   - Transferencia: `https://localhost:7260/swagger/index.html`

### Docker (exemplo)
- Existe um `docker-compose.yaml` de referência para subir as duas APIs. Ajuste conforme necessário.

---

## Como testar (exemplos rápidos)

1. Criar sessão (`POST /api/sessoes`) -> obter JWT.
2. Criar conta (`POST /api/contas-correntes`) -> retorna `numero` no corpo da resposta. Requer `Authorization` header.
3. Consultar saldo (`GET /api/contas-correntes/saldo`). Requer `Authorization` header.
4. Realizar transferência (`POST /api/Transferencia`). Requer `Authorization` header.

Exemplo curl (login):

```bash
curl -X POST "https://localhost:7087/api/sessoes" -H "Content-Type: application/json" -d '{"numero": "1234", "senha": "senha123"}'
```

Exemplo curl (transferência):

```bash
curl -X POST "https://localhost:7260/api/Transferencia" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"idRequisicao":"abc-123-req","numeroContaDestino":4321,"valor":100.0}'
```

---

## Testes

- Existem testes unitários cobrindo validações de UseCases e alguns handlers de integração.
- O projeto de testes foi preparado com `xUnit` (exemplos em `/tests`).

---

## Pontos abertos / Sugestões de melhoria

1. **Resiliência e retry**: para produção, recomenda-se usar `Polly` para políticas de retry/circuit-breaker nas chamadas inter-API.
3. **Observability**: adicionar logs estruturados, correlação de request (idRequisicao), métricas e tracing (OpenTelemetry).
4. **Integração assíncrona**: implementar Kafka (ou outro broker) para publicar transferências e processar tarifas (opcional solicitado).
5. **Testes de integração**: criar testes E2E / integration com bancos em memória ou containers.
6. **Cache**: considerar cache para consultas de leitura pesada (saldo), com invalidação apropriada.

---

## Referências internas

- Endpoints expostos em Swagger (ver URLs acima).
- Repositórios Dapper em `/src/*/Infra/Repositories`.
- Bases de dados SQLite em `/data`.
- Scripts SQL em `/scripts`.

---

