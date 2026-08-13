using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Compactador personalizado — algoritmo de Joao
/// 
/// Formato do arquivo .jcmp:
///   [4 bytes]  Assinatura: "JCMP"
///   [256 bytes] Tabela de substituição: posição = símbolo, valor = byte original
///   [1 byte]   Quantidade de faixas (máx 8)
///   [N * 2 bytes] Metadados: pares (casas, quantidade) — 1 byte cada
///   [4 bytes]  Total de bits no stream
///   [? bytes]  Stream de bits — valores sem zeros à esquerda, agrupados por tamanho
/// </summary>
public static class Compactador
{
    // -------------------------------------------------------------------------
    // COMPACTAR
    // -------------------------------------------------------------------------
    public static void Compactar(string caminhoEntrada, string caminhoSaida)
    {
        byte[] dados = File.ReadAllBytes(caminhoEntrada);

        int[] frequencia = new int[256];
        foreach (byte b in dados)
            frequencia[b]++;

        byte[] tabelaSubstituicao = CriarTabelaSubstituicao(frequencia);

        byte[] tabelaInversa = new byte[256];
        for (int i = 0; i < 256; i++)
            tabelaInversa[tabelaSubstituicao[i]] = (byte)i;

        // 3. Recodificar os dados usando a tabela
        byte[] dadosRecodificados = new byte[dados.Length];
        for (int i = 0; i < dados.Length; i++)
            dadosRecodificados[i] = tabelaSubstituicao[dados[i]];

        // 4. Calcular quantos bits cada símbolo precisa (sem zeros à esquerda)
        //    símbolo 0 é caso especial → representa 0, precisa de 1 bit
        int BitsNecessarios(byte simbolo)
        {
            if (simbolo == 0) return 1;
            int bits = 0;
            int v = simbolo;
            while (v > 0) { bits++; v >>= 1; }
            return bits;
        }

        // 5. Agrupar símbolos por número de bits e ordenar crescentemente
        //    Cada entrada: (símbolo, posição original)
        var porTamanho = new SortedDictionary<int, List<(byte simbolo, int posOriginal)>>();
        for (int i = 0; i < dadosRecodificados.Length; i++)
        {
            byte s = dadosRecodificados[i];
            int bits = BitsNecessarios(s);
            if (!porTamanho.ContainsKey(bits))
                porTamanho[bits] = new List<(byte, int)>();
            porTamanho[bits].Add((s, i));
        }

        // 6. Montar metadados: lista de (casas, quantidade)
        var faixas = new List<(byte casas, int quantidade)>();
        foreach (var kv in porTamanho)
            faixas.Add(((byte)kv.Key, kv.Value.Count));

        // 7. Montar mapa de reordenação: índice no stream → posição original
        //    Necessário para reconstruir a sequência original
        var ordemOriginal = new List<int>();
        foreach (var kv in porTamanho)
            foreach (var (_, posOriginal) in kv.Value)
                ordemOriginal.Add(posOriginal);

        // 8. Montar stream de bits
        var bitWriter = new BitWriter();
        foreach (var kv in porTamanho)
        {
            int casas = kv.Key;
            foreach (var (simbolo, _) in kv.Value)
                bitWriter.EscreverBits(simbolo, casas);
        }
        byte[] streamBits = bitWriter.ToArray();
        int totalBits = bitWriter.TotalBits;

        // 9. Escrever arquivo de saída
        using var fs = new FileStream(caminhoSaida, FileMode.Create);
        using var bw = new BinaryWriter(fs);

        // Assinatura
        bw.Write(new byte[] { (byte)'J', (byte)'C', (byte)'M', (byte)'P' });

        // Tabela de substituição (256 bytes)
        bw.Write(tabelaInversa); // guardamos a inversa para descompactar direto

        // Faixas
        bw.Write((byte)faixas.Count);
        foreach (var (casas, qtd) in faixas)
        {
            bw.Write(casas);
            bw.Write(qtd); // int = 4 bytes
        }

        // Mapa de reordenação (int por posição)
        bw.Write(ordemOriginal.Count);
        foreach (int pos in ordemOriginal)
            bw.Write(pos);

        // Stream de bits
        bw.Write(totalBits);
        bw.Write(streamBits);

        Console.WriteLine($"Original:    {dados.Length} bytes");
        Console.WriteLine($"Compactado:  {fs.Length} bytes");
        Console.WriteLine($"Razão:       {(double)fs.Length / dados.Length:P1}");
    }

