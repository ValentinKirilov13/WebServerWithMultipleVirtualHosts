using Microsoft.Extensions.Configuration;
using WebServerWithMultipleVirtualHosts;

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables("Development")
    .AddJsonFile("appSettings.json")
    .Build();

var webServer = new WebServer(configuration);
webServer.Start();

Console.WriteLine("Enter any key to exit");
Console.ReadKey();

