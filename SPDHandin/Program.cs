namespace SPDHandin;

class Program
{
    private const string URL = "https://cs110.students.cs.ubc.ca/spd.plt";

    public static void Main(string[] args)
    {
        Update.PullFromGithub().Wait();
    }
}