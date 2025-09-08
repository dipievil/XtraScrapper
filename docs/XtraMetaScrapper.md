# XtraMetaScrapper - Extrator de Metadados de ROMs

O **XtraMetaScrapper** é uma ferramenta especializada para extrair metadados completos de ROMs usando a API do ScreenScraper.fr. Parte da XtraScrapper Suite! 🎮

## 🌟 Funcionalidades

- ✅ Escaneia pastas de ROMs recursivamente 
- ✅ Integração com API do ScreenScraper.fr
- ✅ Extração automática de metadados completos
- ✅ Cache SQLite para evitar re-downloads
- ✅ Múltiplos formatos de saída (JSON, XML, CSV)
- ✅ Configuração flexível de estrutura de pastas via JSON
- ✅ Multi-idioma (PT-BR/EN)  
- ✅ Log detalhado das operações
- ✅ Executável único (.exe)
- ✅ Rate limiting para não sobrecarregar o ScreenScraper

## 📋 Metadados Extraídos

| Campo | Descrição | Exemplo |
|-------|-----------|---------|
| **GameName** | Nome do jogo | "Sonic The Hedgehog" |
| **Description** | Descrição/sinopse | "A fast-paced platformer..." |
| **Publisher** | Editora | "Sega" |
| **Developer** | Desenvolvedor | "Sonic Team" |
| **Genre** | Gênero | "Platform" |
| **ReleaseDate** | Data de lançamento | "1991" |
| **Rating** | Nota/avaliação | "4.5/5" |
| **Players** | Número de jogadores | "1" |
| **System** | Sistema | "MegaDrive" |
| **Region** | Região | "World" |
| **Language** | Idioma | "EN" |

## 🚀 Uso Básico

```bash
# Modo simples - usa configurações do appsettings.json
XtraMetaScrapper.exe

# Especificar pasta de ROMs
XtraMetaScrapper.exe --folder "C:\MeusRoms"

# Com credenciais do ScreenScraper (recomendado para mais requests)
XtraMetaScrapper.exe --user "meuuser" --password "minhasenha"
```

## ⚙️ Parâmetros da Linha de Comando

### 📁 Pastas
- `--folder <caminho>` - Pasta dos ROMs (usa appsettings.json se não especificado)
- `--outputfolder <caminho>` - Pasta base para metadados
- `--jsonfolder <caminho>` - Pasta para arquivos JSON
- `--xmlfolder <caminho>` - Pasta para arquivos XML  
- `--csvfolder <caminho>` - Pasta para arquivos CSV

### 🔧 Configuração
- `--outputconfig <arquivo>` - Arquivo JSON com configuração de saída
- `--user <usuário>` - Usuário do ScreenScraper
- `--password <senha>` - Senha do ScreenScraper
- `--apikey <chave>` - Chave da API do ScreenScraper

### ❓ Ajuda
- `--help`, `-h`, `/?` - Mostra ajuda

## 📄 Configuração de Saída (JSON)

Crie um arquivo JSON para personalizar onde salvar cada formato:

### Exemplo: output-config.json
```json
{
  "outputFolder": "./metadata/{SYSTEM}",
  "jsonFolder": "./metadata/{SYSTEM}/json",
  "xmlFolder": "./metadata/{SYSTEM}/xml",
  "csvFolder": "./metadata/{SYSTEM}/csv",
  "exportJson": true,
  "exportXml": false,
  "exportCsv": false
}
```

### Usando a configuração:
```bash
XtraMetaScrapper.exe --outputconfig "output-config.json"
```

### Placeholders disponíveis:
- `{SYSTEM}` - Nome do sistema (ex: MasterSystem, MegaDrive)

## 📄 Formatos de Saída

### 🔹 JSON (Padrão)
```json
{
  "gameName": "Sonic The Hedgehog",
  "description": "A fast-paced platformer game...",
  "publisher": "Sega",
  "developer": "Sonic Team",
  "genre": "Platform",
  "releaseDate": "1991",
  "rating": "4.5/5",
  "players": "1",
  "system": "MegaDrive",
  "region": "World",
  "language": "EN"
}
```

### 🔹 XML
```xml
<?xml version="1.0" encoding="UTF-8"?>
<game>
    <name>Sonic The Hedgehog</name>
    <description>A fast-paced platformer game...</description>
    <publisher>Sega</publisher>
    <developer>Sonic Team</developer>
    <genre>Platform</genre>
    <releasedate>1991</releasedate>
    <rating>4.5/5</rating>
    <players>1</players>
    <system>MegaDrive</system>
</game>
```

### 🔹 CSV
```csv
RomPath,GameName,Description,Publisher,Developer,Genre,ReleaseDate,Rating,Players,System
"C:\ROMs\Sonic.bin","Sonic The Hedgehog","A fast-paced platformer...","Sega","Sonic Team","Platform","1991","4.5/5","1","MegaDrive"
```

