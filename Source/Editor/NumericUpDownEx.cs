namespace Spotlight.Editor
{
    using System;
    using System.Drawing;

    using Gwen.Control;
    using Gwen.Input;

    internal class NumericUpDownEx : NumericUpDown
    {
        public float Increment { get; set; } = 1f;

        private bool mouseDown;
        private bool mouseDownChangeValue;

        public NumericUpDownEx(Base parent) : base(parent)
        {
            SelectAllOnFocus = false;
        }
        
		protected override void OnButtonUp(Base control, EventArgs args)
        {
            Value += Increment;
        }
        
        protected override void OnButtonDown(Base control, ClickedEventArgs args)
        {
            Value -= Increment;
        }
        
        protected override void OnMouseClickedLeft(int x, int y, bool down)
        {
            base.OnMouseClickedLeft(x, y, down);
            if (down)
            {
                if (!mouseDown && CanvasPosToLocal(new Point(x, y)).X < 3) // check if clicked in the left part of the textbox
                {
                    mouseDownChangeValue = true;
                    InputHandler.MouseFocus = this;
                }

                mouseDown = true;
            }
            else
            {
                mouseDown = false;

                InputHandler.MouseFocus = null;
            }
        }
        
        protected override void OnMouseMoved(int x, int y, int dx, int dy)
        {
            base.OnMouseMoved(x, y, dx, dy);
            if (!mouseDown || !mouseDownChangeValue)
            {
                return;
            }

            if (dx > 0)
                Value += Increment;
            if (dx < 0)
                Value -= Increment;
            CursorPos = 0;
            CursorEnd = 0;
        }
    }
}
