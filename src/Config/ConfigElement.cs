namespace UnityExplorer.Config;

public abstract class ConfigElementBase<T> : IConfigElement
{
    public abstract string NameLabel { get; }
    public abstract string DescriptionLabel { get; }
    public abstract string DefaultDescription { get; }
    public abstract bool IsInternal { get; }

    public string Key { get; }

    public Type ElementType { get; } = typeof(T);

    public Action<T> OnValueChanged;
    public Action OnValueChangedNotify { get; set; }

    public object DefaultValue { get; }

    public ConfigHandler Handler => IsInternal
        ? ConfigManager.InternalHandler
        : ConfigManager.Handler;

    public T Value
    {
        get => m_value;
        set => SetValue(value);
    }
    private T m_value;

    object IConfigElement.BoxedValue
    {
        get => m_value;
        set => SetValue((T)value);
    }

    public ConfigElementBase(string key, T defaultValue)
    {
        Key = key;
        m_value = defaultValue;
        DefaultValue = defaultValue;
        ConfigManager.RegisterConfigElement(this);
    }

    private void SetValue(T value)
    {
        if ((m_value == null && value == null) || (m_value != null && m_value.Equals(value)))
        {
            return;
        }

        m_value = value;

        Handler.SetConfigValue(this, value);

        OnValueChanged?.Invoke(value);
        OnValueChangedNotify?.Invoke();

        Handler.OnAnyConfigChanged();
    }

    object IConfigElement.GetLoaderConfigValue() => GetLoaderConfigValue();

    public T GetLoaderConfigValue()
    {
        return Handler.GetConfigValue(this);
    }

    public void RevertToDefaultValue()
    {
        Value = (T)DefaultValue;
    }
}

public class ConfigElement<T>(string key, TranslationKey nameKey, TranslationKey descriptionKey, T defaultValue) : ConfigElementBase<T>(key, defaultValue)
{
    public override string NameLabel => TranslationManager.Get(m_nameKey);
    public override string DescriptionLabel => TranslationManager.Get(m_descriptionKey);
    public override string DefaultDescription => TranslationManager.Get(TranslationManager.Lang.English, m_descriptionKey);

    public override bool IsInternal => false;

    private readonly TranslationKey m_nameKey = nameKey;
    private readonly TranslationKey m_descriptionKey = descriptionKey;
}

public class InternalConfigElement<T>(string key, T defaultValue) : ConfigElementBase<T>(key, defaultValue)
{
    public override string NameLabel => string.Empty;
    public override string DescriptionLabel => string.Empty;
    public override string DefaultDescription => string.Empty;

    public override bool IsInternal => true;
}
