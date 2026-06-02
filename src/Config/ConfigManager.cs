using UnityExplorer.UI;
using UnityExplorer.Translation;

namespace UnityExplorer.Config
{
    public static class ConfigManager
    {
        internal static readonly Dictionary<TranslationKey, IConfigElement> ConfigElements = new();
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
        internal static readonly Dictionary<UIManager.Panels, ConfigElement<string>> PanelSaveData = new();

        internal static ConfigElement<string> GetPanelSaveData(UIManager.Panels panel)
        {
            if (!PanelSaveData.ContainsKey(panel))
                PanelSaveData.Add(panel, new ConfigElement<string>(panel.ToString(), string.Empty, true));
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

        internal static void RegisterConfigElement<T>(ConfigElement<T> configElement)
        {
            if (!configElement.IsInternal)
            {
                Handler.RegisterConfigElement(configElement);
                ConfigElements.Add(configElement.NameKey, configElement);
            }
            else
            {
                InternalHandler.RegisterConfigElement(configElement);
                string key = configElement.NameKey == TranslationKey.None ? configElement.InternalName : configElement.NameKey.ToString();
                InternalConfigs.Add(key, configElement);
            }
        }

        private static void CreateConfigElements()
        {
            Lang_Toggle = new(TranslationKey.Translation, TranslationManager.Lang.English);

            Master_Toggle = new(TranslationKey.UeToggle, KeyCode.F7);

            Hide_On_Startup = new(TranslationKey.HideOnStartup, false);

            Startup_Delay_Time = new(TranslationKey.StartupDelayTime, 1f);

            Target_Display = new(TranslationKey.TargetDisplay, 0);

            Force_Unlock_Mouse = new(TranslationKey.ForceUnlockMouse, true);
            Force_Unlock_Mouse.OnValueChanged += (bool value) => UniverseLib.Config.ConfigManager.Force_Unlock_Mouse = value;

            Force_Unlock_Toggle = new(TranslationKey.ForceUnlockToggleKey, KeyCode.None);

            Disable_EventSystem_Override = new(TranslationKey.DisableEventsystemOverride, false);
            Disable_EventSystem_Override.OnValueChanged += (bool value) => UniverseLib.Config.ConfigManager.Disable_EventSystem_Override = value;

            Disable_Setup_Force_ReLoad_ManagedAssemblies = new(TranslationKey.DisableForceReloadManagedAssemblies,
                false);

            Bypass_UniverseLib_ICall = new(TranslationKey.BypassUniverseLibICall,
                false);

            Default_Output_Path = new(TranslationKey.DefaultOutputPath,
                Path.Combine(ExplorerCore.ExplorerFolder, "Output"));

            DnSpy_Path = new(TranslationKey.DnspyPath,  @"C:/Program Files/dnspy/dnSpy.exe");

            Main_Navbar_Anchor = new(TranslationKey.MainNavbarAnchor, UIManager.VerticalAnchor.Top);

            Log_Unity_Debug = new(TranslationKey.LogUnityDebug, false);

            Log_To_Disk = new(TranslationKey.LogToDisk, true);

            World_MouseInspect_Keybind = new(TranslationKey.WorldMouseInspectKeybind, KeyCode.None);

            UI_MouseInspect_Keybind = new(TranslationKey.UiMouseInspectKeybind, KeyCode.None);

            CSConsole_Assembly_Blacklist = new(TranslationKey.CsharpConsoleAssemblyBlacklist, "");

            Reflection_Signature_Blacklist = new(TranslationKey.MemberSignatureBlacklist,
                "");

            TIME_SCALE_TOGGLE = new(TranslationKey.TimeScaleToggle,
                KeyCode.None);
            LOCK_TIME_SCALE_TO_ZERO = new(TranslationKey.PauseKeybind,
                KeyCode.None);
            LOCK_TIME_SCALE_TO_NORMAL = new(TranslationKey.PlaybackKeybind,
                KeyCode.None);
            LOCK_TIME_SCALE_TO_HALF = new(TranslationKey.SpeedDownKeybind,
                KeyCode.None);
            LOCK_TIME_SCALE_TO_DOUBLE = new(TranslationKey.SpeedUpKeybind,
                KeyCode.None);
        }
    }
}
