using BioEngine.Core.Settings;

namespace BioEngine.Extra.Twitter
{
    [SettingsClass(Name = "Публикация в Twitter", Mode = SettingsMode.OnePerSite)]
    public class TwitterContentSettings : SettingsBase
    {
        [SettingsProperty(Name = "ID твита", Type = SettingType.Number)]
        public long TweetId { get; set; }
    }
}