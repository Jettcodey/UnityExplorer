using System.Globalization;
using UnityExplorer.Config;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace UnityExplorer.UI.Widgets
{
    public class HiDPIWidget
    {
        public static HiDPIWidget instance { get; private set; }
        
        private readonly InputFieldRef inputField;

        public static void SetUp(GameObject parent)
        {
            instance ??= new HiDPIWidget(parent);
        }
        
        private HiDPIWidget(GameObject parent)
        {
            Text dpiLabel = UIFactory.CreateLabel(parent, "HiDPILabel", "DPI:", TextAnchor.MiddleRight, Color.grey);
            UIFactory.SetLayoutElement(dpiLabel.gameObject, minHeight: 25, flexibleHeight: 35);
            
            inputField = UIFactory.CreateInputField(parent, "TimeInput", "timeScale");
            UIFactory.SetLayoutElement(inputField.Component.gameObject, minHeight: 25, minWidth: 40);
            inputField.Component.GetOnEndEdit().AddListener(OnTimeInputEndEdit);

            inputField.Text = UIManager.UICanvas.rootCanvas.scaleFactor.ToString("F2");
        }

        private void OnTimeInputEndEdit(string val)
        {
            if (float.TryParse(val, out float f))
            {
                UIManager.UICanvas.rootCanvas.scaleFactor = f;
                ConfigManager.DPI_Scaling_Factor.Value = f;
            }
        }
    }
}