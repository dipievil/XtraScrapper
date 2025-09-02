# XtraImageScrapper Documentation

## Overview

XtraImageScrapper é um aplicativo console .NET 9 desenvolvido para fazer download de imagens a partir de URLs ou arquivos contendo listas de URLs. É parte da suíte XtraScrapper de ferramentas para colecionadores e entusiastas.

## Features

- ✅ Download de imagens de URLs individuais ou lista de URLs em arquivo
- ✅ Downloads simultâneos configuráveis para melhor performance 
- ✅ Validação de extensões de arquivos de imagem
- ✅ Controle de tamanho máximo de arquivo
- ✅ Interface de linha de comando intuitiva
- ✅ Logs detalhados com estatísticas finais
- ✅ Suporte a localização (PT-BR e EN)
- ✅ Executável único (single file) para distribuição
- ✅ Configuração via appsettings.json

## System Requirements

- Windows 64-bit
- Nenhuma dependência adicional (self-contained)

## Installation

1. Baixe o `XtraImageScrapper.exe` da [latest release](../../releases)
2. Extraia para uma pasta de sua escolha
3. Pronto para usar!

## Configuration

O aplicativo usa o arquivo `appsettings.json` para configurações padrão:

```json
{
  "Settings": {
    "OutputPath": ".\\images",
    "MaxConcurrentDownloads": 5,
    "UserAgent": "XtraImageScrapper/0.0.1",
    "TimeoutSeconds": 30,
    "AllowedExtensions": [ ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" ],
    "MaxFileSizeMB": 10
  }
}
```

### Configurações Disponíveis

| Parâmetro | Descrição | Padrão |
|-----------|-----------|---------|
| `OutputPath` | Pasta de destino para downloads | `.\images` |
| `MaxConcurrentDownloads` | Número máximo de downloads simultâneos | `5` |
| `UserAgent` | User agent para requisições HTTP | `XtraImageScrapper/0.0.1` |
| `TimeoutSeconds` | Timeout para downloads em segundos | `30` |
| `AllowedExtensions` | Extensões de arquivo permitidas | `.jpg, .jpeg, .png, .gif, .bmp, .webp` |
| `MaxFileSizeMB` | Tamanho máximo de arquivo em MB | `10` |

## Usage

### Sintaxe Básica

```bash
XtraImageScrapper.exe [opções]
```

### Parâmetros da Linha de Comando

| Parâmetro | Descrição |
|-----------|-----------|
| `--url <url>` | URL da imagem ou arquivo contendo URLs |
| `--output <pasta>` | Pasta de destino (sobrescreve appsettings.json) |
| `--concurrent <n>` | Número de downloads simultâneos |
| `--help, -h, /?` | Mostra ajuda |

### Exemplos de Uso

#### Download de imagem única
```bash
XtraImageScrapper.exe --url "https://example.com/image.jpg"
```

#### Download de lista de URLs de arquivo
```bash
XtraImageScrapper.exe --url "urls.txt" --output "C:\MinhasImagens"
```

#### Download com configuração customizada
```bash
XtraImageScrapper.exe --url "images.txt" --output "downloads" --concurrent 10
```

### Formato do Arquivo de URLs

Crie um arquivo de texto com uma URL por linha:

```
https://example.com/image1.jpg
https://example.com/image2.png
# Esta linha é um comentário e será ignorada
https://example.com/image3.gif
```

- Linhas vazias são ignoradas
- Linhas começando com `#` são tratadas como comentários
- Apenas URLs válidas de imagens serão processadas

## Features Avançadas

### Validação de Arquivos

- O aplicativo verifica se as URLs apontam para imagens válidas baseado na extensão
- Arquivos muito grandes (acima do limite configurado) são rejeitados
- Arquivos existentes são pulados para evitar duplicatas

### Downloads Simultâneos

- Configure o número de downloads simultâneos para otimizar velocidade
- Limite recomendado: 5-10 downloads simultâneos
- Muito alto pode causar timeouts ou banimento temporário

### Tratamento de Erros

- URLs inválidas são logadas e puladas
- Timeouts de rede são tratados graciosamente  
- Estatísticas finais mostram sucessos vs falhas

## Output

O aplicativo cria logs detalhados durante execução:

```
🚀 Iniciando XtraImageScrapper...
📊 Total de URLs encontradas: 15
🖼️ URLs de imagens válidas: 12
📁 Pasta de destino: C:\Downloads\Images
⚡ Downloads simultâneos: 5

✅ Baixado: image1.jpg
✅ Baixado: image2.png
❌ Falha: https://example.com/broken.jpg - Not Found

📊 ESTATÍSTICAS FINAIS:
   Total de arquivos: 12
   ✅ Sucessos: 10
   ❌ Falhas: 2
   ⏱️ Tempo decorrido: 01:23
   📦 Total baixado: 45.2 MB

🎉 Processo concluído! Total: 10 imagens baixadas
```

## Troubleshooting

### Problemas Comuns

**"Nenhuma URL válida encontrada"**
- Verifique se o arquivo de URLs existe e está no formato correto
- Confirme se as URLs são válidas e acessíveis

**"Erro ao criar pasta"**
- Verifique permissões de escrita na pasta de destino
- Certifique-se de que o caminho é válido

**Downloads lentos ou timeouts**
- Reduza o número de downloads simultâneos
- Aumente o valor de `TimeoutSeconds` no appsettings.json

**Imagens não são baixadas**
- Confirme se as extensões estão na lista `AllowedExtensions`
- Verifique se o tamanho dos arquivos não excede `MaxFileSizeMB`

### Logs e Debugging

O aplicativo usa logging para diagnosticar problemas:

- Nível `Information`: Sucessos e progresso
- Nível `Warning`: Arquivos pulados ou problemas menores  
- Nível `Error`: Falhas de download ou erros críticos

## Performance Tips

1. **Ajuste downloads simultâneos**: 5-10 para maioria dos casos
2. **Use SSD**: Para melhor performance de escrita
3. **Conexão estável**: Downloads simultâneos exigem boa conexão
4. **Filtrage URLs**: Remova URLs inválidas do arquivo de entrada

## Integration

### Como usar em scripts/batch

```batch
@echo off
echo Baixando imagens da lista...
XtraImageScrapper.exe --url "daily_images.txt" --output "downloads\today"
if %ERRORLEVEL% EQU 0 (
    echo Download concluído com sucesso!
) else (
    echo Erro durante download
)
```

### PowerShell

```powershell
$result = & "XtraImageScrapper.exe" --url "images.txt" --output "downloads"
if ($LASTEXITCODE -eq 0) {
    Write-Host "Downloads completed successfully!" -ForegroundColor Green
}
```

## Changelog

### v0.0.1
- 🚀 Release inicial
- ✅ Download de imagens de URLs
- ✅ Suporte a arquivos de lista
- ✅ Downloads simultâneos
- ✅ Validação de extensões
- ✅ Interface localizada PT-BR/EN
- ✅ Executável single-file

## Support

Para reportar bugs ou solicitar features:
- Abra uma [issue no GitHub](../../issues)
- Inclua logs de erro se aplicável
- Descreva o comportamento esperado vs atual

## License

Este projeto é open source e faz parte da suíte XtraScrapper.