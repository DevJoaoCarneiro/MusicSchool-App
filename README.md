# 🎵 MusicSchool App — DDD + SOLID + PostgreSQL

API REST em **.NET** para gestão de uma escola de música, estruturada com **Domain-Driven Design (DDD)** em camadas e
princípios **SOLID**. Usa **PostgreSQL** via **Entity Framework Core** (com Migrations) e traz um fluxo completo
(`Api → Application → Domain → Infrastructure`) de ponta a ponta — **`POST /api/users`** — como referência para
implementar novas features seguindo o mesmo padrão.

---

## 📁 Estrutura do Projeto

```bash
├── backend                # API .NET (DDD)
│   ├── Api
│   ├── Application
│   ├── Domain
│   ├── Infrastructure
│   ├── Tests
│   ├── docs / scripts     # teste de carga (k6)
│   └── MusicSchoolApp.slnx
│
└── frontend               # React + TypeScript (Vite)
```

### Backend

```bash
├── Api
│   ├── Controllers        # camada de apresentação (HTTP)
│   └── Program.cs         # composition root (DI, middlewares)
│
├── Application
│   ├── Request / Response # DTOs de entrada/saída
│   ├── Interfaces         # contratos dos serviços de aplicação
│   └── Services           # orquestração dos casos de uso
│
├── Domain
│   ├── Entities            # entidades de domínio
│   └── Interfaces          # contratos (repositórios, serviços de domínio)
│
├── Infrastructure
│   ├── Context             # DbContext (EF Core)
│   ├── Persistence          # mapeamentos (IEntityTypeConfiguration)
│   ├── Migrations           # migrations do EF Core
│   ├── Repositories         # implementações dos repositórios
│   └── Security             # implementações de serviços de domínio (ex.: hashing)
│
├── Tests
│   └── Services             # testes unitários da camada Application
│
└── MusicSchoolApp.slnx
```

Cada seta de dependência aponta para dentro (`Api → Application → Domain`, `Infrastructure → Domain`), com o `Domain` sem
depender de nenhuma outra camada — a base do DDD. As implementações concretas (`Infrastructure`) são plugadas via injeção
de dependência em `backend/Api/Program.cs`, seguindo o princípio de inversão de dependência (o "D" do SOLID).

## 🧩 Endpoint de usuário

`UserController` (`backend/Api/Controllers/UserController.cs`) implementa `POST /api/users` e serve como referência de como uma
feature nova deve atravessar as camadas: DTO de entrada → `IUserServices` (Application) → entidade `User` (Domain) →
`IUserRepository` (Domain) → `UserRepository` (Infrastructure, via EF Core). Use-o como modelo ao adicionar novos recursos.

Principais características:

- **Senha nunca é persistida em texto plano.** A senha recebida é transformada em hash com **BCrypt**
  (`ISecurityService.HashPassword`, implementado em `Infrastructure/Security/BCryptoSecurityService.cs`) e somente o
  `PasswordHash` (já salgado pelo próprio BCrypt) é gravado no banco. `ISecurityService.VerifyPassword` já está disponível
  para uso futuro em um endpoint de login.
- **Validação de entrada** via Data Annotations (`Name`, `Email`, `Password` obrigatórios, e-mail em formato válido,
  senha com no mínimo 6 caracteres).
- **E-mail único**: checagem na camada de aplicação (`IUserRepository.ExistsByEmailAsync`) antes de criar o usuário,
  reforçada por um índice único (`IX_Users_Email`) no banco — retorna `409 Conflict` em caso de duplicidade.
- **Roles de usuário**: cada usuário possui um `Role` (`Admin`, `Teacher`, `Student` ou `Guardian`), representando os
  diferentes perfis da escola de música (administração, professores, alunos e responsáveis). O valor é enviado como
  string no corpo da requisição (`"role": "Teacher"`) e persistido como texto no banco; valores fora do enum retornam
  `400 Bad Request`.

## 🧪 Testes Unitários

O projeto contém testes unitários cobrindo:

- Serviços da **camada Application**

```bash
cd backend
dotnet test
```

## 🔐 Configuração e segredos

`appsettings.json` só guarda defaults seguros (sem segredos). **Nunca coloque credenciais reais nele** — os arquivos
`appsettings.*.json` (exceto `appsettings.json` e `*.example.json`) e `.env*` estão no `.gitignore` justamente para isso.

Para configurar sua string de conexão local, escolha uma das opções:

**Opção 1 — .NET User Secrets (recomendado):**
```bash
cd backend/Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=MusicSchoolDb;Username=postgres;Password=SUA_SENHA"
```

