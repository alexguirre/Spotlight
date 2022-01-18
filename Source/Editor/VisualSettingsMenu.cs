namespace Spotlight.Editor
{
    using RAGENativeUI;
    using RAGENativeUI.Elements;

    using System;
    using System.Drawing;

    internal class VisualSettingsMenu : EditorMenuBase
    {
        private bool syncing;
        private readonly UIMenuListScrollerItem<string> typeSelector;
        private readonly UIMenuNumericScrollerItem<float> outerAngle, innerAngle;
        private readonly UIMenuNumericScrollerItem<float> intensity, range, falloff;
        private readonly UIMenuCheckboxItem volume;
        private readonly UIMenuNumericScrollerItem<float> volumeIntensity, volumeSize;
        private readonly UIMenuCheckboxItem corona;
        private readonly UIMenuNumericScrollerItem<float> coronaIntensity, coronaSize;
        private readonly UIMenuNumericScrollerItem<byte> colorR, colorG, colorB;
        private readonly UIMenuNumericScrollerItem<float> extraLightEmissive;
        private readonly UIMenuCheckboxItem castShadows;
        private readonly UIMenuCheckboxItem specular;
        private readonly UIMenuItem save;

        private SpotlightData CurrentSpotlightData => typeSelector.Index switch
        {
            0 => Plugin.Settings.Visual.Default,
            1 => Plugin.Settings.Visual.Helicopter,
            2 => Plugin.Settings.Visual.Boat,
            _ => throw new InvalidOperationException("Invalid visual settings index"),
        };

        public VisualSettingsMenu() : base("Spotlight Editor: Visual Settings")
        {
            MaxItemsOnScreen = 20;

            typeSelector = new("Type", "", new[] { "Default", "Helicopter", "Boat" });
            typeSelector.IndexChanged += OnSelectedVisualSettingsChanged;

            outerAngle      = NewFloatScroller("  Outer Angle", "", 0.0f, 90.0f, 0.01f, OnOuterAngleChanged);
            innerAngle      = NewFloatScroller("  Inner Angle", "", 0.0f, 90.0f, 0.01f, OnInnerAngleChanged);

            intensity       = NewFloatScroller("  Intensity", "", 0.01f, 500.0f, 0.01f, OnIntensityChanged);
            range           = NewFloatScroller("  Range", "", 0.0f, 1000.0f, 0.1f, OnRangeChanged);
            falloff         = NewFloatScroller("  Falloff", "", 0.01f, 500.0f, 0.01f, OnFalloffChanged);

            volume          = NewCheckbox("  Volume", "", OnVolumeEnabledChanged);
            volumeIntensity = NewFloatScroller("    Intensity", "", 0.0f, 1.0f, 0.001f, OnVolumeIntensityChanged);
            volumeSize      = NewFloatScroller("    Size", "", 0.0f, 1.0f, 0.001f, OnVolumeSizeChanged);

            corona          = NewCheckbox("  Corona", "", OnCoronaEnabledChanged);
            coronaIntensity = NewFloatScroller("    Intensity", "", 0.0f, 100.0f, 0.01f, OnCoronaIntensityChanged);
            coronaSize      = NewFloatScroller("    Size", "", 0.0f, 10.0f, 0.01f, OnCoronaSizeChanged);

            var colorHeader = new UIMenuItem("  Color") { Skipped = true };
            colorR          = NewByteScroller("    Red", "", 0, 255, 1, OnColorChanged, HudColor.Red, HudColor.RedDark);
            colorG          = NewByteScroller("    Green", "", 0, 255, 1, OnColorChanged, HudColor.Green, HudColor.GreenDark);
            colorB          = NewByteScroller("    Blue", "", 0, 255, 1, OnColorChanged, HudColor.Blue, HudColor.BlueDark);

            extraLightEmissive = NewFloatScroller("  Extra Light Emissive", "", 0.0f, 50.0f, 0.01f, OnExtraLightEmissiveChanged);
            castShadows = NewCheckbox("  Cast Shadows", "", OnCastShadowsChanged);
            specular = NewCheckbox("  Specular", "", OnSpecularChanged);

            save = new("Save") { Enabled = false };
            save.Activated += OnSave;

            AddItems(
                typeSelector,
                outerAngle, innerAngle,
                intensity, range, falloff,
                volume, volumeIntensity, volumeSize,
                corona, coronaIntensity, coronaSize,
                colorHeader, colorR, colorG, colorB,
                extraLightEmissive,
                castShadows, specular,
                save);

            this.WithFastScrollingOn(
                outerAngle, innerAngle,
                intensity, range, falloff,
                volumeIntensity, volumeSize,
                coronaIntensity, coronaSize,
                colorR, colorG, colorB,
                extraLightEmissive);

            SyncItemsToSpotlightData();
        }

        private void SyncItemsToSpotlightData()
        {
            syncing = true;
            var data = CurrentSpotlightData;
            outerAngle.Value = data.OuterAngle;
            innerAngle.Value = data.InnerAngle;
            intensity.Value = data.Intensity;
            range.Value = data.Range;
            falloff.Value = data.Falloff;
            volume.Checked = data.Volume;
            volumeIntensity.Value = data.VolumeIntensity;
            volumeSize.Value = data.VolumeSize;
            corona.Checked = data.Corona;
            coronaIntensity.Value = data.CoronaIntensity;
            coronaSize.Value = data.CoronaSize;
            colorR.Value = data.Color.R;
            colorG.Value = data.Color.G;
            colorB.Value = data.Color.B;
            extraLightEmissive.Value = data.ExtraLightEmissive;
            castShadows.Checked = data.CastShadows;
            specular.Checked = data.Specular;
            syncing = false;
        }

        private void OnOuterAngleChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            if (syncing) return;

            CurrentSpotlightData.OuterAngle = outerAngle.Value;
            save.Enabled = true;
        }

        private void OnInnerAngleChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            if (syncing) return;

            CurrentSpotlightData.InnerAngle = innerAngle.Value;
            save.Enabled = true;
        }

        private void OnIntensityChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            if (syncing) return;

            CurrentSpotlightData.Intensity = intensity.Value;
            save.Enabled = true;
        }

        private void OnRangeChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            if (syncing) return;

            CurrentSpotlightData.Range = range.Value;
            save.Enabled = true;
        }

        private void OnFalloffChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            if (syncing) return;

            CurrentSpotlightData.Falloff = falloff.Value;
            save.Enabled = true;
        }

        private void OnVolumeEnabledChanged(UIMenuCheckboxItem sender, bool Checked)
        {
            if (syncing) return;

            CurrentSpotlightData.Volume = volume.Checked;
            save.Enabled = true;
        }

        private void OnVolumeIntensityChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            if (syncing) return;

            CurrentSpotlightData.VolumeIntensity = volumeIntensity.Value;
            save.Enabled = true;
        }

        private void OnVolumeSizeChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            if (syncing) return;

            CurrentSpotlightData.VolumeSize = volumeSize.Value;
            save.Enabled = true;
        }

        private void OnCoronaEnabledChanged(UIMenuCheckboxItem sender, bool Checked)
        {
            if (syncing) return;

            CurrentSpotlightData.Corona = corona.Checked;
            save.Enabled = true;
        }

        private void OnCoronaIntensityChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            if (syncing) return;

            CurrentSpotlightData.CoronaIntensity = coronaIntensity.Value;
            save.Enabled = true;
        }

        private void OnCoronaSizeChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            if (syncing) return;

            CurrentSpotlightData.CoronaSize = coronaSize.Value;
            save.Enabled = true;
        }

        private void OnColorChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            if (syncing) return;

            CurrentSpotlightData.Color = Color.FromArgb(255, colorR.Value, colorG.Value, colorB.Value);
            save.Enabled = true;
        }

        private void OnExtraLightEmissiveChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            if (syncing) return;

            CurrentSpotlightData.ExtraLightEmissive = extraLightEmissive.Value;
            save.Enabled = true;
        }

        private void OnCastShadowsChanged(UIMenuCheckboxItem sender, bool Checked)
        {
            if (syncing) return;

            CurrentSpotlightData.CastShadows = castShadows.Checked;
            save.Enabled = true;
        }

        private void OnSpecularChanged(UIMenuCheckboxItem sender, bool Checked)
        {
            if (syncing) return;

            CurrentSpotlightData.Specular = specular.Checked;
            save.Enabled = true;
        }

        private void OnSelectedVisualSettingsChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            SyncItemsToSpotlightData();
        }

        private void OnSave(UIMenu sender, UIMenuItem selectedItem)
        {
            save.Enabled = false;
        }

        private static UIMenuCheckboxItem NewCheckbox(string label, string description, ItemCheckboxEvent checkboxHandler)
        {
            var item = new UIMenuCheckboxItem(label, false, description);
            item.CheckboxEvent += checkboxHandler;
            return item;
        }

        private static UIMenuNumericScrollerItem<byte> NewByteScroller(string label, string description, byte min, byte max, byte step, ItemScrollerEvent indexChangedHandler, HudColor foreground = HudColor.White, HudColor background = HudColor.Grey)
        {
            var item = new UIMenuNumericScrollerItem<byte>(label, description, min, max, step) { SliderBar = new() { ForegroundColor = foreground.GetColor(), BackgroundColor = background.GetColor(), Height = 0.45f } };
            item.WithTextEditing();
            item.IndexChanged += indexChangedHandler;
            return item;
        }

        private static UIMenuNumericScrollerItem<float> NewFloatScroller(string label, string description, float min, float max, float step, ItemScrollerEvent indexChangedHandler, HudColor foreground = HudColor.White, HudColor background = HudColor.Grey)
        {
            var item = new UIMenuNumericScrollerItem<float>(label, description, min, max, step) { SliderBar = new() { ForegroundColor = foreground.GetColor(), BackgroundColor = background.GetColor(), Height = 0.45f } };
            item.WithTextEditing();
            item.IndexChanged += indexChangedHandler;
            return item;
        }
    }
}
