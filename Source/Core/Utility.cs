namespace Spotlight.Core
{
    using System;
    using System.Drawing;
    using System.Diagnostics;
    using System.Windows.Forms;
    using System.Runtime.InteropServices;

    using Rage;
    using Rage.Native;

    using Spotlight.Core.Memory;

    internal static class Utility
    {
        static Utility()
        {
            // executing it here first prevents a small freeze during gameplay the first time GetDisabledControlNormal is used, for example when using the MouseSpotlightInputController for the first time
            GetDisabledControlNormal((GameControl)0);
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

        public static float GetDisabledControlNormal(GameControl control)
        {
            return NativeFunction.Natives.GetDisabledControlNormal<float>(0, (int)control);
        }

        public static unsafe Vector3 GetBoneOriginalTranslation(Vehicle vehicle, int index)
        {
            CVehicle* veh = (CVehicle*)vehicle.MemoryAddress;
            NativeVector3 v = veh->inst->archetype->skeleton->skeletonData->bones[index].translation;
            return v;
        }

        public static unsafe Quaternion GetBoneOriginalRotation(Vehicle vehicle, int index)
        {
            CVehicle* veh = (CVehicle*)vehicle.MemoryAddress;
            Quaternion q = veh->inst->archetype->skeleton->skeletonData->bones[index].rotation;
            return q;
        }

        // https://code.google.com/archive/p/slimmath/
        public static bool DecomposeMatrix(Matrix matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation)
        {
            const float ZeroTolerance = 1e-6f;

            //Source: Unknown
            //References: http://www.gamedev.net/community/forums/topic.asp?topic_id=441695

            //Get the translation.
            translation.X = matrix.M41;
            translation.Y = matrix.M42;
            translation.Z = matrix.M43;

            //Scaling is the length of the rows.
            scale.X = (float)Math.Sqrt((matrix.M11 * matrix.M11) + (matrix.M12 * matrix.M12) + (matrix.M13 * matrix.M13));
            scale.Y = (float)Math.Sqrt((matrix.M21 * matrix.M21) + (matrix.M22 * matrix.M22) + (matrix.M23 * matrix.M23));
            scale.Z = (float)Math.Sqrt((matrix.M31 * matrix.M31) + (matrix.M32 * matrix.M32) + (matrix.M33 * matrix.M33));

            //If any of the scaling factors are zero, than the rotation matrix can not exist.
            if (Math.Abs(scale.X) < ZeroTolerance ||
                Math.Abs(scale.Y) < ZeroTolerance ||
                Math.Abs(scale.Z) < ZeroTolerance)
            {
                rotation = Quaternion.Identity;
                return false;
            }

            //The rotation is the left over matrix after dividing out the scaling.
            Matrix rotationmatrix = new Matrix();
            rotationmatrix.M11 = matrix.M11 / scale.X;
            rotationmatrix.M12 = matrix.M12 / scale.X;
            rotationmatrix.M13 = matrix.M13 / scale.X;

            rotationmatrix.M21 = matrix.M21 / scale.Y;
            rotationmatrix.M22 = matrix.M22 / scale.Y;
            rotationmatrix.M23 = matrix.M23 / scale.Y;

            rotationmatrix.M31 = matrix.M31 / scale.Z;
            rotationmatrix.M32 = matrix.M32 / scale.Z;
            rotationmatrix.M33 = matrix.M33 / scale.Z;

            rotationmatrix.M44 = 1f;

            Quaternion.RotationMatrix(ref rotationmatrix, out rotation);
            return true;
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

        public static Vector3 _mm_mul_ps(Vector3 a, Vector3 b)
        {
            /*
                https://software.intel.com/sites/landingpage/IntrinsicsGuide/#text=_mm_mul_ps
                FOR j := 0 to 3
                    i := j*32
                    dst[i+31:i] := a[i+31:i] * b[i+31:i]
                ENDFOR
            */

            return new Vector3(a.X * b.X,
                               a.Y * b.Y,
                               a.Z * b.Z);
        }

        public static Vector3 _mm_add_ps(Vector3 a, Vector3 b)
        {
            /*
                https://software.intel.com/sites/landingpage/IntrinsicsGuide/#text=_mm_add_ps
                FOR j := 0 to 3
                    i := j*32
                    dst[i+31:i] := a[i+31:i] + b[i+31:i]
                ENDFOR
            */

            return new Vector3(a.X + b.X,
                               a.Y + b.Y,
                               a.Z + b.Z);
        }

        public static Vector3 _mm_rsqrt_ps(Vector3 a)
        {
            /*
                https://software.intel.com/sites/landingpage/IntrinsicsGuide/#text=_mm_rsqrt_ps
                FOR j := 0 to 3
                    i := j*32
                    dst[i+31:i] := APPROXIMATE(1.0 / SQRT(a[i+31:i]))
                ENDFOR
            */

            return new Vector3(1.0f / (float)Math.Sqrt(a.X),
                               1.0f / (float)Math.Sqrt(a.Y),
                               1.0f / (float)Math.Sqrt(a.Z));
        }

        public static Vector3 _mm_sub_ps(Vector3 a, Vector3 b)
        {
            /*
                https://software.intel.com/sites/landingpage/IntrinsicsGuide/#text=_mm_sub_ps
                FOR j := 0 to 3
                    i := j*32
                    dst[i+31:i] := a[i+31:i] - b[i+31:i]
                ENDFOR
            */

            return new Vector3(a.X - b.X,
                               a.Y - b.Y,
                               a.Z - b.Z);
        }
    }
}
