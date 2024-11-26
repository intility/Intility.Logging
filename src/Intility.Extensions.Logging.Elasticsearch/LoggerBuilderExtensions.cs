using Elastic.CommonSchema.Serilog;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using Microsoft.Extensions.Options;
using Serilog.Debugging;
using System;
using System.Linq;

namespace Intility.Extensions.Logging
{
    public static class LoggerBuilderExtensions
    {
        /// <summary>
        /// Registers Elasticsearch sink configuration if defined in the <paramref name="configure"/>
        /// </summary>
        /// <param name="configure">An action to configure the Elasticsearch sink options.</param>
        /// <returns>The updated <see cref="ILoggerBuilder"/> instance.</returns>
        public static ILoggerBuilder UseElasticsearch(this ILoggerBuilder builder, Action<ElasticsearchSinkOptions> configure)
        {
            //To prevent a StackOverflowException, hardcode the config section name instead of risking self-calling within the method.
            builder.UseElasticsearch("Elasticsearch", configure);
            return builder;
        }

        /// <summary>
        /// Uses Elasticsearch sink if Endpoints is defined in the <paramref name="configSection"/>.
        /// <br />
        /// IndexFormat is required in config. Optionally specify Username and Password for BasicAuth
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configSection"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static ILoggerBuilder UseElasticsearch(this ILoggerBuilder builder, string configSection = "Elasticsearch", Action<ElasticsearchSinkOptions> configure = null)
        {
            var elasticConfig = builder.Host.Configuration.GetSection(configSection);
            var elasticEndpoints = elasticConfig["Endpoints"];

            if (string.IsNullOrWhiteSpace(elasticEndpoints))
            {
                return builder;
            }

            var username = elasticConfig["Username"];
            var password = elasticConfig["Password"];
            var llmPolicy = elasticConfig["LLMPolicy"];
            var indexFormat = elasticConfig["IndexFormat"];

            var dataSet = string.IsNullOrWhiteSpace(elasticConfig["DataSet"]) ? "generic" : elasticConfig["DataSet"];
            var namespaceName = string.IsNullOrWhiteSpace(elasticConfig["Namespace"]) ? "default" : elasticConfig["Namespace"];

            if (string.IsNullOrWhiteSpace(indexFormat))
            {
                throw new Exception("Failed to initialize Elasticsearch sink",
                    new ArgumentException($"missing elastic config: {configSection}:IndexFormat"));
            }

            var endpoints = elasticEndpoints.Split(',')
                .Select(endpoint => new Uri(endpoint));

            foreach (var endpoint in endpoints)
            {
                var settings = new TransportConfiguration(endpoint)
                    .Authentication(new BasicAuthentication(username, password));

                var transport = new DistributedTransport(settings);
                var sinkOptions = new ElasticsearchSinkOptions(transport)
                {
                    DataStream = new DataStreamName(indexFormat, dataSet, namespaceName),
                    BootstrapMethod = BootstrapMethod.Failure,
                    ChannelDiagnosticsCallback = channel => {
                        SelfLog.WriteLine(
                            $"Failure={channel.PublishSuccess}, " +
                            $"Message={channel}, " +
                            $"Exception={channel.ObservedException}"
                            ); 
                    },
                    TextFormatting = new EcsTextFormatterConfiguration()
                };

                if(!string.IsNullOrWhiteSpace(llmPolicy))
                {
                    sinkOptions.IlmPolicy = llmPolicy;
                }

                configure?.Invoke(sinkOptions);
                builder.Configuration.WriteTo.Elasticsearch(sinkOptions);
            }

            return builder;
        }
    }
}
