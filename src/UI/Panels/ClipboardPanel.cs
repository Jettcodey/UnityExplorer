using Mono.CSharp;
using UnityEngine.EventSystems;
using UnityExplorer.CacheObject;
using UnityExplorer.UI.TinyUI;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Widgets.ButtonList;
using UniverseLib.UI.Widgets.ScrollView;

namespace UnityExplorer.UI.Panels
{
    public class ClipboardPanel : UEPanel, ICellPoolDataSource<RemoveButtonCell>
    {
        private static readonly List<object> Items = new();
        public static object Current => Items.Count > 0 ? Items[Items.Count - 1] : null;
        
        private static readonly Dictionary<ButtonCell, int> cellIndices = new();
        public override string Name => "Clipboard";
        public override UIManager.Panels PanelType => UIManager.Panels.Clipboard;
        public override int MinWidth => 500;
        public override int MinHeight => 300;

        public override Vector2 DefaultAnchorMin => new(0.1f, 0.05f);
        public override Vector2 DefaultAnchorMax => new(0.4f, 0.15f);
        public override bool CanDragAndResize => true;
        public override bool NavButtonWanted => true;
        public override bool ShouldSaveActiveState => true;
        public override bool ShowByDefault => true;

        private static ScrollPool<RemoveButtonCell> scrollPool;
        private static Text currentPasteLabel;

        private static ClipboardPanel instance;

        public ClipboardPanel(UIBase owner) : base(owner) { instance = this; }

        public int ItemCount => Items.Count;

        public void OnCellBorrowed(RemoveButtonCell cell)
        {
            cell.OnClick += (_) =>
            {
                if (cellIndices.TryGetValue(cell, out int idx))
                    OnEntryClicked(idx);
            };

            cell.RemoveButtonRef.OnClick += () =>
            {
                if (!cellIndices.TryGetValue(cell, out int idx))
                {
                    return;
                }

                Items.RemoveAt(idx);
                Refresh();
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

        public void SetCell(RemoveButtonCell cell, int index)
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

            int existing = Items.FindIndex(item => ReferenceEquals(item, obj));

            if (existing >= 0)
            {
                OnEntryRightClicked(existing);
                return;
            }

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
            
            GameObject firstRow = UIFactory.CreateHorizontalGroup(ContentRoot, "FirstRow", false, false, true, true, 5,
                new(2, 1, 2, 2), new(1, 1, 1, 0));
            UIFactory.SetLayoutElement(firstRow, minHeight: 25, flexibleWidth: 999);

            ButtonRef _clearButtonRef = UIFactory.CreateButton(firstRow, "ClearClibpaord", "Clear");
            UIFactory.SetLayoutElement(_clearButtonRef.GameObject, minHeight: 25, flexibleWidth: 999);
            _clearButtonRef.OnClick += ClearClipboard;
            
            scrollPool = UIFactory.CreateScrollPool<RemoveButtonCell>(ContentRoot, "CopyPasteScroller",
                out GameObject scrollObj, out GameObject _, new Color(0.09f, 0.09f, 0.09f));

            UIFactory.SetLayoutElement(scrollObj, minHeight: 25, flexibleHeight: 9999, flexibleWidth: 9999);

            scrollPool.Initialize(this); 
            Refresh();
        }
    }
}