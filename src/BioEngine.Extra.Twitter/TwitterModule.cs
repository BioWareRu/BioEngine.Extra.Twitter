using System;
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
    public class TwitterModule : BioEngineModule<TwitterModuleConfig>
    {
        protected override void CheckConfig()
        {
            base.CheckConfig();
            if (string.IsNullOrEmpty(Config.ConsumerKey))
            {
                throw new ArgumentException("Twitter consumer key is not set");
            }
            if (string.IsNullOrEmpty(Config.ConsumerSecret))
            {
                throw new ArgumentException("Twitter consumer secret is not set");
            }
            if (string.IsNullOrEmpty(Config.AccessToken))
            {
                throw new ArgumentException("Twitter access token is not set");
            }
            if (string.IsNullOrEmpty(Config.AccessTokenSecret))
            {
                throw new ArgumentException("Twitter access token secret is not set");
            }
        }

        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            services.AddSingleton(Config);
            services.AddSingleton<TwitterService>();
            services.AddScoped<IRepositoryHook, TwitterContentHook>();

            PropertiesProvider.RegisterBioEngineContentProperties<TwitterContentPropertiesSet>("twittercontent");
            PropertiesProvider.RegisterBioEngineProperties<TwitterSitePropertiesSet, Site>("twittersite");
        }
    }

    public class TwitterModuleConfig
    {
        public string ConsumerKey { get; set; } = "";
        public string ConsumerSecret { get; set; } = "";
        public string AccessToken { get; set; } = "";
        public string AccessTokenSecret { get; set; } = "";
    }
}
