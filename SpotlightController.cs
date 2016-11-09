namespace Spotlight
{
    // RPH
    using Rage;
    
    internal abstract class SpotlightController
    {
        public Spotlight Owner { get; }

        protected SpotlightController(Spotlight owner)
        {
            Owner = owner;
        }

        public abstract bool GetUpdatedRotationDelta(out Rotator rotation);
    }
}