**Opção 2 — `appsettings.Development.json` (ignorado pelo git):**
```bash
cp backend/Api/appsettings.Development.json.example backend/Api/appsettings.Development.json
# depois edite backend/Api/appsettings.Development.json com sua senha real
```

```bash
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=MusicSchoolDb;Username=postgres;Password=SUA_SENHA"
}
```

- `Host` → `localhost` se o banco estiver local; use o IP/URL se estiver em outro servidor.
- `Port` → porta padrão do PostgreSQL (`5432`).
- `Database` → nome da sua base.
- `Username` / `Password` → credenciais do seu banco.

Depois, aplique as migrations:
```bash
cd backend
dotnet ef database update --project Infrastructure --startup-project Api
```

## 🎨 Frontend

React 19 + TypeScript com Vite, organizado em componentes e integrado à API. Detalhes em [`frontend/README.md`](frontend/README.md).

Com a API rodando (`https://localhost:7260`):

```bash
cd frontend
npm install
npm run dev
```

O front abre em `http://localhost:5173` e encaminha `/api` para o backend. Na primeira subida com banco vazio, a API exige `ADMIN_INITIAL_PASSWORD` para criar o administrador inicial (e aceita `ADMIN_INITIAL_EMAIL` e `ADMIN_INITIAL_NAME`).

## 🚀 Deploy no Railway (Docker)

Backend e frontend têm cada um seu `Dockerfile` e seu `railway.json` (build por Dockerfile, healthcheck em `/health` e restart automático). Nenhuma configuração ou segredo vai dentro das imagens: tudo vem de variáveis de ambiente.

| Serviço | Root Directory | Imagem |
|---|---|---|
| `backend` | `/backend` | build com o SDK do .NET 10, roda no runtime `aspnet` com usuário sem privilégios, na porta da variável `PORT` |
| `frontend` | `/frontend` | build com Node 22, servido pelo nginx sem privilégios, com fallback de rotas para o React |

1. No projeto do Railway, adicione um banco **PostgreSQL** (serviço chamado `Postgres`).
2. Crie dois serviços apontando para este repositório, chamados `backend` e `frontend`, e em **Settings → Source → Root Directory** informe `/backend` e `/frontend`. Os nomes importam: as variáveis usam `${{backend.RAILWAY_PUBLIC_DOMAIN}}` e `${{frontend.RAILWAY_PUBLIC_DOMAIN}}` para um serviço achar o outro.
3. Em cada serviço: **Settings → Networking → Generate Domain**.
4. Em **Variables → Raw Editor** de cada serviço, cadastre as variáveis da tabela abaixo. Para gerar a chave do JWT:

   ```bash
   node -e "console.log(require('crypto').randomBytes(48).toString('base64'))"
   ```

5. Faça o deploy. As migrations rodam na inicialização (`Database__MigrateOnStartup`) e o primeiro Admin é criado com `ADMIN_INITIAL_*`. Depois do primeiro login, remova `ADMIN_INITIAL_PASSWORD`.

A cada push na branch configurada, o Railway refaz o deploy do serviço que teve arquivos alterados (`watchPatterns`).

| Variável | Serviço | Para que serve |
|---|---|---|
| `PORT` | ambos | Porta em que o container escuta |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | backend | Respeita `X-Forwarded-For/Proto` do proxy do Railway: HTTPS correto e rate limit por IP real |
| `AllowedHosts` | backend | Hosts aceitos: o domínio público e o host do healthcheck do Railway |
| `ConnectionStrings__DefaultConnection` | backend | Montada com as variáveis do serviço `Postgres` |
| `Jwt__SecretKey` / `Jwt__Issuer` / `Jwt__Audience` | backend | Assinatura e validação dos tokens (chave com 32+ bytes, aleatória) |
| `Cors__AllowedOrigins__0` | backend | URL do frontend, a única origem aceita pelo navegador |
| `ADMIN_INITIAL_*` | backend | Criação do primeiro Admin quando o banco está vazio |
| `VITE_API_URL` | frontend | URL pública da API, embutida no build do React |

Para testar as imagens localmente:

```bash
docker build -t musicschool-api backend
docker run --rm -p 8080:8080 -e ConnectionStrings__DefaultConnection="..." -e Jwt__SecretKey="..." -e Jwt__Issuer="MusicSchool.Api" -e ADMIN_INITIAL_PASSWORD="..." -e Cors__AllowedOrigins__0="http://localhost:3000" musicschool-api

docker build -t musicschool-web --build-arg VITE_API_URL=http://localhost:8080 frontend
docker run --rm -p 3000:8080 musicschool-web
```
