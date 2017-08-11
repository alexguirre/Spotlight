namespace Spotlight.Editor
{
    using System.Drawing;

    using Rage;
    using Rage.Forms;

    using Gwen;
    using Gwen.Control;

    internal class InputTextForm : GwenForm
    {
        private TextBox textBox;

        public string Input { get { return textBox == null ? null : textBox.Text; } set { if (textBox != null) textBox.Text = value; } }
        public bool Cancelled { get; set; } = true;


        public InputTextForm(string title) : base(title, 700, 90)
        {
        }

        public override void InitializeLayout()
        {
            base.InitializeLayout();
            Window.DisableResizing();
            Position = new Point(Game.Resolution.Width / 2 - Window.Width / 2, Game.Resolution.Height / 2 - Window.Height / 2);

            textBox = new TextBox(Window);
            textBox.Width = Window.Width - 13;

            Button okButton = new Button(Window);
            okButton.SetPosition(Window.Width - 13 - okButton.Width, 30);
            okButton.Text = "OK";
            okButton.Clicked += OnOkButtonClicked;
        }

        private void OnOkButtonClicked(Base sender, ClickedEventArgs e)
        {
            if (e.MouseDown)
            {
                Cancelled = false;
                Window.Close();
            }
        }
    }
}
