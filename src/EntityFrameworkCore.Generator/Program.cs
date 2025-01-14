using System;

using McMaster.Extensions.CommandLineUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;

namespace EntityFrameworkCore.Generator;

[Command("efg", Description = "Entity Framework Core model generation tool")]
[Subcommand(typeof(InitializeCommand))]
[Subcommand(typeof(GenerateCommand))]
[VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
public class Program : CommandBase
{
    public Program(ILoggerFactory logger, IConsole console)
        : base(logger, console)
    {
    }

    protected override int OnExecute(CommandLineApplication application)
    {
        application.ShowHelp();
        return 1;
    }


    public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} {Level:u1} {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            var services = new ServiceCollection()
                .AddLogging(logger => logger
                    .AddSerilog()
                    .SetMinimumLevel(LogLevel.Information)
                )
                .AddSingleton(PhysicalConsole.Singleton)
                .AddTransient<IGeneratorOptionsSerializer, GeneratorOptionsSerializer>()
                .AddTransient<ICodeGenerator, CodeGenerator>()
                .BuildServiceProvider();

            var app = new CommandLineApplication<Program>();

            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services);

            return app.Execute(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }


    private static string GetVersion() => ThisAssembly.Info.InformationalVersion;
}
