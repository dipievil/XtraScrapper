# 🎮 XtraScrapper ROM Organizer 🎮

```
 ██╗  ██╗████████╗██████╗  █████╗ ███████╗ ██████╗██████╗  █████╗ ██████╗ ██████╗ ███████╗██████╗ 
 ╚██╗██╔╝╚══██╔══╝██╔══██╗██╔══██╗██╔════╝██╔════╝██╔══██╗██╔══██╗██╔══██╗██╔══██╗██╔════╝██╔══██╗
  ╚███╔╝    ██║   ██████╔╝███████║███████╗██║     ██████╔╝███████║██████╔╝██████╔╝█████╗  ██████╔╝
  ██╔██╗    ██║   ██╔══██╗██╔══██║╚════██║██║     ██╔══██╗██╔══██║██╔═══╝ ██╔═══╝ ██╔══╝  ██╔══██╗
 ██╔╝ ██╗   ██║   ██║  ██║██║  ██║███████║╚██████╗██║  ██║██║  ██║██║     ██║     ███████╗██║  ██║
 ╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝ ╚═════╝╚═╝  ╚═╝╚═╝  ╚═╝╚═╝     ╚═╝     ╚══════╝╚═╝  ╚═╝
```

**O organizador de ROMs definitivo para colecionadores retro! 🕹️**

---

## 🌟 Sobre o Jogo... quer dizer, App!

XtraScrapper é seu companheiro perfeito para organizar aquela coleção de ROMs que tá uma bagunça! 
Como um verdadeiro power-up dos anos 80, ele usa arquivos DAT pra renomear e organizar seus jogos 
com precisão de pixel-perfect! 

## 🎮 Aplicativos da Suite

A XtraScrapper Suite inclui várias ferramentas especializadas:

### 📦 XtraScrapper (Organizador Principal)
- 🎯 **Organização Automática** - Renomeia ROMs baseado em arquivos DAT
- 🔍 **Verificação CRC32** - Garante que seus jogos são legítimos  
- 📁 **Pastas por Sistema** - Cria subpastas organizadas

### 🖼️ XtraImageScrapper 
- 🌐 **Download Automático** - Baixa imagens do ScreenScraper.fr
- 📷 **Múltiplos Tipos** - Box, screenshots, thumbnails, splash
- 💾 **Cache Inteligente** - SQLite para evitar re-downloads
- ⚙️ **Configuração Flexível** - Estrutura de pastas personalizável
- 🎯 **Suporte MustardOS** - Estrutura compatível com handhelds retro

### 📊 XtraMetaScrapper (Novo!)
- 🔍 **Extração Completa** - Todos os metadados do ScreenScraper.fr
- 📄 **Múltiplos Formatos** - JSON, XML, CSV
- 🏷️ **Dados Ricos** - Nome, descrição, publisher, developer, gênero, etc.
- 💾 **Cache SQLite** - Evita re-scraping desnecessário
- ⚙️ **Configuração Flexível** - Estrutura de saída personalizável

### 🧹 XtraRCleaner (Limpador)
- 🗑️ **Remove Duplicatas** - Baseado em CRC32
- 📊 **Validação DAT** - Mantém apenas ROMs conhecidos
- 🔄 **Modos Flexíveis** - Move, copia ou deleta

### ⚡ Features Comuns:
- 🌍 **Multi-idioma** - Suporte PT-BR e EN
- 💾 **Executável Único** - Sem dependências, só baixar e usar!
- ⚙️ **Configurável** - Via linha de comando ou arquivo de config

---

## 🚀 Como Começar a Jogatina

### 📋 Requisitos do Sistema
- Windows 64-bit
- Nenhuma outra coisa! É self-contained! 🎉

### 💾 Instalação (Super Easy Mode)
1. Baixe o `XtraScrapper-v0.0.1-win-x64.zip` da [última release](../../releases)
2. Extraia onde quiser
3. Pronto! Sem instalação, sem stress! 

### 📂 O que vem no pacote:
```
📦 XtraScrapper-v0.0.1-win-x64/
├── 🎮 XtraScrapper.exe        (O jefe principal - 65MB)
├── ⚙️ appsettings.json        (Configurações)
└── 📖 README.txt              (Instruções básicas)
```

