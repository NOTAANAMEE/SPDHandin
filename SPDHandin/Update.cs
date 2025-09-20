using Octokit;
using FileMode = System.IO.FileMode;

namespace SPDHandin;

public class Update
{
    public static async Task PullFromGithub()
    {
        var client = new GitHubClient(new ProductHeaderValue("HandinScript"));
        var contents = await client.Repository.Content.GetAllContents(
            "NOTAANAMEE", "HandinScript", "script");
        using var httpClient = new HttpClient();
        foreach (var content in contents)
        {
            var response = await httpClient.GetAsync(content.DownloadUrl);
            response.EnsureSuccessStatusCode();
            var filePath = Path.Combine("./script", content.Name);
            await using var fs = new FileStream(filePath, FileMode.Create);
            await using var stream = await response.Content.ReadAsStreamAsync();
            await stream.CopyToAsync(fs);
            Console.WriteLine($"Downloaded {content.Name}");
        }

    }
}