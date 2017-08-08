namespace Spotlight
{
    using System;
    using System.Drawing;
    using System.Diagnostics;
    using System.Windows.Forms;
    using System.Runtime.InteropServices;

    using Rage;
    using Rage.Native;

    using Engine.Memory;

    using static Intrin;

    internal static class Utility
    {
        public static unsafe void DrawSpotlight(ISpotlight spotlight)
        {
            if (!spotlight.IsActive)
                return;

            // TODO: add shadows
            CLightDrawData* drawData = CLightDrawData.New(eLightType.SPOT_LIGHT, eLightFlags.VolumeConeVisible, spotlight.Position, spotlight.Data.Color, spotlight.Data.Brightness);
            NativeVector3 dir = spotlight.Direction;
            drawData->Range = spotlight.Data.Distance;
            drawData->VolumeIntensity = 0.3f;
            drawData->VolumeExponent = 70.0f;
            drawData->VolumeSize = 0.1f;
            drawData->FalloffExponent = spotlight.Data.Falloff;

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
            GameFunctions.SetLightDrawDataRoundnessAndRadius(drawData, spotlight.Data.Roundness, spotlight.Data.Radius);

            // wtf? why calling the wrapper method Utility.DrawCorona crashes, but calling it directly it doesn't?
            // and apparently, now I can call it from a normal gamefiber too, no need for the FrameRender
            //
            // and it stopped working again... :‑|
            //
            //NativeVector3 p = spotlight.Position;
            //NativeVector3 d = spotlight.Direction;
            //GameFunctions.DrawCorona(GameFunctions.DrawCoronaUnkPtr, &p, 2.25f, unchecked((uint)spotlight.Data.Color.ToArgb()), 80.0f, 100.0f, &dir, 1.0f, 5.0f, spotlight.Data.Radius, 3);
        }

        public static void DrawSpotlight(Vector3 position, Vector3 direction, Color color, bool shadow, float radius, float brightness, float distance, float falloff, float roundness)
        {
            const ulong DrawSpotlightNative = 0xd0f64b265c8c8b33;
            const ulong DrawSpotlightWithShadowNative = 0x5bca583a583194db;

            NativeFunction.CallByHash<uint>(shadow ? DrawSpotlightWithShadowNative : DrawSpotlightNative, 
                                            position.X, position.Y, position.Z,
                                            direction.X, direction.Y, direction.Z,
                                            color.R, color.G, color.B,
                                            distance, brightness, roundness, 
                                            radius, falloff, shadow ? 0.0f : 0);
        }

        public static bool IsKeyDownWithModifier(Keys key, Keys modifier)
        {
            return modifier == Keys.None ? Game.IsKeyDown(key) : (Game.IsKeyDownRightNow(modifier) && Game.IsKeyDown(key));
        }

        public static bool IsKeyDownRightNowWithModifier(Keys key, Keys modifier)
        {
            return modifier == Keys.None ? Game.IsKeyDownRightNow(key) : (Game.IsKeyDownRightNow(modifier) && Game.IsKeyDownRightNow(key));
        }

        public static bool IsControllerButtonDownWithModifier(ControllerButtons button, ControllerButtons modifier)
        {
            return modifier == ControllerButtons.None ? Game.IsControllerButtonDown(button) : (Game.IsControllerButtonDownRightNow(modifier) && Game.IsControllerButtonDown(button));
        }

        public static bool IsControllerButtonDownRightNowWithModifier(ControllerButtons button, ControllerButtons modifier)
        {
            return modifier == ControllerButtons.None ? Game.IsControllerButtonDownRightNow(button) : (Game.IsControllerButtonDownRightNow(modifier) && Game.IsControllerButtonDownRightNow(button));
        }
    }


    internal static class Intrin
    {
        [StructLayout(LayoutKind.Explicit)]
        struct FloatUIntUnion
        {
            [FieldOffset(0)] public uint UInt;
            [FieldOffset(0)] public float Float;
        }

        private static uint GetUIntFromFloat(float v)
        {
            FloatUIntUnion u = default(FloatUIntUnion);
            u.Float = v;
            return u.UInt;
        }

        private static float GetFloatFromUInt(uint v)
        {
            FloatUIntUnion u = default(FloatUIntUnion);
            u.UInt = v;
            return u.Float;
        }

        // no idea if all these are the exact equivalents, but they would have to work for now...
        public static Vector3 _mm_andnot_ps(Vector3 a, Vector3 b)
        {
            /*
                https://software.intel.com/sites/landingpage/IntrinsicsGuide/#text=_mm_andnot_ps
                FOR j := 0 to 3
	                i := j*32
	                dst[i+31:i] := ((NOT a[i+31:i]) AND b[i+31:i])
                ENDFOR
            */

            uint x = ~GetUIntFromFloat(a.X) & GetUIntFromFloat(b.X);
            uint y = ~GetUIntFromFloat(a.Y) & GetUIntFromFloat(b.Y);
            uint z = ~GetUIntFromFloat(a.Z) & GetUIntFromFloat(b.Z);

            return new Vector3(GetFloatFromUInt(x), GetFloatFromUInt(y), GetFloatFromUInt(z));
        }

        public static Vector3 _mm_and_ps(Vector3 a, Vector3 b)
        {
            /*
                https://software.intel.com/sites/landingpage/IntrinsicsGuide/#text=_mm_and_ps
                FOR j := 0 to 3
	                i := j*32
	                dst[i+31:i] := (a[i+31:i] AND b[i+31:i])
                ENDFOR
            */

            uint x = GetUIntFromFloat(a.X) & GetUIntFromFloat(b.X);
            uint y = GetUIntFromFloat(a.Y) & GetUIntFromFloat(b.Y);
            uint z = GetUIntFromFloat(a.Z) & GetUIntFromFloat(b.Z);

            return new Vector3(GetFloatFromUInt(x), GetFloatFromUInt(y), GetFloatFromUInt(z));
        }

        public static Vector3 _mm_cmple_ps(Vector3 a, Vector3 b)
        {
            /*
                https://software.intel.com/sites/landingpage/IntrinsicsGuide/#text=_mm_cmple_ps
                FOR j := 0 to 3
	                i := j*32
	                dst[i+31:i] := ( a[i+31:i] <= b[i+31:i] ) ? 0xffffffff : 0
                ENDFOR
            */

            uint x = (GetUIntFromFloat(a.X) <= GetUIntFromFloat(b.X)) ? 0xFFFFFFFF : 0;
            uint y = (GetUIntFromFloat(a.Y) <= GetUIntFromFloat(b.Y)) ? 0xFFFFFFFF : 0;
            uint z = (GetUIntFromFloat(a.Z) <= GetUIntFromFloat(b.Z)) ? 0xFFFFFFFF : 0;

            return new Vector3(GetFloatFromUInt(x), GetFloatFromUInt(y), GetFloatFromUInt(z));
        }

        public static Vector3 _mm_cmplt_ps(Vector3 a, Vector3 b)
        {
            /*
                https://software.intel.com/sites/landingpage/IntrinsicsGuide/#text=_mm_cmplt_ps
                FOR j := 0 to 3
	                i := j*32
	                dst[i+31:i] := ( a[i+31:i] < b[i+31:i] ) ? 0xffffffff : 0
                ENDFOR
            */

            uint x = (GetUIntFromFloat(a.X) < GetUIntFromFloat(b.X)) ? 0xFFFFFFFF : 0;
            uint y = (GetUIntFromFloat(a.Y) < GetUIntFromFloat(b.Y)) ? 0xFFFFFFFF : 0;
            uint z = (GetUIntFromFloat(a.Z) < GetUIntFromFloat(b.Z)) ? 0xFFFFFFFF : 0;

            return new Vector3(GetFloatFromUInt(x), GetFloatFromUInt(y), GetFloatFromUInt(z));
        }

        public static Vector3 _mm_or_ps(Vector3 a, Vector3 b)
        {
            /*
                https://software.intel.com/sites/landingpage/IntrinsicsGuide/#text=_mm_or_ps
                FOR j := 0 to 3
	                i := j*32
	                dst[i+31:i] := a[i+31:i] BITWISE OR b[i+31:i]
                ENDFOR
            */

            uint x = GetUIntFromFloat(a.X) | GetUIntFromFloat(b.X);
            uint y = GetUIntFromFloat(a.Y) | GetUIntFromFloat(b.Y);
            uint z = GetUIntFromFloat(a.Z) | GetUIntFromFloat(b.Z);

            return new Vector3(GetFloatFromUInt(x), GetFloatFromUInt(y), GetFloatFromUInt(z));
        }

        public static Vector3 _mm_shuffle_ps(Vector3 a, Vector3 b, int imm8)
        {
            /*
                https://software.intel.com/sites/landingpage/IntrinsicsGuide/#text=_mm_shuffle_ps
                SELECT4(src, control){
	                CASE(control[1:0])
	                0:	tmp[31:0] := src[31:0]
	                1:	tmp[31:0] := src[63:32]
	                2:	tmp[31:0] := src[95:64]
	                3:	tmp[31:0] := src[127:96]
	                ESAC
	                RETURN tmp[31:0]
                }

                dst[31:0] := SELECT4(a[127:0], imm8[1:0])
                dst[63:32] := SELECT4(a[127:0], imm8[3:2])
                dst[95:64] := SELECT4(b[127:0], imm8[5:4])
                dst[127:96] := SELECT4(b[127:0], imm8[7:6])
            */
            uint Select(Vector3 src, uint control)
            {
                switch ((control & (1 << 1 - 1)))
                {
                    case 0: return GetUIntFromFloat(src.X);
                    case 1: return GetUIntFromFloat(src.Y);
                    case 2: return GetUIntFromFloat(src.Z);
                }
                return 0;
            }

            uint u = unchecked((uint)imm8);

            uint x = Select(a, (u & (1 << 1 - 1)));
            uint y = Select(a, (u & (1 << 3 - 1)));
            uint z = Select(b, (u & (1 << 5 - 1)));

            return new Vector3(GetFloatFromUInt(x), GetFloatFromUInt(y), GetFloatFromUInt(z));
        }
        //x[to:from]

        public static Vector3 _mm_shuffle_epi32(Vector3 a, int imm8)
        {
            /*
                https://software.intel.com/sites/landingpage/IntrinsicsGuide/#text=_mm_shuffle_epi32
                SELECT4(src, control){
	                CASE(control[1:0])
	                0:	tmp[31:0] := src[31:0]
	                1:	tmp[31:0] := src[63:32]
	                2:	tmp[31:0] := src[95:64]
	                3:	tmp[31:0] := src[127:96]
	                ESAC
	                RETURN tmp[31:0]
                }

                dst[31:0] := SELECT4(a[127:0], imm8[1:0])
                dst[63:32] := SELECT4(a[127:0], imm8[3:2])
                dst[95:64] := SELECT4(a[127:0], imm8[5:4])
                dst[127:96] := SELECT4(a[127:0], imm8[7:6])
            */
            uint Select(Vector3 src, uint control)
            {
                switch ((control & (1 << 1 - 1)))
                {
                    case 0: return GetUIntFromFloat(src.X);
                    case 1: return GetUIntFromFloat(src.Y);
                    case 2: return GetUIntFromFloat(src.Z);
                }
                return 0;
            }

            uint u = unchecked((uint)imm8);

            uint x = Select(a, (u & (1 << 1 - 1)));
            uint y = Select(a, (u & (1 << 3 - 1)));
            uint z = Select(a, (u & (1 << 5 - 1)));

            return new Vector3(GetFloatFromUInt(x), GetFloatFromUInt(y), GetFloatFromUInt(z));
        }
    }
}
