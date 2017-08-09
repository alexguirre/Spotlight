namespace Spotlight.Editor
{
    using System;
    using System.IO;
    using System.Drawing;
    using System.Xml.Serialization;

    using Rage;
    using Rage.Forms;
    using Rage.Native;

    using Gwen;
    using Gwen.Control;

    internal class EditorForm : GwenForm
    {
        private ControllerFiber Controller { get; }

        public EditorForm() : base("Spotlight Editor", 650, 300)
        {
            Controller = new ControllerFiber(this);
        }

        public override void InitializeLayout()
        {
            base.InitializeLayout();
            Window.DisableResizing();
            Window.Padding = new Padding(3, 2, 3, 3);
            DockedTabControl d = new DockedTabControl(Window);
            CreateTabPageControls(d.AddPage("Cars"), "Cars");
            CreateTabPageControls(d.AddPage("Boats"), "Boats");
            CreateTabPageControls(d.AddPage("Helicopters"), "Helicopters");
        }


        private void CreateTabPageControls(TabButton tab, string name)
        {
            Base page = tab.Page;
            page.Name = $"{name}Page";

            SpotlightData sData = GetSpotlightDataForControl(page);

            int y = 5;
            int x = 12;
            CreateFloatFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.Angle)}", nameof(SpotlightData.Angle), 0, 90, 0.5f, sData.Angle);
            CreateFloatFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.Intensity)}", nameof(SpotlightData.Intensity), -999, 999, 1f, sData.Intensity);
            CreateFloatFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.Range)}", nameof(SpotlightData.Range), -999, 999, 1f, sData.Range);
            CreateFloatFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.Falloff)}", nameof(SpotlightData.Falloff), -999, 999, 1f, sData.Falloff);
            CreateFloatFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.Roundness)}", nameof(SpotlightData.Roundness), 0, 90, 0.5f, sData.Roundness);
            CreateFloatFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.VolumeIntensity)}", "Volume Intensity", -999, 999, 0.05f, sData.VolumeIntensity);
            CreateFloatFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.VolumeSize)}", "Volume Size", -999, 999, 0.05f, sData.VolumeSize);
            CreateFloatFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.MovementSpeed)}", "Movement Speed", 0, 100, 0.5f, sData.MovementSpeed);

            y = 5;
            x = 285;
            CreateFloatFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.CoronaIntensity)}", "Corona Intensity", -999, 999, 0.05f, sData.CoronaIntensity);
            CreateFloatFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.CoronaSize)}", "Corona Size", -999, 999, 0.05f, sData.CoronaSize);
            CreateBoolFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.CastShadows)}", "Cast Shadows", sData.CastShadows);
            CreateColorFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.Color)}", "Color ", sData.Color);
            CreateSaveButton(page, x, ref y, $"{name}Save", "Save");
        }

        private void CreateFloatFieldControl(Base parent, int x, ref int y, string name, string labelText, int min, int max, float increment, float initialValue)
        {
            Label label = new Label(parent);
            label.Name = $"{name}Label";
            label.Text = labelText;
            label.SetPosition(x, y + 3);
            label.Alignment = Pos.CenterV | Pos.Left;

            NumericUpDownEx upDown = new NumericUpDownEx(parent);
            upDown.Name = $"{name}NumUpDown";
            upDown.Min = min;
            upDown.Max = max;
            upDown.Increment = increment;
            upDown.Value = initialValue;
            upDown.SetPosition(x + 110, y);
            upDown.ValueChanged += OnNumericUpDownValueChanged;

            y += 30;
        }

        private void CreateColorFieldControl(Base parent, int x, ref int y, string name, string labelText, Color initialValue)
        {
            Label label = new Label(parent);
            label.Name = $"{name}Label";
            label.Text = labelText;
            label.SetPosition(x, y + 30);
            label.Alignment = Pos.CenterV | Pos.Left;

            ColorPicker colorPicker = new ColorPicker(parent);
            colorPicker.Name = $"{name}Picker";
            colorPicker.SetPosition(x + 50, y);
            colorPicker.Color = initialValue;
            colorPicker.ColorChanged += OnColorChanged;

            y += 85;
        }

        private void CreateBoolFieldControl(Base parent, int x, ref int y, string name, string labelText, bool initialValue)
        {
            Label label = new Label(parent);
            label.Name = $"{name}Label";
            label.Text = labelText;
            label.SetPosition(x, y + 3);
            label.Alignment = Pos.CenterV | Pos.Left;

            CheckBox checkBox = new CheckBox(parent);
            checkBox.Name = $"{name}CheckBox";
            checkBox.IsChecked = initialValue;
            checkBox.SetPosition(x + 110, y);
            checkBox.CheckChanged += OnCheckboxValueChanged;

            y += 30;
        }

        private void CreateSaveButton(Base parent, int x, ref int y, string name, string text)
        {
            Button button = new Button(parent);
            button.Name = $"{name}Button";
            button.Text = text;
            button.SetSize(100, 40);
            button.SetPosition(x + 110, y);
            button.Clicked += OnSaveButtonClicked;

            y += 45;
        }

        private void OnColorChanged(ColorPicker sender, Color color)
        {
            SetFieldForControl(sender, color);
        }

        private void OnNumericUpDownValueChanged(object sender, EventArgs args)
        {
            SetFieldForControl(((NumericUpDownEx)sender), ((NumericUpDownEx)sender).Value);
        }

        private void OnCheckboxValueChanged(object sender, EventArgs args)
        {
            SetFieldForControl(((CheckBox)sender), ((CheckBox)sender).IsChecked);
        }

        private void OnSaveButtonClicked(object sender, ClickedEventArgs args)
        {
            string fileName = $@"Plugins\Spotlight Resources\Spotlight Data - {GetSpotlightTypeForControl((Button)sender)}.xml";
            SpotlightData data = GetSpotlightDataForControl((Button)sender);

            XmlSerializer ser = new XmlSerializer(typeof(SpotlightData));
            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                ser.Serialize(writer, data);
            }
        }




        private string GetSpotlightTypeForControl(Base control)
        {
            if (control.Name.Contains("Cars"))
            {
                return "Cars";
            }

            if (control.Name.Contains("Boats"))
            {
                return "Boats";
            }

            if (control.Name.Contains("Helicopters"))
            {
                return "Helicopters";
            }

            return null;
        }

        private SpotlightData GetSpotlightDataForControl(Base control)
        {
            switch (GetSpotlightTypeForControl(control))
            {
                case "Cars": return Plugin.Settings.CarsSpotlightData;
                case "Boats": return Plugin.Settings.BoatsSpotlightData;
                case "Helicopters": return Plugin.Settings.HelicoptersSpotlightData;
            }

            return null;
        }

        private void SetFieldForControl(Base control, float value)
        {
            SpotlightData d = GetSpotlightDataForControl(control);



            if (control.Name.Contains(nameof(SpotlightData.Angle))) d.Angle = value;
            else if (control.Name.Contains(nameof(SpotlightData.Intensity))) d.Intensity = value;
            else if (control.Name.Contains(nameof(SpotlightData.Range))) d.Range = value;
            else if (control.Name.Contains(nameof(SpotlightData.Falloff))) d.Falloff = value;
            else if (control.Name.Contains(nameof(SpotlightData.Roundness))) d.Roundness = value;
            else if (control.Name.Contains(nameof(SpotlightData.VolumeIntensity))) d.VolumeIntensity = value;
            else if (control.Name.Contains(nameof(SpotlightData.VolumeSize))) d.VolumeSize = value;
            else if (control.Name.Contains(nameof(SpotlightData.CoronaIntensity))) d.CoronaIntensity = value;
            else if (control.Name.Contains(nameof(SpotlightData.CoronaSize))) d.CoronaSize = value;
            else if (control.Name.Contains(nameof(SpotlightData.MovementSpeed))) d.MovementSpeed = value;

            Game.DisplayHelp($"VolIntensity " + d.VolumeIntensity + " " + value);
        }

        private void SetFieldForControl(Base control, bool value)
        {
            SpotlightData d = GetSpotlightDataForControl(control);

            if (control.Name.Contains(nameof(SpotlightData.CastShadows))) d.CastShadows = value;
        }

        private void SetFieldForControl(Base control, Color value)
        {
            SpotlightData d = GetSpotlightDataForControl(control);

            if (control.Name.Contains(nameof(SpotlightData.Color))) d.Color = value;
        }



        private class ControllerFiber
        {
            public EditorForm Editor { get; }
            public GameFiber Fiber { get; }

            public ControllerFiber(EditorForm editor)
            {
                Editor = editor;
                Fiber = GameFiber.StartNew(FiberLoop, "Editor Controller");
            }

            private void FiberLoop()
            {
                while (!Editor.Window.IsVisible)
                    GameFiber.Yield();
                
                while (Editor.Window.IsVisible)
                {
                    GameFiber.Yield();

                    NativeFunction.Natives.DisableAllControlActions(0);
                }
            }
        }
    }
}
