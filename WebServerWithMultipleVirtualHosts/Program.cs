using Microsoft.Extensions.Configuration;
using WebServerWithMultipleVirtualHosts;

var configuration = new ConfigurationBuilder()
           .AddJsonFile(@"C:\Users\valki\Desktop\Programing\Project_For_Intern_C#\WebServerWithMultipleVirtualHosts\WebServerWithMultipleVirtualHosts\appSettings.json")
           .Build();

var webServer = new WebServer(configuration);
webServer.Start();

Console.WriteLine("Enter any key to exit");
Console.ReadKey();

