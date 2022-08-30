// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.LocalMultiplayerAgent
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;

    using Extensions.Configuration;
    using Extensions.DependencyInjection;
    using Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc(options => options.EnableEndpointRouting = false)
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                });

            services.AddLogging(builder => builder
                .AddConsole()
                .AddDebug());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseMvc();
        }
    }
}
