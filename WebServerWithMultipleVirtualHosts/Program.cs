using Microsoft.Extensions.Configuration;
using WebServerWithMultipleVirtualHosts;

string directory = Directory.GetCurrentDirectory();
string jsonFile = Path.GetFullPath(Path.Combine(directory, @"..\..\..\appSettings.json"));

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables("Development")
    .AddJsonFile(jsonFile)
    .Build();

var webServer = new WebServer(configuration);
webServer.Start();

Console.WriteLine("Enter any key to exit");
Console.ReadKey();

