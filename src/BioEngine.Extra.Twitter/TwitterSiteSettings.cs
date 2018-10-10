using BioEngine.Core.Providers;

namespace BioEngine.Extra.Twitter
{
    [SettingsClass(Name = "Настройки публикации в Twitter", IsEditable = true)]
    public class TwitterSiteSettings : SettingsBase
    {
        [SettingsProperty(Name = "Включено?", Type = SettingType.Checkbox)]
        public bool IsEnabled { get; set; }

        [SettingsProperty(Name = "Consumer Key")]
        public string ConsumerKey { get; set; }

        [SettingsProperty(Name = "Consumer Secret", Type = SettingType.PasswordString)]
        public string ConsumerSecret { get; set; }

        [SettingsProperty(Name = "Access token")]
        public string AccessToken { get; set; }

        [SettingsProperty(Name = "Access token secret", Type = SettingType.PasswordString)]
        public string AccessTokenSecret { get; set; }
    }
}