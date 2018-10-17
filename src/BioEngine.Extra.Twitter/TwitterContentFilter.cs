using System;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Repository;
using BioEngine.Core.Settings;
using BioEngine.Extra.Twitter.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BioEngine.Extra.Twitter
{
    public class TwitterContentFilter : BaseRepositoryFilter
    {
        private readonly SettingsProvider _settingsProvider;
        private readonly TwitterService _twitterService;
        private readonly BioContext _bioContext;
        private readonly ILogger<TwitterContentFilter> _logger;

        public TwitterContentFilter(SettingsProvider settingsProvider, TwitterService twitterService,
            BioContext bioContext, ILogger<TwitterContentFilter> logger)
        {
            _settingsProvider = settingsProvider;
            _twitterService = twitterService;
            _bioContext = bioContext;
            _logger = logger;
        }

        public override bool CanProcess(Type type)
        {
            return typeof(ContentItem).IsAssignableFrom(type);
        }

        public override async Task<bool> AfterSaveAsync<T, TId>(T item, PropertyChange[] changes = null)
        {
            var content = item as ContentItem;
            if (content != null)
            {
                var sites = await _bioContext.Sites.Where(s => content.SiteIds.Contains(s.Id)).ToListAsync();
                foreach (var site in sites)
                {
                    var siteSettings = await _settingsProvider.GetAsync<TwitterSiteSettings>(site);
                    if (!siteSettings.IsEnabled)
                    {
                        _logger.LogInformation($"Facebook is not enabled for site {site.Title}");
                        continue;
                    }

                    var twitterConfig = new TwitterServiceConfiguration()
                    {
                        AccessToken = siteSettings.AccessToken,
                        AccessTokenSecret = siteSettings.AccessTokenSecret,
                        ConsumerKey = siteSettings.ConsumerKey,
                        ConsumerSecret = siteSettings.ConsumerSecret
                    };

                    var itemSettings = await _settingsProvider.GetAsync<TwitterContentSettings>(content);

                    var hasChanges = changes != null && changes.Any(c =>
                                         c.Name == nameof(content.Title) || c.Name == nameof(content.Url));

                    if (itemSettings.TweetId > 0 && (hasChanges || !content.IsPublished))
                    {
                        var deleted = _twitterService.DeleteTweet(itemSettings.TweetId, twitterConfig);
                        if (!deleted)
                        {
                            throw new Exception("Can't delete news tweet");
                        }

                        itemSettings.TweetId = 0;
                    }

                    if (content.IsPublished && (itemSettings.TweetId == 0 || hasChanges))
                    {
                        var text = await ConstructTextAsync(content, site);

                        var tweetId = _twitterService.CreateTweet(text, twitterConfig);
                        if (tweetId > 0)
                        {
                            itemSettings.TweetId = tweetId;
                        }
                    }

                    await _settingsProvider.SetAsync(itemSettings, content);
                }

                return true;
            }

            return false;
        }

        private async Task<string> ConstructTextAsync(ContentItem content, Site site)
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