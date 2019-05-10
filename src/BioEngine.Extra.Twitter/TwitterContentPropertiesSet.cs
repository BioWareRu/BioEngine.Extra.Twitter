using BioEngine.Core.Properties;

namespace BioEngine.Extra.Twitter
{
    [PropertiesSet("Публикация в Twitter", Quantity = PropertiesQuantity.OnePerSite)]
    public class TwitterContentPropertiesSet : PropertiesSet
    {
        [PropertiesElement("ID твита", PropertyElementType.Number)]
        public long TweetId { get; set; }
    }
}
