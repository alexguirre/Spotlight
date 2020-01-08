namespace Spotlight
{
    using System;

    using Rage;
    using Spotlight.Core;
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
        public virtual SpotlightData Data { get; }
        public virtual Vector3 Position { get; set; }
        public virtual Vector3 Direction { get; set; }
        public virtual bool IsActive { get; set; }

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

            eLightFlags lightFlags = eLightFlags.CanRenderUnderground;

            if (Data.Volume)
            {
                lightFlags |= eLightFlags.EnableVolume;
            }

            if (!Data.Specular)
            {
                lightFlags |= eLightFlags.DisableSpecular;
            }

            if (Data.CastShadows)
            {
                // shadows aren't casted in underground areas, such as tunnels, 
                // not sure how to fix this as the game's CSearchLight has the same issue
                lightFlags |= eLightFlags.EnableShadows;
            }

            CLightDrawData* drawData = CLightDrawData.New(eLightType.SPOT_LIGHT, lightFlags, Position, Data.Color, Data.Intensity);
            NativeVector3 dir = Direction;
            NativeVector3 perp = Utility.GetPerpendicular(dir).ToNormalized();
            drawData->Range = Data.Range;
            drawData->VolumeIntensity = Data.VolumeIntensity;
            drawData->VolumeExponent = 70.0f; // doesn't seem to have any effect
            drawData->VolumeSize = Data.VolumeSize;
            drawData->FalloffExponent = Data.Falloff;

            GameFunctions.SetLightDrawDataDirection(drawData, &dir, &perp);
            GameFunctions.SetLightDrawDataAngles(drawData, Data.InnerAngle, Data.OuterAngle);

            if (Data.CastShadows)
            {
                drawData->ShadowRenderId = shadowId;
                drawData->ShadowUnkValue = GameFunctions.GetValueForLightDrawDataShadowUnkValue(drawData);
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
            ulong id = 0xE79C9874 + totalShadowsId;  // 0xE79C9874 is JOAAT hash of "alexguirre", just a random base id for the shadow ids, game uses the script name hash for this 
            return id;
        }
    }
}
