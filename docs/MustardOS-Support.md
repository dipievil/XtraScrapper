# XtraImageScrapper - Suporte ao MustardOS

O XtraImageScrapper agora possui suporte completo ao sistema de organização de imagens do **MustardOS**!

## O que é o MustardOS?

O MustardOS é um sistema operacional customizado para handhelds retro que utiliza uma estrutura específica para organizar artwork e imagens dos jogos.

## Estrutura do MustardOS

```
MUOS/
└── info/
    └── catalogue/
        └── <System>/
            ├── box/          # Box art dos jogos
            ├── preview/      # Screenshots/preview dos jogos
            ├── splash/       # Splash screens
            └── text/         # Descrições (não suportado pelo scrapper)
```

## Mapeamento de Tipos de Imagem

| Tipo ScreenScraper | Tipo de Imagem | Pasta MustardOS |
|---|---|---|
| `box-2D` | Box art | `{SYSTEM}/box/` |
| `ss` | Screenshots | `{SYSTEM}/preview/` |
| `titre` | Imagem principal | `{SYSTEM}/preview/` |
| `wheel` | Logo/wheel | `{SYSTEM}/box/` |
| `fanart` | Fanart/splash | `{SYSTEM}/splash/` |
| `screenmarquee` | Screen marquee | `{SYSTEM}/preview/` |

## Como Usar

### 1. Configuração Automática
O projeto já inclui o arquivo `mustaros-config.json` pré-configurado:

```json
{
  "imageFolder": "./MUOS/info/catalogue/{SYSTEM}",
  "boxFolder": "./MUOS/info/catalogue/{SYSTEM}/box",
  "printFolder": "./MUOS/info/catalogue/{SYSTEM}/preview",
  "mainImagesFolder": "./MUOS/info/catalogue/{SYSTEM}/preview",
  "thumbFolder": "./MUOS/info/catalogue/{SYSTEM}/box",
  "splashFolder": "./MUOS/info/catalogue/{SYSTEM}/splash",
  "previewFolder": "./MUOS/info/catalogue/{SYSTEM}/preview"
}
```

### 2. Execução
```bash
XtraImageScrapper.exe --folderconfig "mustaros-config.json" --folder "C:\ROMs"
```

### 3. Resultado
As imagens serão organizadas automaticamente na estrutura correta:

```
MUOS/
└── info/
    └── catalogue/
        ├── NintendoGameBoy/
        │   ├── box/
        │   │   ├── Tetris.png
        │   │   └── SuperMarioLand.png
        │   ├── preview/
        │   │   ├── Tetris_ss.png
        │   │   └── SuperMarioLand_ss.png
        │   └── splash/
        │       ├── Tetris_fanart.png
        │       └── SuperMarioLand_fanart.png
        └── SegaMasterSystem/
            ├── box/
            ├── preview/
            └── splash/
```

## Compatibilidade

### ✅ Funcionalidades Suportadas
- Estrutura de pastas compatível com MustardOS
- Nomes de arquivo automáticos (mesmo nome do ROM)
- Mapeamento correto dos tipos de imagem
- Placeholder {SYSTEM} para nome do sistema
- Cache de downloads para evitar downloads duplicados

### ❌ Limitações
- Arquivo `text/` não é suportado (apenas imagens)
- Depende da disponibilidade de imagens no ScreenScraper

## Sistemas Suportados

O mapeamento funciona com todos os sistemas suportados pelo ScreenScraper, incluindo:
- Nintendo Game Boy / Game Boy Color
- Nintendo Game Boy Advance  
- Sega Master System
- Sega Mega Drive / Genesis
- Super Nintendo / Super Famicom
- Nintendo Entertainment System / Famicom
- E muitos outros...

## Dicas de Uso

1. **Organize seus ROMs por sistema** em pastas separadas
2. **Use nomes limpos** para os arquivos ROM (sem caracteres especiais)
3. **Execute por sistema** para melhor organização:
   ```bash
   XtraImageScrapper.exe --folderconfig "mustaros-config.json" --folder "C:\ROMs\GameBoy"
   ```
4. **Verifique o resultado** na pasta MUOS gerada

## Troubleshooting

### Problema: Imagens não aparecem no MustardOS
- **Solução**: Verifique se o nome do arquivo ROM é exatamente igual ao nome da imagem
- **Exemplo**: `Tetris.gb` → `Tetris.png`

### Problema: Sistema não reconhecido
- **Solução**: Verifique se o nome da pasta do sistema está correto no ScreenScraper
- **Dica**: Use o log do aplicativo para ver como o sistema está sendo detectado

### Problema: Muitas imagens duplicadas
- **Solução**: O aplicativo já possui cache interno, mas você pode deletar o arquivo `.db` para reprocessar
