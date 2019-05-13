using System;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Properties;
using BioEngine.Core.Repository;
using BioEngine.Extra.Twitter.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BioEngine.Extra.Twitter
{
    public class TwitterContentHook : BaseRepositoryHook
    {
        private readonly PropertiesProvider _propertiesProvider;
        private readonly TwitterService _twitterService;
        private readonly BioContext _bioContext;
        private readonly ILogger<TwitterContentHook> _logger;

        public TwitterContentHook(PropertiesProvider propertiesProvider, TwitterService twitterService,
            BioContext bioContext, ILogger<TwitterContentHook> logger)
        {
            _propertiesProvider = propertiesProvider;
            _twitterService = twitterService;
            _bioContext = bioContext;
            _logger = logger;
        }

        public override bool CanProcess(Type type)
        {
            return typeof(Post).IsAssignableFrom(type);
        }

        public override async Task<bool> AfterSaveAsync<T>(T item, PropertyChange[]? changes = null,
            IBioRepositoryOperationContext? operationContext = null)
        {
            if (item is Post content)
            {
                var sites = await _bioContext.Sites.Where(s => content.SiteIds.Contains(s.Id)).ToListAsync();
                foreach (var site in sites)
                {
                    var sitePropertiesSet = await _propertiesProvider.GetAsync<TwitterSitePropertiesSet>(site);
                    if (!sitePropertiesSet.IsEnabled)
                    {
                        _logger.LogInformation("Twitter is not enabled for site {siteTitle}", site.Title);
                        continue;
                    }

                    if (string.IsNullOrEmpty(sitePropertiesSet.AccessToken))
                    {
                        _logger.LogError("Twitter access token is not configured for site {siteTitle}", site.Title);
                        continue;
                    }

                    if (string.IsNullOrEmpty(sitePropertiesSet.AccessTokenSecret))
                    {
                        _logger.LogError("Twitter access token secret is not configured for site {siteTitle}",
                            site.Title);
                        continue;
                    }

                    if (string.IsNullOrEmpty(sitePropertiesSet.ConsumerKey))
                    {
                        _logger.LogError("Twitter consumer key is not configured for site {siteTitle}", site.Title);
                        continue;
                    }

                    if (string.IsNullOrEmpty(sitePropertiesSet.ConsumerSecret))
                    {
                        _logger.LogError("Twitter consumer secret is not configured for site {siteTitle}", site.Title);
                        continue;
                    }

                    var twitterConfig = new TwitterModuleConfig(sitePropertiesSet.ConsumerKey,
                        sitePropertiesSet.ConsumerSecret, sitePropertiesSet.AccessToken,
                        sitePropertiesSet.AccessTokenSecret);

                    var properties = await _propertiesProvider.GetAsync<TwitterContentPropertiesSet>(content);

                    var hasChanges = changes != null && changes.Any(c =>
                                         c.Name == nameof(content.Title) || c.Name == nameof(content.Url));

                    if (properties.TweetId > 0 && (hasChanges || !content.IsPublished))
                    {
                        var deleted = _twitterService.DeleteTweet(properties.TweetId, twitterConfig);
                        if (!deleted)
                        {
                            throw new Exception("Can't delete news tweet");
                        }

                        properties.TweetId = 0;
                    }

                    if (content.IsPublished && (properties.TweetId == 0 || hasChanges))
                    {
                        var text = await ConstructTextAsync(content, site);

                        var tweetId = _twitterService.CreateTweet(text, twitterConfig);
                        if (tweetId > 0)
                        {
                            properties.TweetId = tweetId;
                        }
                    }

                    await _propertiesProvider.SetAsync(properties, content);
                }

                return true;
            }

            return false;
        }

        private async Task<string> ConstructTextAsync(Post content, Site site)
        {
            var url = $"{site.Url}{content.PublicUrl}";
            var text = $"{content.Title} {url}";

            var sections = await _bioContext.Set<Section>().Where(s => content.SectionIds.Contains(s.Id))
                .ToListAsync();
            var tags = string.Join(" ",
                sections
                    .Where(s => !string.IsNullOrEmpty(s.Hashtag))
                    .Select(s => $"#{s.Hashtag.Replace("#", "")}")
            );
            if (!string.IsNullOrEmpty(tags))
            {
                text = $"{text} {tags}";
            }

            return text;
        }
    }
}
