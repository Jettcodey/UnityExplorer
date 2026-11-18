using UnityEngine.EventSystems;
using UniverseLib.UI;
using UniverseLib.UI.Widgets.ButtonList;
using UniverseLib.UI.Widgets.ScrollView;

namespace UnityExplorer.UI.Panels
{
    public class ClipboardPanel : UEPanel, ICellPoolDataSource<ButtonCell>
    {
        private static readonly List<object> Items = new();
        public static object Current => Items.Count > 0 ? Items[Items.Count - 1] : null;
        
        private static readonly Dictionary<ButtonCell, int> cellIndices = new();
        public override string Name => "Clipboard";
        public override UIManager.Panels PanelType => UIManager.Panels.Clipboard;
        public override int MinWidth => 500;
        public override int MinHeight => 95;
        public override Vector2 DefaultAnchorMin => new(0.1f, 0.05f);
        public override Vector2 DefaultAnchorMax => new(0.4f, 0.15f);
        public override bool CanDragAndResize => true;
        public override bool NavButtonWanted => true;
        public override bool ShouldSaveActiveState => true;
        public override bool ShowByDefault => true;

        private static ScrollPool<ButtonCell> scrollPool;
        private static Text currentPasteLabel;

        public ClipboardPanel(UIBase owner) : base(owner) { }

        public int ItemCount => Items.Count;

        public void OnCellBorrowed(ButtonCell cell)
        {
            cell.Button.OnClick += () =>
            {
                if (cellIndices.TryGetValue(cell, out int idx))
                    OnEntryClicked(idx);
            };
            
            EventTrigger trigger = cell.Button.Component.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new() { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener(data =>
            {
#if CPP
                PointerEventData ped = data.TryCast<PointerEventData>();
#else
        PointerEventData ped = data as PointerEventData;
#endif
                if (ped == null || ped.button != PointerEventData.InputButton.Right)
                    return;

                if (cellIndices.TryGetValue(cell, out int idx))
                    OnEntryRightClicked(idx);
            });
            trigger.triggers.Add(entry);
        }

        public void SetCell(ButtonCell cell, int index)
        {
            if (index < 0 || index >= Items.Count)
            {
                cell.Disable();
                return;
            }

            cellIndices[cell] = index;
            cell.Button.ButtonText.text = ToStringUtility.ToStringWithType(Items[index], typeof(object), false);
        }

        private void OnEntryClicked(int index)
        {
            if (index >= 0 && index < Items.Count)
                InspectorManager.Inspect(Items[index]);
        }
        
        private static void OnEntryRightClicked(int index)
        {
            if (index < 0 || index >= Items.Count)
                return;

            object obj = Items[index];
            Items.RemoveAt(index);
            Items.Add(obj);

            Notification.ShowMessage("Copied!");
            Refresh();
        }

        public static void Copy(object obj)
        {
            if (obj == null) return;
            Items.Add(obj);
            Notification.ShowMessage("Copied!");
            Refresh();
        }

        public static bool TryPaste(Type targetType, out object paste)
        {
            paste = Current;
            if (paste == null) return false;

            Type pasteType = paste.GetActualType();
            if (!targetType.IsAssignableFrom(pasteType))
            {
                Notification.ShowMessage($"Cannot assign '{pasteType.Name}' to '{targetType.Name}'!");
                return false;
            }
            Notification.ShowMessage("Pasted!");
            return true;
        }

        public static void ClearClipboard()
        {
            Items.Clear();
            Refresh();
        }

        private static void Refresh()
        {
            if (currentPasteLabel != null)
                currentPasteLabel.text = Current == null
                    ? "not set"
                    : ToStringUtility.ToStringWithType(Current, typeof(object), false);

            scrollPool?.Refresh(true, false);
        }

        public override void SetDefaultSizeAndPosition()
        {
            base.SetDefaultSizeAndPosition();
            Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MinWidth);
            Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, MinHeight);
        }

        protected override void ConstructPanelContent()
        {
            UIRoot.GetComponent<Image>().color = new(0.1f, 0.1f, 0.1f);
            //
            // GameObject firstRow = UIFactory.CreateHorizontalGroup(ContentRoot, "FirstRow", false, false, true, true, 5,
            //     new(2, 1, 2, 2), new(1, 1, 1, 0));
            // UIFactory.SetLayoutElement(firstRow, minHeight: 25, flexibleWidth: 999);

            scrollPool = UIFactory.CreateScrollPool<ButtonCell>(ContentRoot, "CopyPasteScroller",
                out GameObject scrollObj, out GameObject _, new Color(0.09f, 0.09f, 0.09f));

            UIFactory.SetLayoutElement(scrollObj, minHeight: 25, flexibleHeight: 9999, flexibleWidth: 9999);

            scrollPool.Initialize(this); 
            Refresh();
        }
    }
}