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
            Game.FrameRender += OnDrawCoronaFrameRender;
            APISpotlightMgr.AddSpotlight(this);
        }

        public void Dispose()
        {
            Game.FrameRender -= OnDrawCoronaFrameRender;
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



    internal static class APISpotlightMgr
    {
        private static GameFiber fiber;
        private static List<APISpotlight> spotlights;

        static APISpotlightMgr()
        {
            bool gameFnInit = GameFunctions.Init();
            bool gameMemInit = GameMemory.Init();
            
            if (!gameFnInit || !gameMemInit)
            {
                string str = "";
                if (!gameFnInit)
                {
                    str += nameof(GameFunctions);

                    if (!gameMemInit)
                    {
                        str += " and ";
                        str += nameof(GameMemory);
                    }
                }
                else if (!gameMemInit)
                {
                    str += nameof(GameMemory);
                }
                
                Game.LogTrivial($"[ERROR] Spotlight: Failed to initialize {str}");
            }


            spotlights = new List<APISpotlight>();
            fiber = GameFiber.StartNew(UpdateSpotlights, "Spotlight API Manager");
        }

        private static void UpdateSpotlights()
        {
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
