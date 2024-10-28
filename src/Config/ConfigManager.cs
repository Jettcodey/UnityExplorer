using UnityExplorer.UI;

namespace UnityExplorer.Config
{
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
                InternalConfigs.Add(configElement.NameKey, configElement);
            }
        }

        private static void CreateConfigElements()
        {
            Lang_Toggle = new("translation", TranslationManager.Lang.English);

            Master_Toggle = new("ue_toggle", KeyCode.F7);

            Hide_On_Startup = new("hide_on_startup", false);

            Startup_Delay_Time = new("startup_delay_time", 1f);

            Target_Display = new("target_display", 0);

            Force_Unlock_Mouse = new("force_unlock_mouse", true);
            Force_Unlock_Mouse.OnValueChanged += (bool value) => UniverseLib.Config.ConfigManager.Force_Unlock_Mouse = value;

            Force_Unlock_Toggle = new("force_unlock_toggle_key", KeyCode.None);

            Disable_EventSystem_Override = new("disable_eventsystem_override", false);
            Disable_EventSystem_Override.OnValueChanged += (bool value) => UniverseLib.Config.ConfigManager.Disable_EventSystem_Override = value;

            Disable_Setup_Force_ReLoad_ManagedAssemblies = new("Disable Force reload ManagedAssemblies",
                "If enabled, UnityExplorer will not reload ManagedAssemblies on setup(Currently only Mono is supported).\n<b>May require restart to take effect.</b>",
                false);

            Bypass_UniverseLib_ICall = new("Bypass UniverseLib ICall",
                "If enabled, UnityExplorer will bypass UniverseLib's ICall Reflection system. This may help with compatibility in some games.\n<b>May require restart to take effect.</b>",
                false);

            Default_Output_Path = new("default_output_path",
                Path.Combine(ExplorerCore.ExplorerFolder, "Output"));

            DnSpy_Path = new("dnspy_path",  @"C:/Program Files/dnspy/dnSpy.exe");

            Main_Navbar_Anchor = new("main_navbar_anchor", UIManager.VerticalAnchor.Top);

            Log_Unity_Debug = new("log_unity_debug", false);

            Log_To_Disk = new("log_to_disk", true);

            World_MouseInspect_Keybind = new("world_mouse_inspect_keybind", KeyCode.None);

            UI_MouseInspect_Keybind = new("ui_mouse_inspect_keybind", KeyCode.None);

            CSConsole_Assembly_Blacklist = new("csharp_console_assembly_blacklist", "");

            Reflection_Signature_Blacklist = new("Member Signature Blacklist",
                "Use this to blacklist certain member signatures if they are known to cause a crash or other issues.\r\n" +
                "Seperate signatures with a semicolon ';'.\r\n" +
                "For example, to blacklist Camera.main, you would add 'UnityEngine.Camera.main;'",
                "");

            TIME_SCALE_TOGGLE = new("TimeScale Toggle",
                "Shortcut key for locking/unlocking TimeScale",
                KeyCode.None);
            LOCK_TIME_SCALE_TO_ZERO = new("Pause Keybind",
                "Shortcut key for setting TimeScale to 0.0",
                KeyCode.None);
            LOCK_TIME_SCALE_TO_NORMAL = new("Playback Keybind",
                "Shortcut key for setting TimeScale to 1.0",
                KeyCode.None);
            LOCK_TIME_SCALE_TO_HALF = new("Speed-Down Keybind",
                "Shortcut key for setting TimeScale to half",
                KeyCode.None);
            LOCK_TIME_SCALE_TO_DOUBLE = new("Speed-Up Keybind",
                "Shortcut key for setting TimeScale to double",
                KeyCode.None);
        }
    }
}
