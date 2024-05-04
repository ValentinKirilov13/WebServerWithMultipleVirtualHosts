using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Net;
using System.Text;

namespace WebServerWithMultipleVirtualHosts
{
    public class WebServer
    {
        private readonly IConfiguration _configuration;
        private readonly ICollection<VirtualHost> _virtualHosts;

        public WebServer(IConfiguration configuration)
        {
            _configuration = configuration;
            _virtualHosts = GetProvidedHosts();
        }

        public void Start()
        {
            foreach (var hostConfig in _virtualHosts)
            {
                var host = new WebHostBuilder()
                    .UseKestrel()
                    .Configure(app => app.Run(HandleRequest))
                    .UseUrls($"http://localhost:{hostConfig.Port}")
                    .UseEnvironment("Development")
                    .Build();

                host.RunAsync();
            }
        }

        private async Task HandleRequest(HttpContext context)
        {
            VirtualHost? requestedHost = _virtualHosts
                .FirstOrDefault(x => x.Port == context.Request.Host.Port.ToString());

            if (requestedHost == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            requestedHost.Loger.Information(GetRequestInfo(context));

            string requestPath = context.Request.Path.Value ?? string.Empty;

            if (requestPath == "/")
            {
                var files = Directory.GetFiles(requestedHost.RootDir);

                await context.Response.WriteAsync($"<h1>Files from {requestedHost.Name}:</h1>");

                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);

                    await context.Response.WriteAsync($"<a href=/download/{fileName}>{fileName}</a><br>");
                }
            }
            else if (requestPath.StartsWith("/download/"))
            {
                var fileName = requestPath.Replace("/download/", string.Empty);
                var filePath = Path.Combine(requestedHost.RootDir, fileName);

                if (File.Exists(filePath))
                {
                    context.Response.ContentType = "application/octet-stream";
                    context.Response.Headers.Append("Content-Disposition", $"attachment; filename={fileName}");

                    await context.Response.SendFileAsync(filePath);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }

        private List<VirtualHost> GetProvidedHosts()
        {
            var hosts = new List<VirtualHost>();

            var virtualHosts = _configuration
                .GetSection("VirtualHosts")
                .GetChildren()
                .AsEnumerable();

            foreach (var host in virtualHosts)
            {
                string rootDirectoryPath = host.GetSection("RootDir").Value ?? string.Empty;
                string logFilePath = host.GetSection("LogFile").Value ?? string.Empty;

                Log.Logger = new LoggerConfiguration()
                 .WriteTo.File(logFilePath)
                 .CreateLogger();

                Directory.CreateDirectory(rootDirectoryPath);

                hosts.Add(new VirtualHost()
                {
                    Name = host.GetSection("Name").Value ?? string.Empty,
                    Port = host.GetSection("Port").Value ?? string.Empty,
                    RootDir = rootDirectoryPath,
                    LogFile = logFilePath,
                    Loger = Log.Logger
                });
            }

            return hosts;
        }

        private string GetRequestInfo(HttpContext context)
        {
            var request = context.Request;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"Request Information:");
            sb.AppendLine($"Protocol: {request.Protocol}");
            sb.AppendLine($"Method: {request.Method}");
            sb.AppendLine($"Scheme: {request.Scheme}");
            sb.AppendLine($"Host: {request.Host}");
            sb.AppendLine($"Path: {request.Path}");
            sb.AppendLine($"QueryString: {request.QueryString}");

            sb.AppendLine($"Headers:");
            foreach (var (key, value) in request.Headers)
            {
                sb.AppendLine($"  {key}: {value}");
            }

            sb.AppendLine($"Remote IP Address: {context.Connection.RemoteIpAddress}");
            sb.AppendLine();

            return sb.ToString();
        }
    }
}
