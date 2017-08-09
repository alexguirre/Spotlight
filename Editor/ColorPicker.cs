namespace Spotlight.Editor
{
    using System.Drawing;

    using Gwen.Control;
    using Gwen.ControlInternal;

    internal sealed class ColorPicker : Base
    {
        public delegate void ColorChangedEventHandler(ColorPicker sender, Color color);


        private bool updateSlidersValuesOnColorChanged = true;
        private bool updateUpDownsValuesOnColorChanged = true;

        public event ColorChangedEventHandler ColorChanged;

        private Color color;
        public Color Color
        {
            get { return color; }
            set
            {
                if (value != color)
                {
                    color = value;
                    OnColorChanged(color);
                }
            }
        }

        public ColorPicker(Base parent) : base(parent)
        {
            SetSize(290, 70);
            CreateControls();
            Color = Color.White;
        }

        private void CreateControls()
        {
            CreateColorSlider("Red", 0);
            CreateColorSlider("Green", 25);
            CreateColorSlider("Blue", 50);

            ColorDisplay totalDisplay = new ColorDisplay(this);
            totalDisplay.SetPosition(220, 1);
            totalDisplay.SetSize(70, 70);
            totalDisplay.Color = Color.White;
            totalDisplay.Name = $"TotalDisplay";
        }

        private void CreateColorSlider(string name, int y)
        {
            HorizontalSlider slider = new HorizontalSlider(this);
            slider.Min = 0f;
            slider.Max = 255f;
            slider.SetPosition(0f, y);
            slider.SetSize(150, 20);
            slider.ValueChanged += OnSliderValueChanged;
            slider.Name = $"{name}Slider";

            IntNumericUpDown upDown = new IntNumericUpDown(this);
            upDown.Min = 0;
            upDown.Max = 255;
            upDown.SetPosition(160, y + 2);
            upDown.SetSize(32, 16);
            upDown.ValueChanged += OnUpDownValueChanged;
            upDown.Name = $"{name}UpDown";

            ColorDisplay colorDisplay = new ColorDisplay(this);
            colorDisplay.SetPosition(200, y + 3);
            colorDisplay.SetSize(16, 16);
            colorDisplay.Color = Color.White;
            colorDisplay.Name = $"{name}Display";
        }

        private void OnColorChanged(Color color)
        {
            ((ColorDisplay)FindChildByName("TotalDisplay")).Color = color;
            ((ColorDisplay)FindChildByName("RedDisplay")).Color = Color.FromArgb(color.R, 0, 0);
            ((ColorDisplay)FindChildByName("GreenDisplay")).Color = Color.FromArgb(0, color.G, 0);
            ((ColorDisplay)FindChildByName("BlueDisplay")).Color = Color.FromArgb(0, 0, color.B);

            if (updateSlidersValuesOnColorChanged)
            {
                ((HorizontalSlider)FindChildByName("RedSlider")).Value = color.R;
                ((HorizontalSlider)FindChildByName("GreenSlider")).Value = color.G;
                ((HorizontalSlider)FindChildByName("BlueSlider")).Value = color.B;
            }

            if (updateUpDownsValuesOnColorChanged)
            {
                ((NumericUpDown)FindChildByName("RedUpDown")).Value = color.R;
                ((NumericUpDown)FindChildByName("GreenUpDown")).Value = color.G;
                ((NumericUpDown)FindChildByName("BlueUpDown")).Value = color.B;
            }

            ColorChanged?.Invoke(this, color);
        }

        private void OnSliderValueChanged(Base sender, System.EventArgs arguments)
        {
            updateSlidersValuesOnColorChanged = false;
            switch (sender.Name)
            {
                case "RedSlider":
                    int rValue = (int)((Slider)sender).Value;
                    Color = Color.FromArgb(rValue, Color.G, Color.B);
                    break;
                case "GreenSlider":
                    int gValue = (int)((Slider)sender).Value;
                    Color = Color.FromArgb(Color.R, gValue, Color.B);
                    break;
                case "BlueSlider":
                    int bValue = (int)((Slider)sender).Value;
                    Color = Color.FromArgb(Color.R, Color.G, bValue);
                    break;
            }
            updateSlidersValuesOnColorChanged = true;
        }

        private void OnUpDownValueChanged(Base sender, System.EventArgs arguments)
        {
            updateUpDownsValuesOnColorChanged = false;
            switch (sender.Name)
            {
                case "RedUpDown":
                    int rValue = (int)((NumericUpDown)sender).Value;
                    Color = Color.FromArgb(rValue, Color.G, Color.B);
                    break;
                case "GreenUpDown":
                    int gValue = (int)((NumericUpDown)sender).Value;
                    Color = Color.FromArgb(Color.R, gValue, Color.B);
                    break;
                case "BlueUpDown":
                    int bValue = (int)((NumericUpDown)sender).Value;
                    Color = Color.FromArgb(Color.R, Color.G, bValue);
                    break;
            }
            updateUpDownsValuesOnColorChanged = true;
        }
    }
}
