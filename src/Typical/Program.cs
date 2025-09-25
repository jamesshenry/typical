using ConsoleAppFramework;
using Microsoft.Extensions.DependencyInjection;
using Typical;
using Typical.Services;
using Velopack;

VelopackApp.Build().Run();

var services = new ServiceCollection();

services.RegisterAppServices();

ConsoleApp.ServiceProvider = services.BuildServiceProvider();

var app = ConsoleApp.Create();

app.Add<ApplicationCommands>();

await app.RunAsync(args);
