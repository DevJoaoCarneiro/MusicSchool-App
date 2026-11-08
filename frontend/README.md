# MusicSchool — Frontend

React 19 + TypeScript + Vite, com arquitetura baseada em componentes.

O frontend é apenas uma fachada: exibe dados e envia ações. Regras de negócio, validações e permissões são decididas pela API, e as mensagens exibidas são as que ela devolve.

## Rodando com o backend

1. Suba a API (`backend/Api`). Pelo perfil padrão ela responde em `https://localhost:7260`.
2. No frontend:

```bash
npm install
npm run dev
```

O Vite encaminha `/api` para a API. Para apontar para outro endereço, copie `.env.example` para `.env` e ajuste `VITE_API_PROXY_TARGET`.

Em produção, defina `VITE_API_URL` com a URL pública da API antes do `npm run build`.

## Scripts

```bash
npm run dev
npm run build
npm run lint
npm run preview
```

## Testes de integração (Playwright)

Os testes em `e2e/` usam o front e a API de verdade: abrem o Chrome instalado na máquina e percorrem todas as telas e botões.

1. Suba a API apontando para um **banco de testes**. Os testes criam alunos, matrículas, aulas e mensalidades com nomes únicos, e podem ser repetidos sem limpar o banco.
2. Rode, informando um usuário Admin dessa API:

```bash
E2E_ADMIN_EMAIL=admin@exemplo.com E2E_ADMIN_PASSWORD=senha npm run test:e2e
```

- Se nenhum front estiver rodando em `http://localhost:5173`, o Playwright sobe o `npm run dev` sozinho. Para usar outro endereço, defina `E2E_BASE_URL`.
- O setup cria os usuários Secretaria, Professor e Aluno usados nos testes de permissão.
- A API limita tentativas de login por minuto. Os testes reaproveitam as sessões, mas em execuções seguidas pode ser preciso aumentar `RateLimiting__Login__PermitLimit` na API de testes.
- `npm run test:e2e:ui` abre o modo interativo, e `npm run test:e2e:report` mostra o relatório da última execução.

### Vídeo da execução

```bash
E2E_ADMIN_EMAIL=admin@exemplo.com E2E_ADMIN_PASSWORD=senha npm run test:e2e:video
```

- Grava todos os testes (em câmera lenta, `E2E_SLOWMO=250`) e gera `test-results/video/index.html`: um player que toca os vídeos em sequência, com a lista de testes e o status de cada um.
- Com o ffmpeg completo instalado (no PATH ou em `FFMPEG_PATH`), gera também `test-results/video/execucao-completa-<resultado>.webm`, um arquivo único.
- Na primeira vez, rode `npx playwright install ffmpeg` (componente de gravação do Playwright, 1,6 MB).
- No `npm run test:e2e` normal, o vídeo só é guardado quando um teste falha.

| Arquivo | O que cobre |
|---|---|
| `auth.spec.ts` | login, erro de login, sair, rota protegida, perfil sem acesso, token inválido, 404 |
| `navigation.spec.ts` | todos os itens do menu, cards da Início, topo, recolher menu, gaveta no celular |
| `roles.spec.ts` | o que Secretaria e Professor veem e o que fica bloqueado |
| `instruments.spec.ts` | criar, duplicado, editar, excluir, exclusão bloqueada, fechar modal |
| `teachers.spec.ts` | cadastro com especialidade, busca, detalhe, edição, exclusão, CPF e e-mail inválidos |
| `students.spec.ts` | menor com responsável, CPF inválido, edição, detalhe, exclusão, exclusão bloqueada |
| `users.spec.ts` | paginação, criação com perfil, e-mail repetido |
| `enrollments.spec.ts` | criação, duplicada, validação, filtros, detalhe, encerramento |
| `agenda.spec.ts` | aula avulsa, semana/mês, períodos, filtros, recorrência com conflito, concluir, reagendar, cancelar, cancelar série |
| `tuitions.spec.ts` | gerar, não duplicar, mês inválido, filtros, registrar pagamento |
| `change-password.spec.ts` | senha atual errada sem deslogar, senha igual, troca com sucesso |
| `responsive.spec.ts` | nenhuma tela (nem a agenda no mês, nem o login) transborda na horizontal em 360, 768 e 1024px |

## Estrutura

```
src/
├── components/
│   ├── ui/          # componentes visuais reutilizáveis
│   └── layout/      # estrutura de página
├── pages/           # telas, compostas a partir dos componentes
├── routes/          # rotas e guardas por perfil
├── contexts/        # sessão e avisos (providers)
├── hooks/           # hooks reutilizáveis
├── services/        # chamadas HTTP à API (http.ts + um service por recurso)
├── config/          # menu, perfis e rótulos
├── types/           # contratos da API
├── utils/           # máscaras, datas e formatação
└── styles/          # tokens de design e estilos globais
```

Imports absolutos com o alias `@/` apontando para `src/`.

## Sessão

O token JWT devolvido no login fica no `sessionStorage` e vai em todas as requisições. Se a API responder `401`, a sessão é encerrada e o usuário volta para o login.
