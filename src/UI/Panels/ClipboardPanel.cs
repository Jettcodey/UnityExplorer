using UniverseLib.UI;

namespace UnityExplorer.UI.Panels
{
    public class ClipboardPanel : UEPanel
    {
        public static object Current { get; private set; }

        public override string Name => TranslationManager.Get(TranslationKey.Clipboard);
        public override UIManager.Panels PanelType => UIManager.Panels.Clipboard;

        public override int MinWidth => 500;
        public override int MinHeight => 95;
        public override Vector2 DefaultAnchorMin => new(0.1f, 0.05f);
        public override Vector2 DefaultAnchorMax => new(0.4f, 0.15f);

        public override bool CanDragAndResize => true;
        public override bool NavButtonWanted => true;
        public override bool ShouldSaveActiveState => true;
        public override bool ShowByDefault => true;

        private static Text CurrentPasteLabel;

        public ClipboardPanel(UIBase owner) : base(owner)
        {
        }

        public static void Copy(object obj)
        {
            Current = obj;
            Notification.ShowMessage(TranslationManager.Get(TranslationKey.Copied));
            UpdateCurrentPasteInfo();
        }

        public static bool TryPaste(Type targetType, out object paste)
        {
            paste = Current;
            Type pasteType = Current?.GetActualType();

            if (Current != null && !targetType.IsAssignableFrom(pasteType))
            {
                Notification.ShowMessage(
                    TranslationManager.Get(TranslationKey.CannotAssign, pasteType.Name, targetType.Name));
                return false;
            }

            Notification.ShowMessage(TranslationManager.Get(TranslationKey.Pasted));
            return true;
        }

        public static void ClearClipboard()
        {
            Current = null;
            UpdateCurrentPasteInfo();
        }

        private static void UpdateCurrentPasteInfo()
        {
            CurrentPasteLabel.text = ToStringUtility.ToStringWithType(Current, typeof(object), false);
        }

        private static void InspectClipboard()
        {
            if (Current.IsNullOrDestroyed())
            {
                Notification.ShowMessage(TranslationManager.Get(TranslationKey.CannotInspect));
                return;
            }

            InspectorManager.Inspect(Current);
        }

        public override void SetDefaultSizeAndPosition()
        {
            base.SetDefaultSizeAndPosition();

            this.Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MinWidth);
            this.Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, MinHeight);
        }

        protected override void ConstructPanelContent()
        {
            this.UIRoot.GetComponent<Image>().color = new(0.1f, 0.1f, 0.1f);

            // Actual panel content

            GameObject firstRow = UIFactory.CreateHorizontalGroup(ContentRoot, "FirstRow", false, false, true, true, 5, new(2, 2, 2, 2), new(1, 1, 1, 0));
            UIFactory.SetLayoutElement(firstRow, minHeight: 25, flexibleWidth: 999);

            // Title for "Current Paste:"
            Text currentPasteTitle = UIFactory.CreateLabel(firstRow, "CurrentPasteTitle", TranslationManager.Get(TranslationKey.CurrentPaste), TextAnchor.MiddleLeft, color: Color.grey);
            UIFactory.SetLayoutElement(currentPasteTitle.gameObject, minHeight: 25, minWidth: 100, flexibleWidth: 999);

            // Clear clipboard button
            UniverseLib.UI.Models.ButtonRef clearButton = UIFactory.CreateButton(firstRow, "ClearPasteButton", TranslationManager.Get(TranslationKey.ClearClipboard));
            UIFactory.SetLayoutElement(clearButton.Component.gameObject, minWidth: 120, minHeight: 25, flexibleWidth: 0);
            clearButton.OnClick += () => Copy(null);

            // Current Paste info row
            GameObject currentPasteHolder = UIFactory.CreateHorizontalGroup(ContentRoot, "SecondRow", false, false, true, true, 0,
                new(2, 2, 2, 2), childAlignment: TextAnchor.UpperCenter);

            // Actual current paste info label
            CurrentPasteLabel = UIFactory.CreateLabel(currentPasteHolder, "CurrentPasteInfo", TranslationManager.Get(TranslationKey.NotSet), TextAnchor.UpperLeft);
            UIFactory.SetLayoutElement(CurrentPasteLabel.gameObject, minHeight: 25, minWidth: 100, flexibleWidth: 999, flexibleHeight: 999);
            UpdateCurrentPasteInfo();

            // Inspect button
            UniverseLib.UI.Models.ButtonRef inspectButton = UIFactory.CreateButton(currentPasteHolder, "InspectButton", TranslationManager.Get(TranslationKey.Inspect));
            UIFactory.SetLayoutElement(inspectButton.Component.gameObject, minHeight: 25, flexibleHeight: 0, minWidth: 80, flexibleWidth: 0);
            inspectButton.OnClick += InspectClipboard;
        }
    }
}
