# Estrutura do Projeto

Este documento descreve a organização do projeto XtraScrapper após a reestruturação.

## Organização

O projeto agora possui uma solution centralizada na raiz (`XtraScrapper.sln`) que inclui todos os projetos:

- **CRCChecker** - Biblioteca para verificação de CRC32
- **XtraImageScrapper** - Aplicação para download de imagens de jogos
- **XtraRCleaner** - Ferramenta de limpeza de ROMs
- **XtraScrapper** - Aplicação principal

## Estrutura de Pastas

```
XtraScrapper/
├── XtraScrapper.sln          # Solution principal
├── build/                    # Pasta de saída dos builds
├── docs/                     # Documentação
├── scripts/                  # Scripts de build
└── src/                      # Código fonte
    ├── CRCChecker/          # Biblioteca CRC32
    ├── XtraImageScrapper/    # App de scraping de imagens
    ├── XtraRCleaner/        # App de limpeza de ROMs
    └── XtraScrapper/        # App principal
```

## Build e Publish

### Build da Solution
```bash
dotnet build XtraScrapper.sln
```

### Publish Individual
```bash
# Publicar um projeto específico
dotnet publish src/XtraScrapper/XtraScrapper.csproj -c Release
dotnet publish src/XtraImageScrapper/XtraImageScrapper.csproj -c Release
dotnet publish src/XtraRCleaner/XtraRCleaner.csproj -c Release
```

### Pasta de Saída

Todos os projetos são configurados para publicar na pasta `.\build` na raiz do repositório, mantendo os arquivos organizados em um local centralizado.

## Scripts

Os scripts em `scripts/` facilitam o build de todos os projetos:
- `build-all.bat` - Build para Windows
- `build-all.sh` - Build para Linux/macOS

## Mudanças Realizadas

1. ✅ Criada solution na raiz (`XtraScrapper.sln`)
2. ✅ Adicionados todos os 4 projetos à solution
3. ✅ Removida a solution antiga (`src/XtraScrapper/XtraScrapper.sln`)
4. ✅ Mantida configuração de publish para pasta `.\build`
5. ✅ Testados builds e publishes - tudo funcionando
