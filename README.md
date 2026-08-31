# 🎵 MusicSchool App — DDD + SOLID + PostgreSQL

API REST em **.NET** para gestão de uma escola de música, estruturada com **Domain-Driven Design (DDD)** em camadas e
princípios **SOLID**. Usa **PostgreSQL** via **Entity Framework Core** (com Migrations) e traz um fluxo completo
(`Api → Application → Domain → Infrastructure`) de ponta a ponta — **`POST /api/user`** — como referência para
implementar novas features seguindo o mesmo padrão.

---

## 📁 Estrutura do Projeto (DDD + SOLID)

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
de dependência em `Api/Program.cs`, seguindo o princípio de inversão de dependência (o "D" do SOLID).

## 🧩 Endpoint de usuário

`UserController` (`Api/Controllers/UserController.cs`) implementa `POST /api/user` e serve como referência de como uma
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
dotnet test
```

## 🔐 Configuração e segredos

`appsettings.json` só guarda defaults seguros (sem segredos). **Nunca coloque credenciais reais nele** — os arquivos
`appsettings.*.json` (exceto `appsettings.json` e `*.example.json`) e `.env*` estão no `.gitignore` justamente para isso.

Para configurar sua string de conexão local, escolha uma das opções:

**Opção 1 — .NET User Secrets (recomendado):**
```bash
cd Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=MusicSchoolDb;Username=postgres;Password=SUA_SENHA"
```

**Opção 2 — `appsettings.Development.json` (ignorado pelo git):**
```bash
cp Api/appsettings.Development.json.example Api/appsettings.Development.json
# depois edite Api/appsettings.Development.json com sua senha real
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
dotnet ef database update --project Infrastructure --startup-project Api
```
