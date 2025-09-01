namespace CRCChecker;

public class Crc32Service : ICrc32Service
{
    private const uint DefaultPolynomial = 0xEDB88320u;
    private readonly uint[] _table;
    private uint _crc;

    public Crc32Service()
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

    public void Reset()
    {
        _crc = 0xFFFFFFFFu;
    }
}

