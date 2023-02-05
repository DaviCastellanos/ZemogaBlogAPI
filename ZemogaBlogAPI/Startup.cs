using ZemogaBlogAPI.Infrastructure.AppSettings;
using ZemogaBlogAPI.Core.Interfaces;
using ZemogaBlogAPI.Infrastructure.CosmosDbData.Repository;
using ZemogaBlogAPI.Infrastructure.Extensions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Collections.Generic;
using System;

[assembly: FunctionsStartup(typeof(ZemogaBlogAPI.Startup))]

namespace ZemogaBlogAPI
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigureServices(builder.Services);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton<IConfiguration>(configuration);

            services.AddLogging();

            ContainerInfo posts= new ContainerInfo() { Name = "posts", PartitionKey = "id" };
            ContainerInfo comments = new ContainerInfo() { Name = "comments", PartitionKey = "id" };


            List<ContainerInfo> containers = new List<ContainerInfo>() { posts, comments };

            services.AddCosmosDb("https://zemoga-db.documents.azure.com:443/",
                                 Environment.GetEnvironmentVariable("PrimaryKey"),
                                 "zemoga",
                                containers);

            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
        }
    }
}
