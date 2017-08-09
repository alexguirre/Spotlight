namespace Spotlight.Editor
{
    using Gwen.Control;

    internal class IntNumericUpDown : NumericUpDown
    {
        public IntNumericUpDown(Base parent) : base(parent)
        {
        }

        protected override bool IsTextAllowed(string text)
        {
            return !text.Contains(",") && !text.Contains(".") && base.IsTextAllowed(text);
        }

        protected override bool IsTextAllowed(string text, int position)
        {
            return !text.Contains(",") && !text.Contains(".") && base.IsTextAllowed(text, position);
        }
    }
}
