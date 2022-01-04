namespace Spotlight.API
{
    using System;
    using System.Collections.Generic;

    using Rage;

    using Spotlight.Core.Memory;

    public sealed class APISpotlight : BaseSpotlight, IDisposable
    {
        public Entity TrackedEntity { get; set; }

        public APISpotlight(SpotlightData data) : base(data)
        {
            APISpotlightMgr.AddSpotlight(this);
        }

        public void Dispose()
        {
            APISpotlightMgr.RemoveSpotlight(this);
        }

        ~APISpotlight()
        {
            Dispose();
        }

        internal void Update()
        {
            if (!IsActive)
                return;

            if (TrackedEntity)
            {
                Direction = (TrackedEntity.Position - Position).ToNormalized();
            }

            DrawLight();
        }
    }



    internal static unsafe class APISpotlightMgr
    {
        private static GameFiber fiber;
        private static List<APISpotlight> spotlights;
        private static StaticFinalizer finalizer;

        static APISpotlightMgr()
        {
            if (!(GameFunctions.Init() && GameMemory.Init() && GameOffsets.Init()))
            {
                Game.LogTrivial($"[ERROR] Failed to initialize spotlight API");
                return;
            }

            spotlights = new List<APISpotlight>();
            fiber = GameFiber.StartNew(UpdateSpotlights, "Spotlight API Manager");

            finalizer = new StaticFinalizer(Dispose);
        }

        static void Dispose()
        {
            spotlights.Clear();
        }

        private static void UpdateSpotlights()
        {
            WinFunctions.CopyTlsValues(WinFunctions.GetProcessMainThreadId(), WinFunctions.GetCurrentThreadId(), GameOffsets.TlsAllocator);
            while (true)
            {
                GameFiber.Yield();
                for (int i = 0; i < spotlights.Count; i++)
                {
                    spotlights[i]?.Update();
                }
            }
        }

        public static void AddSpotlight(APISpotlight spotlight)
        {
            spotlights?.Add(spotlight);
        }

        public static void RemoveSpotlight(APISpotlight spotlight)
        {
            if (spotlights != null && spotlights.Contains(spotlight))
            {
                spotlights.Remove(spotlight);
            }
        }
    }
}
