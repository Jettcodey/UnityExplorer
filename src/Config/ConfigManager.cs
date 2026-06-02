using UnityExplorer.UI;

namespace UnityExplorer.Config;

public static class ConfigManager
{
    internal static readonly Dictionary<string, IConfigElement> ConfigElements = new();
    internal static readonly Dictionary<string, IConfigElement> InternalConfigs = new();

    // Each Mod Loader has its own ConfigHandler.
    // See the UnityExplorer.Loader namespace for the implementations.
    public static ConfigHandler Handler { get; private set; }

    // Actual UE Settings
    public static ConfigElement<TranslationManager.Lang> Lang_Toggle;
    public static ConfigElement<KeyCode> Master_Toggle;
    public static ConfigElement<bool> Hide_On_Startup;
    public static ConfigElement<float> Startup_Delay_Time;
    public static ConfigElement<bool> Disable_EventSystem_Override;
    public static ConfigElement<bool> Disable_Setup_Force_ReLoad_ManagedAssemblies;
    public static ConfigElement<bool> Bypass_UniverseLib_ICall;
    public static ConfigElement<int> Target_Display;
    public static ConfigElement<bool> Force_Unlock_Mouse;
    public static ConfigElement<KeyCode> Force_Unlock_Toggle;
    public static ConfigElement<string> Default_Output_Path;
    public static ConfigElement<string> DnSpy_Path;
    public static ConfigElement<bool> Log_Unity_Debug;
    public static ConfigElement<bool> Log_To_Disk;
    public static ConfigElement<UIManager.VerticalAnchor> Main_Navbar_Anchor;
    public static ConfigElement<KeyCode> World_MouseInspect_Keybind;
    public static ConfigElement<KeyCode> UI_MouseInspect_Keybind;
    public static ConfigElement<string> CSConsole_Assembly_Blacklist;
    public static ConfigElement<string> Reflection_Signature_Blacklist;

    public static ConfigElement<KeyCode> TIME_SCALE_TOGGLE;
    public static ConfigElement<KeyCode> LOCK_TIME_SCALE_TO_ZERO;
    public static ConfigElement<KeyCode> LOCK_TIME_SCALE_TO_NORMAL;
    public static ConfigElement<KeyCode> LOCK_TIME_SCALE_TO_HALF;
    public static ConfigElement<KeyCode> LOCK_TIME_SCALE_TO_DOUBLE;

    // internal configs
    internal static InternalConfigHandler InternalHandler { get; private set; }
    internal static readonly Dictionary<UIManager.Panels, InternalConfigElement<string>> PanelSaveData = new();

    internal static InternalConfigElement<string> GetPanelSaveData(UIManager.Panels panel)
    {
        if (!PanelSaveData.ContainsKey(panel))
            PanelSaveData.Add(panel, new InternalConfigElement<string>(panel.ToString(), string.Empty));
        return PanelSaveData[panel];
    }

    public static void Init(ConfigHandler configHandler)
    {
        Handler = configHandler;
        Handler.Init();

        InternalHandler = new InternalConfigHandler();
        InternalHandler.Init();

        CreateConfigElements();

        Handler.LoadConfig();
        InternalHandler.LoadConfig();

#if STANDALONE
        if (Loader.Standalone.ExplorerEditorBehaviour.Instance)
            Loader.Standalone.ExplorerEditorBehaviour.Instance.LoadConfigs();
#endif
    }

    internal static void RegisterConfigElement<T>(ConfigElementBase<T> configElement)
    {
        if (!configElement.IsInternal)
        {
            Handler.RegisterConfigElement(configElement);
            ConfigElements.Add(configElement.Key, configElement);
        }
        else
        {
            InternalHandler.RegisterConfigElement(configElement);
            InternalConfigs.Add(configElement.Key, configElement);
        }
    }

