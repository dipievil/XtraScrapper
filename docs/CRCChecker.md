# XtraRCleaner - Limpador de ROMs

Aplicativo console que verifica ROMs duplicadas baseado em um arquivo DAT e cria um conjunto limpo.

## Funcionalidades

- ✅ Verifica conteúdo das pastas e move arquivos únicos baseado em CRC32
- ✅ Suporte a arquivos ZIP e ROMs descompactadas  
- ✅ Modo backup (apenas copia) e purge (deleta duplicatas)
- ✅ Multi-idioma (PT-BR/EN)
- ✅ Log detalhado das operações
- ✅ Executável único (.exe)

## Uso

```bash
# Modo padrão (move arquivos únicos)
XtraRCleaner.exe --input "C:\RomsOriginais" --output "C:\MinhasRoms"

# Modo backup (apenas copia)
XtraRCleaner.exe --input "C:\RomsOriginais" --output "C:\MinhasRoms" --backup

# Modo limpeza (deleta duplicados)
XtraRCleaner.exe --input "C:\RomsOriginais" --output "C:\MinhasRoms" --purge
```

## Parâmetros

- `--input <caminho>` - **Obrigatório**: Pasta com ROMs originais para verificar (busca recursiva)
- `--output <caminho>` - **Obrigatório**: Pasta de destino para os arquivos limpos
- `--backup` - Opcional: Copia arquivos ao invés de mover
- `--purge` - Opcional: Deleta arquivos duplicados

## Estrutura de Pastas

O aplicativo processa **recursivamente** todas as pastas e subpastas da pasta de entrada:

```
pasta_trabalho/
├── games.dat          # Arquivo DAT com CRCs válidos
├── appsettings.json   # Configurações (opcional)
└── XtraRCleaner.exe

pasta_entrada/          # Pasta informada no --input
├── sistema1/
│   ├── jogo1.rom
│   └── jogo2.zip
├── sistema2/
│   └── subpasta/
│       └── jogo3.sms
└── avulsos/
    └── jogo4.gg
```

Após execução na pasta de saída:
```
pasta_saida/            # Pasta informada no --output
├── new/               # ROMs únicas (não duplicadas)
└── checked/           # ROMs que estão no DAT
```

## Configuração (appsettings.json)

```json
{
  "Settings": {
    "DatFilePath": "games.dat",
    "LogFilePath": "XtraRCleaner_{0:yyyyMMdd_HHmmss}.log"
  }
}
```

## Formato do Arquivo DAT

O aplicativo suporta arquivos DAT no formato ClrMamePro:

```
clrmamepro (
    name "Nome do Sistema"
    description "Descrição"
)

game (
    name "Nome do Jogo"
    rom ( name "jogo.rom" size 12345 crc 1234ABCD )
)
```

## Log de Saída

O aplicativo gera um log detalhado:

```
2025-09-01 18:30:15 [Information] R-Type (World).sms >> ok
2025-09-01 18:30:16 [Information] Desert Strike (UE) [!].zip >> já existe
2025-09-01 18:30:17 [Information] unknown_game.rom >> não está no DAT
```

## Formatos Suportados

- `.rom` - ROMs genéricas
- `.sms` - Sega Master System
- `.gg` - Game Gear  
- `.zip` - Arquivos compactados
- `.bin` - Arquivos binários

## Exemplo de Uso Completo

1. Prepare a estrutura:
   ```
   C:\RomCleaner\
   ├── XtraRCleaner.exe
   └── games.dat
   
   C:\MinhasRomsOriginais\
   ├── MasterSystem/
   │   ├── jogo1.sms
   │   └── jogo2.zip
   └── GameGear/
       └── jogo3.gg
   ```

2. Execute:
   ```bash
   XtraRCleaner.exe --input "C:\MinhasRomsOriginais" --output "C:\RomsLimpas"
   ```

3. Resultado:
   ```
   C:\RomsLimpas\
   ├── checked\
   │   ├── jogo1.sms      # CRC válido no DAT
   │   └── jogo2.zip      # CRC válido no DAT
   └── XtraRCleaner_20250901_183015.log
   ```

## Códigos de Retorno

- `0` - Sucesso
- `1` - Erro (argumentos inválidos, arquivo DAT não encontrado, etc.)
