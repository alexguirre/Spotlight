namespace Spotlight
{
    using System;

    using Rage;
    
    using Spotlight.Core.Memory;
    
    public interface ISpotlight
    {
        SpotlightData Data { get; }
        Vector3 Position { get; }
        Vector3 Direction { get; }
        bool IsActive { get; set; }
    }

    public abstract unsafe class BaseSpotlight : ISpotlight
    {
        public SpotlightData Data { get; }
        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
        public bool IsActive { get; set; }

        private readonly ulong shadowId;

        internal BaseSpotlight(SpotlightData data)
        {
            Data = data;
            shadowId = GetNewShadowId();
        }

        protected internal void DrawLight()
        {
            if (!IsActive)
                return;
            
            CLightDrawData* drawData = CLightDrawData.New(eLightType.SPOT_LIGHT, Data.Volume ? eLightFlags.VolumeEnabled : eLightFlags.None, Position, Data.Color, Data.Intensity);
            NativeVector3 dir = Direction;
            drawData->Range = Data.Range;
            drawData->VolumeIntensity = Data.VolumeIntensity;
            drawData->VolumeExponent = 70.0f; // doesn't seem to have any effect
            drawData->VolumeSize = Data.VolumeSize;
            drawData->FalloffExponent = Data.Falloff;
            
            // this doesn't create a vector exactly like the one created in the original game code, but it looks like the game just wants a vector perpendicular
            // to the direction vector and while testing this seemed to work fine
            // if any issue arises, revert to the old code
            NativeVector3 dirPerpendicular = new Vector3(dir.Y - dir.Z, dir.Z - dir.X, -dir.X - dir.Y).ToNormalized();

            GameFunctions.SetLightDrawDataDirection(drawData, &dir, &dirPerpendicular);
            GameFunctions.SetLightDrawDataAngles(drawData, Data.InnerAngle, Data.OuterAngle);

            if (Data.CastShadows)
            {
                drawData->Flags |= eLightFlags.ShadowsEnabled;
                drawData->ShadowRenderId = shadowId;
                drawData->ShadowUnkValue = GameFunctions.GetValueForLightDrawDataShadowUnkValue(drawData);
            }

            if (!Data.Specular)
            {
                drawData->Flags |= eLightFlags.DisableSpecular;
            }

            if (Data.Corona)
            {
                GameMemory.Coronas->Draw(Position, Data.CoronaSize, Data.Color.Raw, Data.CoronaIntensity, 100.0f, Direction, Data.InnerAngle, Data.OuterAngle, 3);
            }
        }



        private static uint totalShadowsId = 0;
        private static ulong GetNewShadowId()
        {
            totalShadowsId++;
            if (totalShadowsId > 200) // own limit, I don't know if game has a limit or in case it has, if it's lower
                totalShadowsId = 1;
            ulong id = 0x46B9FB69 + totalShadowsId;  // 0x46B9FB69 is the value that the original game functions take from the RagePluginHook's CGameScriptId (it's JOOAT hash of "RagePluginHook")
            return id;
        }
    }
}
