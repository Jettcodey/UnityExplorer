namespace UnityExplorer.Config;

public abstract class ConfigHandler
{
    public abstract void RegisterConfigElement<T>(ConfigElementBase<T> element);

    public abstract void SetConfigValue<T>(ConfigElementBase<T> element, T value);

    public abstract T GetConfigValue<T>(ConfigElementBase<T> element);

    public abstract void Init();

    public abstract void LoadConfig();

    public abstract void SaveConfig();

    public virtual void OnAnyConfigChanged() { }
}
