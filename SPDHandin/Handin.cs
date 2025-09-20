using System.Diagnostics;

namespace SPDHandin;

public class Handin
{
    public const string SERVER_PORT = "cs110.students.cs.ubc.ca:7979";

    public static void HandinTest()
    {
        HandinFile("/Users/ruoyu/DrRacketProject/Setup-Test/test-file.rkt",
            "rxu53", "welcome20070226F");
    }

    private static string[] ReadCWL(FileStream fs)
    {
        fs.Position = 0;
        var reader = new StreamReader(fs);
        var cwlGetter = "(@cwl";
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (line == null) continue;
            if (!line.Contains(cwlGetter)) continue;
            line = line.Trim();
            var cwlLine = line[cwlGetter.Length..^1].Trim();
            return [..cwlLine.Split(' ').Where(a => a != string.Empty)];
        }
        return [];
    }

    private static string ReadAS(FileStream fs)
    {
        fs.Position = 0;
        var reader = new StreamReader(fs);
        var asGetter = "(@assignment";
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (line == null) continue;
            if (!line.Contains(asGetter)) continue;
            line = line.Trim();
            var asLine = line[asGetter.Length..^1].Trim();
            return asLine;
        }

        return "";
    }

    public static void HandinFile(string fileName, string id, string pwd)
    {
        FileStream fs = new (fileName, FileMode.Open);
        var cwls = ReadCWL(fs);
        cwls = cwls.Select(a => $"\"{a}\"").ToArray();
        var script = File.ReadAllText("./script/handin-template.rkt");
        var runScript = script.Replace("${cwl}", string.Join(' ', cwls));
        runScript = runScript.Replace("${id}", id);
        runScript = runScript.Replace("${pwd}", pwd);
        runScript = runScript.Replace("${file}", fileName);
        runScript = runScript.Replace("${as}", ReadAS(fs));
        File.WriteAllText("./script/temp.rkt", runScript);
        fs.Close();
        ProcessStartInfo starInfo = new ProcessStartInfo()
        {
            FileName = FindRacket(),
            Arguments = "./script/temp.rkt",
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        
        Process.Start(starInfo)?.WaitForExit();
        
        //File.Delete("./temp.rkt");
    }

    private static string FindRacket()
    {
        #if WINDOWS
        return "racket";
        #else
        var paths = "/Applications";//Linux & macOS
        #endif
        var dirs = Directory.GetDirectories(paths);
        foreach (var dir in dirs)
        {
            if (dir.Contains("Racket "))
                return dir + "/bin/racket";
        }

        return "";
    }
}