---

## 🎮 Como Jogar... quer dizer, Usar!

### 🏁 Start Game (Modo Rápido)
```bash
# Usar configurações padrão do appsettings.json
XtraScrapper.exe

# Especificar pasta de ROMs e arquivo DAT
XtraScrapper.exe --folder "C:\MeusJogos" --dat "mega-drive.dat"

# Organizar em subpastas por sistema
XtraScrapper.exe --move-sys

# Combo completo!
XtraScrapper.exe --folder "C:\ROMs" --dat "games.dat" --move-sys
```

### 🎛️ Controles (Parâmetros)

| Botão | Comando | O que faz |
|-------|---------|-----------|
| 📁 | `--folder <caminho>` | Define onde estão seus ROMs |
| 💿 | `--dat <arquivo>` | Especifica o arquivo DAT a usar |
| 🗂️ | `--move-sys` | Cria subpastas por sistema |
| ❓ | `--help` ou `-h` | Mostra a tela de ajuda |

### ⚙️ Arquivo de Config (appsettings.json)
```json
{
  "DatFilePath": "games.dat",
  "RomsFolderPath": "roms"
}
```

**Protip**: Os parâmetros da linha de comando sempre sobrescrevem o arquivo de config! 💡

---

## 🏆 Exemplos de Gameplay

### 🎮 Cenário 1: Organizando Master System
```bash
# Seus ROMs: Sonic.sms, Alex Kidd.bin, random_game.rom
XtraScrapper.exe --folder "C:\Master System" --dat "sms.dat"

# Resultado: 
# ✅ Sonic.sms → Sonic The Hedgehog (World).sms
# ✅ Alex Kidd.bin → Alex Kidd in Miracle World (World).bin  
# ✅ random_game.rom → Wonder Boy (World).rom
```

### 🗂️ Cenário 2: Organizando com Subpastas
```bash
XtraScrapper.exe --folder "C:\ROMs" --dat "mega-drive.dat" --move-sys

# Antes:
# C:\ROMs\sonic.bin
# C:\ROMs\streets_of_rage.rom

# Depois:
# C:\ROMs\Mega Drive\Sonic The Hedgehog (World).bin
# C:\ROMs\Mega Drive\Streets of Rage (World).rom
```

### 🎯 Cenário 3: Modo Automático
```bash
# Configure uma vez no appsettings.json:
{
  "DatFilePath": "meus-jogos.dat",
  "RomsFolderPath": "C:\\MinhaColecao"
}

# Depois só execute:
XtraScrapper.exe
```

---

## 🔧 Troubleshooting (Debug Mode)

### ❌ Problemas Comuns

**"Arquivo DAT não encontrado"**
- Verifique se o arquivo `.dat` existe no local especificado
- Use caminho absoluto: `C:\Games\system.dat`

**"Pasta de ROMs não existe"**  
- O app cria a pasta automaticamente se não existir
- Verifique permissões de escrita

**"ROM não foi renomeada"**
- ROM pode não estar no arquivo DAT
- Verifique se o CRC32 confere
- Alguns ROMs podem ter nomes de região diferentes

### 📝 Log de Jogatina
O app gera um log detalhado das operações:
```
📝 Log salvo em: XtraScrapper_20241201_143022.log
```

Abra o arquivo pra ver o que rolou com cada ROM! 🕵️

---

## 🎮 Formatos Suportados

| Extensão | Sistema | Status |
|----------|---------|--------|
| `.sms` | Master System | ✅ |
| `.gg` | Game Gear | ✅ |  
| `.rom` | Genérico | ✅ |
| `.bin` | Mega Drive/Genesis | ✅ |
| `.zip` | Qualquer (compactado) | ✅ |

---

## 🎯 Pro Tips para Colecionadores

### 🏅 Achievement Unlocked: Organização Perfeita
1. **Use DATs oficiais** - Baixe de sites como No-Intro ou TOSEC
2. **Backup primeiro** - Sempre faça backup antes de organizar
3. **Uma pasta por sistema** - Use `--move-sys` pra organizar melhor  
4. **Verifique os logs** - Confira se tudo foi renomeado certinho
5. **Mantenha DATs atualizados** - ROMs novos aparecem sempre!