## 📂 Estrutura de Saída

```
metadata/
├── MegaDrive/
│   ├── json/
│   │   ├── Sonic The Hedgehog.json
│   │   └── Streets of Rage.json
│   ├── xml/
│   │   ├── Sonic The Hedgehog.xml
│   │   └── Streets of Rage.xml
│   └── csv/
│       └── MegaDrive_metadata.csv
└── MasterSystem/
    ├── json/
    └── xml/
```

## ⚙️ Configuração (appsettings.json)

```json
{
  "Settings": {
    "RomsFolder": "./roms",
    "DatabasePath": "XtraMetaScrapper.db",
    "LogFilePath": "XtraMetaScrapper_{0:yyyyMMdd_HHmmss}.log",
    "OutputConfigPath": "output-config.json",
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

## 🎮 Exemplos de Uso

### 📋 Cenário 1: Extração Simples
```bash
# Extrair metadados da pasta padrão (JSON apenas)
XtraMetaScrapper.exe

# Resultado:
# metadata/MegaDrive/json/Sonic The Hedgehog.json
# metadata/MegaDrive/json/Streets of Rage.json
```

### 📊 Cenário 2: Múltiplos Formatos
```bash
# Configurar para exportar JSON, XML e CSV
# (edite output-config.json primeiro)
XtraMetaScrapper.exe --outputconfig "output-config.json"

# Resultado:
# metadata/MegaDrive/json/Sonic.json
# metadata/MegaDrive/xml/Sonic.xml  
# metadata/MegaDrive/csv/MegaDrive_metadata.csv
```

### 🔐 Cenário 3: Com Credenciais
```bash
# Com conta do ScreenScraper para mais quota
XtraMetaScrapper.exe --user "meuuser" --password "minhasenha" --folder "C:\ROMs"
```

## 📊 Cache e Performance

### 💾 Banco SQLite
- Cache automático de metadados extraídos
- Evita re-scraping desnecessário
- Arquivo: `XtraMetaScrapper.db`

### ⚡ Rate Limiting
- Respeita limites da API do ScreenScraper
- Configurável via `MaxRequestsPerSecond`
- Previne bloqueios por spam

## 🌐 Credenciais do ScreenScraper

Para obter mais quota de requests, crie uma conta gratuita em:
- **Website**: https://www.screenscraper.fr/
- **Limite sem conta**: ~10.000 requests/dia  
- **Limite com conta**: ~100.000 requests/dia

## 🔧 Troubleshooting

### ❌ Problemas Comuns

**"Pasta de ROMs não encontrada"**
- Verifique se a pasta existe
- Use caminho absoluto: `C:\ROMs`

**"Metadados não encontrados"**
- ROM pode não estar no ScreenScraper
- Verifique se o CRC32 está correto
- Tente com nome diferente do arquivo

**"Rate limit exceeded"**
- Diminua `MaxRequestsPerSecond`
- Use credenciais do ScreenScraper
- Aguarde e tente novamente

### 📝 Logs Detalhados
Verifique o arquivo de log gerado:
```
📝 Log salvo em: XtraMetaScrapper_20250905_151022.log
```

## 📊 Códigos de Retorno

- `0` - Sucesso
- `1` - Erro (argumentos inválidos, pasta não encontrada, etc.)

## 🎯 Dicas de Performance

1. **Use credenciais**: Conta do ScreenScraper aumenta muito a quota
2. **Organize por sistema**: Mantenha ROMs organizados em subpastas
3. **Verifique o cache**: O banco SQLite evita re-scraping desnecessário
4. **Monitor o log**: Acompanhe o progresso e possíveis erros
5. **Escolha formatos**: JSON é mais rápido que XML/CSV

## 🔄 Integração com outros Apps

O XtraMetaScrapper complementa perfeitamente outros apps da suite:

1. **XtraScrapper** - Organiza os ROMs primeiro
2. **XtraMetaScrapper** - Extrai os metadados  
3. **XtraImageScrapper** - Baixa as imagens
4. **Resultado**: Coleção completamente organizada! 🎉

## 📚 Limitações

- Depende da disponibilidade da API do ScreenScraper.fr
- ROMs devem ter CRC32 correto para identificação precisa
- Alguns jogos podem não ter todos os metadados disponíveis
- Rate limiting pode tornar o processo lento para grandes coleções

## 🆕 Novidades

### v0.1.0 - "Metadata Power-Up!" (Set 2025)
- ✅ Extração completa de metadados
- ✅ Múltiplos formatos de saída (JSON/XML/CSV)
- ✅ Cache SQLite inteligente
- ✅ Rate limiting configurável
- ✅ Estrutura de pastas flexível
- ✅ Multi-idioma PT-BR/EN
