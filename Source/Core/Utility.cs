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

        public static bool IsPauseMenuActive => NativeFunction.Natives.IsPauseMenuActive<bool>();

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
            NativeVector3 v = veh->inst->entry->skeleton->skeletonData->bones[index].translation;
            return v;
        }

        public static unsafe Quaternion GetBoneOriginalRotation(Vehicle vehicle, int index)
        {
            CVehicle* veh = (CVehicle*)vehicle.MemoryAddress;
            Quaternion q = veh->inst->entry->skeleton->skeletonData->bones[index].rotation;
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

        public static string GetLocalizedString(IntPtr stringPtr)
        {
            return NativeFunction.Natives.x7B5280EBA9840C72<string>(stringPtr); //_GET_LABEL_TEXT
        }

        public static unsafe bool IsStringEmpty(IntPtr stringPtr)
        {
            return *(byte*)stringPtr == 0;
        }

        public static Vector3 GetPerpendicular(Vector3 v)
        {
            float x = Math.Abs(v.X);
            float y = Math.Abs(v.Y);
            float z = Math.Abs(v.Z);
            if (x < y)
            {
                if (x < z)
                {
                    return Vector3.Cross(v, Vector3.WorldEast);
                }
                else
                {
                    return Vector3.Cross(v, Vector3.WorldUp);
                }
            }
            else
            {
                if (y < z)
                {
                    return Vector3.Cross(v, Vector3.WorldNorth);
                }
                else
                {
                    return Vector3.Cross(v, Vector3.WorldUp);
                }
            }
        }

        public static string FormatDistance(float meters)
            => NativeFunction.Natives.xD3D15555431AB793<bool>() ? // SHOULD_USE_METRIC_MEASUREMENTS
                $"{(int)Math.Round(meters)} meters" :
                $"{(int)Math.Round(MetersToFeet(meters))} feet";

        private static float MetersToFeet(float meters) => meters * 3.2808f;
    }
}
