# Teste de carga da API

O seed de carga é separado do `AdminSeed` e só é executado no ambiente `LoadTest` quando `LOAD_TEST_SEED=true`.

## Preparar o banco e a API

Use um banco PostgreSQL exclusivo para carga. Configure a conexão e as credenciais do administrador por variáveis de ambiente:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "LoadTest"
$env:LOAD_TEST_SEED = "true"
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=music_school_load;Username=<DB_USER>;Password=<DB_PASSWORD>"
$env:ADMIN_INITIAL_NAME = "Load Test Admin"
$env:ADMIN_INITIAL_EMAIL = "<ADMIN_EMAIL>"
$env:ADMIN_INITIAL_PASSWORD = "<ADMIN_PASSWORD>"
$env:Jwt__SecretKey = "<JWT_SECRET_WITH_AT_LEAST_32_CHARACTERS>"
$env:Jwt__Issuer = "MusicSchool.LoadTest"

dotnet run --project .\Api\Api.csproj --urls http://localhost:5080
```

O seed cria 100 professores, 1.000 alunos, matrículas e 10.000 aulas. Ele é idempotente pelo prefixo dos e-mails dos alunos.

## Executar o k6

Com a API em execução, abra outro terminal na raiz do repositório:

```powershell
New-Item -ItemType Directory -Force .\artifacts | Out-Null
$env:BASE_URL = "http://localhost:5080"
$env:ADMIN_EMAIL = "<ADMIN_EMAIL>"
$env:ADMIN_PASSWORD = "<ADMIN_PASSWORD>"
$env:VUS = "10"
$env:DURATION = "30s"

k6 run .\scripts\k6\music-school-load.js
```

Para HTTPS local com certificado não confiável, defina `$env:INSECURE_TLS = "true"`.

A execução gera:

- `artifacts/load-test-summary.json`: métricas completas exportadas pelo k6;
- `artifacts/load-test-report.html`: relatório visual com p95, média, mínimos e máximos.

O teste falha quando o p95 de qualquer endpoint ultrapassa 500 ms ou quando a taxa de requisições com erro chega a 1%. O resultado deve ser anexado ao card como evidência da medição.

## Comparação antes/depois

Para obter uma comparação válida, execute o mesmo comando com a mesma duração, quantidade de VUs e banco de mesmo volume antes e depois da alteração avaliada. Preserve os arquivos com nomes distintos, por exemplo:

```powershell
k6 run .\scripts\k6\music-school-load.js --out json=artifacts\before-stream.json
Copy-Item .\artifacts\load-test-summary.json .\artifacts\before-summary.json
Copy-Item .\artifacts\load-test-report.html .\artifacts\before-report.html
```

Depois repita a execução após aplicar a alteração e salve os arquivos com o prefixo `after`.
