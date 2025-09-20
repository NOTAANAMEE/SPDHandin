using System.IO.Compression;

namespace SPDHandin;

public class Pull
{
    /// <summary>
    /// The URL to check for updates.
    /// </summary>
    public const string URL = "https://cs110.students.cs.ubc.ca/spd.plt";

    public const string DOWNLOAD_PATH = "./update/plt.bs64";
    
    public const string ZIP_PATH = "./update/update.gz";
    
    public const string UNZIP_PATH = "./update/update.plt";

    public static async Task UpdateFile()
    {
        Directory.CreateDirectory("./update");
        await Download();
        await BS64ToFile();
        await UnGZ();
        await ReadPLT();
        //await ReadPLT("this-collection.rkt");
        //await ReadPLT("info.rkt");
    }
    
    private static async Task Download()
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync(URL);
        await using var fs = new FileStream(DOWNLOAD_PATH, FileMode.Create);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStreamAsync();
        await content.CopyToAsync(fs);
    }
    
    private static async Task BS64ToFile()
    {
        using var reader = new StreamReader(DOWNLOAD_PATH);
        await using var writer = new FileStream(ZIP_PATH, FileMode.Create);
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (line == null) continue;
            var bytes = Convert.FromBase64String(line);
            writer.Write(bytes, 0, bytes.Length);
        }
    }

    private static async Task UnGZ()
    {
        await using var input = File.OpenRead(ZIP_PATH);
        await using var gzip = new GZipStream(input, CompressionMode.Decompress);
        await using var output = File.Create(UNZIP_PATH);
        await gzip.CopyToAsync(output);
    }

    private static async Task ReadPLT(string filename = "client.rkt")
    {
        await using FileStream fs = new (UNZIP_PATH, FileMode.Open);
        using var reader = new StreamReader(fs);
        var finalPath = $"./script/{filename}";
        await using var writer = new StreamWriter(finalPath);
        var line = "";
        while (!line.Contains($"(\"spd-handin\" \"{filename}\")")) 
            line = await reader.ReadLineAsync() ?? "";
        var length = int.Parse(await reader.ReadLineAsync() ?? "0");
        reader.Read();//skip *
        var buffer = ReadBytes(reader, length);
        await writer.WriteAsync(buffer, 0, length);
    }

    private static char[] ReadBytes(StreamReader sr, int length)
    {
        var buffer = new char[length];
        var read = 0;
        while (read < length)
        {
            var bytesRead = sr.ReadBlock(buffer, read, length - read);
            if (bytesRead == 0) break;
            read += bytesRead;
        }
        return buffer;
    }
}
