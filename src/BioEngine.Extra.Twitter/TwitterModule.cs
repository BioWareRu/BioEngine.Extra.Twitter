using BioEngine.Core.Entities;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Modules;
using BioEngine.Core.Providers;
using BioEngine.Extra.Twitter.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BioEngine.Extra.Twitter
{
    public class TwitterModule : BioEngineModule
    {
        public override void ConfigureServices(WebHostBuilderContext builderContext, IServiceCollection services)
        {
            services.Configure<TwitterServiceConfiguration>(configuration =>
            {
                configuration.ConsumerKey = builderContext.Configuration["BE_TWITTER_CONSUMER_KEY"];
                configuration.ConsumerSecret = builderContext.Configuration["BE_TWITTER_CONSUMER_SECRET"];
                configuration.AccessToken = builderContext.Configuration["BE_TWITTER_ACCESS_TOKEN"];
                configuration.AccessTokenSecret = builderContext.Configuration["BE_TWITTER_ACCESS_TOKEN_SECRET"];
            });
            services.AddSingleton<TwitterService>();
            services.AddScoped<IRepositoryFilter, TwitterContentFilter>();

            SettingsProvider.RegisterBioEngineContentSettings<TwitterContentSettings>();
            SettingsProvider.RegisterBioEngineSettings<TwitterSiteSettings, Site>();
        }
    }
}