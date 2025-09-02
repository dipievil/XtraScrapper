# XtraImageScrapper - Baixador de Imagens de ROMs

Aplicativo console que faz scraping de imagens de ROMs do ScreenScraper.fr automaticamente.

## Funcionalidades

- ✅ Escaneia pastas de ROMs recursivamente 
- ✅ Integração com API do ScreenScraper.fr
- ✅ Download automático de imagens (box, screenshots, thumbnails, etc.)
- ✅ Cache SQLite para evitar re-downloads
- ✅ Configuração flexível de estrutura de pastas via JSON
- ✅ Multi-idioma (PT-BR/EN)  
- ✅ Log detalhado das operações
- ✅ Executável único (.exe)
- ✅ Rate limiting para não sobrecarregar o ScreenScraper

## Uso Básico

```bash
# Modo simples - usa configurações do appsettings.json
XtraImageScrapper.exe

# Especificar pasta de ROMs
XtraImageScrapper.exe --folder "C:\MeusRoms"

# Com credenciais do ScreenScraper (recomendado para mais requests)
XtraImageScrapper.exe --user "meuuser" --password "minhasenha"
```

## Parâmetros da Linha de Comando

### Pasta de ROMs
- `--folder <caminho>` - **Obrigatório**: Pasta com ROMs para processar (busca recursiva)

### Configuração de Pastas
- `--imagefolder <caminho>` - Pasta base para imagens
- `--boxfolder <caminho>` - Pasta para imagens de caixa/box
- `--printfolder <caminho>` - Pasta para screenshots/prints
- `--thumbfolder <caminho>` - Pasta para thumbnails
- `--videofolder <caminho>` - Pasta para vídeos
- `--folderconfig <arquivo>` - Arquivo JSON com configuração completa de pastas

### ScreenScraper.fr
- `--user <usuario>` - Username do ScreenScraper (opcional)
- `--password <senha>` - Password do ScreenScraper (opcional)  
- `--apikey <chave>` - API Key do ScreenScraper (opcional)

### Ajuda
- `--help`, `-h`, `/?` - Mostra ajuda

## Estrutura de Pastas

O aplicativo processa **recursivamente** todas as pastas e subpastas da pasta de ROMs:

```
pasta_roms/                    # Pasta informada no --folder
├── MasterSystem/
│   ├── sonic.sms
│   └── alexkidd.zip
├── MegaDrive/
│   ├── streets_of_rage.bin
│   └── subpasta/
│       └── outro_jogo.rom
└── GameBoy/
    └── tetris.gb
```

Após execução, as imagens são salvas seguindo o padrão:
```
pasta_roms/
├── MasterSystem/
│   ├── images/
│   │   ├── sonic_box-2D.png      # Imagem da caixa
│   │   ├── sonic_ss.jpg          # Screenshot
│   │   └── sonic_titre.png       # Imagem principal
│   ├── sonic.sms
│   └── alexkidd.zip
└── MegaDrive/
    ├── images/
    │   ├── streets_of_rage_box-2D.png
    │   └── streets_of_rage_ss.jpg
    └── streets_of_rage.bin
```

## Configuração de Pastas (JSON)

Crie um arquivo JSON para personalizar onde salvar cada tipo de imagem:

### Exemplo: folder-config.json
```json
{
  "imageFolder": "./roms/{SYSTEM}/images",
  "boxFolder": "./roms/{SYSTEM}/images/box",
  "printFolder": "./roms/{SYSTEM}/images/screenshots", 
  "mainImagesFolder": "./roms/{SYSTEM}/images/main",
  "videoFolder": "./roms/{SYSTEM}/videos",
  "thumbFolder": "./roms/{SYSTEM}/images/thumbs"
}
```

### Usando a configuração:
```bash
XtraImageScrapper.exe --folderconfig "folder-config.json"
```

### Placeholders disponíveis:
- `{SYSTEM}` - Nome do sistema (ex: MasterSystem, MegaDrive)

## Configuração (appsettings.json)

```json
{
  "Settings": {
    "RomsFolder": "./roms",
    "DatabasePath": "XtraImageScrapper.db",
    "LogFilePath": "XtraImageScrapper_{0:yyyyMMdd_HHmmss}.log",
    "FolderConfigPath": "folder-config.json",
    "ScreenScraperUser": "",
    "ScreenScraperPassword": "", 
    "ScreenScraperApiKey": "",
    "MaxRequestsPerSecond": 1,
    "TimeoutSeconds": 30
  }
}
```

