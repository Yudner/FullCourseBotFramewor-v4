// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EmptyBot v4.6.2

using Clinic.Bot.Data;
using Clinic.Bot.Dialogs;
using Clinic.Bot.Dialogs.Qualification;
using Clinic.Bot.Infrastructure.Luis;
using Clinic.Bot.Infrastructure.QnAMakerAI;
using Clinic.Bot.Infrastructure.SendGridEmail;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Clinic.Bot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var storage = new AzureBlobStorage(
               Configuration.GetSection("StorageConnectionString").Value,
               Configuration.GetSection("StorageContainer").Value
            );

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddDbContext<DataBaseService>(options => {
                options.UseCosmos(
                  Configuration["CosmosEndPoint"],
                  Configuration["CosmosKey"],
                  Configuration["CosmosDataBase"]
                );
            });
            services.AddScoped<IDataBaseService, DataBaseService>();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            
            var userState = new UserState(storage);
            services.AddSingleton(userState);

            var conversationState = new ConversationState(storage);
            services.AddSingleton(conversationState);

            services.AddSingleton<ISendGridEmailService, SendGridEmailService>();
            services.AddSingleton<ILuisService, LuisService>();
            services.AddSingleton<IQnAMakerAIService, QnAMakerAIService>();
            services.AddTransient<RootDialog>();
            services.AddTransient<QualificationDialog>();
            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, ClinicBot<RootDialog>>();
            services.AddApplicationInsightsTelemetry();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        //[System.Obsolete]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            //Agregar
            app.UseRouting();

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();

            //Agregar
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
