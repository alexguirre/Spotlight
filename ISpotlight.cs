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


    public abstract class BaseSpotlight : ISpotlight
    {
        public SpotlightData Data { get; }
        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
        public bool IsActive { get; set; }

        internal BaseSpotlight(SpotlightData data)
        {
            Data = data;
        }

        protected internal unsafe void DrawLight()
        {
            if (!IsActive)
                return;

            // TODO: add shadows
            CLightDrawData* drawData = CLightDrawData.New(eLightType.SPOT_LIGHT, eLightFlags.VolumeConeVisible, Position, Data.Color, Data.Brightness);
            NativeVector3 dir = Direction;
            drawData->Range = Data.Distance;
            drawData->VolumeIntensity = 0.3f;
            drawData->VolumeExponent = 70.0f;
            drawData->VolumeSize = 0.1f;
            drawData->FalloffExponent = Data.Falloff;

            // no idea how this works, copied from a game function
            // not event sure if these functions are the exact equivalents
            // but at least it works :P
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

            Vector3 v = new Vector3();
            v.X = (v19.X * dir.Y) - (v17.X * dir.Z);
            v.Y = (v18.X * dir.Z) - (v19.X * dir.X);
            v.Z = (v17.X * dir.X) - (v18.X * dir.Y);
            NativeVector3 u = v.ToNormalized();

            GameFunctions.SetLightDrawDataDirection(drawData, &dir, &u);
            GameFunctions.SetLightDrawDataRoundnessAndRadius(drawData, Data.Roundness, Data.Radius);

            // wtf? why calling the wrapper method Utility.DrawCorona crashes, but calling it directly it doesn't?
            // and apparently, now I can call it from a normal gamefiber too, no need for the FrameRender
            //
            // and it stopped working again... :‑|
            //
            // well, calling it from the FrameRender now...
            //NativeVector3 p = Position;
            //NativeVector3 d = Direction;
            //GameFunctions.DrawCorona(CCoronaDrawQueue.GetInstance(), &p, 2.25f, 0xFFFFFFFF, 80.0f, 100.0f, &d, 1.0f, 0.0f, Data.Radius, 3);
        }


        protected internal unsafe void OnDrawCoronaFrameRender(object sender, GraphicsEventArgs e)
        {
            if (!IsActive)
                return;
            NativeVector3 p = Position;
            NativeVector3 d = Direction;
            GameFunctions.DrawCorona(CCoronaDrawQueue.GetInstance(), &p, 1.5f, unchecked((uint)Data.Color.ToArgb()), Data.Brightness * 0.625f, 100f, &d, 1.0f, 0.0f, Data.Radius, 3);
        }
    }
}
