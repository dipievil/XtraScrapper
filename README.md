# 🎮 XtraScrapper ROM Organizer 🎮

```
 ██╗  ██╗████████╗██████╗  █████╗ ███████╗ ██████╗██████╗  █████╗ ██████╗ ██████╗ ███████╗██████╗ 
 ╚██╗██╔╝╚══██╔══╝██╔══██╗██╔══██╗██╔════╝██╔════╝██╔══██╗██╔══██╗██╔══██╗██╔══██╗██╔════╝██╔══██╗
  ╚███╔╝    ██║   ██████╔╝███████║███████╗██║     ██████╔╝███████║██████╔╝██████╔╝█████╗  ██████╔╝
  ██╔██╗    ██║   ██╔══██╗██╔══██║╚════██║██║     ██╔══██╗██╔══██║██╔═══╝ ██╔═══╝ ██╔══╝  ██╔══██╗
 ██╔╝ ██╗   ██║   ██║  ██║██║  ██║███████║╚██████╗██║  ██║██║  ██║██║     ██║     ███████╗██║  ██║
 ╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝ ╚═════╝╚═╝  ╚═╝╚═╝  ╚═╝╚═╝     ╚═╝     ╚══════╝╚═╝  ╚═╝
```

**A suíte definitiva de ferramentas para colecionadores retro! 🕹️**

---

## 🌟 Sobre a Suíte

XtraScrapper é uma coleção completa de ferramentas para colecionadores e entusiastas retro! 

### 🛠️ Ferramentas Disponíveis:

#### 🎮 XtraScrapper (ROM Organizer)
O organizador de ROMs principal que usa arquivos DAT para renomear e organizar seus jogos com precisão de pixel-perfect!

#### 📸 XtraImageScrapper (Image Downloader) **NEW!**
Baixa imagens de URLs ou sites de forma inteligente e organizada, perfeito para capas, screenshots e arte de jogos!

#### 🧹 XtraRCleaner (ROM Cleaner)
Limpa e organiza suas coleções de ROMs removendo duplicatas e arquivos problemáticos.

### ⚡ Power-Ups Compartilhados:
- 🎯 **Interface Unificada** - Mesma experiência em todas as ferramentas
- 🔍 **Precisão Técnica** - Verificação por CRC32 e checksums  
- 📁 **Organização Inteligente** - Cria estruturas de pastas organizadas
- 🌍 **Multi-idioma** - Suporte PT-BR e EN em todas as ferramentas
- 💾 **Executáveis Únicos** - Sem dependências, só baixar e usar!
- ⚙️ **Altamente Configurável** - Via linha de comando ou arquivos de config

---

## 🚀 Como Começar a Jogatina

### 📋 Requisitos do Sistema
- Windows 64-bit
- Nenhuma outra coisa! Todos são self-contained! 🎉

### 💾 Instalação (Super Easy Mode)
1. Baixe os executáveis da [última release](../../releases):
   - 🎮 `XtraScrapper.exe` - Organizador de ROMs
   - 📸 `XtraImageScrapper.exe` - Downloader de imagens **NEW!**  
   - 🧹 `XtraRCleaner.exe` - Limpador de ROMs
2. Extraia onde quiser
3. Pronto! Sem instalação, sem stress! 

### 📂 O que vem no pacote:
```
📦 XtraScrapper-Suite/
├── 🎮 XtraScrapper.exe         (Organizador de ROMs - 65MB)
├── 📸 XtraImageScrapper.exe    (Downloader de imagens - ~45MB) NEW!
├── 🧹 XtraRCleaner.exe         (Limpador de ROMs - ~40MB)
├── ⚙️ appsettings.json         (Configurações)
└── 📖 README.txt               (Instruções básicas)
```

---

## 🎮 Como Jogar... quer dizer, Usar!

### 🏁 XtraScrapper (ROM Organizer)
```bash
# Usar configurações padrão do appsettings.json
XtraScrapper.exe

# Especificar pasta de ROMs e arquivo DAT
XtraScrapper.exe --folder "C:\MeusJogos" --dat "mega-drive.dat"

# Organizar em subpastas por sistema
XtraScrapper.exe --move-sys
```

### 📸 XtraImageScrapper (Image Downloader) **NEW!**
```bash
# Baixar uma imagem
XtraImageScrapper.exe --url "https://example.com/image.jpg"

# Baixar lista de imagens de arquivo
XtraImageScrapper.exe --url "urls.txt" --output "C:\Imagens"

# Downloads simultâneos customizados
XtraImageScrapper.exe --url "images.txt" --concurrent 10
```

