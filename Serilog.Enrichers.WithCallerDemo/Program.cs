// See https://aka.ms/new-console-template for more information

using Serilog;
using Serilog.Enrichers.WithCaller;

Log.Logger = new LoggerConfiguration()
    .Enrich.WithCaller(true)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message} (at {Caller}){NewLine}{Exception}")
    .MinimumLevel.Debug()
    .CreateLogger();

Console.WriteLine("Hello, World!\n");
Log.Debug("debug");
Log.Information("information");
Log.Warning("warning");
Log.Error(new Exception(), "error");

Console.WriteLine("\n...press enter to close");
Console.ReadLine();