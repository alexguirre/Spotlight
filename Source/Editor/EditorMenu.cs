namespace Spotlight.Editor
{
    using RAGENativeUI;
    using RAGENativeUI.Elements;

    internal abstract class EditorMenuBase : UIMenu
    {
        public EditorMenuBase(string title) : base(string.Empty, title)
        {
            Plugin.EditorMenuPool.Add(this);

            RemoveBanner();
            Width = 0.29f;
        }
    }

    internal class EditorMenu : EditorMenuBase
    {
        public EditorMenu() : base("Spotlight Editor")
        {
            var visualSettings = new UIMenuItem("Visual Settings");

            AddItems(visualSettings);
            BindMenuToItem(new VisualSettingsMenu(), visualSettings);
        }
    }
}