    private static void CreateConfigElements()
    {
        Lang_Toggle = new("Language", TranslationKey.Translation, TranslationKey.TranslationHint, TranslationManager.Lang.English);

        Master_Toggle = new("UnityExplorer Toggle", TranslationKey.UeToggle, TranslationKey.UeToggleHint, KeyCode.F7);

        Hide_On_Startup = new("Hide On Startup", TranslationKey.HideOnStartup, TranslationKey.HideOnStartupHint, false);

        Startup_Delay_Time = new("Startup Delay Time", TranslationKey.StartupDelayTime, TranslationKey.StartupDelayTimeHint, 1f);

        Target_Display = new("Target Display", TranslationKey.TargetDisplay, TranslationKey.TargetDisplayHint, 0);

        Force_Unlock_Mouse = new("Force Unlock Mouse", TranslationKey.ForceUnlockMouse, TranslationKey.ForceUnlockMouseHint, true);
        Force_Unlock_Mouse.OnValueChanged += (bool value) => UniverseLib.Config.ConfigManager.Force_Unlock_Mouse = value;

        Force_Unlock_Toggle = new("Force Unlock Toggle Key", TranslationKey.ForceUnlockToggleKey, TranslationKey.ForceUnlockToggleKeyHint, KeyCode.None);

        Disable_EventSystem_Override = new("Disable EventSystem override", TranslationKey.DisableEventsystemOverride, TranslationKey.DisableEventsystemOverrideHint, false);
        Disable_EventSystem_Override.OnValueChanged += (bool value) => UniverseLib.Config.ConfigManager.Disable_EventSystem_Override = value;

        Disable_Setup_Force_ReLoad_ManagedAssemblies = new(
            "Disable Force reload ManagedAssemblies",
            TranslationKey.DisableForceReloadManagedAssemblies,
            TranslationKey.DisableForceReloadManagedAssembliesHint,
            false);

        Bypass_UniverseLib_ICall = new("Bypass UniverseLib ICall", TranslationKey.BypassUniverseLibICall, TranslationKey.BypassUniverseLibICallHint, false);

        Default_Output_Path = new("Default Output Path", TranslationKey.DefaultOutputPath, TranslationKey.DefaultOutputPathHint,
            Path.Combine(ExplorerCore.ExplorerFolder, "Output"));

        DnSpy_Path = new("dnSpy Path", TranslationKey.DnspyPath, TranslationKey.DnspyPathHint, @"C:/Program Files/dnspy/dnSpy.exe");

        Main_Navbar_Anchor = new("Main Navbar Anchor", TranslationKey.MainNavbarAnchor, TranslationKey.MainNavbarAnchorHint, UIManager.VerticalAnchor.Top);

        Log_Unity_Debug = new("Log Unity Debug", TranslationKey.LogUnityDebug, TranslationKey.LogUnityDebugHint, false);

        Log_To_Disk = new("Log To Disk", TranslationKey.LogToDisk, TranslationKey.LogToDiskHint, true);

        World_MouseInspect_Keybind = new("World Mouse-Inspect Keybind", TranslationKey.WorldMouseInspectKeybind, TranslationKey.WorldMouseInspectKeybindHint, KeyCode.None);

        UI_MouseInspect_Keybind = new("UI Mouse-Inspect Keybind", TranslationKey.UiMouseInspectKeybind, TranslationKey.UiMouseInspectKeybindHint, KeyCode.None);

        CSConsole_Assembly_Blacklist = new("CSharp Console Assembly Blacklist", TranslationKey.CsharpConsoleAssemblyBlacklist, TranslationKey.CsharpConsoleAssemblyBlacklistHint, "");

        Reflection_Signature_Blacklist = new("Member Signature Blacklist", TranslationKey.MemberSignatureBlacklist, TranslationKey.MemberSignatureBlacklistHint,"");

        TIME_SCALE_TOGGLE = new("TimeScale Toggle", TranslationKey.TimeScaleToggle, TranslationKey.TimeScaleToggleHint, KeyCode.None);
        LOCK_TIME_SCALE_TO_ZERO = new("Pause Keybind", TranslationKey.PauseKeybind, TranslationKey.PauseKeybindHint, KeyCode.None);
        LOCK_TIME_SCALE_TO_NORMAL = new("Playback Keybind", TranslationKey.PlaybackKeybind, TranslationKey.PlaybackKeybindHint, KeyCode.None);
        LOCK_TIME_SCALE_TO_HALF = new("Speed-Down Keybind", TranslationKey.SpeedDownKeybind, TranslationKey.SpeedDownKeybindHint, KeyCode.None);
        LOCK_TIME_SCALE_TO_DOUBLE = new("Speed-Up Keybind", TranslationKey.SpeedUpKeybind, TranslationKey.SpeedUpKeybindHint, KeyCode.None);
    }
}