    // -------------------------------------------------------------------------
    // DESCOMPACTAR
    // -------------------------------------------------------------------------
    public static void Descompactar(string caminhoEntrada, string caminhoSaida)
    {
        using var fs = new FileStream(caminhoEntrada, FileMode.Open);
        using var br = new BinaryReader(fs);

        // Assinatura
        byte[] assinatura = br.ReadBytes(4);
        if (assinatura[0] != 'J' || assinatura[1] != 'C' ||
            assinatura[2] != 'M' || assinatura[3] != 'P')
            throw new InvalidDataException("Arquivo não é um .jcmp válido.");

        // Tabela inversa (símbolo → byte original)
        byte[] tabelaInversa = br.ReadBytes(256);

        // Faixas
        int qtdFaixas = br.ReadByte();
        var faixas = new List<(int casas, int quantidade)>();
        for (int i = 0; i < qtdFaixas; i++)
        {
            int casas = br.ReadByte();
            int qtd = br.ReadInt32();
            faixas.Add((casas, qtd));
        }

        // Mapa de reordenação
        int totalElementos = br.ReadInt32();
        int[] ordemOriginal = new int[totalElementos];
        for (int i = 0; i < totalElementos; i++)
            ordemOriginal[i] = br.ReadInt32();

        // Stream de bits
        int totalBits = br.ReadInt32();
        byte[] streamBits = br.ReadBytes((totalBits + 7) / 8);

        // Ler símbolos do stream
        var bitReader = new BitReader(streamBits, totalBits);
        byte[] resultado = new byte[totalElementos];
        int idx = 0;

        foreach (var (casas, quantidade) in faixas)
        {
            for (int i = 0; i < quantidade; i++)
            {
                byte simbolo = bitReader.LerBits(casas);
                int posOriginal = ordemOriginal[idx];
                resultado[posOriginal] = tabelaInversa[simbolo];
                idx++;
            }
        }

        File.WriteAllBytes(caminhoSaida, resultado);
        Console.WriteLine($"Descompactado: {resultado.Length} bytes → {caminhoSaida}");
    }

    // -------------------------------------------------------------------------
    // TABELA DE SUBSTITUIÇÃO
    // -------------------------------------------------------------------------
    private static byte[] CriarTabelaSubstituicao(int[] frequencia)
    {
        // Ordena bytes por frequência decrescente
        // O mais frequente recebe símbolo 0, o segundo recebe 1, etc.
        var ordenados = Enumerable.Range(0, 256)
            .OrderByDescending(i => frequencia[i])
            .ToArray();

        byte[] tabela = new byte[256];
        for (int simbolo = 0; simbolo < 256; simbolo++)
            tabela[ordenados[simbolo]] = (byte)simbolo;

        return tabela;
    }
}

// =============================================================================
// BIT WRITER
// =============================================================================
public class BitWriter
{
    private readonly List<byte> _buffer = new();
    private byte _bytAtual = 0;
    private int _bitPos = 0; // próximo bit a escrever (0 = MSB)
    public int TotalBits { get; private set; }

    public void EscreverBits(byte valor, int casas)
    {
        // Escreve 'casas' bits do valor, do MSB para o LSB
        for (int i = casas - 1; i >= 0; i--)
        {
            int bit = (valor >> i) & 1;
            _bytAtual |= (byte)(bit << (7 - _bitPos));
            _bitPos++;
            TotalBits++;

            if (_bitPos == 8)
            {
                _buffer.Add(_bytAtual);
                _bytAtual = 0;
                _bitPos = 0;
            }
        }
    }

    public byte[] ToArray()
    {
        if (_bitPos > 0)
            _buffer.Add(_bytAtual); // último byte com padding de zeros
        return _buffer.ToArray();
    }
}

// =============================================================================
// BIT READER
// =============================================================================
public class BitReader
{
    private readonly byte[] _buffer;
    private readonly int _totalBits;
    private int _bitLido = 0;

    public BitReader(byte[] buffer, int totalBits)
    {
        _buffer = buffer;
        _totalBits = totalBits;
    }

    public byte LerBits(int casas)
    {
        byte resultado = 0;
        for (int i = casas - 1; i >= 0; i--)
        {
            if (_bitLido >= _totalBits) break;

            int byteIdx = _bitLido / 8;
            int bitIdx = 7 - (_bitLido % 8);
            int bit = (_buffer[byteIdx] >> bitIdx) & 1;
            resultado |= (byte)(bit << i);
            _bitLido++;
        }
        return resultado;
    }
}

// =============================================================================
// PROGRAMA PRINCIPAL
// =============================================================================
class Program
{
    static void Main(string[] args)
    {
        //if (args.Length < 3)
        //{
        //    Console.WriteLine("Uso:");
        //    Console.WriteLine("  Compactar:    Compactador c <entrada> <saida.jcmp>");
        //    Console.WriteLine("  Descompactar: Compactador d <entrada.jcmp> <saida>");
        //    return;
        //}

        string modo = args[0].ToLower();
        string entrada = args[1];
        string saida = args[2];

        switch (modo)
        {
            case "c":
                Compactador.Compactar(entrada, saida);
                break;
            case "d":
                Compactador.Descompactar(entrada, saida);
                break;
            default:
                Console.WriteLine("Modo inválido. Use 'c' para compactar ou 'd' para descompactar.");
                break;
        }
    }
}