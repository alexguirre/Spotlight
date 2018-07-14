namespace Spotlight.Editor
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Drawing;
    using System.Reflection;
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

        public EditorForm() : base("Spotlight Editor", 650, 325)
        {
            Controller = new ControllerFiber(this);
        }

        public override void InitializeLayout()
        {
            base.InitializeLayout();
            Window.DisableResizing();
            Window.Padding = new Padding(0, 2, 0, 0);
            DockedTabControl d = new DockedTabControl(Window);
            CreateVisualSettingsTabPageControls(d.AddPage("Visual Settings "), "VisualSettings");
            CreateOffsetsTabPageControls(d.AddPage("Offsets "), "Offsets");
        }

        private void CreateVisualSettingsTabPageControls(TabButton tab, string name)
        {
            Base page = tab.Page;
            page.Name = $"{name}Page";
            page.Padding = new Padding(-6, 0, -6, -6);

            DockedTabControl d = new DockedTabControl(page);
            TabButton defaultButton = d.AddPage("Default ");
            TabButton boatButton = d.AddPage("Boat ");
            TabButton heliButton = d.AddPage("Helicopter ");
            CreateSpotlightDataTabPageControls(defaultButton, $"{name}Default");
            CreateSpotlightDataTabPageControls(boatButton, $"{name}Boat");
            CreateSpotlightDataTabPageControls(heliButton, $"{name}Helicopter");

            if (Game.LocalPlayer.Character.IsInAnyVehicle(false))
            {
                // there isn't any TabControl.CurrentButton setter so we're doing it the reflection way
                FieldInfo currentButtonFieldInfo = typeof(TabControl).GetField("m_CurrentButton", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo pageFieldInfo = typeof(TabButton).GetField("m_Page", BindingFlags.NonPublic | BindingFlags.Instance);
                Model m = Game.LocalPlayer.Character.CurrentVehicle.Model;

                Base b = (Base)pageFieldInfo.GetValue(d.CurrentButton);
                b.IsHidden = true;

                if (m.IsHelicopter)
                {
                    currentButtonFieldInfo.SetValue(d, heliButton);
                }
                else if (m.IsBoat)
                {
                    currentButtonFieldInfo.SetValue(d, boatButton);
                }
                else
                {
                    currentButtonFieldInfo.SetValue(d, defaultButton);
                }

                b = (Base)pageFieldInfo.GetValue(d.CurrentButton);
                b.IsHidden = false;
            }
        }

        private void CreateOffsetsTabPageControls(TabButton tab, string name)
        {
            Base page = tab.Page;
            page.Name = $"{name}Page";

            Label label = new Label(page);
            label.Name = $"{name}Label";
            label.Text = "Model ";
            label.SetPosition(12, 5 + 3);
            label.Alignment = Pos.CenterV | Pos.Left;

            ComboBox comboBox = new ComboBox(page);
            comboBox.Name = $"{name}ComboBox";
            comboBox.SetPosition(60, 5);
            comboBox.Width += 20;
            comboBox.AddItem("New...", $"{name}ComboBoxItemNew");
            int j = 1;
            foreach (KeyValuePair<string, VehicleData> entry in Plugin.Settings.Vehicles.Data)
            {
                comboBox.AddItem(entry.Key, $"{name}ComboBoxItem{j++}", new Model(entry.Key));
            }
            comboBox.SelectByText(Plugin.Settings.Vehicles.Data.First().Key);
            comboBox.ItemSelected += OnOffsetsComboBoxItemSelected;

            Vector3 v = Plugin.Settings.Vehicles.Data.First().Value.Offset;
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
                upDown.ValueChanged += OnOffsetsValueChanged;
            }

            Label disableTurretLabel = new Label(page);
            disableTurretLabel.Name = $"{name}DisableTurretLabel";
            disableTurretLabel.Text = "Disable Turret";
            disableTurretLabel.SetPosition(430, 5);
            disableTurretLabel.Alignment = Pos.CenterV | Pos.Left;

            CheckBox disableTurretCheckBox = new CheckBox(page);
            disableTurretCheckBox.Name = $"{name}DisableTurretCheckBox";
            disableTurretCheckBox.SetPosition(500, 5);
            disableTurretCheckBox.CheckChanged += OnOffsetsValueChanged;

            Button saveButton = new Button(page);
            saveButton.Name = $"{name}SaveButton";
            saveButton.Text = "Save ";
            saveButton.Width = 150;
            saveButton.SetPosition(12, 65);
            saveButton.Clicked += OnOffsetsSaveButtonClicked;
            saveButton.SetToolTipText("Saves all offsets to the vehicle settings file");


            if (Game.LocalPlayer.Character.IsInAnyVehicle(false))
            {
                Model m = Game.LocalPlayer.Character.CurrentVehicle.Model;

                comboBox.SelectByUserData(m);
            }
        }

        private void CreateSpotlightDataTabPageControls(TabButton tab, string name)
        {
            Base page = tab.Page;
            page.Name = $"{name}Page";

            SpotlightData sData = GetSpotlightDataForControl(page);

            int y = 5;
            int x = 12;
            CreateFloatFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.OuterAngle)}", "Outer Angle ", 0, 90, 0.5f, sData.OuterAngle);
            CreateFloatFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.InnerAngle)}", "Inner Angle ", 0, 90, 0.5f, sData.InnerAngle);
            CreateFloatFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.Intensity)}", "Intensity ", -9999, 9999, 1f, sData.Intensity);
            CreateFloatFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.Range)}", "Range ", -9999, 9999, 1f, sData.Range);
            CreateFloatFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.Falloff)}", "Falloff ", -9999, 9999, 1f, sData.Falloff);
            CreateFloatFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.VolumeIntensity)}", "Volume Intensity ", -9999, 9999, 0.05f, sData.VolumeIntensity);
            CreateFloatFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.VolumeSize)}", "Volume Size ", -9999, 9999, 0.05f, sData.VolumeSize);
            CreateFloatFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.MovementSpeed)}", "Movement Speed ", 0, 100, 0.5f, sData.MovementSpeed);

            y += 6;
            CreateSaveButton(page, x, ref y, $"{name}Save", "Save ");

            y = 5;
            x = 285;
            CreateFloatFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.CoronaIntensity)}", "Corona Intensity ", 0, 9999, 0.05f, sData.CoronaIntensity);
            CreateFloatFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.CoronaSize)}", "Corona Size ", -9999, 9999, 0.05f, sData.CoronaSize);
            CreateBoolFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.CastShadows)}", "Cast Shadows ", sData.CastShadows);
            CreateBoolFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.Volume)}", "Volume ", sData.Volume);
            CreateBoolFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.Corona)}", "Corona ", sData.Corona);
            CreateBoolFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.Specular)}", "Specular ", sData.Specular);
            CreateColorFieldControl(page, x, ref y, $"{name}{nameof(SpotlightData.Color)}", "Color ", sData.Color);

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

            y += 25;
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

            y += 80;
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

            y += 25;
        }

        private void CreateSaveButton(Base parent, int x, ref int y, string name, string text)
        {
            Button button = new Button(parent);
            button.Name = $"{name}Button";
            button.Text = text;
            button.Width = 150;
            button.SetPosition(x, y);
            button.Clicked += OnSaveButtonClicked;
            button.SetToolTipText("Saves the visual settings to VisualSettings.xml.");

            y += 25;
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
            XmlSerializer ser = new XmlSerializer(typeof(VisualSettings));
            using (StreamWriter writer = new StreamWriter(Plugin.Settings.VisualSettingsFileName, false))
            {
                ser.Serialize(writer, Plugin.Settings.Visual);
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
                bool b = false;
                if (Plugin.Settings.Vehicles.Data.ContainsKey(selectedModel))
                {
                    v = Plugin.Settings.Vehicles.Data[selectedModel].Offset;
                    b = Plugin.Settings.Vehicles.Data[selectedModel].DisableTurret;
                }
                
                ((NumericUpDownEx)Window.FindChildByName("OffsetsXNumUpDown", true)).Value = v.X;
                ((NumericUpDownEx)Window.FindChildByName("OffsetsYNumUpDown", true)).Value = v.Y;
                ((NumericUpDownEx)Window.FindChildByName("OffsetsZNumUpDown", true)).Value = v.Z;
                ((CheckBox)Window.FindChildByName("OffsetsDisableTurretCheckBox", true)).IsChecked = b;
            }
        }

        private void OnOffsetsValueChanged(object sender, EventArgs args)
        {
            string selectedModel = ((ComboBox)Window.FindChildByName("OffsetsComboBox", true)).SelectedItem.Text;
            Vector3 v = new Vector3(((NumericUpDownEx)Window.FindChildByName("OffsetsXNumUpDown", true)).Value,
                                    ((NumericUpDownEx)Window.FindChildByName("OffsetsYNumUpDown", true)).Value,
                                    ((NumericUpDownEx)Window.FindChildByName("OffsetsZNumUpDown", true)).Value);
            bool b = ((CheckBox)Window.FindChildByName("OffsetsDisableTurretCheckBox", true)).IsChecked;

            Dictionary<string, Tuple<Vector3, bool>> clone = Plugin.Settings.Vehicles.Data.ToDictionary(e => e.Key, e => Tuple.Create((Vector3)e.Value.Offset, e.Value.DisableTurret));

            bool disableTurretChanged = b != (clone.ContainsKey(selectedModel) ? clone[selectedModel].Item2 : false);
            clone[selectedModel] = Tuple.Create(v, b);
            Plugin.Settings.UpdateVehicleSettings(clone, false);
            Model m = selectedModel;
            foreach (VehicleSpotlight s in Plugin.Spotlights)
            {
                if(s.Vehicle.Model == m)
                {
                    s.VehicleData.Offset = v;
                    s.VehicleData.DisableTurret = b;
                    if (disableTurretChanged)
                    {
                        s.OnDisableTurretChanged();
                    }
                }
            }
        }

        private void OnOffsetsSaveButtonClicked(object sender, ClickedEventArgs args)
        {
            Dictionary<string, Tuple<Vector3, bool>> clone = Plugin.Settings.Vehicles.Data.ToDictionary(e => e.Key, e => Tuple.Create((Vector3)e.Value.Offset, e.Value.DisableTurret));
            Plugin.Settings.UpdateVehicleSettings(clone, true);
        }



        private string GetSpotlightTypeForControl(Base control)
        {
            if (control.Name.Contains("Default"))
            {
                return "Default";
            }

            if (control.Name.Contains("Boat"))
            {
                return "Boat";
            }

            if (control.Name.Contains("Helicopter"))
            {
                return "Helicopter";
            }

            return null;
        }

        private SpotlightData GetSpotlightDataForControl(Base control)
        {
            switch (GetSpotlightTypeForControl(control))
            {
                case "Default": return Plugin.Settings.Visual.Default;
                case "Boat": return Plugin.Settings.Visual.Boat;
                case "Helicopter": return Plugin.Settings.Visual.Helicopter;
            }

            return null;
        }

        private void SetFieldForControl(Base control, float value)
        {
            SpotlightData d = GetSpotlightDataForControl(control);

            if (control.Name.Contains(nameof(SpotlightData.OuterAngle))) d.OuterAngle = value;
            else if (control.Name.Contains(nameof(SpotlightData.InnerAngle))) d.InnerAngle = value;
            else if (control.Name.Contains(nameof(SpotlightData.Intensity)) && !control.Name.Contains("Volume") && !control.Name.Contains("Corona")) d.Intensity = value;
            else if (control.Name.Contains(nameof(SpotlightData.Range))) d.Range = value;
            else if (control.Name.Contains(nameof(SpotlightData.Falloff))) d.Falloff = value;
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
            else if (control.Name.Contains(nameof(SpotlightData.Volume))) d.Volume = value;
            else if (control.Name.Contains(nameof(SpotlightData.Corona))) d.Corona = value;
            else if (control.Name.Contains(nameof(SpotlightData.Specular))) d.Specular = value;
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
                while (Editor.Window.IsVisible)
                {
                    GameFiber.Yield();

                    NativeFunction.Natives.DisableAllControlActions(0);
                }
            }
        }
    }
}