### Configurações importantes:
- **MaxRequestsPerSecond**: Limite de requests por segundo (padrão: 1)
- **TimeoutSeconds**: Timeout para requests HTTP (padrão: 30s)
- **ScreenScraperUser/Password**: Credenciais para mais quota de requests

## Tipos de Imagens Baixadas

O aplicativo baixa automaticamente os seguintes tipos de mídia do ScreenScraper:

| Tipo ScreenScraper | Descrição | Pasta Padrão |
|---|---|---|
| `box-2D` | Imagem da caixa/box | `{SYSTEM}/images` |
| `ss` | Screenshot do jogo | `{SYSTEM}/images` |
| `titre` | Imagem principal/título | `{SYSTEM}/images` |
| `wheel` | Logo/wheel | `{SYSTEM}/images` |

## Sistemas Suportados

| Sistema | Extensões | ID ScreenScraper |
|---|---|---|
| Master System | `.sms` | 2 |
| Game Gear | `.gg` | 21 |
| Mega Drive | `.md`, `.gen`, `.smd`, `.bin` | 1 |
| NES | `.nes`, `.fds` | 3 |
| Game Boy | `.gb` | 9 |
| Game Boy Color | `.gbc` | 10 |
| Game Boy Advance | `.gba` | 12 |
| Nintendo 64 | `.n64`, `.z64`, `.v64` | 14 |
| Arquivos ZIP | `.zip` | (detecta automaticamente) |

## Cache e Performance

- **SQLite Database**: Armazena cache dos downloads para evitar re-downloads
- **Rate Limiting**: Respeita limites do ScreenScraper (1 request/segundo padrão)
- **Verificação Local**: Pula arquivos que já existem localmente
- **CRC32**: Usa CRC32 para identificação precisa dos ROMs

## Log de Saída

O aplicativo gera logs detalhados:

```
2025-01-01 18:30:15 [Information] Processing: Sonic The Hedgehog
2025-01-01 18:30:16 [Information] Found game in ScreenScraper: Sonic The Hedgehog
2025-01-01 18:30:17 [Information] Downloaded image: box-2D for Sonic The Hedgehog
2025-01-01 18:30:18 [Information] Downloaded image: ss for Sonic The Hedgehog
```

## Exemplo de Uso Completo

### 1. Estrutura inicial:
```
C:\RomScraper\
├── XtraImageScrapper.exe
├── appsettings.json
└── folder-config.json

C:\MinhasRoms\
├── MasterSystem/
│   ├── sonic.sms
│   └── alexkidd.zip
└── MegaDrive/
    └── streets_of_rage.bin
```

### 2. Execute:
```bash
XtraImageScrapper.exe --folder "C:\MinhasRoms" --user "meuuser" --password "minhasenha"
```

### 3. Resultado:
```
C:\MinhasRoms\
├── MasterSystem/
│   ├── images/
│   │   ├── sonic_box-2D.png
│   │   ├── sonic_ss.jpg
│   │   ├── alexkidd_box-2D.png
│   │   └── alexkidd_ss.jpg
│   ├── sonic.sms
│   └── alexkidd.zip
├── MegaDrive/
│   ├── images/
│   │   ├── streets_of_rage_box-2D.png
│   │   └── streets_of_rage_ss.jpg
│   └── streets_of_rage.bin
└── XtraImageScrapper.db
```

## Credenciais do ScreenScraper

Para obter mais quota de requests, crie uma conta gratuita em:
- **Website**: https://www.screenscraper.fr/
- **Limite sem conta**: ~10.000 requests/dia  
- **Limite com conta**: ~100.000 requests/dia

## Códigos de Retorno

- `0` - Sucesso
- `1` - Erro (argumentos inválidos, pasta não encontrada, etc.)

## Dicas de Performance

1. **Use credenciais**: Conta do ScreenScraper aumenta muito a quota
2. **Organize por sistema**: Mantenha ROMs organizados em subpastas
3. **Verifique o cache**: O banco SQLite evita re-downloads desnecessários
4. **Monitor o log**: Acompanhe o progresso e possíveis erros

## Limitações

- Depende da disponibilidade da API do ScreenScraper.fr
- ROMs devem ter CRC32 correto para identificação precisa
- Alguns jogos podem não ter todas as imagens disponíveis
- Rate limiting pode tornar o processo lento para grandes coleções