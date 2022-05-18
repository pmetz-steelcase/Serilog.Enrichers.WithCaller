<article>

## Note:

Performance of this approach is low due to Reflection and Diagnostic.
However, it's helpful if you need the method Parameters and Values

## Example 
(see on GitHub ..\Serilog.Enrichers.WithCaller\Serilog.Enrichers.WithCallerDemo project):


```C#
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
```
```
Hello, World!

[10:38:57 DBG] debug (at Program.<Main>$(System.String[]) C:\Source\Github\pmetz-steelcase\Serilog.Enrichers.WithCaller\Serilog.Enrichers.WithCallerDemo\Program.cs:13)
[10:38:58 INF] information (at Program.<Main>$(System.String[]) C:\Source\Github\pmetz-steelcase\Serilog.Enrichers.WithCaller\Serilog.Enrichers.WithCallerDemo\Program.cs:14)
[10:38:59 WRN] warning (at Program.<Main>$(System.String[]) C:\Source\Github\pmetz-steelcase\Serilog.Enrichers.WithCaller\Serilog.Enrichers.WithCallerDemo\Program.cs:15)
[10:39:00 ERR] error (at Program.<Main>$(System.String[]) C:\Source\Github\pmetz-steelcase\Serilog.Enrichers.WithCaller\Serilog.Enrichers.WithCallerDemo\Program.cs:16)
System.Exception: Exception of type 'System.Exception' was thrown.

...press enter to close

```
</article>
