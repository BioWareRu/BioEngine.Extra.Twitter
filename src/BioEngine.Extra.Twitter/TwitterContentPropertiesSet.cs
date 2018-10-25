using BioEngine.Core.Properties;

namespace BioEngine.Extra.Twitter
{
    [PropertiesSet(Name = "Публикация в Twitter", Quantity = PropertiesQuantity.OnePerSite)]
    public class TwitterContentPropertiesSet : PropertiesSet
    {
        [PropertiesElement(Name = "ID твита", Type = PropertyElementType.Number)]
        public long TweetId { get; set; }
    }
}