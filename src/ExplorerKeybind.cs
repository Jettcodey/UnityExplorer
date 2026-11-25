using UnityExplorer.Config;

using UnityExplorer.UI;
using UnityExplorer.UI.Widgets;
using UniverseLib.Input;

namespace UnityExplorer;

public static class ExplorerKeybind
{
    public static void Update()
    {
        // check master toggle
        if (InputManager.GetKeyDown(ConfigManager.Master_Toggle.Value))
        {
            UIManager.ShowMenu = !UIManager.ShowMenu;
        }

        // This is for debug only
        // Some games get upset with the UnityExplorer taking over the mouse and when exiting the UI, the game
        // wont go back into focus.. We can probably expose some sort of Debugger keybinds.
        if (InputManager.GetKeyDown(KeyCode.Minus) && InputManager.GetKeyDown(KeyCode.Equals))
        {
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
        }
        
        TimeScaleKeyBindUpdate();
    }

    private static void TimeScaleKeyBindUpdate()
    {
        var widget = TimeScaleWidget.Instance;
        if (widget == null)
        {
            return;
        }

        if (InputManager.GetKeyDown(ConfigManager.TIME_SCALE_TOGGLE.Value))
        {
            widget.OnPauseButtonClicked();
        }
        if (InputManager.GetKeyDown(ConfigManager.LOCK_TIME_SCALE_TO_ZERO.Value))
        {
            widget.LockTo(0.0f);
        }
        if (InputManager.GetKeyDown(ConfigManager.LOCK_TIME_SCALE_TO_NORMAL.Value))
        {
            widget.LockTo(1.0f);
        }
        if (InputManager.GetKeyDown(ConfigManager.LOCK_TIME_SCALE_TO_HALF.Value))
        {
            widget.LockTo(widget.DesiredTime * 0.5f);
        }
        if (InputManager.GetKeyDown(ConfigManager.LOCK_TIME_SCALE_TO_DOUBLE.Value))
        {
            widget.LockTo(widget.DesiredTime * 2.0f);
        }
    }
}
