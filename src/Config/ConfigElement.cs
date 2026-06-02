using UnityExplorer.Translation;

namespace UnityExplorer.Config
{
    public class ConfigElement<T> : IConfigElement
    {
        public string Name => NameKey == TranslationKey.None ? InternalName : TranslationManager.Get(NameKey);
        public string Description => DescriptionKey == TranslationKey.None ? "" : TranslationManager.Get(DescriptionKey);
        public string DefaultDescription => DescriptionKey == TranslationKey.None ? "" : TranslationManager.Get(TranslationManager.Lang.English, DescriptionKey);

        public TranslationKey NameKey { get; }
        public TranslationKey DescriptionKey { get; }
        public string InternalName { get; }

        public bool IsInternal { get; }
        public Type ElementType => typeof(T);

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

        public ConfigElement(TranslationKey nameKey, T defaultValue)
        {
            NameKey = nameKey;
            DescriptionKey = GetHintKey(nameKey);
            InternalName = nameKey.ToString();

            m_value = defaultValue;
            DefaultValue = defaultValue;

            IsInternal = false;

            ConfigManager.RegisterConfigElement(this);
        }

        public ConfigElement(TranslationKey nameKey, TranslationKey descriptionKey, T defaultValue)
        {
            NameKey = nameKey;
            DescriptionKey = descriptionKey;
            InternalName = nameKey.ToString();

            m_value = defaultValue;
            DefaultValue = defaultValue;

            IsInternal = false;

            ConfigManager.RegisterConfigElement(this);
        }

        public ConfigElement(string internalName, T defaultValue, bool isInternal = false)
        {
            this.InternalName = internalName;
            NameKey = TranslationKey.None;
            DescriptionKey = TranslationKey.None;

            m_value = defaultValue;
            DefaultValue = defaultValue;

            IsInternal = isInternal;

            ConfigManager.RegisterConfigElement(this);
        }

        private TranslationKey GetHintKey(TranslationKey key)
        {
            if (Enum.TryParse(key.ToString() + "Hint", out TranslationKey hintKey))
                return hintKey;
            return TranslationKey.None;
        }

        private void SetValue(T value)
        {
            if ((m_value == null && value == null) || (m_value != null && m_value.Equals(value)))
                return;

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
}
