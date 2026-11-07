# MusicSchool — Frontend

React 19 + TypeScript + Vite, com arquitetura baseada em componentes.

O frontend é apenas uma fachada: exibe dados e envia ações. Regras de negócio, validações e permissões são decididas pela API.

## Scripts

```bash
npm install
npm run dev
npm run build
npm run lint
npm run preview
```

## Estrutura

```
src/
├── components/
│   ├── ui/          # componentes visuais reutilizáveis
│   └── layout/      # estrutura de página
├── pages/           # telas, compostas a partir dos componentes
├── routes/          # configuração de rotas
├── contexts/        # estado global (providers)
├── hooks/           # hooks reutilizáveis
├── services/        # contratos com a API (hoje respondem com os mocks)
├── mocks/           # dados em memória usados pelos services
├── config/          # configurações estáticas
├── types/           # tipos compartilhados
├── utils/           # funções utilitárias
└── styles/          # tokens de design e estilos globais
```

Imports absolutos com o alias `@/` apontando para `src/`.

## Mocks

As telas consomem apenas os arquivos de `services/`, que devolvem o mesmo envelope da API (`{ message, status, data }`). Na integração, só a implementação dos services muda; páginas e componentes continuam iguais.

Contas de demonstração (senha `123456`): `admin@escola.com`, `secretaria@escola.com`, `professor@escola.com`, `aluno@escola.com`.
