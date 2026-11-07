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
├── services/        # comunicação com a API
├── config/          # configurações estáticas
├── types/           # tipos compartilhados
├── utils/           # funções utilitárias
└── styles/          # tokens de design e estilos globais
```

Imports absolutos com o alias `@/` apontando para `src/`.
