# XtraRCleaner - Limpador de ROMs

Aplicativo console que verifica ROMs duplicadas baseado em um arquivo DAT e cria um conjunto limpo.

## ⚠️ Mudanças Importantes na v0.2.1

1. **Arquivo DAT obrigatório**: Agora deve ser especificado via `--dat`
2. **Purge seguro**: Nunca deleta ROMs não encontrados no DAT
3. **Extensão .nes**: Suporte adicionado para arquivos NES

## Funcionalidades

- ✅ Verifica conteúdo das pastas e move arquivos únicos baseado em CRC32
- ✅ Suporte a arquivos ZIP e ROMs descompactadas  
- ✅ Modo backup (apenas copia) e purge (deleta duplicatas SEGURO)
- ✅ Multi-idioma (PT-BR/EN)
- ✅ Log detalhado das operações
- ✅ Executável único (.exe)
- ✅ Extensões suportadas: `.rom`, `.sms`, `.gg`, `.zip`, `.bin`, `.nes`

## Uso

```bash
# ⚠️  ARQUIVO DAT É OBRIGATÓRIO!
XtraRCleaner.exe --input "C:\ROMs\NES" --output "C:\ROMs\Organized" --dat "nes.dat"

# Modo backup (copia ao invés de mover)
XtraRCleaner.exe --input "C:\ROMs\NES" --output "C:\ROMs\Valid" --dat "nes.dat" --backup

# Modo purge (remove duplicatas APÓS organizar)
XtraRCleaner.exe --input "C:\ROMs\NES" --output "C:\ROMs\Clean" --dat "nes.dat" --purge
```

## Parâmetros

| Parâmetro | Obrigatório | Descrição |
|-----------|-------------|-----------|
| `--input` | ✅ | Pasta contendo ROMs para processar |
| `--output` | ✅ | Pasta onde organizar ROMs válidos |
| `--dat` | ✅ | **OBRIGATÓRIO** - Arquivo DAT com CRCs conhecidos |
| `--backup` | ❌ | Modo backup (copia ao invés de mover) |
| `--purge` | ❌ | Remove duplicatas (seguro - só após organizar) |

## 🛡️ Segurança do Modo Purge

O modo `--purge` agora é **100% seguro**:
- ✅ ROMs **válidos** (no DAT): Movidos para pasta `checked`
- ✅ ROMs **duplicados**: Deletados apenas APÓS mover o primeiro
- ✅ ROMs **desconhecidos**: Movidos para pasta `new` (nunca deletados)
- ❌ **NUNCA** deleta ROMs não encontrados no DAT

## Estrutura de Pastas

O aplicativo assume a seguinte estrutura:

```
pasta_trabalho/
├── games.dat          # Arquivo DAT com CRCs válidos
├── old/               # ROMs originais para verificar
├── appsettings.json   # Configurações (opcional)
└── XtraRCleaner.exe
```

Após execução:
```
pasta_saida/
├── new/               # ROMs únicas (não duplicadas)
└── checked/           # ROMs que estão no DAT
```

## Configuração (appsettings.json)

```json
{
  "Settings": {
    "DatFilePath": "games.dat",
    "OldRomsPath": ".\\old",
    "NewRomsPath": ".\\new", 
    "CheckedRomsPath": ".\\checked",
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
   ├── games.dat
   └── old\
       ├── jogo1.sms
       ├── jogo2.zip
       └── duplicado.rom
   ```

2. Execute:
   ```bash
   XtraRCleaner.exe --output "C:\RomsLimpas"
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
