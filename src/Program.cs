using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using nai;
using nai.commands;
using nai.commands.images;
using nai.db;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using nai.i18n;


var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole();

builder.Configuration
    .SetBasePath(new DirectoryInfo("./").FullName)
    .AddYamlFile("config.yml")
    .AddEnvironmentVariables();

var services = builder.Services;


services.UseTelegram(x => x
    .Use<PortraitImageGenCommand>()
    .Use<LandImageGenCommand>()
    .Use<BalanceCommand>()
    .Use<GrantBalanceCommand>()
    .Use<EnhanceCommand>()
    .Use<VariationsCommand>()
    .Use<Img2ImgP>()
    .Use<PayCommand>()
    .Use<AuthCommand>()
    .Use<InvoiceCommand>()
    .Use<StartCommand>()
    .Use<WallPaperGenCommand>()
    .Use<EngineCommand>()
    .Use<GetConfigCommand>()
    );
services.UseQueue();
services.AddSingleton<Config>();
services.AddSingleton<Db>();
services.AddSingleton<Localization>();


await builder.Build().RunAsync();