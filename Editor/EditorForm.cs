namespace Spotlight.Editor
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Drawing;
    using System.Xml.Serialization;
    using System.Collections.Generic;

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
            CreateSpotlightDataTabPageControls(d.AddPage("Cars"), "Cars");
            CreateSpotlightDataTabPageControls(d.AddPage("Boats"), "Boats");
            CreateSpotlightDataTabPageControls(d.AddPage("Helicopters"), "Helicopters");
            CreateOffsetsTabPageControls(d.AddPage("Offsets"), "Offsets");
        }

        private void CreateOffsetsTabPageControls(TabButton tab, string name)
        {
            Base page = tab.Page;
            page.Name = $"{name}Page";

            Label label = new Label(page);
            label.Name = $"{name}Label";
            label.Text = "Model";
            label.SetPosition(12, 5 + 3);
            label.Alignment = Pos.CenterV | Pos.Left;

            ComboBox comboBox = new ComboBox(page);
            comboBox.Name = $"{name}ComboBox";
            comboBox.SetPosition(60, 5);
            comboBox.Width += 20;
            comboBox.AddItem("New...", $"{name}ComboBoxItemNew");
            int j = 1;
            foreach (KeyValuePair<string, Vector3> entry in Plugin.Settings.SpotlightOffsets)
            {
                comboBox.AddItem(entry.Key, $"{name}ComboBoxItem{j++}");
            }
            comboBox.SelectByText(Plugin.Settings.SpotlightOffsets.First().Key);
            comboBox.ItemSelected += OnOffsetsComboBoxItemSelected;

            Vector3 v = Plugin.Settings.SpotlightOffsets.First().Value;
            string[] vecComponents = "X,Y,Z".Split(',');
            for (int i = 0; i < vecComponents.Length; i++)
            {
                string vecComp = vecComponents[i];

                int x = 250;
                int y = 5 + 30 * i;

                Label vecCompLabel = new Label(page);
                vecCompLabel.Name = $"{name}{vecComp}Label";
                vecCompLabel.Text = vecComp;
                vecCompLabel.SetPosition(x, y + 3);
                vecCompLabel.Alignment = Pos.CenterV | Pos.Left;

                NumericUpDownEx upDown = new NumericUpDownEx(page);
                upDown.Name = $"{name}{vecComp}NumUpDown";
                upDown.Min = -999;
                upDown.Max = 999;
                upDown.Increment = 0.01f;
                upDown.Value = v[i];
                upDown.SetPosition(x + 15, y);
                upDown.ValueChanged += OnOffsetsNumericUpDownValueChanged;
            }

            Button saveButton = new Button(page);
            saveButton.Name = $"{name}SaveButton";
            saveButton.Text = "Save";
            saveButton.SetPosition(12, 50);
            saveButton.Clicked += OnOffsetsSaveButtonClicked;
            saveButton.SetToolTipText("Saves all offsets to Offsets.ini.");
        }

        private void CreateSpotlightDataTabPageControls(TabButton tab, string name)
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
            button.SetPosition(x + 110, y + 10);
            button.Clicked += OnSaveButtonClicked;
            button.SetToolTipText("Saves the current spotlight settings.");

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


        private void OnOffsetsComboBoxItemSelected(object sender, ItemSelectedEventArgs e)
        {
            ComboBox c = ((ComboBox)Window.FindChildByName("OffsetsComboBox", true));
            MenuItem selectedItem = ((MenuItem)e.SelectedItem);

            if (selectedItem.Name == c.Name + "ItemNew")
            {
                GameFiber.StartNew(() =>
                {
                    InputTextForm f = new InputTextForm("Enter model name...");
                    f.Show();
                    f.Window.MakeModal();
                    while (f.Window.IsVisible)
                        GameFiber.Yield();
                    if (!f.Cancelled)
                    {
                        string n = f.Input;
                        c.SelectedItem = c.AddItem(n);
                    }
                    else
                    {
                        c.SelectByName(c.Name + "Item1");
                    }
                });
            }
            else
            {
                string selectedModel = selectedItem.Text;
                Vector3 v = Vector3.Zero;
                if (Plugin.Settings.SpotlightOffsets.ContainsKey(selectedModel))
                {
                    v = Plugin.Settings.SpotlightOffsets[selectedModel];
                }
                
                ((NumericUpDownEx)Window.FindChildByName("OffsetsXNumUpDown", true)).Value = v.X;
                ((NumericUpDownEx)Window.FindChildByName("OffsetsYNumUpDown", true)).Value = v.Y;
                ((NumericUpDownEx)Window.FindChildByName("OffsetsZNumUpDown", true)).Value = v.Z;
            }
        }

        private void OnOffsetsNumericUpDownValueChanged(object sender, EventArgs args)
        {
            string selectedModel = ((ComboBox)Window.FindChildByName("OffsetsComboBox", true)).SelectedItem.Text;
            Vector3 v = new Vector3(((NumericUpDownEx)Window.FindChildByName("OffsetsXNumUpDown", true)).Value,
                                    ((NumericUpDownEx)Window.FindChildByName("OffsetsYNumUpDown", true)).Value,
                                    ((NumericUpDownEx)Window.FindChildByName("OffsetsZNumUpDown", true)).Value);

            Dictionary<string, Vector3> clone = Plugin.Settings.SpotlightOffsets.ToDictionary(e => e.Key, e => e.Value);
            clone[selectedModel] = v;
            Plugin.Settings.UpdateOffsets(clone, false);
            foreach (VehicleSpotlight s in Plugin.Spotlights)
            {
                s.UpdateOffset();
            }
        }

        private void OnOffsetsSaveButtonClicked(object sender, ClickedEventArgs args)
        {
            Dictionary<string, Vector3> clone = Plugin.Settings.SpotlightOffsets.ToDictionary(e => e.Key, e => e.Value);
            Plugin.Settings.UpdateOffsets(clone, true);
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
            else if (control.Name.Contains(nameof(SpotlightData.Intensity)) && !control.Name.Contains("Volume") && !control.Name.Contains("Corona")) d.Intensity = value;
            else if (control.Name.Contains(nameof(SpotlightData.Range))) d.Range = value;
            else if (control.Name.Contains(nameof(SpotlightData.Falloff))) d.Falloff = value;
            else if (control.Name.Contains(nameof(SpotlightData.Roundness))) d.Roundness = value;
            else if (control.Name.Contains(nameof(SpotlightData.VolumeIntensity))) d.VolumeIntensity = value;
            else if (control.Name.Contains(nameof(SpotlightData.VolumeSize))) d.VolumeSize = value;
            else if (control.Name.Contains(nameof(SpotlightData.CoronaIntensity))) d.CoronaIntensity = value;
            else if (control.Name.Contains(nameof(SpotlightData.CoronaSize))) d.CoronaSize = value;
            else if (control.Name.Contains(nameof(SpotlightData.MovementSpeed))) d.MovementSpeed = value;
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
                while (true)
                {
                    GameFiber.Yield();
                    
                    if (Editor.Window.IsVisible)
                    {
                        NativeFunction.Natives.DisableAllControlActions(0);
                    }
                }
            }
        }
    }
}
