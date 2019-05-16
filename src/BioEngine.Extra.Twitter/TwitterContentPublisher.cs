using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Publishers;
using BioEngine.Extra.Twitter.Service;
using Microsoft.Extensions.Logging;

namespace BioEngine.Extra.Twitter
{
    public class TwitterContentPublisher : BaseContentPublisher<TwitterPublishConfig, TwitterPublishRecord>
    {
        private readonly TwitterService _twitterService;

        public TwitterContentPublisher(TwitterService twitterService, BioContext dbContext,
            ILogger<IContentPublisher<TwitterPublishConfig>> logger) :
            base(dbContext, logger)
        {
            _twitterService = twitterService;
        }

        protected override Task<TwitterPublishRecord> DoPublishAsync(IContentEntity entity, Site site,
            TwitterPublishConfig config)
        {
            var text = ConstructText(entity, site, config.Tags);

            var tweetId = _twitterService.CreateTweet(text, config.Config);
            if (tweetId == 0)
            {
                throw new Exception($"Can't create tweet for item {entity.Title} ({entity.Id.ToString()})");
            }

            var record = new TwitterPublishRecord
            {
                ContentId = entity.Id, Type = entity.GetType().FullName, TweetId = tweetId, SiteId = site.Id
            };

            return Task.FromResult(record);
        }

        private string ConstructText(IContentEntity content, Site site, List<string> configTags)
        {
            var url = $"{site.Url}{content.PublicUrl}";
            var text = $"{content.Title} {url}";

            var tags = string.Join(" ", configTags
                .Select(s => $"#{s.Replace("#", "")}")
            );
            if (!string.IsNullOrEmpty(tags))
            {
                text = $"{text} {tags}";
            }

            return text;
        }

        protected override Task<bool> DoDeleteAsync(TwitterPublishRecord record, TwitterPublishConfig config)
        {
            var deleted = _twitterService.DeleteTweet(record.TweetId, config.Config);
            if (!deleted)
            {
                throw new Exception("Can't delete news tweet");
            }

            return Task.FromResult(true);
        }
    }
}
