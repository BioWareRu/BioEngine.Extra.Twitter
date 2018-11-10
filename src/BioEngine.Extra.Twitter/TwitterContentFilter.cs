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
    public class TwitterContentFilter : BaseRepositoryFilter
    {
        private readonly PropertiesProvider _propertiesProvider;
        private readonly TwitterService _twitterService;
        private readonly BioContext _bioContext;
        private readonly ILogger<TwitterContentFilter> _logger;

        public TwitterContentFilter(PropertiesProvider propertiesProvider, TwitterService twitterService,
            BioContext bioContext, ILogger<TwitterContentFilter> logger)
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

        public override async Task<bool> AfterSaveAsync<T, TId>(T item, PropertyChange[] changes = null)
        {
            var content = item as Post;
            if (content != null)
            {
                var sites = await _bioContext.Sites.Where(s => content.SiteIds.Contains(s.Id)).ToListAsync();
                foreach (var site in sites)
                {
                    var sitePropertiesSet = await _propertiesProvider.GetAsync<TwitterSitePropertiesSet>(site);
                    if (!sitePropertiesSet.IsEnabled)
                    {
                        _logger.LogInformation($"Facebook is not enabled for site {site.Title}");
                        continue;
                    }

                    var twitterConfig = new TwitterServiceConfiguration()
                    {
                        AccessToken = sitePropertiesSet.AccessToken,
                        AccessTokenSecret = sitePropertiesSet.AccessTokenSecret,
                        ConsumerKey = sitePropertiesSet.ConsumerKey,
                        ConsumerSecret = sitePropertiesSet.ConsumerSecret
                    };

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