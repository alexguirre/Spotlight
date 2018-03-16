namespace Spotlight
{
    using System;

    using Rage;
    
    using Spotlight.Core.Memory;

    using static Core.Intrin;

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
            drawData->Range = Data.Range;
            drawData->VolumeIntensity = Data.VolumeIntensity;
            drawData->VolumeExponent = 70.0f; // doesn't seem to have any effect
            drawData->VolumeSize = Data.VolumeSize;
            drawData->FalloffExponent = Data.Falloff;
            
            // TODO: figure out how SSE instrinsics work, and what this does
            NativeVector3 v16 = _mm_andnot_ps(new Vector3(-0.0f, -0.0f, -0.0f), dir);
            NativeVector3 v17 = _mm_and_ps(_mm_cmple_ps(v16, _mm_shuffle_epi32(v16, -46)), _mm_cmplt_ps(v16, _mm_shuffle_epi32(v16, -55)));
            NativeVector3 v18 = _mm_and_ps(
                                    _mm_or_ps(
                                      _mm_andnot_ps(
                                        _mm_or_ps(
                                          _mm_or_ps(_mm_shuffle_epi32(v17, 85), _mm_shuffle_epi32(v17, 0)),
                                          _mm_shuffle_epi32(v17, -86)),
                                          new Vector3(Single.NaN, Single.NaN, Single.NaN)),
                                      v17),
                                    new Vector3(1.0f, 1.0f, 1.0f));
            v17 = _mm_shuffle_ps(v18, v18, 85);
            NativeVector3 v19 = _mm_shuffle_ps(v18, v18, -86);

            Vector3 v = new Vector3(x: (v19.X * dir.Y) - (v17.X * dir.Z),
                                    y: (v18.X * dir.Z) - (v19.X * dir.X),
                                    z: (v17.X * dir.X) - (v18.X * dir.Y));
            NativeVector3 u = v.ToNormalized();

            GameFunctions.SetLightDrawDataDirection(drawData, &dir, &u);
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
