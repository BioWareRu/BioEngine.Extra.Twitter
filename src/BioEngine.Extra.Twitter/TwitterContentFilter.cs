using System;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Providers;
using BioEngine.Core.Repository;
using BioEngine.Extra.Twitter.Service;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Extra.Twitter
{
    public class TwitterContentFilter : BaseRepositoryFilter
    {
        private readonly SettingsProvider _settingsProvider;
        private readonly TwitterService _twitterService;
        private readonly BioContext _bioContext;

        public TwitterContentFilter(SettingsProvider settingsProvider, TwitterService twitterService,
            BioContext bioContext)
        {
            _settingsProvider = settingsProvider;
            _twitterService = twitterService;
            _bioContext = bioContext;
        }

        public override bool CanProcess(Type type)
        {
            return typeof(ContentItem).IsAssignableFrom(type);
        }

        public override async Task<bool> AfterSave<T, TId>(T item)
        {
            var content = item as ContentItem;
            if (content != null)
            {
                var itemSettings = await _settingsProvider.Get<TwitterContentSettings>(content);

                if (itemSettings.TwitterId > 0)
                {
                    var deleted = _twitterService.DeleteTweet(itemSettings.TwitterId);
                    if (!deleted)
                    {
                        throw new Exception("Can't delete news tweet");
                    }

                    itemSettings.TwitterId = 0;
                }

                if (content.IsPublished)
                {
                    var text = $"{content.Title} {content.PublicUrl}";

                    var sections = await _bioContext.Set<Section>().Where(s => content.SectionIds.Contains(s.Id))
                        .ToListAsync();
                    if (sections.Any())
                    {
                        foreach (var section in sections)
                        {
                            if (!string.IsNullOrEmpty(section.Hashtag))
                            {
                                text = $"{text} #{section.Hashtag.Replace("#", "")}";
                            }
                        }
                    }

                    var tweetId = _twitterService.CreateTweet(text);
                    if (tweetId > 0)
                    {
                        itemSettings.TwitterId = tweetId;
                    }
                }

                await _settingsProvider.Set(itemSettings, content);
                return true;
            }

            return false;
        }
    }
}