### 🎮 Workflow Recomendado
```bash
# 1. Baixe o DAT oficial do sistema
# 2. Organize os ROMs
XtraScrapper.exe --folder "C:\ROMs\MegaDrive" --dat "mega-drive.dat" --move-sys

# 3. Baixe as imagens dos jogos
XtraImageScrapper.exe --folder "C:\ROMs\MegaDrive" --user "meuuser" --password "minhasenha"

# Para MustardOS (handhelds retro)
XtraImageScrapper.exe --folderconfig "mustaros-config.json" --folder "C:\ROMs\GameBoy"

# 4. Extraia os metadados dos jogos
XtraMetaScrapper.exe --folder "C:\ROMs\MegaDrive" --user "meuuser" --password "minhasenha"

# 4. Confira os logs gerados
# 5. Profit! 🎉
```

## 📚 Documentação Detalhada

- 📖 [XtraScrapper (Organizador)](docs/XtraScrapper.md)
- 🖼️ [XtraImageScrapper (Download de Imagens)](docs/XtraImageScrapper.md)  
- 📊 [XtraMetaScrapper (Extração de Metadados)](docs/XtraMetaScrapper.md)
- 🎯 [Suporte ao MustardOS](docs/MustardOS-Support.md)
- 🧹 [XtraRCleaner (Limpador)](docs/XtraRCleaner.md)

---

## 🏆 High Score (Changelog)

### 🆕 v0.2.0 - "Metadata Power-Up!" (Set 2025)
- 📊 **XtraMetaScrapper**: Novo app para extração de metadados completos
- 🔍 Extração de nome, descrição, publisher, developer, gênero, etc.
- 📄 Múltiplos formatos de saída: JSON, XML, CSV
- 💾 Cache SQLite para evitar re-scraping
- ⚙️ Configuração flexível de estrutura de saída

### 🆕 v0.1.1 - "MustardOS Power-Up!" (Set 2025)
- 🎯 **Suporte ao MustardOS**: Configuração automática para handhelds retro
- 📁 Novos tipos de pasta: splash e preview
- 🔧 Novos parâmetros: --splashfolder e --previewfolder
- 📄 Configuração pré-definida: mustaros-config.json
- 📖 Documentação completa do MustardOS

### 🆕 v0.1.0 - "Image Power-Up!" (Jan 2025)
- 🖼️ **XtraImageScrapper**: Novo app para download de imagens
- 🌐 Integração com ScreenScraper.fr API
- 💾 Cache SQLite para performance
- ⚙️ Configuração flexível de pastas via JSON
- 📷 Suporte a múltiplos tipos de imagem (box, screenshot, etc.)

### 🌟 v0.0.1 - "First Level Complete!" (Sept 2024)
- 🚀 Release inicial com funcionalidades core
- 🎮 Interface de linha de comando  
- 💿 Suporte a arquivos DAT
- 📁 Organização em subpastas por sistema
- 💾 Executável self-contained (65MB)
- 🌍 Suporte multi-idioma
- 🔍 Verificação CRC32

---

## 🆘 Suporte & Comunidade

### 🐛 Achou um Bug?
Abra uma [issue no GitHub](../../issues) com:
- Versão do Windows
- Comando usado
- Arquivo de log
- Descrição do problema

### 💡 Quer uma Feature Nova?
Suggestion box tá aberto nas [issues](../../issues)! 

### 🤝 Contribuir
Pull requests são sempre bem-vindos! É open source, galera! 

---

## 📜 Créditos

```
🎮 Desenvolvido com .NET 9.0
🏆 Criado por dipievil  
🌟 Inspirado na nostalgia dos anos 80/90
💾 Feito com ❤️ para a comunidade retro gaming

    ╔═══════════════════════════════════════╗
    ║         THANK YOU FOR PLAYING!        ║
    ║                                       ║  
    ║    🎮 Happy ROM Organizing! 🕹️        ║
    ╚═══════════════════════════════════════╝
```

---

**Game Over? Não! Isso é só o começo da sua jornada de organização retro! 🚀**

*Press START to organize your ROMs!* ⭐