
using CliWrap;
using FileImportedServices;


const string ServiceName = "iWebSync Service";

if (args is { Length: 1 })
{
    string executablePath =
        Path.Combine(AppContext.BaseDirectory, "IWebSync.exe");

    if (args[0] is "/Install")
    {
        try
        {

            await Cli.Wrap("sc")
                .WithArguments(new[] { "create", ServiceName, $"binPath={executablePath}", "start=auto" })
                .ExecuteAsync();
            Console.WriteLine("Service installation successful.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Service installation failed: {ex.Message}");
        }
    }
    else if (args[0] is "/Uninstall")
    {
        await Cli.Wrap("sc")
            .WithArguments(new[] { "stop", ServiceName })
            .ExecuteAsync();

        await Cli.Wrap("sc")
            .WithArguments(new[] { "delete", ServiceName })
            .ExecuteAsync();
    }

    return;
}

IHost host = Host.CreateDefaultBuilder(args).UseWindowsService()
    .ConfigureServices(services =>
    {

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
