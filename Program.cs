using System.Xml.Linq;

public record GameInfo(string Name, string Crc);

internal class Crc32
{
    private const uint DefaultPolynomial = 0xEDB88320u;
    private readonly uint[] _table;
    private uint _crc;

    public Crc32()
    {
        _table = InitializeTable(DefaultPolynomial);
        Reset();
    }

    private static uint[] InitializeTable(uint polynomial)
    {
        var table = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            uint entry = i;
            for (int j = 0; j < 8; j++)
                entry = (entry >> 1) ^ ((entry & 1) == 1 ? polynomial : 0);
            table[i] = entry;
        }
        return table;
    }

    public void Append(System.IO.Stream stream)
    {
        byte[] buffer = new byte[8192];
        int bytesRead;
        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            for (int i = 0; i < bytesRead; i++)
            {
                _crc = (_crc >> 8) ^ _table[(byte)(_crc ^ buffer[i])];
            }
        }
    }

    public byte[] GetHashAndReset()
    {
        uint result = ~_crc;
        byte[] bytes = BitConverter.GetBytes(result);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        Reset();
        return bytes;
    }

    private void Reset()
    {
        _crc = 0xFFFFFFFFu;
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("--- Iniciando o XtraScrapper ---");

        string datFilePath = "exemplo.dat";
        if (!File.Exists(datFilePath))
        {
            Console.WriteLine($"Erro: O arquivo '{datFilePath}' não foi encontrado!");
            return;
        }
        
        Console.WriteLine($"A carregar o arquivo DAT: {datFilePath}...");
        XDocument datFile = XDocument.Load(datFilePath);

        var gamesList = datFile.Descendants("game")
                               .Select(g => new GameInfo(
                                   g.Attribute("name")?.Value ?? "Nome não encontrado", 
                                   g.Element("rom")?.Attribute("crc")?.Value.ToLower() ?? "CRC não encontrado"
                               ))
                               .ToList();

        Console.WriteLine($"\n{gamesList.Count} jogos foram carregados do arquivo DAT.");
        
        Console.WriteLine("\n--- A procurar ROMs para processar ---");
        string romsFolderPath = "roms_para_testar";

        if (!Directory.Exists(romsFolderPath))
        {
            Console.WriteLine($"Erro: A pasta '{romsFolderPath}' não foi encontrada!");
            Console.WriteLine("Certifique-se de que a criou dentro da pasta do projeto.");
            return;
        }

        string[] romFiles = Directory.GetFiles(romsFolderPath);
        Console.WriteLine($"Encontrados {romFiles.Length} arquivos na pasta '{romsFolderPath}'.\n");

        foreach (string filePath in romFiles)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                var crc32 = new Crc32();
                
                crc32.Append(fileStream);
                
                byte[] hashBytes = crc32.GetHashAndReset();

                string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                Console.WriteLine($"- arquivo: {Path.GetFileName(filePath)} | CRC32: {hashString}");
            }
        }
        
        Console.WriteLine("\n--- Processamento de ROMs concluído ---");
    }
}