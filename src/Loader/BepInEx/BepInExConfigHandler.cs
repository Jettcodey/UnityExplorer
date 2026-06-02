#if BIE
using BepInEx.Configuration;
using UnityExplorer.Config;
using UnityExplorer.Translation;

namespace UnityExplorer.Loader.BIE;

public class BepInExConfigHandler : ConfigHandler
{
    private ConfigFile Config => ExplorerBepInPlugin.Instance.Config;

    private const string CTG_NAME = "UnityExplorer";

    public override void Init()
    {
        // Not necessary
    }

    public override void RegisterConfigElement<T>(ConfigElementBase<T> config)
    {
        ConfigEntry<T> entry = Config.Bind(CTG_NAME, config.Key, config.Value, config.DefaultDescription);

        entry.SettingChanged += (object o, EventArgs e) =>
        {
            config.Value = entry.Value;
        };
    }

    public override T GetConfigValue<T>(ConfigElementBase<T> element)
    {
        if (Config.TryGetEntry(CTG_NAME, element.Key, out ConfigEntry<T> configEntry))
        {
            return configEntry.Value;
        }
        else
        {
            throw new Exception($"Could not get config entry '{element.Key}'");
        }
    }

    public override void SetConfigValue<T>(ConfigElementBase<T> element, T value)
    {
        if (Config.TryGetEntry(CTG_NAME, element.Key, out ConfigEntry<T> configEntry))
        {
            configEntry.Value = value;
        }
        else
        {
            ExplorerCore.Log($"Could not get config entry '{element.Key}'");
        }
    }

    public override void LoadConfig()
    {
        foreach (KeyValuePair<string, IConfigElement> entry in ConfigManager.ConfigElements)
        {
            ConfigDefinition def = new(CTG_NAME, entry.Key);
            if (Config.ContainsKey(def) && Config[def] is ConfigEntryBase configEntry)
            {
                IConfigElement config = entry.Value;
                config.BoxedValue = configEntry.BoxedValue;
            }
        }
    }

    public override void SaveConfig()
    {
        Config.Save();
    }
}

#endif
