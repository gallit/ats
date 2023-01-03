using Ats.Bll;
using Ats.Helper;
using Ats.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Diagnostics;

System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var loggerConfig = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration);

var logger = loggerConfig.CreateLogger();

using IHost host = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureServices((_, services) =>
    {
        services.Configure<Config>(configuration.GetSection("Config"));
        services.AddSingleton(logger);
        services.AddHostedService<Worker>();
    })
.Build();

host.Run();