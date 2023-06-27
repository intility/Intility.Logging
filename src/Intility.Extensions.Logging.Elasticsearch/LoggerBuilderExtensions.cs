﻿using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Linq;

namespace Intility.Extensions.Logging
{
    public static class LoggerBuilderExtensions
    {
        /// <summary>
        /// Uses Elasticsearch sink if Endpoints is defined in the <paramref name="configSection"/>.
        /// <br />
        /// IndexFormat is required in config. Optionally specify Username and Password for BasicAuth
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configSection"></param>
        /// <returns></returns>
        public static ILoggerBuilder UseElasticsearch(this ILoggerBuilder builder, string configSection = "Elasticsearch")
        {
            var elasticConfig = builder.Host.Configuration.GetSection(configSection);
            var elasticEndpoints = elasticConfig["Endpoints"];

            if (string.IsNullOrWhiteSpace(elasticEndpoints))
            {
                return builder;
            }

            var username = elasticConfig["Username"];
            var password = elasticConfig["Password"];
            var indexFormat = elasticConfig["IndexFormat"];
            var versionString = elasticConfig["Version"];
            bool dataStream = bool.TryParse(elasticConfig["DataStream"], out var result) && result;

            if (string.IsNullOrWhiteSpace(indexFormat))
            {
                throw new Exception("Failed to initialize Elasticsearch sink", 
                    new ArgumentException($"missing elastic config: {configSection}:IndexFormat"));
            }

            var endpoints = elasticEndpoints.Split(',')
                .Select(endpoint => new Uri(endpoint));

            var versionFormat = string.IsNullOrWhiteSpace(versionString)
                ? AutoRegisterTemplateVersion.ESv7
                : Enum.Parse<AutoRegisterTemplateVersion>(versionString);

            var options = new ElasticsearchSinkOptions(endpoints)
            {
                TypeName = "logevent",
                IndexFormat = indexFormat,
                NumberOfShards = 1,
                NumberOfReplicas = 1,
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = versionFormat
            };

            if (dataStream)
            {
                options.BatchAction = ElasticOpType.Create;
            }

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                options.ModifyConnectionSettings = c => c.BasicAuthentication(username, password);
            }

            builder.Configuration.WriteTo.Elasticsearch(options);

            return builder;
        }
    }
}
