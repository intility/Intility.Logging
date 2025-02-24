<h1 align="center">
  <img src="https://avatars.githubusercontent.com/u/35199565" width="124px"/><br/>
  Intility.Logging
</h1>

<p align="center">Logging enhancement for backend projects using <b>aspnet</b> or <b>dotnet generic host</b><br/> by providing common infrastructure and sensible defaults.<br/><br/>Focus on <b>writing</b> and <b>designing</b> business-logic <br />and less time worrying about operational concerns.</p>

<p align="center">
<a href="https://github.com/Intility/Intility.Logging/actions">
    <img alt="compile workflow" src="https://github.com/Intility/Intility.Logging/actions/workflows/build-and-release.yaml/badge.svg" style="max-width:100%;">
</a>

<a href="https://github.com/Intility/Intility.Logging/actions">
    <img alt="publish workflow" src="https://github.com/Intility/Intility.Logging/actions/workflows/build.yaml/badge.svg" style="max-width:100%;">
</a>

<br />

<a href="https://www.nuget.org/packages/Intility.Logging.AspNetCore/">
    <img alt="nuget" src="https://img.shields.io/nuget/v/Intility.Logging.AspNetCore?label=Intility.Logging.AspNetCore" style="max-width:100%;">
</a>

<a href="https://www.nuget.org/packages/Intility.Extensions.Logging/">
    <img alt="nuget" src="https://img.shields.io/nuget/v/Intility.Extensions.Logging?label=Intility.Extensions.Logging" style="max-width:100%;">
</a>

<a href="https://www.nuget.org/packages/Intility.Extensions.Logging.Elasticsearch/">
    <img alt="nuget" src="https://img.shields.io/nuget/v/Intility.Extensions.Logging.Elasticsearch?label=Intility.Extensions.Logging.Elasticsearch" style="max-width:100%;">
</a>

<a href="https://www.nuget.org/packages/Intility.Extensions.Logging.Sentry/">
    <img alt="nuget" src="https://img.shields.io/nuget/v/Intility.Extensions.Logging.Sentry?label=Intility.Extensions.Logging.Sentry" style="max-width:100%;">
</a>
</p>

## ⚡️ Quick start

First of all this package is already included in the [Intility templates](https://github.com/Intility/templates), but can be installed separately. Installation is done with the `dotnet add package` command or via Visual Studio Package Manager.

```shell
# install common infrastructure
dotnet add package Intility.Logging.AspNetCore
```

To instrument the runtime with the new logging capabilities you will need to use an extension method on the `IHostBuilder` interface

```csharp
// Program.cs
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseIntilityLogging((hostContext, logging) =>
        {
            // add default metadata on log events
            logging.UseDefaultEnrichers();
        })
        //...
```

## ⚙️ Configuration

The base package inclues a Console sink with a format supporting structured logging. Use configuration section `Serilog` to configure the loglevel override and other supported settings. See [Serilog]() for more information.

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "System": "Warning",
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information"
      }
    },
    "Properties": {
      "Application": "MyApp"
    }
  }
}
```

## 🛰️ Addition logging destinations

Additional sinks can be installed separately if needed. Simply register the new sinks to the logging builder after package installation is complete.

```shell
# install Elasticsearch sink
dotnet add package Intility.Extensions.Logging.Elasticsearch

# install Sentry sink
dotnet add package Intility.Extensions.Logging.Sentry
```

```csharp
// Program.cs
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseIntilityLogging((hostContext, logging) =>
        {
            logging.UseDefaultEnrichers()
                .UseElasticsearch()
                .UseSentry();
        })
        //...
```

```json
{
  "Elasticsearch": {
    "Endpoints": "localhost:9200",
    "IndexFormat": "my-service-{0:yyyy.MM}"
  },
  "Sentry": {
    "Dsn": "https://examplePublicKey@o0.ingest.sentry.io/0",
    "MaxRequestBodySize": "Always",
    "SendDefaultPii": true,
    "MinimumBreadcrumbLevel": "Debug",
    "MinimumEventLevel": "Warning",
    "AttachStackTrace": true,
    "Debug": true,
    "DiagnosticsLevel": "Error"
  }
}
```
