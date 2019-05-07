using BioEngine.Core.Entities;
using BioEngine.Core.Modules;
using BioEngine.Core.Properties;
using BioEngine.Core.Repository;
using BioEngine.Extra.Twitter.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Extra.Twitter
{
    public class TwitterModule : BioEngineModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            services.Configure<TwitterServiceConfiguration>(config =>
            {
                config.ConsumerKey = configuration["BE_TWITTER_CONSUMER_KEY"];
                config.ConsumerSecret = configuration["BE_TWITTER_CONSUMER_SECRET"];
                config.AccessToken = configuration["BE_TWITTER_ACCESS_TOKEN"];
                config.AccessTokenSecret = configuration["BE_TWITTER_ACCESS_TOKEN_SECRET"];
            });
            services.AddSingleton<TwitterService>();
            services.AddScoped<IRepositoryHook, TwitterContentHook>();

            PropertiesProvider.RegisterBioEngineContentProperties<TwitterContentPropertiesSet>("twittercontent");
            PropertiesProvider.RegisterBioEngineProperties<TwitterSitePropertiesSet, Site>("twittersite");
        }
    }
}
