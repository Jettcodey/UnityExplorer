using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Widgets.ButtonList;

namespace UnityExplorer.UI.TinyUI
{
    public class RemoveButtonCell : ButtonCell
    {
        public ButtonRef RemoveButtonRef;

        public override GameObject CreateContent(GameObject parent)
        {
            GameObject root = base.CreateContent(parent);
            RemoveButtonRef = UIFactory.CreateButton(root, "RemoveButon", "Remove");
            UIFactory.SetLayoutElement(RemoveButtonRef.GameObject, minHeight: 25, minWidth: 80);
            return root;
        }
    }
}