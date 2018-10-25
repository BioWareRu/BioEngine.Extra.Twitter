using BioEngine.Core.Properties;

namespace BioEngine.Extra.Twitter
{
    [PropertiesSet(Name = "Настройки публикации в Twitter", IsEditable = true)]
    public class TwitterSitePropertiesSet : PropertiesSet
    {
        [PropertiesElement(Name = "Включено?", Type = PropertyElementType.Checkbox)]
        public bool IsEnabled { get; set; }

        [PropertiesElement(Name = "Consumer Key")]
        public string ConsumerKey { get; set; }

        [PropertiesElement(Name = "Consumer Secret", Type = PropertyElementType.PasswordString)]
        public string ConsumerSecret { get; set; }

        [PropertiesElement(Name = "Access token")]
        public string AccessToken { get; set; }

        [PropertiesElement(Name = "Access token secret", Type = PropertyElementType.PasswordString)]
        public string AccessTokenSecret { get; set; }
    }
}