### 🧹 XtraRCleaner (ROM Cleaner)
```bash
# Limpar ROMs duplicados
XtraRCleaner.exe --input "C:\ROMs" --output "C:\ROMs_Limpos"

# Modo backup (preserva originais)
XtraRCleaner.exe --backup --input "C:\ROMs"
```

### 🎛️ Controles (Parâmetros Principais)

#### 🎮 XtraScrapper
| Botão | Comando | O que faz |
|-------|---------|-----------|
| 📁 | `--folder <caminho>` | Define onde estão seus ROMs |
| 💿 | `--dat <arquivo>` | Especifica o arquivo DAT a usar |
| 🗂️ | `--move-sys` | Cria subpastas por sistema |
| ❓ | `--help` ou `-h` | Mostra a tela de ajuda |

#### 📸 XtraImageScrapper **NEW!**
| Botão | Comando | O que faz |
|-------|---------|-----------|
| 🔗 | `--url <url/arquivo>` | URL da imagem ou arquivo com URLs |
| 📁 | `--output <pasta>` | Pasta de destino das imagens |
| ⚡ | `--concurrent <n>` | Número de downloads simultâneos |
| ❓ | `--help` ou `-h` | Mostra a tela de ajuda |

#### 🧹 XtraRCleaner  
| Botão | Comando | O que faz |
|-------|---------|-----------|
| 📂 | `--input <pasta>` | Pasta de ROMs para limpar |
| 📁 | `--output <pasta>` | Pasta de destino limpa |
| 💾 | `--backup` | Modo backup (preserva originais) |
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

### 📸 Cenário 2: Baixando Capas de Jogos **NEW!**
```bash
# Baixar capas de uma lista
XtraImageScrapper.exe --url "capas-megadrive.txt" --output "C:\Capas"

# Resultado:
# ✅ Baixado: sonic_cover.jpg  
# ✅ Baixado: streets_of_rage_cover.png
# ✅ Baixado: golden_axe_cover.jpg
# 📊 Total: 45 imagens baixadas em 02:15
```

### 🗂️ Cenário 3: Combo Completo
```bash
# 1. Organizar ROMs primeiro
XtraScrapper.exe --folder "C:\ROMs" --dat "mega-drive.dat" --move-sys

# 2. Baixar capas organizadas
XtraImageScrapper.exe --url "covers.txt" --output "C:\ROMs\Mega Drive\Covers"

# 3. Limpar duplicatas  
XtraRCleaner.exe --input "C:\ROMs" --backup
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

### 🏅 Achievement Unlocked: Coleção Perfeita
1. **Use DATs oficiais** - Baixe de sites como No-Intro ou TOSEC
2. **Backup primeiro** - Sempre faça backup antes de organizar
3. **Workflow completo** - Organize ROMs → Baixe imagens → Limpe duplicatas
4. **Verifique os logs** - Confira se tudo foi processado certinho
5. **Mantenha atualizado** - ROMs e imagens novas aparecem sempre!

### 🎮 Workflow Recomendado da Suíte
```bash
# 1. Organize seus ROMs primeiro
XtraScrapper.exe --folder "C:\ROMs\MegaDrive" --dat "mega-drive.dat" --move-sys

# 2. Baixe capas e screenshots  
XtraImageScrapper.exe --url "megadrive-covers.txt" --output "C:\ROMs\MegaDrive\Images"

# 3. Limpe duplicatas e arquivos problemáticos
XtraRCleaner.exe --input "C:\ROMs\MegaDrive" --backup

# 4. Profit! 🎉
```

---

## 🏆 High Score (Changelog)

### 🌟 v0.0.1 - "First Level Complete!" (Sept 2024)
- 🚀 Release inicial com funcionalidades core
- 🎮 XtraScrapper: Interface de linha de comando para organização de ROMs
- 💿 Suporte a arquivos DAT e verificação CRC32
- 📁 Organização em subpastas por sistema
- 💾 Executáveis self-contained (65MB)
- 🌍 Suporte multi-idioma PT-BR/EN

### 🆕 v0.1.0 - "Power-Up Unlocked!" (Coming Soon)
- 📸 **XtraImageScrapper**: Novo app para download de imagens!
- 🧹 **XtraRCleaner**: Limpador avançado de ROMs
- ⚡ Downloads simultâneos configuráveis
- 🎯 Validação inteligente de tipos de arquivo
- 📊 Estatísticas detalhadas de processamento
- 🔧 Configurações avançadas via appsettings.json

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