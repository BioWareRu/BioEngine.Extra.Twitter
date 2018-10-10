using BioEngine.Core.Providers;

namespace BioEngine.Extra.Twitter
{
    [SettingsClass(Name = "Публикация в Twitter")]
    public class TwitterContentSettings : SettingsBase
    {
        [SettingsProperty(Name = "ID твита", Type = SettingType.Number)]
        public long TwitterId { get; set; }
    }
}