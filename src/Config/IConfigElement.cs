namespace UnityExplorer.Config;

public interface IConfigElement
{
    string Key { get; }
    string NameLabel { get; }
    string DescriptionLabel { get; }
    string DefaultDescription { get; }

    bool IsInternal { get; }
    Type ElementType { get; }

    object BoxedValue { get; set; }
    object DefaultValue { get; }

    object GetLoaderConfigValue();

    void RevertToDefaultValue();

    Action OnValueChangedNotify { get; set; }
}