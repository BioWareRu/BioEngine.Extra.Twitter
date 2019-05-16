using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Modules;
using BioEngine.Core.Properties;
using BioEngine.Core.Publishers;
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
            services.AddSingleton<TwitterService>();
            services.AddScoped<IContentPublisher<TwitterPublishConfig>, TwitterContentPublisher>();
            services.AddScoped<TwitterContentPublisher>();

            PropertiesProvider.RegisterBioEngineProperties<TwitterSitePropertiesSet, Site>("twittersite");
        }

        public override void RegisterEntities(BioEntitiesManager entitiesManager)
        {
            base.RegisterEntities(entitiesManager);
            entitiesManager.Register<TwitterPublishRecord>();
        }
    }
}
